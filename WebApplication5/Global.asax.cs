using System.Data.Entity;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;
using WebApplication5.Models;

namespace WebApplication5
{
    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<EncryptAppDbContext, Migrations.Configuration>());
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_PostAuthorizeRequest()
        {
            if (HttpContext.Current.Request.Url.AbsolutePath.StartsWith("/api"))
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }
    }
}
