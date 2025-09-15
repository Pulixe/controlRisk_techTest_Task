using Microsoft.EntityFrameworkCore;
using TaskApi.Functions.Models;

namespace TaskApi.Functions.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TaskItem
            var t = modelBuilder.Entity<TaskItem>();
            t.HasKey(x => x.Id);
            t.Property(x => x.Title).HasMaxLength(250).IsRequired();
            t.Property(x => x.CreatedBy).HasMaxLength(100);
            t.Property(x => x.AssignedTo).HasMaxLength(100);
            t.HasIndex(x => x.Status);
            t.HasIndex(x => x.DueDate);
            t.HasIndex(x => x.UserId);

            // User
            var u = modelBuilder.Entity<User>();
            u.HasKey(x => x.Id);
            u.Property(x => x.Sub).IsRequired();
            u.HasIndex(x => x.Sub).IsUnique();
            u.Property(x => x.Name).HasMaxLength(250);
            u.Property(x => x.Email).HasMaxLength(320);

            // Relations
            u.HasMany(x => x.Tasks)
             .WithOne(x => x.User!)
             .HasForeignKey(x => x.UserId)
             .IsRequired();
        }
    }
}
