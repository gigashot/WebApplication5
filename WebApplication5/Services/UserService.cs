using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Models.DTOs;
using WebApplication5.Repositories;
using WebApplication5.Repositories.Interfaces;
using WebApplication5.Services.Interfaces;

namespace WebApplication5.Services
{
    public class UserService : IUserService
    {
        private readonly EncryptAppDbContext _context;
        private readonly IUserRepository _userRepository;

        public UserService()
        {
            _context = new EncryptAppDbContext();
            _userRepository = new UserRepository(_context);
        }

        public async Task<List<UserSearchResultDto>> Search(string query, int excludeUserId)
        {
            var users = await _userRepository.SearchByUsername(query, excludeUserId);

            return users.Select(u => new UserSearchResultDto
            {
                UserId = u.UserId,
                Username = u.Username
            }).ToList();
        }

        public async Task<LoginResponseDto> GetProfile(int userId)
        {
            var user = await _userRepository.GetById(userId);

            if (user == null)
                return null;

            return new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                PublicKey = user.PublicKey
            };
        }
    }
}
