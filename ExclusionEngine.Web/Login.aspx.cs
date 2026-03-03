using System;

namespace ExclusionEngine.Web
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            var user = Repository.ValidateUser(UsernameTextBox.Text.Trim(), PasswordTextBox.Text);
            if (user == null)
            {
                ErrorMessage.Text = "Invalid credentials.";
                return;
            }

            Session["UserId"] = user.UserId;
            Session["Username"] = user.Username;
            Response.Redirect("~/Default.aspx");
        }
    }
}
