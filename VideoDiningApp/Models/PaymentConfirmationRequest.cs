namespace VideoDiningApp.Models
{
    public class PaymentConfirmationRequest
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string TransactionId { get; set; }
    }
}
