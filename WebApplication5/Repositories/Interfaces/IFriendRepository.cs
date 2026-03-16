using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models.Entities;

namespace WebApplication5.Repositories.Interfaces
{
    public interface IFriendRepository
    {
        Task<bool> AreFriends(int userId1, int userId2);
        Task<List<User>> GetFriends(int userId);
        Task CreateFriendship(int userId1, int userId2);
    }
}
