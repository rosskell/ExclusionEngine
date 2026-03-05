using System;
using System.Configuration;
using System.Web;

namespace ExclusionEngine.Web
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected void RequestResetButton_Click(object sender, EventArgs e)
        {
            var email = EmailTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                ForgotPasswordMessage.Text = "<span class='error'>Email is required.</span>";
                return;
            }

            var token = Repository.CreatePasswordResetToken(email);
            if (string.IsNullOrWhiteSpace(token))
            {
                ForgotPasswordMessage.Text = "<span class='success'>If that email exists, a reset link has been generated.</span>";
                return;
            }

            var baseUrl = ConfigurationManager.AppSettings["AppBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/").TrimStart('/');
                if (!baseUrl.EndsWith("/")) baseUrl += "/";
            }

            var link = baseUrl + "ResetPassword.aspx?token=" + HttpUtility.UrlEncode(token);

            try
            {
                EmailService.SendPasswordResetEmail(email, link);
                ForgotPasswordMessage.Text = "<span class='success'>If that email exists, a reset link has been sent.</span>";
            }
            catch
            {
                ForgotPasswordMessage.Text = $"<span class='warn'>Reset link generated (email not sent in this environment): {HttpUtility.HtmlEncode(link)}</span>";
            }
        }
    }
}
