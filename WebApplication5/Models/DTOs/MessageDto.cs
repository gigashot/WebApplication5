using System;

namespace WebApplication5.Models.DTOs
{
    public class MessageDto
    {
        public long MessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string EncryptedContent { get; set; }
        public DateTime SentAt { get; set; }
        public bool Delivered { get; set; }
        public bool Read { get; set; }
    }
}
