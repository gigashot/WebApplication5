using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Repositories;
using WebApplication5.Repositories.Interfaces;
using WebApplication5.Services.Interfaces;

namespace WebApplication5.Services
{
    public class KeyService : IKeyService
    {
        private readonly EncryptAppDbContext _context;
        private readonly IUserRepository _userRepository;

        public KeyService()
        {
            _context = new EncryptAppDbContext();
            _userRepository = new UserRepository(_context);
        }

        public async Task<string> GetPublicKey(int userId)
        {
            return await _userRepository.GetPublicKey(userId);
        }
    }
}
