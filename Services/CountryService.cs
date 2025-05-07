using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using WanderGlobe.Models;

namespace WanderGlobe.Services
{
    public class CountryService : ICountryService
    {
        private readonly ApplicationDbContext _context;

        public CountryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Country>> GetAllCountriesAsync()
        {
            var countries = await _context.Countries.ToListAsync();

            // Se non ci sono paesi nel database, aggiungiamoli
            if (!countries.Any())
            {
                countries = await SeedCountriesAsync();
            }

            return countries;
        }

        public async Task<List<VisitedCountry>> GetVisitedCountriesByUserAsync(string userId)
        {
            return await _context.VisitedCountries
                .Include(vc => vc.Country)
                .Where(vc => vc.UserId == userId)
                .ToListAsync();
        }

        public async Task<double> GetVisitedPercentageAsync(string userId)
        {
            var visitedCount = await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .CountAsync();

            // Usa un denominatore fisso: numero totale di paesi nel mondo
            // Il numero di paesi riconosciuti dall'ONU è 193
            const int totalCountries = 193;

            return Math.Round((double)visitedCount / totalCountries * 100, 1);
        }

        public async Task AddVisitedCountryAsync(VisitedCountry visitedCountry)
        {
            // Verifica se il paese esiste prima di procedere
            var countryExists = await _context.Countries.AnyAsync(c => c.Id == visitedCountry.CountryId);

            if (!countryExists)
            {
                throw new ArgumentException($"Il paese con ID {visitedCountry.CountryId} non esiste nel database");
            }

            _context.VisitedCountries.Add(visitedCountry);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveVisitedCountryAsync(string userId, int countryId)
        {
            var visitedCountry = await _context.VisitedCountries
                .FirstOrDefaultAsync(vc => vc.UserId == userId && vc.CountryId == countryId);

            if (visitedCountry != null)
            {
                _context.VisitedCountries.Remove(visitedCountry);
                await _context.SaveChangesAsync();
            }
        }

        // Metodo per popolare il database con i paesi principali
        private async Task<List<Country>> SeedCountriesAsync()
        {
            var countries = GetDefaultCountries();

            _context.Countries.AddRange(countries);
            await _context.SaveChangesAsync();

            return countries;
        }

        // Lista di paesi di default per popolare il database
        private List<Country> GetDefaultCountries()
        {
            return new List<Country>
            {
                new Country { Id = 1, Name = "Italia", Code = "IT", Latitude = 41.9028, Longitude = 12.4964 },
                new Country { Id = 2, Name = "Francia", Code = "FR", Latitude = 48.8566, Longitude = 2.3522 },
                new Country { Id = 3, Name = "Regno Unito", Code = "GB", Latitude = 51.5074, Longitude = -0.1278 },
                new Country { Id = 4, Name = "Germania", Code = "DE", Latitude = 52.5200, Longitude = 13.4050 },
                new Country { Id = 5, Name = "Spagna", Code = "ES", Latitude = 40.4168, Longitude = -3.7038 },
                new Country { Id = 6, Name = "Portogallo", Code = "PT", Latitude = 38.7223, Longitude = -9.1393 },
                new Country { Id = 7, Name = "Svizzera", Code = "CH", Latitude = 46.9480, Longitude = 7.4474 },
                new Country { Id = 8, Name = "Austria", Code = "AT", Latitude = 48.2082, Longitude = 16.3738 },
                new Country { Id = 9, Name = "Belgio", Code = "BE", Latitude = 50.8503, Longitude = 4.3517 },
                new Country { Id = 10, Name = "Paesi Bassi", Code = "NL", Latitude = 52.3676, Longitude = 4.9041 },
                new Country { Id = 11, Name = "Stati Uniti", Code = "US", Latitude = 38.9072, Longitude = -77.0369 },
                new Country { Id = 12, Name = "Canada", Code = "CA", Latitude = 45.4215, Longitude = -75.6972 },
                new Country { Id = 13, Name = "Giappone", Code = "JP", Latitude = 35.6762, Longitude = 139.6503 },
                new Country { Id = 14, Name = "Cina", Code = "CN", Latitude = 39.9042, Longitude = 116.4074 },
                new Country { Id = 15, Name = "Australia", Code = "AU", Latitude = -35.2809, Longitude = 149.1300 },
                new Country { Id = 16, Name = "Russia", Code = "RU", Latitude = 55.7558, Longitude = 37.6173 },
                new Country { Id = 17, Name = "Brasile", Code = "BR", Latitude = -15.7801, Longitude = -47.9292 },
                new Country { Id = 18, Name = "India", Code = "IN", Latitude = 28.6139, Longitude = 77.2090 },
                new Country { Id = 19, Name = "Sud Africa", Code = "ZA", Latitude = -25.7461, Longitude = 28.1881 },
                new Country { Id = 20, Name = "Messico", Code = "MX", Latitude = 19.4326, Longitude = -99.1332 },
                new Country { Id = 21, Name = "Argentina", Code = "AR", Latitude = -34.6037, Longitude = -58.3816 },
                new Country { Id = 22, Name = "Egitto", Code = "EG", Latitude = 30.0444, Longitude = 31.2357 },
                new Country { Id = 23, Name = "Grecia", Code = "GR", Latitude = 37.9838, Longitude = 23.7275 },
                new Country { Id = 24, Name = "Svezia", Code = "SE", Latitude = 59.3293, Longitude = 18.0686 },
                new Country { Id = 25, Name = "Norvegia", Code = "NO", Latitude = 59.9139, Longitude = 10.7522 }
                // Puoi aggiungere altri paesi secondo necessità
            };
        }
    }
}