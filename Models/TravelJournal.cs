// Models/TravelJournal.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class TravelJournal
    {
        // Composite Primary Key (as defined in DbContext)
        public string UserId { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public DateTime VisitDate { get; set; } // This is part of the PK

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
        
        // THIS PROPERTY IS EXPECTED BY Badges.cshtml.cs
        public string Notes { get; set; } = string.Empty; // Ensure this exists and matches casing.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; } = null!;
        
        public virtual List<Photo> Photos { get; set; } = new List<Photo>();
    }
}