using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Models.DTOs;
using WebApplication5.Models.Entities;
using WebApplication5.Repositories;
using WebApplication5.Repositories.Interfaces;
using WebApplication5.Services.Interfaces;

namespace WebApplication5.Services
{
    public class MessageService : IMessageService
    {
        private readonly EncryptAppDbContext _context;
        private readonly IMessageRepository _messageRepository;
        private readonly IFriendRepository _friendRepository;

        public MessageService()
        {
            _context = new EncryptAppDbContext();
            _messageRepository = new MessageRepository(_context);
            _friendRepository = new FriendRepository(_context);
        }

        public async Task<ApiResponse<MessageDto>> SaveMessage(int senderId, SendMessageDto dto)
        {
            if (!await _friendRepository.AreFriends(senderId, dto.ReceiverId))
            {
                return new ApiResponse<MessageDto>
                {
                    Success = false,
                    Message = "You can only send messages to friends."
                };
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                EncryptedContent = dto.EncryptedContent,
                SenderEncryptedContent = dto.SenderEncryptedContent,
                SentAt = DateTime.UtcNow,
                Delivered = false,
                Read = false
            };

            var created = await _messageRepository.Create(message);

            var messageDto = new MessageDto
            {
                MessageId = created.MessageId,
                SenderId = created.SenderId,
                ReceiverId = created.ReceiverId,
                EncryptedContent = created.EncryptedContent,
                SentAt = created.SentAt,
                Delivered = created.Delivered,
                Read = created.Read
            };

            return ApiResponse<MessageDto>.Ok(messageDto, "Message sent.");
        }

        public async Task<List<MessageDto>> GetHistory(int userId, int friendId, int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;
            var messages = await _messageRepository.GetConversation(userId, friendId, skip, pageSize);

            return messages.Select(m => new MessageDto
            {
                MessageId = m.MessageId,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                // Return the copy encrypted for the requesting user
                EncryptedContent = m.SenderId == userId
                    ? (m.SenderEncryptedContent ?? m.EncryptedContent)
                    : m.EncryptedContent,
                SentAt = m.SentAt,
                Delivered = m.Delivered,
                Read = m.Read
            }).ToList();
        }

        public async Task MarkDelivered(long messageId, int userId)
        {
            await _messageRepository.MarkDelivered(messageId);
        }

        public async Task MarkRead(long messageId, int userId)
        {
            await _messageRepository.MarkRead(messageId);
        }
    }
}
