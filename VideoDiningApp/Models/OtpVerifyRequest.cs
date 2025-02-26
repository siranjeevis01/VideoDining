using System.ComponentModel.DataAnnotations;

public class OtpVerifyRequest
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; }
    public Guid GroupOrderId { get; set; }
    public string RazorpaySignature { get; set; }
}
