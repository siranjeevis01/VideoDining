using System.ComponentModel.DataAnnotations;

namespace VideoDiningApp.DTOs
{
    public class CancelOrderRequest
    {
        public string UserEmail { get; set; }
        public Guid GroupOrderId { get; set; }  // FIXED: Added Group Order ID
    }

}
