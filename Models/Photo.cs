using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        public string FileName { get; set; } = string.Empty;
        
        public string FilePath { get; set; } = string.Empty;
        
        public string Url { get; set; } = string.Empty;
        
        public string Caption { get; set; } = string.Empty;
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        public int TravelJournalCountryId { get; set; }
        
        public string TravelJournalUserId { get; set; } = string.Empty;
        
        // Add this navigation property
        public virtual TravelJournal TravelJournal { get; set; } = null!;
    }
}
