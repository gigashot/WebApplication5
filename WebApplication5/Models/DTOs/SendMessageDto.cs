using System.ComponentModel.DataAnnotations;

namespace WebApplication5.Models.DTOs
{
    public class SendMessageDto
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        public string EncryptedContent { get; set; }

        public string SenderEncryptedContent { get; set; }
    }
}
