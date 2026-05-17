namespace AgriConsult.API.Models
{
    public class Expert
    {
        public int Id { get; set; }

        public string Name { get; set; } = string .Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public double Rating { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
