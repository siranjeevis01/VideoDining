namespace VideoDiningApp.Models
{
    public class VideoCall
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int InitiatorId { get; set; }
        public string RoomUrl { get; set; }
        public string Status { get; set; } = "Pending"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<VideoCallParticipant> Participants { get; set; }
    }

    public class VideoCallParticipant
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsConnected { get; set; } = true;
        public bool HasRejected { get; set; } = false;
    }

}
