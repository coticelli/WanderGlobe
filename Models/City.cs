// Models/City.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Aggiungi questo

namespace WanderGlobe.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCapital { get; set; }

        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public Country Country { get; set; } = null!;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public List<VisitedCity> Visits { get; set; } = new List<VisitedCity>(); // NUOVA PROPRIETÀ
    }
}