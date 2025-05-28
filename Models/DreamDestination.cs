// Models/DreamDestination.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class DreamDestination
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string DestinationName { get; set; } = string.Empty;

        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City? City { get; set; }

        public int? CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country? Country { get; set; }

        public string? Notes { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow; // Was AddedDate, if you need CreatedAt, use that name
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAchieved { get; set; } = false;
        public int Priority { get; set; }
        public DateTime? TargetDate { get; set; }

        [NotMapped]
        public List<string> Tags { get; set; } = new List<string>();

        // If Badges.cshtml.cs needs CountryCode directly (less ideal, better via navigation)
        // you could add a [NotMapped] helper property, but it's better to fix the usage.
        // [NotMapped]
        // public string? CountryCode => Country?.Code;
        public string CityName => City?.Name ?? DestinationName;
        public string CountryName => Country?.Name ?? City?.Country?.Name ?? "Unknown";
        public string CountryCode => Country?.Code ?? City?.Country?.Code ?? "XX";

        // New properties
        public double Latitude => City?.Latitude ?? Country?.Latitude ?? 0;
        public double Longitude => City?.Longitude ?? Country?.Longitude ?? 0;
        public string ImageUrl => City?.ImageUrl ?? Country?.FlagUrl ?? "/images/placeholder-destination.jpg";
    }
}