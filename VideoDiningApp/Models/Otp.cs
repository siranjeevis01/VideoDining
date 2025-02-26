using System;
using System.ComponentModel.DataAnnotations;

namespace VideoDiningApp.Models
{
    public class Otp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryTime { get; set; }
    }
}
