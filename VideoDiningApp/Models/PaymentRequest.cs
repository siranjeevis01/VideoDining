public class PaymentRequest
{
    public int OrderId { get; set; }
    public Guid GroupOrderId { get; set; } 
    public string PaymentDetails { get; set; }
    public string Email { get; set; }
    public string Otp { get; set; }
    public string RazorpaySignature { get; set; }
}
