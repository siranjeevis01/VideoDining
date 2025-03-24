using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class FriendRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int FriendId { get; set; }

        [Required]
        public string Status { get; set; } = "pending"; 

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("FriendId")]
        public User FriendUser { get; set; }
    }
}
