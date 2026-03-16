using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models.Entities;

namespace WebApplication5.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message> Create(Message message);
        Task<List<Message>> GetConversation(int user1, int user2, int skip, int take);
        Task MarkDelivered(long messageId);
        Task MarkRead(long messageId);
        Task<List<Message>> GetUndelivered(int userId);
    }
}
