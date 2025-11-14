using ABCRetailers.Models;
using Microsoft.EntityFrameworkCore;

namespace ABCRetailers.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CartItem to use the "Cart" table
            modelBuilder.Entity<CartItem>().ToTable("Cart");

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();
        }
    }
}