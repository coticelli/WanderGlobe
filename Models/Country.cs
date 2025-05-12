using System.Collections.Generic;
using WanderGlobe.Models;

namespace WanderGlobe.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // ISO code
        public string Continent { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? FlagUrl { get; set; }
        public List<VisitedCountry> VisitedByUsers { get; set; } = new List<VisitedCountry>();
        public virtual ICollection<TravelJournal> TravelJournals { get; set; } = new List<TravelJournal>();
        public List<City> Cities { get; set; } = new List<City>();
    }
}