using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models.Entities;

namespace WebApplication5.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetById(int userId);
        Task<User> GetByUsername(string username);
        Task<bool> UsernameExists(string username);
        Task<User> Create(User user);
        Task UpdateLastLogin(int userId);
        Task<List<User>> SearchByUsername(string query, int excludeUserId);
        Task<string> GetPublicKey(int userId);
    }
}
