using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using WanderGlobe.Models;
using WanderGlobe.Models.Custom;

namespace WanderGlobe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        
        // Proprietà di navigazione
        public virtual List<VisitedCountry> VisitedCountries { get; set; } = new List<VisitedCountry>();
        public virtual List<UserBadge> Badges { get; set; } = new List<UserBadge>();
        public virtual List<DreamDestination> DreamDestinations { get; set; } = new List<DreamDestination>();
        public virtual ICollection<TravelJournal> TravelJournals { get; set; } = new List<TravelJournal>();
    }
}