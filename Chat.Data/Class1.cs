using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Data
{
    public class UserContext : DbContext
    {
        public UserContext()
            : base("ChatDB")
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<MsgRecord> MsgRecords { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Connection>().ToTable("Connection");
            modelBuilder.Entity<MsgRecord>().ToTable("MsgRecord");
            modelBuilder.Entity<Connection>()
                .HasRequired(o => o.User)
                .WithMany(o => o.Connections)
                .HasForeignKey(o => o.UserID)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }

    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string Alias { get; set; }


        public virtual ICollection<Connection> Connections { get; set; }
    }

    public class Connection
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public string ConnectionID { get; set; }
        public Guid UserID { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    public class MsgRecord
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FromUserID { get; set; }

        public string ToUserID { get; set; }

        public string Content { get; set; }

        public bool Sended { get; set; }

        public DateTime LastUpdatedOn { get; set; }



    }
}
