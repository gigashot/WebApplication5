using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MessageId { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        [Required]
        public string EncryptedContent { get; set; }

        public string SenderEncryptedContent { get; set; }

        public DateTime SentAt { get; set; }

        public bool Delivered { get; set; }

        public bool Read { get; set; }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }
    }
}
