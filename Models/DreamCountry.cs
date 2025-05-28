using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WanderGlobe.Models
{
    public class DreamCountry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } // Foreign Key per ApplicationUser
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public int CountryId { get; set; } // Foreign Key per Country
        public Country Country { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; } // Note opzionali per il sogno
        public bool IsPlanned { get; set; } // Se è attivamente in pianificazione vs solo wishlist
    }
}