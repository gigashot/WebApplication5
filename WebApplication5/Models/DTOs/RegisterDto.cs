using System.ComponentModel.DataAnnotations;

namespace WebApplication5.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        public string PublicKey { get; set; }
    }
}
