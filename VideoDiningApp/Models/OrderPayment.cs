namespace VideoDiningApp.Models
{
    public class OrderPayment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }  // Friend’s ID
        public decimal Amount { get; set; } // Amount this user needs to pay
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Canceled
        public string PaymentLink { get; set; } // Razorpay link
    }
}
