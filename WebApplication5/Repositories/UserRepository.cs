using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Models.Entities;
using WebApplication5.Repositories.Interfaces;

namespace WebApplication5.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EncryptAppDbContext _context;

        public UserRepository(EncryptAppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetById(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User> GetByUsername(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UsernameExists(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<User> Create(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateLastLogin(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> SearchByUsername(string query, int excludeUserId)
        {
            return await _context.Users
                .Where(u => u.Username.Contains(query) && u.UserId != excludeUserId)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<string> GetPublicKey(int userId)
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.PublicKey)
                .FirstOrDefaultAsync();
            return user;
        }
    }
}
