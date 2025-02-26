using System;

namespace VideoDiningApp.Models
{
    public class VideoCallSession
    {
        public int Id { get; set; }
        public string RoomUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
