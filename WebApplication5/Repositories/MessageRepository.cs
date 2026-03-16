using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Models.Entities;
using WebApplication5.Repositories.Interfaces;

namespace WebApplication5.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly EncryptAppDbContext _context;

        public MessageRepository(EncryptAppDbContext context)
        {
            _context = context;
        }

        public async Task<Message> Create(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetConversation(int user1, int user2, int skip, int take)
        {
            return await _context.Messages
                .Where(m =>
                    (m.SenderId == user1 && m.ReceiverId == user2) ||
                    (m.SenderId == user2 && m.ReceiverId == user1))
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task MarkDelivered(long messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.Delivered = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkRead(long messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.Read = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Message>> GetUndelivered(int userId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.Delivered)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}
