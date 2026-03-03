using System;
using System.Linq;
using System.Web;

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
            var entry = BuildEntry();
            var cass = SatoriCassService.StandardizeAddress(entry);

            if (cass.HasChanges && ConfirmedStandardized.Value != "true")
            {
                var entered = HttpUtility.JavaScriptStringEncode(entry.FormattedAddress);
                var standardized = HttpUtility.JavaScriptStringEncode(cass.Standardized.FormattedAddress);
                ModalScriptLiteral.Text = $"<script>showCassModal('{entered}','{standardized}');</script>";
                MessageLabel.Text = "<span class='warn'>Review the standardized address and accept to save.</span>";
                Session["PendingEntry"] = cass.Standardized;
                return;
            }

            var toSave = Session["PendingEntry"] as CustomerEntryInput ?? entry;
            Repository.SaveEntry(UserId, toSave);
            Session.Remove("PendingEntry");
            ConfirmedStandardized.Value = "false";
            MessageLabel.Text = "<span class='success'>Customer entry saved.</span>";
            ClearForm();
            BindRecent();
        }

        private CustomerEntryInput BuildEntry()
        {
            return new CustomerEntryInput
            {
                ClientId = int.Parse(ClientDropDown.SelectedValue),
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
        }

        private void ClearForm()
        {
            CustomerNumberTextBox.Text = "";
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            Address1TextBox.Text = "";
            Address2TextBox.Text = "";
            CityTextBox.Text = "";
            StateTextBox.Text = "";
            ZipTextBox.Text = "";
            EmailTextBox.Text = "";
        }
    }
}
