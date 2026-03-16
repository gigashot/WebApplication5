using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models.Entities
{
    [Table("Connections")]
    public class Connection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [MaxLength(100)]
        public string ConnectionId { get; set; }

        public int UserId { get; set; }

        public DateTime ConnectedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
