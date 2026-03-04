using System;

namespace ExclusionEngine.Web
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        protected void ResetPasswordButton_Click(object sender, EventArgs e)
        {
            var token = Request.QueryString["token"];
            var newPassword = NewPasswordTextBox.Text;

            if (string.IsNullOrWhiteSpace(token))
            {
                ResetMessage.Text = "<span class='error'>Invalid reset token.</span>";
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                ResetMessage.Text = "<span class='error'>Password must be at least 8 characters.</span>";
                return;
            }

            var ok = Repository.ResetPasswordWithToken(token, newPassword);
            ResetMessage.Text = ok
                ? "<span class='success'>Password reset complete. You can now sign in.</span>"
                : "<span class='error'>Token expired or invalid.</span>";
        }
    }
}
