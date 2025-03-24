using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class VideoCall
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int FriendId { get; set; }

        [Required]
        public int FriendUserId { get; set; }

        [Required]
        public int OrderId { get; set; }

        public DateTime CallStartTime { get; set; }
        public DateTime? CallEndTime { get; set; }

        [Required]
        public string CallStatus { get; set; } = "Pending"; 

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("FriendUserId")]
        public User FriendUser { get; set; }
    }
}
