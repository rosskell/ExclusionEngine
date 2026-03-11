using System;
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
                SyncClientSelectionEnabled();
            }
        }

        protected void BackToDefaultButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void IsAdminCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SyncClientSelectionEnabled();
        }

        private void SyncClientSelectionEnabled()
        {
            var admin = IsAdminCheckBox.Checked;
            ClientCheckBoxList.Enabled = !admin;
            if (admin)
            {
                foreach (ListItem item in ClientCheckBoxList.Items)
                {
                    item.Selected = false;
                }
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
            var companyName = AdminCompanyNameTextBox.Text.Trim();
            var password = AdminPasswordTextBox.Text;
            var clientIds = IsAdminCheckBox.Checked
                ? new System.Collections.Generic.List<int>()
                : ClientCheckBoxList.Items.Cast<ListItem>()
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
                        CompanyName = companyName,
                        IsAdmin = IsAdminCheckBox.Checked,
                        IsDisabled = IsDisabledCheckBox.Checked,
                        ClientIds = clientIds
                    }, password);

                    AdminMessageLabel.Text = "<span class='success'>User created.</span>";
                }
                else
                {
                    var editUserId = int.Parse(EditingUserId.Value);
                    if (Session["UserId"] != null && editUserId == Convert.ToInt32(Session["UserId"]))
                    {
                        IsDisabledCheckBox.Checked = false;
                    }

                    Repository.UpdateUser(new UserAdminModel
                    {
                        UserId = editUserId,
                        Username = username,
                        Email = email,
                        CompanyName = companyName,
                        IsAdmin = IsAdminCheckBox.Checked,
                        IsDisabled = IsDisabledCheckBox.Checked,
                        ClientIds = clientIds
                    }, password);

                    AdminMessageLabel.Text = "<span class='success'>User updated.</span>";
                }

                ResetEditor();
                BindUsers();
            }
            catch (Exception ex)
            {
                AdminMessageLabel.Text = ErrorHandling.ToUserMessage(ex);
            }
        }

        protected void UsersGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var rowIndex = Convert.ToInt32(e.CommandArgument);
            var userId = Convert.ToInt32(UsersGrid.DataKeys[rowIndex].Value);

            if (e.CommandName == "EditUser")
            {
                var user = Repository.GetUserForAdmin(userId);
                if (user == null)
                {
                    AdminMessageLabel.Text = "<span class='error'>User not found.</span>";
                    return;
                }

                EditingUserId.Value = user.UserId.ToString();
                AdminUsernameTextBox.Text = user.Username;
                AdminEmailTextBox.Text = user.Email;
                AdminCompanyNameTextBox.Text = user.CompanyName;
                AdminPasswordTextBox.Text = string.Empty;
                IsAdminCheckBox.Checked = user.IsAdmin;
                IsDisabledCheckBox.Checked = user.IsDisabled;

                foreach (ListItem item in ClientCheckBoxList.Items)
                {
                    item.Selected = user.ClientIds.Contains(int.Parse(item.Value));
                }

                SyncClientSelectionEnabled();
                SaveUserButton.Text = "Update User";
                return;
            }

            if (e.CommandName == "ToggleDisable")
            {
                var user = Repository.GetUserForAdmin(userId);
                if (user == null)
                {
                    AdminMessageLabel.Text = "<span class='error'>User not found.</span>";
                    return;
                }

                if (Session["UserId"] != null && userId == Convert.ToInt32(Session["UserId"]))
                {
                    AdminMessageLabel.Text = "<span class='error'>You cannot disable your own account.</span>";
                    return;
                }

                Repository.DisableUser(userId, !user.IsDisabled);
                BindUsers();
                AdminMessageLabel.Text = user.IsDisabled
                    ? "<span class='success'>User enabled.</span>"
                    : "<span class='success'>User disabled.</span>";
                return;
            }

            if (e.CommandName == "DeleteUser")
            {
                if (Session["UserId"] != null && userId == Convert.ToInt32(Session["UserId"]))
                {
                    AdminMessageLabel.Text = "<span class='error'>You cannot delete your own account.</span>";
                    return;
                }

                try
                {
                    Repository.DeleteUser(userId);
                    BindUsers();
                    AdminMessageLabel.Text = "<span class='success'>User deleted.</span>";
                }
                catch (Exception ex)
                {
                    AdminMessageLabel.Text = ErrorHandling.ToUserMessage(ex);
                }
            }
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
            AdminCompanyNameTextBox.Text = string.Empty;
            AdminPasswordTextBox.Text = string.Empty;
            IsAdminCheckBox.Checked = false;
            IsDisabledCheckBox.Checked = false;
            foreach (ListItem item in ClientCheckBoxList.Items)
            {
                item.Selected = false;
            }
            SyncClientSelectionEnabled();
            SaveUserButton.Text = "Save User";
        }
    }
}
