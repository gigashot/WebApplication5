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
    public class FriendRepository : IFriendRepository
    {
        private readonly EncryptAppDbContext _context;

        public FriendRepository(EncryptAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AreFriends(int userId1, int userId2)
        {
            return await _context.Friends
                .AnyAsync(f =>
                    (f.UserId1 == userId1 && f.UserId2 == userId2) ||
                    (f.UserId1 == userId2 && f.UserId2 == userId1));
        }

        public async Task<List<User>> GetFriends(int userId)
        {
            var friends = await _context.Friends
                .Include(f => f.User1)
                .Include(f => f.User2)
                .Where(f => f.UserId1 == userId || f.UserId2 == userId)
                .ToListAsync();

            return friends
                .Select(f => f.UserId1 == userId ? f.User2 : f.User1)
                .ToList();
        }

        public async Task CreateFriendship(int userId1, int userId2)
        {
            var friendship = new Friend
            {
                UserId1 = userId1,
                UserId2 = userId2,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friends.Add(friendship);
            await _context.SaveChangesAsync();
        }
    }
}
