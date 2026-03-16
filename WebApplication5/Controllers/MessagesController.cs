using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication5.Filters;
using WebApplication5.Models.DTOs;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [RoutePrefix("api/messages")]
    [SessionAuth]
    public class MessagesController : ApiController
    {
        private readonly MessageService _messageService = new MessageService();

        [HttpGet]
        [Route("{friendId:int}")]
        public async Task<IHttpActionResult> GetHistory(int friendId, [FromUri] int page = 1, [FromUri] int pageSize = 50)
        {
            var userId = (int)HttpContext.Current.Session["UserId"];
            var messages = await _messageService.GetHistory(userId, friendId, page, pageSize);
            return Ok(ApiResponse<object>.Ok(messages));
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = (int)HttpContext.Current.Session["UserId"];
            var result = await _messageService.SaveMessage(userId, dto);

            if (!result.Success)
                return Content(System.Net.HttpStatusCode.BadRequest, result);

            return Ok(result);
        }
    }
}
