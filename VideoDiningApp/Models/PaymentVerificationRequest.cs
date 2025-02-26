namespace VideoDiningApp.Models
{
    public class PaymentVerificationRequest
    {
        public int OrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public Guid GroupOrderId { get; set; }
    }
}
