using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models.Entities
{
    [Table("Friends")]
    public class Friend
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FriendId { get; set; }

        [ForeignKey("User1")]
        public int UserId1 { get; set; }

        [ForeignKey("User2")]
        public int UserId2 { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User User1 { get; set; }

        public virtual User User2 { get; set; }
    }
}
