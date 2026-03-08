using Microsoft.EntityFrameworkCore;
using Portfolyo.Models;

namespace Portfolyo.Data
{
    public class AdminAuthDbContext : DbContext
    {
        public AdminAuthDbContext(DbContextOptions<AdminAuthDbContext> options) : base(options) { }

        public DbSet<AdminUser> AdminUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tablo adını sabitle
            modelBuilder.Entity<AdminUser>().ToTable("AdminUsers");

            // Username unique olsun
            modelBuilder.Entity<AdminUser>()
                .HasIndex(x => x.Username)
                .IsUnique();
        }
    }
}