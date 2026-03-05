using System;
using System.Configuration;
using System.IO;

namespace ExclusionEngine.Web
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] != null)
            {
                LoggedInUserLabel.Text = "Signed in: " + Session["Username"];
            }
            else
            {
                LoggedInUserLabel.Text = string.Empty;
            }

            var logoUrl = ConfigurationManager.AppSettings["CompanyLogoUrl"];
            if (string.IsNullOrWhiteSpace(logoUrl))
            {
                logoUrl = "~/Images/CompuTechDirectLogo.svg";
            }

            CompanyLogoImage.ImageUrl = logoUrl;

            if (logoUrl.StartsWith("~/", StringComparison.Ordinal))
            {
                var physical = Server.MapPath(logoUrl);
                CompanyLogoImage.Visible = File.Exists(physical);
            }
            else
            {
                CompanyLogoImage.Visible = true;
            }
        }
    }
}
