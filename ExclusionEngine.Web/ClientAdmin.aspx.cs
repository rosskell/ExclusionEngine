using System;
using System.Web;
using System.Web.UI.WebControls;

namespace ExclusionEngine.Web
{
    public partial class ClientAdmin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!(Session["IsAdmin"] is bool isAdmin) || !isAdmin)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindClients();
            }
        }

        protected void BackButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        private void BindClients()
        {
            int? activeFilter = null;
            if (int.TryParse(ClientStatusFilterDropDown.SelectedValue, out int parsedActive))
                activeFilter = parsedActive;

            ClientsGrid.DataSource = Repository.GetAllClients(
                includeInactive: true,
                clientCodeFilter: SearchClientCodeTextBox.Text.Trim(),
                clientNameFilter: SearchClientNameTextBox.Text.Trim(),
                activeFilter: activeFilter);
            ClientsGrid.DataBind();
        }

        protected void SearchClientsButton_Click(object sender, EventArgs e)
        {
            BindClients();
        }

        protected void ClearClientSearchButton_Click(object sender, EventArgs e)
        {
            SearchClientCodeTextBox.Text = string.Empty;
            SearchClientNameTextBox.Text = string.Empty;
            ClientStatusFilterDropDown.SelectedValue = string.Empty;
            BindClients();
        }

        protected void SaveClientButton_Click(object sender, EventArgs e)
        {
            var code = ClientCodeTextBox.Text.Trim();
            var name = ClientNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                ClientMessageLabel.Text = "<span class='error'>Client code and name are required.</span>";
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(EditingClientId.Value))
                {
                    Repository.CreateClient(new ClientModel { ClientCode = code, ClientName = name, IsActive = IsActiveCheckBox.Checked });
                    ClientMessageLabel.Text = "<span class='success'>Client created.</span>";
                }
                else
                {
                    Repository.UpdateClient(new ClientModel
                    {
                        ClientId = int.Parse(EditingClientId.Value),
                        ClientCode = code,
                        ClientName = name,
                        IsActive = IsActiveCheckBox.Checked
                    });
                    ClientMessageLabel.Text = "<span class='success'>Client updated.</span>";
                }

                ResetEditor();
                BindClients();
            }
            catch (Exception ex)
            {
                ClientMessageLabel.Text = ErrorHandling.ToUserMessage(ex);
            }
        }

        protected void ClientsGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var rowIndex = Convert.ToInt32(e.CommandArgument);
            var clientId = Convert.ToInt32(ClientsGrid.DataKeys[rowIndex].Value);

            if (e.CommandName == "EditClient")
            {
                var client = Repository.GetClientById(clientId);
                if (client == null)
                {
                    ClientMessageLabel.Text = "<span class='error'>Client not found.</span>";
                    return;
                }

                EditingClientId.Value = client.ClientId.ToString();
                ClientCodeTextBox.Text = client.ClientCode;
                ClientNameTextBox.Text = client.ClientName;
                IsActiveCheckBox.Checked = client.IsActive;
                SaveClientButton.Text = "Update Client";
                return;
            }

            if (e.CommandName == "DeleteClient")
            {
                try
                {
                    Repository.DeleteClient(clientId);
                    BindClients();
                    ClientMessageLabel.Text = "<span class='success'>Client deactivated. Existing customer records were kept.</span>";
                }
                catch (Exception ex)
                {
                    ClientMessageLabel.Text = ErrorHandling.ToUserMessage(ex);
                }
            }
        }

        protected void CancelClientEditButton_Click(object sender, EventArgs e)
        {
            ResetEditor();
            ClientMessageLabel.Text = "<span class='success'>Edit canceled.</span>";
        }

        private void ResetEditor()
        {
            EditingClientId.Value = string.Empty;
            ClientCodeTextBox.Text = string.Empty;
            ClientNameTextBox.Text = string.Empty;
            SaveClientButton.Text = "Save Client";
            IsActiveCheckBox.Checked = true;
        }
    }
}
