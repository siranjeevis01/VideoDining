using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VideoDiningApp.Models;
using VideoDiningApp.Enums;

namespace VideoDiningApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }  

        public DbSet<Cart> Carts { get; set; }  
        public DbSet<CartItem> CartItems { get; set; }  
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<VideoCallRequest> VideoCallRequests { get; set; }
        public DbSet<VideoCallSession> VideoCallSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Friendship>().ToTable("Friendships");

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User1)
                .WithMany(u => u.FriendshipsAsUser1)
                .HasForeignKey(f => f.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User2)
                .WithMany(u => u.FriendshipsAsUser2)
                .HasForeignKey(f => f.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.GroupOrderId)
                .HasDatabaseName("IX_Order_GroupOrderId");

            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentStatus)
                .HasConversion<string>()
                .HasColumnType("nvarchar(20)");

            modelBuilder.Entity<Order>()
                .Property(o => o.FoodItemsSerialized)
                .HasColumnName("FoodItems");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
        }
    }
}
