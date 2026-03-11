using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;

namespace ExclusionEngine.Web
{
    public partial class CsvImport : System.Web.UI.Page
    {
        private int UserId => Convert.ToInt32(Session["UserId"]);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindClients();
            }
        }

        private void BindClients()
        {
            var clients = Repository.GetClientsForUser(UserId);
            ClientDropDown.DataSource = clients;
            ClientDropDown.DataTextField = "ClientDisplay";
            ClientDropDown.DataValueField = "ClientId";
            ClientDropDown.DataBind();
        }

        protected void ImportButton_Click(object sender, EventArgs e)
        {
            ResultsPanel.Visible = false;
            MessageLabel.Text = string.Empty;

            if (!int.TryParse(ClientDropDown.SelectedValue, out var clientId) || clientId <= 0)
            {
                MessageLabel.Text = "<span class='error'>Please select a client.</span>";
                return;
            }

            if (!CsvFileUpload.HasFile)
            {
                MessageLabel.Text = "<span class='error'>Please select a CSV file to upload.</span>";
                return;
            }

            if (!Repository.UserHasClientAccess(UserId, clientId))
            {
                MessageLabel.Text = "<span class='error'>You are not authorized for the selected client.</span>";
                return;
            }

            List<string[]> rows;
            try
            {
                using (var reader = new StreamReader(CsvFileUpload.FileContent, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    rows = ParseCsv(reader);
                }
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"<span class='error'>Failed to read CSV: {HttpUtility.HtmlEncode(ex.Message)}</span>";
                return;
            }

            if (rows.Count < 2)
            {
                MessageLabel.Text = "<span class='error'>CSV must contain a header row and at least one data row.</span>";
                return;
            }

            Dictionary<string, int> headers;
            try
            {
                headers = MapHeaders(rows[0]);
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"<span class='error'>{HttpUtility.HtmlEncode(ex.Message)}</span>";
                return;
            }

            var results = new DataTable();
            results.Columns.Add("Row");
            results.Columns.Add("Name");
            results.Columns.Add("Address");
            results.Columns.Add("Status");
            results.Columns.Add("ImportNotes");

            var successCount = 0;
            var errorCount = 0;
            var skippedCount = 0;

            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];

                // Skip entirely blank rows
                var allBlank = true;
                foreach (var cell in row)
                {
                    if (!string.IsNullOrWhiteSpace(cell))
                    {
                        allBlank = false;
                        break;
                    }
                }

                if (allBlank)
                {
                    skippedCount++;
                    continue;
                }

                var entry = BuildEntry(clientId, row, headers);
                var rowNum = (i + 1).ToString(); // +1 because row 1 is header
                string statusText;
                string notesText;

                if (string.IsNullOrWhiteSpace(entry.Address1) || string.IsNullOrWhiteSpace(entry.City) || string.IsNullOrWhiteSpace(entry.State))
                {
                    statusText = "Error";
                    notesText = "Address1, City, and State are required.";
                    errorCount++;
                    results.Rows.Add(rowNum, entry.FullName, entry.FormattedAddress, statusText, notesText);
                    continue;
                }

                try
                {
                    var cass = SatoriCassService.StandardizeAddress(entry);
                    CustomerEntryInput toSave;

                    if (cass.HasError)
                    {
                        toSave = entry;
                        notesText = "CASS warning: " + cass.ErrorMessage + " (saved with original address)";
                    }
                    else if (cass.HasChanges)
                    {
                        toSave = cass.Standardized;
                        notesText = "Address standardized by CASS.";
                    }
                    else
                    {
                        toSave = cass.Standardized ?? entry;
                        notesText = string.Empty;
                    }

                    Repository.SaveEntry(UserId, toSave);
                    statusText = "Imported";
                    successCount++;
                }
                catch (Exception ex)
                {
                    statusText = "Error";
                    notesText = ex.Message;
                    errorCount++;
                }

                results.Rows.Add(rowNum, entry.FullName, entry.FormattedAddress, statusText, notesText);
            }

            ResultsPanel.Visible = true;
            SummaryLabel.Text = $"<p><strong>{successCount} imported</strong>, {errorCount} failed" +
                (skippedCount > 0 ? $", {skippedCount} blank rows skipped" : string.Empty) + ".</p>";
            ResultsGrid.DataSource = results;
            ResultsGrid.DataBind();

            MessageLabel.Text = errorCount == 0
                ? $"<span class='success'>Import complete. {successCount} records imported.</span>"
                : $"<span class='warn'>Import complete with errors. {successCount} imported, {errorCount} failed.</span>";
        }

        private static CustomerEntryInput BuildEntry(int clientId, string[] row, Dictionary<string, int> headers)
        {
            var zipRaw = GetCell(row, headers, "zip");
            return new CustomerEntryInput
            {
                ClientId = clientId,
                CustomerNumber = GetCell(row, headers, "customernumber"),
                FirstName = ToTitleCase(GetCell(row, headers, "firstname")),
                LastName = ToTitleCase(GetCell(row, headers, "lastname")),
                Address1 = GetCell(row, headers, "address1"),
                Address2 = GetCell(row, headers, "address2"),
                City = GetCell(row, headers, "city"),
                State = (GetCell(row, headers, "state") ?? string.Empty).Trim().ToUpperInvariant(),
                Zip = ExtractZip5(zipRaw),
                Zip4 = ExtractZip4(zipRaw),
                Email = GetCell(row, headers, "email"),
                Phone = GetCell(row, headers, "phone"),
                Notes = GetCell(row, headers, "notes")
            };
        }

        private static string GetCell(string[] row, Dictionary<string, int> headers, string canonicalKey)
        {
            if (!headers.TryGetValue(canonicalKey, out var idx) || idx >= row.Length)
                return string.Empty;
            return (row[idx] ?? string.Empty).Trim();
        }

        private static Dictionary<string, int> MapHeaders(string[] headerRow)
        {
            var aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "customernumber", new[] { "customernumber", "customer number", "customer#", "cust#", "customer_number", "custno", "cust no", "customer no" } },
                { "firstname",      new[] { "firstname", "first name", "first_name", "fname", "first" } },
                { "lastname",       new[] { "lastname", "last name", "last_name", "lname", "last", "surname" } },
                { "address1",       new[] { "address1", "address 1", "address_1", "address", "street", "streetaddress", "street address", "addr1", "addr 1" } },
                { "address2",       new[] { "address2", "address 2", "address_2", "apt", "suite", "unit", "addr2", "addr 2" } },
                { "city",           new[] { "city" } },
                { "state",          new[] { "state", "st" } },
                { "zip",            new[] { "zip", "zipcode", "zip code", "zip_code", "postalcode", "postal code", "postal_code", "zip5" } },
                { "email",          new[] { "email", "emailaddress", "email address", "email_address", "e-mail" } },
                { "phone",          new[] { "phone", "phonenumber", "phone number", "phone_number", "telephone", "tel", "mobile" } },
                { "notes",          new[] { "notes", "note", "comments", "comment", "remarks", "memo" } }
            };

            // Build reverse: alias text -> canonical key
            var reverse = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in aliases)
            {
                foreach (var alias in kvp.Value)
                    reverse[alias] = kvp.Key;
            }

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headerRow.Length; i++)
            {
                var h = (headerRow[i] ?? string.Empty).Trim();
                if (reverse.TryGetValue(h, out var canonical) && !map.ContainsKey(canonical))
                    map[canonical] = i;
            }

            if (!map.ContainsKey("address1") && !map.ContainsKey("city") && !map.ContainsKey("state"))
            {
                throw new InvalidOperationException(
                    "CSV header row was not recognized. Ensure the first row contains column headers. " +
                    "Expected at minimum: Address1, City, State.");
            }

            return map;
        }

        private static List<string[]> ParseCsv(TextReader reader)
        {
            var rows = new List<string[]>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                rows.Add(ParseCsvLine(line));
            }

            return rows;
        }

        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(field.ToString());
                        field.Clear();
                    }
                    else
                    {
                        field.Append(c);
                    }
                }
            }

            fields.Add(field.ToString());
            return fields.ToArray();
        }

        private static string ExtractZip5(string zip)
        {
            var digits = DigitsOnly(zip);
            return digits.Length >= 5 ? digits.Substring(0, 5) : digits;
        }

        private static string ExtractZip4(string zip)
        {
            var raw = (zip ?? string.Empty).Trim();
            var dashIdx = raw.IndexOf('-');
            if (dashIdx >= 0)
            {
                var suffix = DigitsOnly(raw.Substring(dashIdx + 1));
                return suffix.Length >= 4 ? suffix.Substring(0, 4) : suffix;
            }

            var digits = DigitsOnly(raw);
            return digits.Length >= 9 ? digits.Substring(5, 4) : string.Empty;
        }

        private static string DigitsOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var sb = new StringBuilder();
            foreach (var c in value)
            {
                if (char.IsDigit(c)) sb.Append(c);
            }

            return sb.ToString();
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLowerInvariant());
        }
    }
}
