// Models/DreamCountry.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class DreamCountry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Changed from ApplicationUserId for convention, ensure FK matches
        [ForeignKey("UserId")] // This now matches the property name
        public ApplicationUser User { get; set; } = null!; // Changed from ApplicationUser for convention

        [Required]
        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public Country Country { get; set; } = null!;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public bool IsPlanned { get; set; }
    }
}