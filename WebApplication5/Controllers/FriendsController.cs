using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication5.Filters;
using WebApplication5.Models.DTOs;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [RoutePrefix("api/friends")]
    [SessionAuth]
    public class FriendsController : ApiController
    {
        private readonly FriendService _friendService = new FriendService();

        [HttpPost]
        [Route("request")]
        public async Task<IHttpActionResult> SendRequest([FromBody] SendFriendRequestDto dto)
        {
            var userId = (int)HttpContext.Current.Session["UserId"];
            var result = await _friendService.SendRequest(userId, dto.ReceiverId);

            if (!result.Success)
                return Content(System.Net.HttpStatusCode.BadRequest, result);

            return Ok(result);
        }

        [HttpGet]
        [Route("requests")]
        public async Task<IHttpActionResult> GetPendingRequests()
        {
            var userId = (int)HttpContext.Current.Session["UserId"];
            var requests = await _friendService.GetPendingRequests(userId);
            return Ok(ApiResponse<object>.Ok(requests));
        }

        [HttpPost]
        [Route("requests/{id:int}/accept")]
        public async Task<IHttpActionResult> AcceptRequest(int id)
        {
            var userId = (int)HttpContext.Current.Session["UserId"];
            var result = await _friendService.AcceptRequest(userId, id);

            if (!result.Success)
                return Content(System.Net.HttpStatusCode.BadRequest, result);

            return Ok(result);
        }

        [HttpPost]
        [Route("requests/{id:int}/reject")]
        public async Task<IHttpActionResult> RejectRequest(int id)
        {
            var userId = (int)HttpContext.Current.Session["UserId"];
            var result = await _friendService.RejectRequest(userId, id);

            if (!result.Success)
                return Content(System.Net.HttpStatusCode.BadRequest, result);

            return Ok(result);
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetFriends()
        {
            var userId = (int)HttpContext.Current.Session["UserId"];
            var friends = await _friendService.GetFriends(userId);
            return Ok(ApiResponse<object>.Ok(friends));
        }
    }
}
