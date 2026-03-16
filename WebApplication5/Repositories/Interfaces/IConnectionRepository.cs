using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models.Entities;

namespace WebApplication5.Repositories.Interfaces
{
    public interface IConnectionRepository
    {
        Task Add(string connectionId, int userId);
        Task Remove(string connectionId);
        Task<List<Connection>> GetByUserId(int userId);
        Task<bool> IsUserOnline(int userId);
        Task<List<int>> GetOnlineUserIds(List<int> userIds);
    }
}
