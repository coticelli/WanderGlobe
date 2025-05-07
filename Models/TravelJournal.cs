namespace WanderGlobe.Models
{
    public class TravelJournal
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CountryId { get; set; }
        public Country Country { get; set; }

        public DateTime VisitDate { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<Photo> Photos { get; set; } = new List<Photo>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
