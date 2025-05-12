using System;

namespace WanderGlobe.Models
{
    public class TravelJournal
    {
        // Chiave composta
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;
        
        public DateTime VisitDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int Rating { get; set; } // Da 1 a 5
    }
}