using System.Web;
using System.Web.Http;
using System.Web.SessionState;

namespace WebApplication5
{
    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
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
