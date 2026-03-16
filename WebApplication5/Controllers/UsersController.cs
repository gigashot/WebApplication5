using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication5.Filters;
using WebApplication5.Models.DTOs;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [RoutePrefix("api/users")]
    [SessionAuth]
    public class UsersController : ApiController
    {
        private readonly UserService _userService = new UserService();

        [HttpGet]
        [Route("search")]
        public async Task<IHttpActionResult> Search([FromUri] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(ApiResponse<object>.Ok(new object[0]));

            var userId = (int)HttpContext.Current.Session["UserId"];
            var results = await _userService.Search(q, userId);
            return Ok(ApiResponse<object>.Ok(results));
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetUser(int id)
        {
            var profile = await _userService.GetProfile(id);
            if (profile == null)
                return NotFound();

            return Ok(ApiResponse<LoginResponseDto>.Ok(profile));
        }

        [HttpGet]
        [Route("me")]
        public async Task<IHttpActionResult> GetMe()
        {
            var userId = (int)HttpContext.Current.Session["UserId"];
            var profile = await _userService.GetProfile(userId);
            return Ok(ApiResponse<LoginResponseDto>.Ok(profile));
        }
    }
}
