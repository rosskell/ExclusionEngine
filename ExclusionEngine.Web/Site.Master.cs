using System;

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
        }
    }
}
