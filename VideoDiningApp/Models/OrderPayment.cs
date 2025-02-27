namespace VideoDiningApp.Models
{
    public class OrderPayment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }  
        public decimal Amount { get; set; } 
        public string PaymentStatus { get; set; } = "Pending"; 
        public string PaymentLink { get; set; } 
    }
}
