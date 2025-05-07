namespace WanderGlobe.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Caption { get; set; }

        public int TravelJournalId { get; set; }
        public TravelJournal TravelJournal { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
