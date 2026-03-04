using System;
using System.Web;
using System.Web.UI.WebControls;

namespace ExclusionEngine.Web
{
    public partial class _Default : System.Web.UI.Page
    {
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
                BindRecent();
            }
        }

        private int UserId => Convert.ToInt32(Session["UserId"]);

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
            RecentGrid.DataSource = Repository.GetRecentEntriesForUser(UserId);
            RecentGrid.DataBind();
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
            if (cass.HasChanges)
            {
                var entered = HttpUtility.JavaScriptStringEncode(entry.FormattedAddress);
                var standardized = HttpUtility.JavaScriptStringEncode(cass.Standardized.FormattedAddress);
                ModalScriptLiteral.Text = $"<script>showCassModal('{entered}','{standardized}');</script>";
                MessageLabel.Text = "<span class='warn'>Review the standardized address and save your selection.</span>";
                Session["PendingOriginalEntry"] = entry;
                Session["PendingStandardizedEntry"] = cass.Standardized;
                return;
            }

            SaveEntry(entry);
        }

        protected void RecentGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditEntry") return;

            var rowIndex = Convert.ToInt32(e.CommandArgument);
            var entryId = Convert.ToInt32(RecentGrid.DataKeys[rowIndex].Value);
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
            ZipTextBox.Text = entry.Zip;
            EmailTextBox.Text = entry.Email;

            ValidateAddressButton.Text = "Validate + Update";
            CancelEditButton.Visible = true;
            MessageLabel.Text = "<span class='warn'>Editing existing record. Save to update.</span>";
        }

        protected void CancelEditButton_Click(object sender, EventArgs e)
        {
            ResetEditor();
            MessageLabel.Text = "<span class='success'>Edit canceled.</span>";
        }

        private void SaveEntry(CustomerEntryInput entry)
        {
            if (IsEditing)
            {
                Repository.UpdateEntry(UserId, Convert.ToInt32(EditingEntryId.Value), entry);
                MessageLabel.Text = "<span class='success'>Customer entry updated.</span>";
            }
            else
            {
                Repository.SaveEntry(UserId, entry);
                MessageLabel.Text = "<span class='success'>Customer entry saved.</span>";
            }

            Session.Remove("PendingOriginalEntry");
            Session.Remove("PendingStandardizedEntry");
            ConfirmedStandardized.Value = "false";
            UseOriginalAddress.Value = "false";
            ResetEditor();
            BindRecent();
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
                Zip = ZipTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim()
            };

            if (string.IsNullOrWhiteSpace(entry.CustomerNumber)) validationError = "Customer Number is required.";
            else if (string.IsNullOrWhiteSpace(entry.FirstName)) validationError = "First Name is required.";
            else if (string.IsNullOrWhiteSpace(entry.LastName)) validationError = "Last Name is required.";
            else if (string.IsNullOrWhiteSpace(entry.Address1)) validationError = "Address 1 is required.";
            else if (string.IsNullOrWhiteSpace(entry.City)) validationError = "City is required.";
            else if (string.IsNullOrWhiteSpace(entry.State) || entry.State.Length != 2) validationError = "State must be two letters.";
            else if (string.IsNullOrWhiteSpace(entry.Zip)) validationError = "Zip is required.";

            return string.IsNullOrEmpty(validationError);
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
