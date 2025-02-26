namespace VideoDiningApp.Models
{
    public class PaymentLinkRequest
    {
        public Guid GroupOrderId { get; set; }  // FIXED: Added Group Order ID
    }
}
