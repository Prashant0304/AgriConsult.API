namespace AgriConsult.API.Models
{
    public class Consultation
    {
        public int Id { get; set; }

        public int FarmerId { get; set; }
        public int ExpertId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduedAt { get; set; }


    }
}
