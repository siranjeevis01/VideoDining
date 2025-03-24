using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace VideoDiningApp.Models
{
    public class OrderPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public bool HasPaid { get; set; }
        public string OtpCode { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsPaid { get; set; }

        public OrderPayment() { }

        public void Initialize(Order order, User user)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            User = user ?? throw new ArgumentNullException(nameof(user));
            OrderId = order.Id;
            UserId = user.Id;
            IsPaid = false;
            OtpCode = GenerateOtp();
            IsDelivered = false;
        }

        private string GenerateOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[16];
                rng.GetBytes(randomBytes);
                return BitConverter.ToString(randomBytes).Replace("-", "").Substring(0, 6);
            }
        }
    }
}
