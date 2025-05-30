using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class VisitedCity
    {
        // Option 1: Composite Primary Key
        // [Key]
        // [Column(Order = 0)]
        // [ForeignKey("User")]
        // public string UserId { get; set; }
        // public virtual ApplicationUser User { get; set; }

        // [Key]
        // [Column(Order = 1)]
        // [ForeignKey("City")]
        // public int CityId { get; set; }
        // public virtual City City { get; set; }

        // Option 2: Separate Auto-Incrementing Primary Key (Simpler) - RECOMMENDED
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int CityId { get; set; }
        public virtual City City { get; set; }


        [Required]
        public DateTime VisitDate { get; set; }
        public string? Notes { get; set; } // Corresponds to visitExperience
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}