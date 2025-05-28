// Models/Photo.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        
        // If a photo needs to be directly linked to a user, independent of a journal entry:
        [Required] // Make this required if a photo must always have an uploader
        public string UserId { get; set; } = string.Empty; // UNCOMMENTED THIS
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!; // UNCOMMENTED THIS

        [Required]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        public string FilePath { get; set; } = string.Empty; 
        
        [Required]
        public string Url { get; set; } = string.Empty; 
        
        public string? Caption { get; set; }
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        // Foreign Key properties for TravelJournal (if a photo can belong to a journal)
        // Make these nullable if a photo does NOT always belong to a journal
        public string? TravelJournalUserId { get; set; } 
        public int? TravelJournalCountryId { get; set; }
        public DateTime? TravelJournalVisitDate { get; set; } 
        
        [ForeignKey("TravelJournalUserId, TravelJournalCountryId, TravelJournalVisitDate")]
        public virtual TravelJournal? TravelJournal { get; set; } // Made nullable
    }
}