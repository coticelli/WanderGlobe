using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public string FileName { get; set; } = string.Empty; // Original filename

        [Required]
        public string Url { get; set; } = string.Empty; // Web-accessible URL, e.g., /images/user_photos/guid_name.jpg
        
        public string? Caption { get; set; }
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // --- Link to a specific VisitedCity record ---
        public int? VisitedCityId { get; set; } // Foreign key to VisitedCity
        [ForeignKey("VisitedCityId")]
        public virtual VisitedCity? VisitedCity { get; set; } // Navigation property

        // --- Optional link to a TravelJournal ---
        // If a photo can also belong to a TravelJournal entry independent of a VisitedCity
        public int? TravelJournalId { get; set; } 
        [ForeignKey("TravelJournalId")]
        public virtual TravelJournal? TravelJournal { get; set; }

        // Legacy fields - keep them as nullable to support migration
        public string? TravelJournalUserId { get; set; }
        public int? TravelJournalCountryId { get; set; }
        public DateTime? TravelJournalVisitDate { get; set; }
    }
}