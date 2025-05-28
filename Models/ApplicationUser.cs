// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System; // For DateTime
using System.Collections.Generic;
// using WanderGlobe.Models.Custom; // Add this if DreamDestination is in that namespace

namespace WanderGlobe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public string? ProfilePicture { get; set; }

        // Relazioni
        public virtual List<VisitedCountry> VisitedCountries { get; set; } = new List<VisitedCountry>();
        public virtual List<VisitedCity> VisitedCities { get; set; } = new List<VisitedCity>();
        public virtual List<TravelJournal> TravelJournals { get; set; } = new List<TravelJournal>();
        public virtual List<DreamDestination> DreamDestinations { get; set; } = new List<DreamDestination>();
        public virtual List<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
        public virtual List<DreamCountry> DreamedCountries { get; set; } = new List<DreamCountry>(); // Added for DreamCountry
    }
}