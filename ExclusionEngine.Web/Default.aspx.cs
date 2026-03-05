using System;
using System.Web;
using System.Web.UI.WebControls;

namespace ExclusionEngine.Web
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ModalScriptLiteral.Text = string.Empty;

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            UserAdminButton.Visible = IsAdmin;
            ClientAdminButton.Visible = IsAdmin;

            if (!IsPostBack)
            {
                BindClients();
                BindRecent();
            }
        }

        private int UserId => Convert.ToInt32(Session["UserId"]);
        private bool IsAdmin => Session["IsAdmin"] != null && Convert.ToBoolean(Session["IsAdmin"]);
        private bool IsEditing => !string.IsNullOrWhiteSpace(EditingEntryId.Value);

        private void BindClients()
        {
            var clients = Repository.GetClientsForUser(UserId);
            ClientDropDown.DataSource = clients;
            ClientDropDown.DataTextField = "ClientDisplay";
            ClientDropDown.DataValueField = "ClientId";
            ClientDropDown.DataBind();
        }

        private void BindRecent()
        {
            RecentGrid.DataSource = Repository.GetRecentEntriesForUser(
                UserId,
                SearchLastNameTextBox.Text.Trim(),
                SearchAddress1TextBox.Text.Trim());
            RecentGrid.DataBind();
        }

        protected void SearchEntriesButton_Click(object sender, EventArgs e)
        {
            BindRecent();
        }

        protected void ClearSearchButton_Click(object sender, EventArgs e)
        {
            SearchLastNameTextBox.Text = string.Empty;
            SearchAddress1TextBox.Text = string.Empty;
            BindRecent();
        }

        protected void LogoutButton_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Login.aspx");
        }

        protected void ValidateAddressButton_Click(object sender, EventArgs e)
        {
            if (!TryBuildEntry(out var entry, out var validationError))
            {
                MessageLabel.Text = $"<span class='error'>{HttpUtility.HtmlEncode(validationError)}</span>";
                return;
            }

            if (!Repository.UserHasClientAccess(UserId, entry.ClientId))
            {
                MessageLabel.Text = "<span class='error'>You are not authorized for the selected client.</span>";
                return;
            }

            if (ConfirmedStandardized.Value == "true")
            {
                var toSave = UseOriginalAddress.Value == "true"
                    ? Session["PendingOriginalEntry"] as CustomerEntryInput ?? entry
                    : Session["PendingStandardizedEntry"] as CustomerEntryInput ?? entry;

                SaveEntry(toSave);
                return;
            }

            var cass = SatoriCassService.StandardizeAddress(entry);
            if (cass.HasChanges || cass.HasError)
            {
                var entered = HttpUtility.JavaScriptStringEncode(entry.FormattedAddress);
                var standardized = HttpUtility.JavaScriptStringEncode(cass.Standardized.FormattedAddress);
                var cassErrorMessage = HttpUtility.JavaScriptStringEncode(cass.ErrorMessage ?? string.Empty);
                var cassHasErrorJs = cass.HasError ? "true" : "false";
                ModalScriptLiteral.Text = $"<script>showCassModal('{entered}','{standardized}',{cassHasErrorJs},'{cassErrorMessage}');</script>";
                MessageLabel.Text = cass.HasError
                    ? "<span class='warn'>CASS validation reported an issue. Review and choose how to proceed.</span>"
                    : "<span class='warn'>Review the standardized address and save your selection.</span>";
                Session["PendingOriginalEntry"] = entry;
                Session["PendingStandardizedEntry"] = cass.Standardized;
                return;
            }

            SaveEntry(entry);
        }

        protected void RecentGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var rowIndex = Convert.ToInt32(e.CommandArgument);
            var entryId = Convert.ToInt32(RecentGrid.DataKeys[rowIndex].Value);

            if (e.CommandName == "EditEntry")
            {
                ClearPendingCassState();
                var entry = Repository.GetEntryForEdit(UserId, entryId);

                if (entry == null)
                {
                    MessageLabel.Text = "<span class='error'>Entry not found or not authorized.</span>";
                    return;
                }

                EditingEntryId.Value = entry.EntryId.ToString();
                ClientDropDown.SelectedValue = entry.ClientId.ToString();
                CustomerNumberTextBox.Text = entry.CustomerNumber;
                FirstNameTextBox.Text = entry.FirstName;
                LastNameTextBox.Text = entry.LastName;
                Address1TextBox.Text = entry.Address1;
                Address2TextBox.Text = entry.Address2;
                CityTextBox.Text = entry.City;
                StateTextBox.Text = entry.State;
                ZipTextBox.Text = string.IsNullOrWhiteSpace(entry.Zip4) ? entry.Zip : (entry.Zip + "-" + entry.Zip4);
                EmailTextBox.Text = entry.Email;

                ValidateAddressButton.Text = "Validate + Update";
                CancelEditButton.Visible = true;
                MessageLabel.Text = "<span class='warn'>Editing existing record. Save to update.</span>";
                return;
            }

            if (e.CommandName == "DeleteEntry")
            {
                try
                {
                    Repository.DeleteEntry(UserId, entryId);
                    ClearPendingCassState();
                    ResetEditor();
                    BindRecent();
                    MessageLabel.Text = "<span class='success'>Customer entry deleted.</span>";
                }
                catch (Exception ex)
                {
                    MessageLabel.Text = $"<span class='error'>{HttpUtility.HtmlEncode(ex.Message)}</span>";
                }
            }
        }

        protected void CancelEditButton_Click(object sender, EventArgs e)
        {
            ClearPendingCassState();
            ResetEditor();
            MessageLabel.Text = "<span class='success'>Edit canceled.</span>";
        }

        private void SaveEntry(CustomerEntryInput entry)
        {
            try
            {
                if (IsEditing)
                {
                    PreserveExistingCassFieldsForEdit(entry);
                    Repository.UpdateEntry(UserId, Convert.ToInt32(EditingEntryId.Value), entry);
                    MessageLabel.Text = "<span class='success'>Customer entry updated.</span>";
                }
                else
                {
                    Repository.SaveEntry(UserId, entry);
                    MessageLabel.Text = "<span class='success'>Customer entry saved.</span>";
                }

                ClearPendingCassState();
                ResetEditor();
                BindRecent();
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"<span class='error'>{HttpUtility.HtmlEncode(ex.Message)}</span>";
            }
        }

        private void PreserveExistingCassFieldsForEdit(CustomerEntryInput entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(EditingEntryId.Value)) return;

            var existing = Repository.GetEntryForEdit(UserId, Convert.ToInt32(EditingEntryId.Value));
            if (existing == null) return;

            if (string.IsNullOrWhiteSpace(entry.Zip4))
            {
                entry.Zip4 = existing.Zip4;
            }

            if (string.IsNullOrWhiteSpace(entry.DeliveryPointBarcode))
            {
                entry.DeliveryPointBarcode = existing.DeliveryPointBarcode;
            }

            if (string.IsNullOrWhiteSpace(entry.Dpv))
            {
                entry.Dpv = existing.Dpv;
            }
        }

        private void ClearPendingCassState()
        {
            Session.Remove("PendingOriginalEntry");
            Session.Remove("PendingStandardizedEntry");
            ConfirmedStandardized.Value = "false";
            UseOriginalAddress.Value = "false";
            ModalScriptLiteral.Text = string.Empty;
        }

        private void ResetEditor()
        {
            EditingEntryId.Value = string.Empty;
            ValidateAddressButton.Text = "Validate + Save";
            CancelEditButton.Visible = false;
            ClearForm();
        }

        private bool TryBuildEntry(out CustomerEntryInput entry, out string validationError)
        {
            validationError = string.Empty;
            entry = null;

            if (!int.TryParse(ClientDropDown.SelectedValue, out var clientId) || clientId <= 0)
            {
                validationError = "Please select a valid client.";
                return false;
            }

            entry = new CustomerEntryInput
            {
                ClientId = clientId,
                CustomerNumber = CustomerNumberTextBox.Text.Trim(),
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                Address1 = Address1TextBox.Text.Trim(),
                Address2 = Address2TextBox.Text.Trim(),
                City = CityTextBox.Text.Trim(),
                State = StateTextBox.Text.Trim().ToUpperInvariant(),
                Zip = string.Empty,
                Zip4 = string.Empty,
                Email = EmailTextBox.Text.Trim()
            };

            ParseZipInput(ZipTextBox.Text, out var zip5, out var zip4);
            entry.Zip = zip5;
            entry.Zip4 = zip4;

            if (string.IsNullOrWhiteSpace(entry.Address1)) validationError = "Address 1 is required.";
            else if (string.IsNullOrWhiteSpace(entry.City)) validationError = "City is required.";
            else if (string.IsNullOrWhiteSpace(entry.State) || entry.State.Length != 2) validationError = "State must be two letters.";

            return string.IsNullOrEmpty(validationError);
        }

        private static void ParseZipInput(string zipInput, out string zip5, out string zip4)
        {
            var raw = (zipInput ?? string.Empty).Trim();
            var digits = new System.Text.StringBuilder(raw.Length);
            foreach (var c in raw)
            {
                if (char.IsDigit(c)) digits.Append(c);
            }

            var value = digits.ToString();
            zip5 = value.Length >= 5 ? value.Substring(0, 5) : value;
            zip4 = value.Length >= 9 ? value.Substring(5, 4) : string.Empty;
        }

        private void ClearForm()
        {
            CustomerNumberTextBox.Text = string.Empty;
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            Address1TextBox.Text = string.Empty;
            Address2TextBox.Text = string.Empty;
            CityTextBox.Text = string.Empty;
            StateTextBox.Text = string.Empty;
            ZipTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
        }
    }
}
