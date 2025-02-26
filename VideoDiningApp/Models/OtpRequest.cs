using System.ComponentModel.DataAnnotations;

namespace VideoDiningApp.DTOs
{
    public class OtpRequest
    {
        public string Email { get; set; }
        public Guid GroupOrderId { get; set; }  // FIXED: Added Group Order ID
    }

}
