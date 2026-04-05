using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WebApplication5.Models;
using WebApplication5.Models.DTOs;
using WebApplication5.Models.Entities;
using WebApplication5.Repositories;
using WebApplication5.Services;

namespace WebApplication5.Hubs
{
    public class ChatHub : Hub
    {
        // Fast in-memory lookup: userId -> connectionId
        private static readonly ConcurrentDictionary<int, string> OnlineUsers
            = new ConcurrentDictionary<int, string>();

        public async Task Register(int userId)
        {
            var connectionId = Context.ConnectionId;
            OnlineUsers[userId] = connectionId;

            using (var db = new EncryptAppDbContext())
            {
                // Remove any stale connections for this user
                var stale = await db.Connections.Where(c => c.UserId == userId).ToListAsync();
                db.Connections.RemoveRange(stale);

                db.Connections.Add(new Connection
                {
                    ConnectionId = connectionId,
                    UserId = userId,
                    ConnectedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                // Notify friends that this user is online
                var friendRepo = new FriendRepository(db);
                var friends = await friendRepo.GetFriends(userId);
                foreach (var friend in friends)
                {
                    string friendConnId;
                    if (OnlineUsers.TryGetValue(friend.UserId, out friendConnId))
                    {
                        Clients.Client(friendConnId).userOnline(userId);
                    }
                }

                // Deliver undelivered messages
                var undelivered = await db.Messages
                    .Where(m => m.ReceiverId == userId && !m.Delivered)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                foreach (var msg in undelivered)
                {
                    Clients.Client(connectionId).receiveMessage(new MessageDto
                    {
                        MessageId = msg.MessageId,
                        SenderId = msg.SenderId,
                        ReceiverId = msg.ReceiverId,
                        EncryptedContent = msg.EncryptedContent,
                        SentAt = msg.SentAt,
                        Delivered = true,
                        Read = false
                    });

                    msg.Delivered = true;
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task SendMessage(int toUserId, string encryptedForRecipient, string encryptedForSender)
        {
            int fromUserId = GetCallerUserId();
            if (fromUserId == 0) return;

            using (var db = new EncryptAppDbContext())
            {
                // Validate friendship
                var friendRepo = new FriendRepository(db);
                if (!await friendRepo.AreFriends(fromUserId, toUserId))
                {
                    Clients.Caller.error("You are not friends with this user.");
                    return;
                }

                // Persist message with both encrypted copies
                var message = new Message
                {
                    SenderId = fromUserId,
                    ReceiverId = toUserId,
                    EncryptedContent = encryptedForRecipient,
                    SenderEncryptedContent = encryptedForSender,
                    SentAt = DateTime.UtcNow,
                    Delivered = false,
                    Read = false
                };
                db.Messages.Add(message);
                await db.SaveChangesAsync();

                // Send sender their own copy (encrypted for them)
                var senderDto = new MessageDto
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    EncryptedContent = encryptedForSender,
                    SentAt = message.SentAt,
                    Delivered = false,
                    Read = false
                };

                // Send recipient their copy (encrypted for them)
                var recipientDto = new MessageDto
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    EncryptedContent = encryptedForRecipient,
                    SentAt = message.SentAt,
                    Delivered = false,
                    Read = false
                };

                // Acknowledge to sender
                Clients.Caller.messageSent(senderDto);

                // Deliver to recipient if online
                string recipientConnId;
                if (OnlineUsers.TryGetValue(toUserId, out recipientConnId))
                {
                    recipientDto.Delivered = true;
                    Clients.Client(recipientConnId).receiveMessage(recipientDto);

                    message.Delivered = true;
                    await db.SaveChangesAsync();

                    // Notify sender of delivery
                    Clients.Caller.messageDelivered(message.MessageId);
                }
            }
        }

        public void SendTypingIndicator(int toUserId)
        {
            int fromUserId = GetCallerUserId();
            if (fromUserId == 0) return;

            string recipientConnId;
            if (OnlineUsers.TryGetValue(toUserId, out recipientConnId))
            {
                Clients.Client(recipientConnId).typingIndicator(fromUserId);
            }
        }

        public async Task MarkMessageDelivered(long messageId)
        {
            int userId = GetCallerUserId();
            if (userId == 0) return;

            using (var db = new EncryptAppDbContext())
            {
                var message = await db.Messages.FindAsync(messageId);
                if (message == null || message.ReceiverId != userId) return;

                message.Delivered = true;
                await db.SaveChangesAsync();

                string senderConnId;
                if (OnlineUsers.TryGetValue(message.SenderId, out senderConnId))
                {
                    Clients.Client(senderConnId).messageDelivered(messageId);
                }
            }
        }

        public async Task MarkMessageRead(long messageId)
        {
            int userId = GetCallerUserId();
            if (userId == 0) return;

            using (var db = new EncryptAppDbContext())
            {
                var message = await db.Messages.FindAsync(messageId);
                if (message == null || message.ReceiverId != userId) return;

                message.Delivered = true;
                message.Read = true;
                await db.SaveChangesAsync();

                string senderConnId;
                if (OnlineUsers.TryGetValue(message.SenderId, out senderConnId))
                {
                    Clients.Client(senderConnId).messageRead(messageId);
                }
            }
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var userId = OnlineUsers.FirstOrDefault(kv => kv.Value == Context.ConnectionId).Key;
            if (userId != 0)
            {
                string removed;
                OnlineUsers.TryRemove(userId, out removed);

                using (var db = new EncryptAppDbContext())
                {
                    var conn = await db.Connections.FindAsync(Context.ConnectionId);
                    if (conn != null)
                    {
                        db.Connections.Remove(conn);
                        await db.SaveChangesAsync();
                    }

                    var friendRepo = new FriendRepository(db);
                    var friends = await friendRepo.GetFriends(userId);
                    foreach (var friend in friends)
                    {
                        string friendConnId;
                        if (OnlineUsers.TryGetValue(friend.UserId, out friendConnId))
                        {
                            Clients.Client(friendConnId).userOffline(userId);
                        }
                    }
                }
            }

            await base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userId = OnlineUsers.FirstOrDefault(kv => kv.Value == Context.ConnectionId).Key;
            if (userId != 0)
            {
                OnlineUsers[userId] = Context.ConnectionId;
            }
            return base.OnReconnected();
        }

        private int GetCallerUserId()
        {
            var entry = OnlineUsers.FirstOrDefault(kv => kv.Value == Context.ConnectionId);
            return entry.Key;
        }

        public static string GetConnectionId(int userId)
        {
            string connId;
            OnlineUsers.TryGetValue(userId, out connId);
            return connId;
        }

        public static bool IsUserOnline(int userId)
        {
            return OnlineUsers.ContainsKey(userId);
        }

        public static List<int> GetOnlineUserIds()
        {
            return OnlineUsers.Keys.ToList();
        }
    }
}
