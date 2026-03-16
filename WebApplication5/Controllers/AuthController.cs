using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication5.Models.DTOs;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly AuthService _authService = new AuthService();

        [HttpPost]
        [Route("register")]
        public async Task<IHttpActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Register(dto);
            if (!result.Success)
                return Content(System.Net.HttpStatusCode.Conflict, result);

            return Ok(result);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Login(dto);
            if (!result.Success)
                return Content(System.Net.HttpStatusCode.Unauthorized, result);

            var session = HttpContext.Current.Session;
            session["UserId"] = result.Data.UserId;
            session["Username"] = result.Data.Username;

            return Ok(result);
        }

        [HttpPost]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            HttpContext.Current?.Session?.Clear();
            return Ok(ApiResponse.Ok("Logged out"));
        }
    }
}
