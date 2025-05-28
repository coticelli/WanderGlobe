// Models/Badge.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WanderGlobe.Models
{
    public class Badge
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty; // Or IconCssClass

        // Describes how the badge is earned, could be text or structured data
        public string Criteria { get; set; } = string.Empty;
        public int? RequiredCount { get; set; } // e.g., 5 cities for "City Explorer"
        public string? CriteriaType { get; set; } // e.g., "VisitedCitiesCount", "VisitedCountriesInContinent"

        // Navigation property
        public virtual List<UserBadge> UserBadges { get; set; } = new List<UserBadge>(); // RENAMED from Users
    }
}