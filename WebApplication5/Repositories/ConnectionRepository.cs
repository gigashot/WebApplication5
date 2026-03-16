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
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly EncryptAppDbContext _context;

        public ConnectionRepository(EncryptAppDbContext context)
        {
            _context = context;
        }

        public async Task Add(string connectionId, int userId)
        {
            var connection = new Connection
            {
                ConnectionId = connectionId,
                UserId = userId,
                ConnectedAt = DateTime.UtcNow
            };

            _context.Connections.Add(connection);
            await _context.SaveChangesAsync();
        }

        public async Task Remove(string connectionId)
        {
            var connection = await _context.Connections.FindAsync(connectionId);
            if (connection != null)
            {
                _context.Connections.Remove(connection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Connection>> GetByUserId(int userId)
        {
            return await _context.Connections
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> IsUserOnline(int userId)
        {
            return await _context.Connections
                .AnyAsync(c => c.UserId == userId);
        }

        public async Task<List<int>> GetOnlineUserIds(List<int> userIds)
        {
            return await _context.Connections
                .Where(c => userIds.Contains(c.UserId))
                .Select(c => c.UserId)
                .Distinct()
                .ToListAsync();
        }
    }
}
