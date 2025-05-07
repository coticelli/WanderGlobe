using WanderGlobe.Models.Custom;
using WanderGlobe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public class TravelJournalService : ITravelJournalService
    {
        private readonly ApplicationDbContext _context;

        public TravelJournalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimelineEntry>> GetTimelineByUserAsync(string userId, string sort = "desc")
        {
            // Simula il recupero della timeline
            var entries = new List<TimelineEntry>
            {
                new TimelineEntry
                {
                    Id = 1,
                    UserId = userId,
                    CountryId = 1,
                    CountryName = "Italia",
                    CountryCode = "IT",
                    CityName = "Roma",
                    VisitDate = DateTime.Now.AddMonths(-2),
                    Notes = "Un viaggio incredibile in una città storica",
                    Photos = new List<TimelinePhoto>
                    {
                        new TimelinePhoto { Id = 1, Url = "/images/sample-photos/photo1.jpg", Caption = "Colosseo", UploadDate = DateTime.Now.AddMonths(-2) },
                        new TimelinePhoto { Id = 2, Url = "/images/sample-photos/photo2.jpg", Caption = "Fontana di Trevi", UploadDate = DateTime.Now.AddMonths(-2).AddDays(1) }
                    },
                    Weather = new TimelineWeather { Month = 5, Temperature = 24, Condition = "Soleggiato" }
                },
                new TimelineEntry
                {
                    Id = 2,
                    UserId = userId,
                    CountryId = 2,
                    CountryName = "Francia",
                    CountryCode = "FR",
                    CityName = "Parigi",
                    VisitDate = DateTime.Now.AddMonths(-5),
                    Notes = "Città romantica e piena di cultura",
                    Photos = new List<TimelinePhoto>
                    {
                        new TimelinePhoto { Id = 3, Url = "/images/sample-photos/photo3.jpg", Caption = "Torre Eiffel", UploadDate = DateTime.Now.AddMonths(-5) }
                    },
                    Weather = new TimelineWeather { Month = 2, Temperature = 8, Condition = "Piovoso" }
                }
            };

            // Ordina in base al parametro
            return sort.ToLower() == "asc"
                ? entries.OrderBy(e => e.VisitDate).ToList()
                : entries.OrderByDescending(e => e.VisitDate).ToList();
        }

        public async Task<List<int>> GetVisitedYearsAsync(string userId)
        {
            // Simula il recupero degli anni delle visite
            return new List<int> { DateTime.Now.Year, DateTime.Now.Year - 1, DateTime.Now.Year - 2 };
        }

        public async Task<bool> AddJournalNoteAsync(TimelineNote note)
        {
            // Simula l'aggiunta di una nota
            Console.WriteLine($"Aggiunta nota per paese {note.CountryId}: {note.Content}");
            return true;
        }

        public async Task<bool> AddPhotoAsync(int countryId, string userId, string caption, string imageUrl)
        {
            // Simula l'aggiunta di una foto
            Console.WriteLine($"Aggiunta foto per paese {countryId}: {caption}");
            return true;
        }
    }
}