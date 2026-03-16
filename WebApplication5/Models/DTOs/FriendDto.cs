namespace WebApplication5.Models.DTOs
{
    public class FriendDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PublicKey { get; set; }
        public bool IsOnline { get; set; }
    }
}
