using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Models;

namespace VideoDiningApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<VideoCall> VideoCalls { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }
        public DbSet<FriendPayment> FriendPayments { get; set; }
        public DbSet<OrderFriend> OrderFriends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Seed Admin User (Hardcoded Date to Avoid Dynamic Migration Issues)
            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = 1,
                Name = "Super Admin",
                Email = "siranjeeviwd@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                CreatedAt = new DateTime(2025, 03, 24, 0, 0, 0, DateTimeKind.Utc)
            });

            // ✅ Unique Constraint for Friend Relationships
            modelBuilder.Entity<Friend>()
                .HasIndex(f => new { f.UserId, f.FriendId })
                .IsUnique();

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.FriendUser)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Friend Requests - Unique & Proper FK Setup
            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.UserId, fr.FriendId })
                .IsUnique();

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.User)
                .WithMany()
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.FriendUser)
                .WithMany()
                .HasForeignKey(fr => fr.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Video Call Relationships
            modelBuilder.Entity<VideoCall>()
                .HasOne(vc => vc.User)
                .WithMany(u => u.VideoCalls)
                .HasForeignKey(vc => vc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VideoCall>()
                .HasOne(vc => vc.FriendUser)
                .WithMany()
                .HasForeignKey(vc => vc.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Friend Payments (Mapping via Email)
            modelBuilder.Entity<FriendPayment>()
                .HasOne(fp => fp.Order)
                .WithMany(o => o.FriendPayments)
                .HasForeignKey(fp => fp.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FriendPayment>()
                .HasOne(fp => fp.Friend)
                .WithMany()
                .HasForeignKey(fp => fp.FriendEmail)
                .HasPrincipalKey(u => u.Email)  // 🔹 Maps Friend Email to User Email
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Order Friends (Composite Key)
            modelBuilder.Entity<OrderFriend>()
                .HasKey(of => new { of.OrderId, of.FriendId });

            modelBuilder.Entity<OrderFriend>()
                .HasOne(of => of.Order)
                .WithMany(o => o.OrderFriends)
                .HasForeignKey(of => of.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderFriend>()
                .HasOne(of => of.Friend)
                .WithMany(f => f.OrderFriends)
                .HasForeignKey(of => of.FriendId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Payment Status as Enum (Stored as String)
            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasDefaultValue(PaymentStatus.Pending);

            // ✅ Order Relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
