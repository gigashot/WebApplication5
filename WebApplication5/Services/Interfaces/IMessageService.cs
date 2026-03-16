using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models.DTOs;

namespace WebApplication5.Services.Interfaces
{
    public interface IMessageService
    {
        Task<ApiResponse<MessageDto>> SaveMessage(int senderId, SendMessageDto dto);
        Task<List<MessageDto>> GetHistory(int userId, int friendId, int page, int pageSize);
        Task MarkDelivered(long messageId, int userId);
        Task MarkRead(long messageId, int userId);
    }
}
