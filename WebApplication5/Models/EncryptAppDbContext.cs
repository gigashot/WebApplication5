using System.Data.Entity;
using WebApplication5.Models.Entities;

namespace WebApplication5.Models
{
    public class EncryptAppDbContext : DbContext
    {
        public EncryptAppDbContext() : base("name=EncryptAppConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Connection> Connections { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Friends: unique composite index on (UserId1, UserId2)
            modelBuilder.Entity<Friend>()
                .HasIndex(f => new { f.UserId1, f.UserId2 })
                .IsUnique()
                .HasName("IX_Friends_UserPair");

            // Disable cascade delete on Friend to avoid multiple cascade paths
            modelBuilder.Entity<Friend>()
                .HasRequired(f => f.User1)
                .WithMany()
                .HasForeignKey(f => f.UserId1)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Friend>()
                .HasRequired(f => f.User2)
                .WithMany()
                .HasForeignKey(f => f.UserId2)
                .WillCascadeOnDelete(false);

            // FriendRequest: disable cascade delete
            modelBuilder.Entity<FriendRequest>()
                .HasRequired(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FriendRequest>()
                .HasRequired(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .WillCascadeOnDelete(false);

            // Message: disable cascade delete
            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .WillCascadeOnDelete(false);

            // Message indexes for conversation queries
            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.SenderId, m.ReceiverId, m.SentAt })
                .HasName("IX_Messages_Conversation");

            // Connection: disable cascade delete
            modelBuilder.Entity<Connection>()
                .HasRequired(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
