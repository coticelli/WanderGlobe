using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Data;
using WanderGlobe.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WanderGlobe.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public int CountriesVisited { get; set; }
        public int ContinentsVisited { get; set; }
        public int BadgesEarned { get; set; }
        public int TotalMiles { get; set; }
        public double WorldPercentage { get; set; }
        public List<RecentVisitViewModel> RecentVisits { get; set; } = new();
        public List<PinViewModel> VisitedPins { get; set; } = new();

        public IndexModel(
            ILogger<IndexModel> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Calcola statistiche
                var visitedCountries = await _context.VisitedCountries
                    .Include(vc => vc.Country)
                    .Where(vc => vc.UserId == user.Id)
                    .ToListAsync();

                CountriesVisited = visitedCountries.Count;
                ContinentsVisited = visitedCountries.Select(vc => vc.Country.Continent).Distinct().Count();

                // Badge (simulati)
                BadgesEarned = CountriesVisited > 0 ? Math.Min(5, CountriesVisited / 2) : 0;

                // Kilometri (simulati)
                TotalMiles = CountriesVisited * 1500;

                // Percentuale del mondo (193 paesi riconosciuti dalle Nazioni Unite)
                WorldPercentage = Math.Round(((double)CountriesVisited / 193) * 100, 1);

                // Visite recenti
                RecentVisits = visitedCountries
                    .OrderByDescending(vc => vc.VisitDate)
                    .Take(5)
                    .Select(vc => new RecentVisitViewModel
                    {
                        CountryId = vc.CountryId,
                        CountryName = vc.Country.Name,
                        CountryCode = vc.Country.Code,
                        CityName = GetCapital(vc.Country.Code),
                        VisitDate = vc.VisitDate,
                        Note = GetRandomNote(),
                        ImageUrl = GetCountryImage(vc.Country.Code)
                    })
                    .ToList();

                // Pin per la mappa
                VisitedPins = visitedCountries
                    .Select(vc => new PinViewModel
                    {
                        Id = vc.CountryId,
                        Name = vc.Country.Name,
                        Lat = vc.Country.Latitude,
                        Lng = vc.Country.Longitude
                    })
                    .ToList();
            }
            else
            {
                // Dati di esempio per utenti non autenticati
                CountriesVisited = 0;
                ContinentsVisited = 0;
                BadgesEarned = 0;
                TotalMiles = 0;
                WorldPercentage = 0;

                // Visite di esempio
                RecentVisits = new List<RecentVisitViewModel>
                {
                    new RecentVisitViewModel
                    {
                        CountryId = 1,
                        CountryName = "Italia",
                        CountryCode = "IT",
                        CityName = "Roma",
                        VisitDate = DateTime.Now.AddMonths(-2),
                        Note = "Visita alla città eterna, un'esperienza indimenticabile!",
                        ImageUrl = "/images/destinations/italy.jpg"
                    },
                    new RecentVisitViewModel
                    {
                        CountryId = 2,
                        CountryName = "Francia",
                        CountryCode = "FR",
                        CityName = "Parigi",
                        VisitDate = DateTime.Now.AddMonths(-5),
                        Note = "La città dell'amore e delle luci, bellissima!",
                        ImageUrl = "/images/destinations/france.jpg"
                    },
                    new RecentVisitViewModel
                    {
                        CountryId = 3,
                        CountryName = "Spagna",
                        CountryCode = "ES",
                        CityName = "Madrid",
                        VisitDate = DateTime.Now.AddMonths(-8),
                        Note = "Cultura, tapas e vita notturna incredibile",
                        ImageUrl = "/images/destinations/spain.jpg"
                    }
                };

                // Pin di esempio
                VisitedPins = new List<PinViewModel>
                {
                    new PinViewModel { Id = 1, Name = "Italia", Lat = 41.9028, Lng = 12.4964 },
                    new PinViewModel { Id = 2, Name = "Francia", Lat = 48.8566, Lng = 2.3522 },
                    new PinViewModel { Id = 3, Name = "Spagna", Lat = 40.4168, Lng = -3.7038 }
                };
            }
        }

        // Helper per ottenere il nome della capitale di un paese
        private string GetCapital(string countryCode)
        {
            var capitals = new Dictionary<string, string>
            {
                {"IT", "Roma"}, {"GB", "Londra"}, {"FR", "Parigi"}, {"DE", "Berlino"},
                {"ES", "Madrid"}, {"PT", "Lisbona"}, {"NL", "Amsterdam"}, {"BE", "Bruxelles"},
                {"GR", "Atene"}, {"US", "Washington D.C."}, {"JP", "Tokyo"}, {"CN", "Pechino"},
                // Aggiungi altre capitali in base alle necessità
            };

            return capitals.ContainsKey(countryCode) ? capitals[countryCode] : $"Capitale di {countryCode}";
        }

        // Helper per generare note casuali
        private string GetRandomNote()
        {
            var notes = new List<string>
            {
                "Viaggio fantastico, cultura e paesaggi meravigliosi!",
                "Cibo delizioso e persone molto accoglienti.",
                "Esperienza indimenticabile, tornerò sicuramente.",
                "Architettura straordinaria e storia affascinante.",
                "Spiagge bellissime e mare cristallino."
            };

            Random rnd = new Random();
            return notes[rnd.Next(notes.Count)];
        }

        // Helper per ottenere immagini dei paesi
        private string GetCountryImage(string countryCode)
        {
            return $"/images/destinations/{countryCode.ToLower()}.jpg";
        }
    }

    public class RecentVisitViewModel
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string CityName { get; set; }
        public DateTime VisitDate { get; set; }
        public string Note { get; set; }
        public string ImageUrl { get; set; }
    }

    public class PinViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}