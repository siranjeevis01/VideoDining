using System.ComponentModel.DataAnnotations.Schema;
using VideoDiningApp.Enums;

public class Payment
{
    public int Id { get; set; }

    [ForeignKey("Order")]
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public string UserEmail { get; set; }
    public string PaymentId { get; set; }
    public string RazorpaySignature { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public Guid GroupOrderId { get; set; }  

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionId { get; set; }
}
