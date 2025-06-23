using System.Data.Common;
using System.Data.Entity;
using AkribisFAM.Manager;

namespace AkribisFAM
{
    public class MyDBContext : DbContext
    {
        public MyDBContext() : base("name=Conn")
        {
            // Set initializer for SQLite
            Database.SetInitializer(new CreateDatabaseIfNotExists<MyDBContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MyDBContext>());


        }

        public MyDBContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRight> UserRights { get; set; }
        public DbSet<UserLevel> UserLevels { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-UserLevel relationship
            modelBuilder.Entity<User>()
                .HasRequired(u => u.UserLevel)
                .WithMany()
                .HasForeignKey(u => u.UserLevelId);

            modelBuilder.Entity<User>()
                .HasOptional(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserLevel>()
                .HasMany(ul => ul.UserRights)
                .WithMany(ur => ur.UserLevels)
                .Map(m =>
                {
                    m.ToTable("LevelRight"); // Junction table
                    m.MapLeftKey("UserLevelId");
                    m.MapRightKey("UserRightId");
                });

        }
    }

}
