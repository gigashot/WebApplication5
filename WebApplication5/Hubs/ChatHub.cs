using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public void Register(int userId)
        {
            var connectionId = Context.ConnectionId;
            OnlineUsers[userId] = connectionId;

            // Persist to database
            using (var db = new EncryptAppDbContext())
            {
                // Remove any stale connections for this user
                var stale = db.Connections.Where(c => c.UserId == userId).ToList();
                db.Connections.RemoveRange(stale);

                db.Connections.Add(new Connection
                {
                    ConnectionId = connectionId,
                    UserId = userId,
                    ConnectedAt = DateTime.UtcNow
                });
                db.SaveChanges();

                // Notify friends that this user is online
                var friendRepo = new FriendRepository(db);
                var friends = friendRepo.GetFriends(userId).Result;
                foreach (var friend in friends)
                {
                    string friendConnId;
                    if (OnlineUsers.TryGetValue(friend.UserId, out friendConnId))
                    {
                        Clients.Client(friendConnId).userOnline(userId);
                    }
                }

                // Deliver undelivered messages
                var undelivered = db.Messages
                    .Where(m => m.ReceiverId == userId && !m.Delivered)
                    .OrderBy(m => m.SentAt)
                    .ToList();

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
                db.SaveChanges();
            }
        }

        public void SendMessage(int toUserId, string encryptedPayload)
        {
            int fromUserId = GetCallerUserId();
            if (fromUserId == 0) return;

            using (var db = new EncryptAppDbContext())
            {
                // Validate friendship
                var friendRepo = new FriendRepository(db);
                if (!friendRepo.AreFriends(fromUserId, toUserId).Result)
                {
                    Clients.Caller.error("You are not friends with this user.");
                    return;
                }

                // Persist message
                var message = new Message
                {
                    SenderId = fromUserId,
                    ReceiverId = toUserId,
                    EncryptedContent = encryptedPayload,
                    SentAt = DateTime.UtcNow,
                    Delivered = false,
                    Read = false
                };
                db.Messages.Add(message);
                db.SaveChanges();

                var dto = new MessageDto
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    EncryptedContent = message.EncryptedContent,
                    SentAt = message.SentAt,
                    Delivered = false,
                    Read = false
                };

                // Acknowledge to sender
                Clients.Caller.messageSent(dto);

                // Deliver to recipient if online
                string recipientConnId;
                if (OnlineUsers.TryGetValue(toUserId, out recipientConnId))
                {
                    dto.Delivered = true;
                    Clients.Client(recipientConnId).receiveMessage(dto);

                    message.Delivered = true;
                    db.SaveChanges();

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

        public void MarkMessageDelivered(long messageId)
        {
            int userId = GetCallerUserId();
            if (userId == 0) return;

            using (var db = new EncryptAppDbContext())
            {
                var message = db.Messages.Find(messageId);
                if (message == null || message.ReceiverId != userId) return;

                message.Delivered = true;
                db.SaveChanges();

                string senderConnId;
                if (OnlineUsers.TryGetValue(message.SenderId, out senderConnId))
                {
                    Clients.Client(senderConnId).messageDelivered(messageId);
                }
            }
        }

        public void MarkMessageRead(long messageId)
        {
            int userId = GetCallerUserId();
            if (userId == 0) return;

            using (var db = new EncryptAppDbContext())
            {
                var message = db.Messages.Find(messageId);
                if (message == null || message.ReceiverId != userId) return;

                message.Delivered = true;
                message.Read = true;
                db.SaveChanges();

                string senderConnId;
                if (OnlineUsers.TryGetValue(message.SenderId, out senderConnId))
                {
                    Clients.Client(senderConnId).messageRead(messageId);
                }
            }
        }

        public override Task OnConnected()
        {
            // userId will be set via Register() call from client
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // Find which user this connection belongs to
            var userId = OnlineUsers.FirstOrDefault(kv => kv.Value == Context.ConnectionId).Key;
            if (userId != 0)
            {
                string removed;
                OnlineUsers.TryRemove(userId, out removed);

                // Remove from DB
                using (var db = new EncryptAppDbContext())
                {
                    var conn = db.Connections.Find(Context.ConnectionId);
                    if (conn != null)
                    {
                        db.Connections.Remove(conn);
                        db.SaveChanges();
                    }

                    // Notify friends
                    var friendRepo = new FriendRepository(db);
                    var friends = friendRepo.GetFriends(userId).Result;
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

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            // Re-register if needed
            var userId = OnlineUsers.FirstOrDefault(kv => kv.Value == Context.ConnectionId).Key;
            if (userId != 0)
            {
                OnlineUsers[userId] = Context.ConnectionId;
            }
            return base.OnReconnected();
        }

        // Helper to get caller userId from the in-memory map
        private int GetCallerUserId()
        {
            var entry = OnlineUsers.FirstOrDefault(kv => kv.Value == Context.ConnectionId);
            return entry.Key;
        }

        // Static helper for external access (e.g., from controllers)
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
