using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models.Entities
{
    [Table("FriendRequests")]
    public class FriendRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [ForeignKey("Sender")]
        public int SenderId { get; set; }

        [ForeignKey("Receiver")]
        public int ReceiverId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User Sender { get; set; }

        public virtual User Receiver { get; set; }
    }
}
