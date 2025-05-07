namespace WanderGlobe.Models
{
    public class VisitedCountry
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CountryId { get; set; }
        public Country Country { get; set; }

        public DateTime VisitDate { get; set; }
        public bool IsFavorite { get; set; }
        public string? Notes { get; set; } 
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
