using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class OrderParticipant
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string RazorpayPaymentId { get; set; }
        public string UserEmail { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
