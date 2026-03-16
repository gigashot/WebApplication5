using System;

namespace WebApplication5.Models.DTOs
{
    public class FriendRequestDto
    {
        public int RequestId { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendFriendRequestDto
    {
        public int ReceiverId { get; set; }
    }
}
