using System;
using System.Configuration;
using System.Net.Mail;

namespace ExclusionEngine.Web
{
    public static class EmailService
    {
        public static void SendPasswordResetEmail(string toEmail, string resetLink)
        {
            var from = ConfigurationManager.AppSettings["FromEmail"];
            if (string.IsNullOrWhiteSpace(from))
            {
                from = "no-reply@exclusionengine.local";
            }

            using (var msg = new MailMessage(from, toEmail))
            {
                msg.Subject = "Exclusion Engine Password Reset";
                msg.Body = "Use the link below to reset your password:\n\n" + resetLink;
                using (var client = new SmtpClient())
                {
                    client.Send(msg);
                }
            }
        }
    }
}
