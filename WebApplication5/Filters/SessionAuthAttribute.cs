using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApplication5.Models.DTOs;

namespace WebApplication5.Filters
{
    public class SessionAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var session = HttpContext.Current?.Session;
            if (session == null || session["UserId"] == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    ApiResponse.Error("Not authenticated"));
                return;
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
