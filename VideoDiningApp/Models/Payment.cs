using System;
using System.ComponentModel.DataAnnotations.Schema;
using VideoDiningApp.Models;  // Ensure this is included

namespace VideoDiningApp.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }  // ✅ Now correctly an int

        public Order Order { get; set; }  // ✅ Ensures relationship
        public string UserEmail { get; set; }  // ✅ Add this
        public string PaymentId { get; set; }
        public string RazorpaySignature { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public Guid GroupOrderId { get; set; }  // ✅ Add this
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
