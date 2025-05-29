// Folder: Models/Custom
// File: CustomModel.cs
using System;
using System.Collections.Generic;
// No other 'using' directives should be strictly necessary for these simple model definitions
// unless you add data annotations from System.ComponentModel.DataAnnotations for example.

namespace WanderGlobe.Models.Custom
{
    // --- Models specifically for Timeline functionality ---

    public class TimelineEntry
    {
        /// <summary>
        /// Unique identifier for this timeline entry (e.g., VisitedCity.Id).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the user this timeline entry belongs to.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// ID of the country associated with this visit.
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Name of the country.
        /// </summary>
        public string CountryName { get; set; } = string.Empty;

        /// <summary>
        /// ISO code of the country (e.g., "IT", "US").
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// Name of the city visited.
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// Date of the visit.
        /// </summary>
        public DateTime VisitDate { get; set; }

        /// <summary>
        /// User's notes or description for this visit.
        /// </summary>
        public string? Notes { get; set; } // Nullable if notes are optional

        /// <summary>
        /// Photos associated with this timeline entry/visit.
        /// </summary>
        public List<TimelinePhoto> Photos { get; set; } = new List<TimelinePhoto>();

        /// <summary>
        /// Weather information for the time of the visit.
        /// </summary>
        public TimelineWeather Weather { get; set; } = new TimelineWeather(); // Initialize to avoid null
    }

    public class TimelinePhoto
    {
        /// <summary>
        /// Unique identifier of the photo (from the Photo entity).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Web-accessible URL of the photo.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional caption for the photo.
        /// </summary>
        public string? Caption { get; set; }

        /// <summary>
        /// Date the photo was uploaded or taken.
        /// </summary>
        public DateTime UploadDate { get; set; }
    }

    public class TimelineWeather // Standardized definition
    {
        /// <summary>
        /// Month of the weather observation (e.g., 1 for January).
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Temperature, typically in Celsius. Using double for more precision from APIs.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Textual description of the weather condition (e.g., "Soleggiato", "Nuvoloso").
        /// </summary>
        public string Condition { get; set; } = "N/D"; // Default to "Not Available"

        /// <summary>
        /// Optional URL for a weather icon.
        /// </summary>
        public string? IconUrl { get; set; }
    }

    public class TimelineNote // This class might be for a different "journal notes" feature
    {
        /// <summary>
        /// Unique identifier for the note.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the user who wrote the note.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// ID of the country this note might be generally associated with.
        /// Consider linking to VisitedCity.Id if notes are per specific visit.
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Content of the note.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Date the note was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    // --- Other Custom Models (for Planned Trips, Recommendations, Map) ---
    // These are included as you provided them.

    public class PlannedTrip
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int CompletionPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DestinationName { get; set; } = string.Empty; // Could be CityName or a custom name
        public DateTime UpdatedAt { get; set; }
        
        public List<ChecklistItem> Checklist { get; set; } = new List<ChecklistItem>();
    }

    public class ChecklistItem
    {
        public int Id { get; set; }
        public string PlannedTripId { get; set; } = string.Empty; // Should match PlannedTrip.Id type
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "other"; // e.g., "Flights", "Accommodation", "Activity"
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
    
    public class RecommendedDestination
    {
        public string Id { get; set; } = string.Empty; // Assuming string ID for AI/external source
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string ReasonToVisit { get; set; } = string.Empty;
        
        public int MatchPercentage { get; set; }
        public string Weather { get; set; } = string.Empty; // Consider using TimelineWeather or similar structured type
        public string CostLevel { get; set; } = string.Empty; // e.g., "$", "$$", "$$$"
        public int Accommodations { get; set; } // Could be count or a qualitative score
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class MapDestinationsViewModel
    {
        public List<MapDestinationItem> Wishlist { get; set; } = new List<MapDestinationItem>();
        public List<MapDestinationItem> PlannedTrips { get; set; } = new List<MapDestinationItem>();
        public List<MapDestinationItem> VisitedCities { get; set; } = new List<MapDestinationItem>();
    }

    public class MapDestinationItem
    {
        public string Id { get; set; } = string.Empty; // Could be CityId, PlannedTripId, etc.
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Type { get; set; } = string.Empty; // e.g., "wishlist", "planned", "visited"
        public DreamPriority Priority { get; set; }
        public int CompletionPercentage { get; set; } // Relevant for PlannedTrips
        public string? ImageUrl { get; set; }
    }

    public enum DreamPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }
}