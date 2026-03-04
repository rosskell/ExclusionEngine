using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace ExclusionEngine.Web
{
    public partial class AdminUsers : System.Web.UI.Page
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
                BindUsers();
            }
        }

        private void BindClients()
        {
            ClientCheckBoxList.DataSource = Repository.GetAllClients();
            ClientCheckBoxList.DataTextField = "ClientDisplay";
            ClientCheckBoxList.DataValueField = "ClientId";
            ClientCheckBoxList.DataBind();
        }

        private void BindUsers()
        {
            UsersGrid.DataSource = Repository.GetAllUsersForAdmin();
            UsersGrid.DataBind();
        }

        protected void SaveUserButton_Click(object sender, EventArgs e)
        {
            var username = AdminUsernameTextBox.Text.Trim();
            var email = AdminEmailTextBox.Text.Trim();
            var password = AdminPasswordTextBox.Text;
            var clientIds = ClientCheckBoxList.Items.Cast<ListItem>()
                .Where(i => i.Selected)
                .Select(i => int.Parse(i.Value))
                .ToList();

            if (string.IsNullOrWhiteSpace(username))
            {
                AdminMessageLabel.Text = "<span class='error'>Username is required.</span>";
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                AdminMessageLabel.Text = "<span class='error'>Email is required.</span>";
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(EditingUserId.Value))
                {
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        AdminMessageLabel.Text = "<span class='error'>Password is required for new users.</span>";
                        return;
                    }

                    Repository.CreateUser(new UserAdminModel
                    {
                        Username = username,
                        Email = email,
                        IsAdmin = IsAdminCheckBox.Checked,
                        ClientIds = clientIds
                    }, password);

                    AdminMessageLabel.Text = "<span class='success'>User created.</span>";
                }
                else
                {
                    Repository.UpdateUser(new UserAdminModel
                    {
                        UserId = int.Parse(EditingUserId.Value),
                        Username = username,
                        Email = email,
                        IsAdmin = IsAdminCheckBox.Checked,
                        ClientIds = clientIds
                    }, password);

                    AdminMessageLabel.Text = "<span class='success'>User updated.</span>";
                }

                ResetEditor();
                BindUsers();
            }
            catch (Exception ex)
            {
                AdminMessageLabel.Text = $"<span class='error'>{HttpUtility.HtmlEncode(ex.Message)}</span>";
            }
        }

        protected void UsersGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditUser") return;

            var rowIndex = Convert.ToInt32(e.CommandArgument);
            var userId = Convert.ToInt32(UsersGrid.DataKeys[rowIndex].Value);
            var user = Repository.GetUserForAdmin(userId);
            if (user == null)
            {
                AdminMessageLabel.Text = "<span class='error'>User not found.</span>";
                return;
            }

            EditingUserId.Value = user.UserId.ToString();
            AdminUsernameTextBox.Text = user.Username;
            AdminEmailTextBox.Text = user.Email;
            AdminPasswordTextBox.Text = string.Empty;
            IsAdminCheckBox.Checked = user.IsAdmin;

            foreach (ListItem item in ClientCheckBoxList.Items)
            {
                item.Selected = user.ClientIds.Contains(int.Parse(item.Value));
            }

            SaveUserButton.Text = "Update User";
        }

        protected void CancelUserEditButton_Click(object sender, EventArgs e)
        {
            ResetEditor();
            AdminMessageLabel.Text = "<span class='success'>Edit canceled.</span>";
        }

        private void ResetEditor()
        {
            EditingUserId.Value = string.Empty;
            AdminUsernameTextBox.Text = string.Empty;
            AdminEmailTextBox.Text = string.Empty;
            AdminPasswordTextBox.Text = string.Empty;
            IsAdminCheckBox.Checked = false;
            foreach (ListItem item in ClientCheckBoxList.Items)
            {
                item.Selected = false;
            }
            SaveUserButton.Text = "Save User";
        }
    }
}
