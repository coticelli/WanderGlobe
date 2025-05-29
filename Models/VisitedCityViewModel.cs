namespace WanderGlobe.Models
{
    public class VisitedCityViewModel
    {
        // This should be the unique Primary Key of your VisitedCity database record.
        // It's crucial for identifying the specific visit for edits, photo associations, etc.
        public int VisitedCityRecordId { get; set; }

        public int CityId { get; set; } // Foreign Key to the City entity
        public string CityName { get; set; } = string.Empty;

        public int CountryId { get; set; } // Foreign Key to the Country entity (via City.CountryId)
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty; // e.g., "IT", "ES"

        public string? Continent { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime VisitDate { get; set; }
        public string? Description { get; set; } // Corresponds to VisitedCity.Notes

        // Optional: For displaying a city-specific image if it's different from the generic country/capital image.
        // This could be a filename like "barcelona.jpg" or a full URL.
        // Your service layer would be responsible for populating this if the logic exists.
        public string? CitySpecificImage { get; set; }

        public DateTime CreatedAt { get; set; } // From VisitedCity.CreatedAt
        public DateTime? UpdatedAt { get; set; } // From VisitedCity.UpdatedAt


        // You can add any other properties that your Timeline view might need,
        // potentially flattened from related entities or calculated.
        // For example:
        // public int NumberOfPhotos { get; set; }
        // public bool IsFavorite { get; set; } // If you add such a feature to VisitedCity
    }
}