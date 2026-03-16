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

        public int UserId1 { get; set; }

        public int UserId2 { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId1")]
        public virtual User User1 { get; set; }

        [ForeignKey("UserId2")]
        public virtual User User2 { get; set; }
    }
}
