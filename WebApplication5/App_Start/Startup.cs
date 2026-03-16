using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebApplication5.App_Start.Startup))]

namespace WebApplication5.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
