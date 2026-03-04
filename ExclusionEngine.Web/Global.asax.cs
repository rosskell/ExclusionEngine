using System;
using System.Web;

namespace ExclusionEngine.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Repository.EnsureSchema();
            Repository.EnsureSeedData();
        }
    }
}
