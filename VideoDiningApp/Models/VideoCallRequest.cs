using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VideoDiningApp.Models;

public class VideoCallRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int FriendId { get; set; }

    public string RoomUrl { get; set; } = "";

    [Required]
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public TimeSpan? Duration { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [ForeignKey("FriendId")]
    public User Friend { get; set; }
}
