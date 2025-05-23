﻿using System;
using System.Collections.Generic;

namespace WanderGlobe.Models.Custom
{
    // Modelli dedicati per le funzionalità Timeline
    public class TimelineEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string CityName { get; set; }
        public DateTime VisitDate { get; set; }
        public string Notes { get; set; }
        public List<TimelinePhoto> Photos { get; set; } = new List<TimelinePhoto>();
        public TimelineWeather Weather { get; set; }
    }

    public class TimelinePhoto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Caption { get; set; }
        public DateTime UploadDate { get; set; }
    }

    public class TimelineWeather
    {
        public int Month { get; set; }
        public int Temperature { get; set; }
        public string Condition { get; set; } = string.Empty;
    }

    public class TimelineNote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CountryId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Modelli dedicati per le funzionalità DreamMap
    public class DreamDestination
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DreamPriority Priority { get; set; }
        public string ImageUrl { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

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
        
        // Relazione con ChecklistItem
        public List<ChecklistItem> Checklist { get; set; } = new List<ChecklistItem>();
    }

    // Aggiungi al tuo modello di dati
 public class ChecklistItem
    {
        public int Id { get; set; }
        public string PlannedTripId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "other";
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
    

    public class RecommendedDestination
    {
        public string Id { get; set; } // Changed to string to match your Gemini implementation
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        
        // New properties for AI recommendations
        public string Description { get; set; } = string.Empty;
        public string ReasonToVisit { get; set; } = string.Empty;
        
        // Optional existing properties that might be useful later
        public int MatchPercentage { get; set; }
        public string Weather { get; set; } = string.Empty;
        public string CostLevel { get; set; } = string.Empty;
        public int Accommodations { get; set; }
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
        public string Id { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? Priority { get; set; }
        public int? CompletionPercentage { get; set; }
    }

    public enum DreamPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }
}