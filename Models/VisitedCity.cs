// Models/VisitedCity.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class VisitedCity
    {
        // Chiave Primaria Semplice (consigliata per facilità)
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        public int CityId { get; set; }
        [ForeignKey("CityId")]
        public City City { get; set; } = null!;

        public DateTime VisitDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // Opzionale, se vuoi tracciare aggiornamenti
    }
}