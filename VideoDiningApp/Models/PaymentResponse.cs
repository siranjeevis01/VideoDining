using System;
using VideoDiningApp.Enums;

namespace VideoDiningApp.Models
{
    public class PaymentResponse
    {
        public string PaymentId { get; set; }
        public PaymentStatus Status { get; set; }  // Use enum instead of string
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Reason { get; set; }  // Optional - reason for failure if applicable
    }
}
