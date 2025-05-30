using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data; // Assuming this is your DbContext namespace
using WanderGlobe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var countries = await _context.Countries.OrderBy(c => c.Name).ToListAsync();

            // Seeding logic should ideally be handled by a dedicated seeder class
            // or run once on application startup, not on every call to GetAllCountriesAsync.
            // For simplicity, keeping it here as per your original structure.
            if (!countries.Any())
            {
                countries = await SeedCountriesAsync(); // This will re-query after seeding
            }

            return countries;
        }

        public async Task<int> GetTotalCountryCountAsync()
        {
            // This could be a fixed number if you're counting against a known total,
            // or the count from your Countries table if that's your definition.
            // Your GetVisitedPercentageAsync uses a fixed 193.
            return await _context.Countries.CountAsync();
        }

        public async Task<List<VisitedCountry>> GetVisitedCountriesByUserAsync(string userId)
        {
            return await _context.VisitedCountries
                .Include(vc => vc.Country) // Eager load the Country navigation property
                .Where(vc => vc.UserId == userId)
                .OrderByDescending(vc => vc.VisitDate) // Optional: order by visit date
                .ToListAsync();
        }

        public async Task<double> GetVisitedPercentageAsync(string userId)
        {
            var visitedCount = await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.CountryId) // Ensure we count distinct countries visited
                .Distinct()
                .CountAsync();

            // Using a fixed denominator for now, as in your original code.
            // Consider if this should be GetTotalCountryCountAsync() from your DB.
            const int totalCountriesInWorld = 193; // Example: UN recognized countries

            if (totalCountriesInWorld == 0) return 0; // Prevent division by zero

            return Math.Round((double)visitedCount / totalCountriesInWorld * 100, 1);
        }

        public async Task AddVisitedCountryAsync(VisitedCountry visitedCountry)
        {
            if (visitedCountry == null)
            {
                throw new ArgumentNullException(nameof(visitedCountry));
            }

            // 1. Verify the country exists.
            var countryExists = await _context.Countries.AnyAsync(c => c.Id == visitedCountry.CountryId);
            if (!countryExists)
            {
                // This exception will be caught by your GlobeModel if you add a city whose country isn't in DB.
                throw new ArgumentException($"Il paese con ID {visitedCountry.CountryId} non esiste nel database. Impossibile registrare la visita.");
            }

            // 2. Check if this exact visit (user + country) already exists.
            // Your GlobeModel is designed to catch ArgumentException for duplicates.
            // This service method should throw it if a duplicate is found and it's not supposed to update.
            var existingVisit = await _context.VisitedCountries
                .FirstOrDefaultAsync(vc => vc.UserId == visitedCountry.UserId && vc.CountryId == visitedCountry.CountryId);

            if (existingVisit != null)
            {
                // If your design is that a user can only visit a country once (and subsequent "adds"
                // might be errors or attempts to update visit date/notes), then throw.
                // Your GlobeModel's OnPostAddCountryAsync catches ArgumentException for this.
                // If you wanted to *update* the existing visit, you'd do it here:
                // existingVisit.VisitDate = visitedCountry.VisitDate;
                // existingVisit.Notes = visitedCountry.Notes;
                // _context.VisitedCountries.Update(existingVisit);
                throw new ArgumentException("Questo paese è già stato visitato e registrato. Per modificare, usa un'altra funzione.");
            }
            else
            {
                // If it's a new visit, add it.
                _context.VisitedCountries.Add(visitedCountry);
            }

            try
            {
                await _context.SaveChangesAsync(); // Persist changes to the database
            }
            catch (DbUpdateException ex)
            {
                // Log the full exception details for better debugging
                // Consider using a proper logging framework like Serilog or NLog
                Console.WriteLine($"DbUpdateException while saving VisitedCountry: {ex.ToString()}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.ToString()}");
                }
                // Re-throw a more user-friendly or generic exception if needed,
                // or let the specific DbUpdateException propagate if your higher layers can handle it.
                throw new Exception("Errore durante il salvataggio della visita nel database. Riprova.", ex);
            }
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
            // No exception if not found, it's idempotent.
        }


        // --- Seeding Logic ---
        // IMPORTANT: Seeding should ideally be done once during application startup,
        // for example, in Program.cs or a dedicated DbInitializer class.
        // Calling it from GetAllCountriesAsync can lead to performance issues and
        // unintended re-seeding or conflicts if not handled carefully.
        // I'm keeping your structure for now but strongly advise refactoring seeding.

        private async Task<List<Country>> SeedCountriesAsync()
        {
            var countries = GetDefaultCountries();

            // Ensure IDs are not explicitly set if the database is generating them.
            // If you are hardcoding IDs, make sure they are unique and don't conflict.
            _context.Countries.AddRange(countries);
            await _context.SaveChangesAsync(); // Save countries to get their DB-generated IDs

            // Now that countries are saved and have IDs, seed cities.
            // Re-fetch countries to ensure you have the ones with DB IDs for relationships.
            var savedCountries = await _context.Countries.ToListAsync();
            await SeedCitiesAsync(savedCountries);

            return savedCountries; // Return the countries from the DB
        }

        private async Task SeedCitiesAsync(List<Country> countriesWithDbIds)
        {
            var citiesToSeed = new List<City>();

            foreach (var country in countriesWithDbIds)
            {
                // GetCapitalCity now needs the country object with its DB-assigned Id
                citiesToSeed.Add(GetCapitalCity(country));
            }

            // For additional cities, you need to find the correct CountryId from countriesWithDbIds
            var additionalCities = GetAdditionalCities(countriesWithDbIds);
            citiesToSeed.AddRange(additionalCities);

            // Avoid adding duplicate cities if this method is called multiple times
            var existingCityNames = await _context.Cities.Select(c => new { c.Name, c.CountryId }).ToListAsync();
            var newCities = citiesToSeed.Where(cityToSeed =>
                !existingCityNames.Any(ec => ec.Name == cityToSeed.Name && ec.CountryId == cityToSeed.CountryId)
            ).ToList();


            if (newCities.Any())
            {
                _context.Cities.AddRange(newCities);
                await _context.SaveChangesAsync();
            }
        }

        private City GetCapitalCity(Country countryWithDbId) // Expects Country object with DB ID
        {
            // Definitions of capitals for each country
            // Ensure countryWithDbId.Id is used for CountryId
            switch (countryWithDbId.Code)
            {
                case "IT":
                    return new City { CountryId = countryWithDbId.Id, Name = "Roma", IsCapital = true, Latitude = 41.9028, Longitude = 12.4964 };
                // ... (rest of your cases, ensuring CountryId = countryWithDbId.Id)
                case "FR":
                    return new City { CountryId = countryWithDbId.Id, Name = "Parigi", IsCapital = true, Latitude = 48.8566, Longitude = 2.3522 };
                case "GB":
                    return new City { CountryId = countryWithDbId.Id, Name = "Londra", IsCapital = true, Latitude = 51.5074, Longitude = -0.1278 };
                case "DE":
                    return new City { CountryId = countryWithDbId.Id, Name = "Berlino", IsCapital = true, Latitude = 52.5200, Longitude = 13.4050 };
                case "ES":
                    return new City { CountryId = countryWithDbId.Id, Name = "Madrid", IsCapital = true, Latitude = 40.4168, Longitude = -3.7038 };
                case "PT":
                    return new City { CountryId = countryWithDbId.Id, Name = "Lisbona", IsCapital = true, Latitude = 38.7223, Longitude = -9.1393 };
                case "CH":
                    return new City { CountryId = countryWithDbId.Id, Name = "Berna", IsCapital = true, Latitude = 46.9480, Longitude = 7.4474 };
                case "AT":
                    return new City { CountryId = countryWithDbId.Id, Name = "Vienna", IsCapital = true, Latitude = 48.2082, Longitude = 16.3738 };
                case "BE":
                    return new City { CountryId = countryWithDbId.Id, Name = "Bruxelles", IsCapital = true, Latitude = 50.8503, Longitude = 4.3517 };
                case "NL":
                    return new City { CountryId = countryWithDbId.Id, Name = "Amsterdam", IsCapital = true, Latitude = 52.3676, Longitude = 4.9041 };
                case "US":
                    return new City { CountryId = countryWithDbId.Id, Name = "Washington D.C.", IsCapital = true, Latitude = 38.9072, Longitude = -77.0369 };
                case "CA":
                    return new City { CountryId = countryWithDbId.Id, Name = "Ottawa", IsCapital = true, Latitude = 45.4215, Longitude = -75.6972 };
                case "JP":
                    return new City { CountryId = countryWithDbId.Id, Name = "Tokyo", IsCapital = true, Latitude = 35.6762, Longitude = 139.6503 };
                case "CN":
                    return new City { CountryId = countryWithDbId.Id, Name = "Pechino", IsCapital = true, Latitude = 39.9042, Longitude = 116.4074 };
                case "AU":
                    return new City { CountryId = countryWithDbId.Id, Name = "Canberra", IsCapital = true, Latitude = -35.2809, Longitude = 149.1300 };
                case "RU":
                    return new City { CountryId = countryWithDbId.Id, Name = "Mosca", IsCapital = true, Latitude = 55.7558, Longitude = 37.6173 };
                case "BR":
                    return new City { CountryId = countryWithDbId.Id, Name = "Brasilia", IsCapital = true, Latitude = -15.7801, Longitude = -47.9292 };
                case "IN":
                    return new City { CountryId = countryWithDbId.Id, Name = "Nuova Delhi", IsCapital = true, Latitude = 28.6139, Longitude = 77.2090 };
                case "ZA":
                    return new City { CountryId = countryWithDbId.Id, Name = "Pretoria", IsCapital = true, Latitude = -25.7461, Longitude = 28.1881 };
                case "MX":
                    return new City { CountryId = countryWithDbId.Id, Name = "Città del Messico", IsCapital = true, Latitude = 19.4326, Longitude = -99.1332 };
                case "AR":
                    return new City { CountryId = countryWithDbId.Id, Name = "Buenos Aires", IsCapital = true, Latitude = -34.6037, Longitude = -58.3816 };
                case "EG":
                    return new City { CountryId = countryWithDbId.Id, Name = "Il Cairo", IsCapital = true, Latitude = 30.0444, Longitude = 31.2357 };
                case "GR":
                    return new City { CountryId = countryWithDbId.Id, Name = "Atene", IsCapital = true, Latitude = 37.9838, Longitude = 23.7275 };
                case "SE":
                    return new City { CountryId = countryWithDbId.Id, Name = "Stoccolma", IsCapital = true, Latitude = 59.3293, Longitude = 18.0686 };
                case "NO":
                    return new City { CountryId = countryWithDbId.Id, Name = "Oslo", IsCapital = true, Latitude = 59.9139, Longitude = 10.7522 };
                default:
                    // Fallback for countries not explicitly listed, using the country's own lat/lng
                    return new City { CountryId = countryWithDbId.Id, Name = $"Capitale di {countryWithDbId.Name}", IsCapital = true, Latitude = countryWithDbId.Latitude, Longitude = countryWithDbId.Longitude };
            }
        }

        private List<City> GetAdditionalCities(List<Country> countriesWithDbIds)
        {
            var additionalCities = new List<City>();
            // Helper to find country by code and get its DB ID
            Func<string, int?> getCountryIdByCode = (code) =>
                countriesWithDbIds.FirstOrDefault(c => c.Code == code)?.Id;

            int? italyId = getCountryIdByCode("IT");
            if (italyId.HasValue)
            {
                additionalCities.Add(new City { CountryId = italyId.Value, Name = "Milano", IsCapital = false, Latitude = 45.4642, Longitude = 9.1900 });
                additionalCities.Add(new City { CountryId = italyId.Value, Name = "Napoli", IsCapital = false, Latitude = 40.8518, Longitude = 14.2681 });
                // ... more Italian cities
            }

            int? franceId = getCountryIdByCode("FR");
            if (franceId.HasValue)
            {
                additionalCities.Add(new City { CountryId = franceId.Value, Name = "Marsiglia", IsCapital = false, Latitude = 43.2965, Longitude = 5.3698 });
                // ... more French cities
            }

            int? spainId = getCountryIdByCode("ES");
            if (spainId.HasValue)
            {
                additionalCities.Add(new City { CountryId = spainId.Value, Name = "Barcellona", IsCapital = false, Latitude = 41.3851, Longitude = 2.1734 });
                // ... more Spanish cities
            }

            int? usaId = getCountryIdByCode("US");
            if (usaId.HasValue)
            {
                additionalCities.Add(new City { CountryId = usaId.Value, Name = "New York", IsCapital = false, Latitude = 40.7128, Longitude = -74.0060 });
                // ... more US cities
            }
            // Add other countries and their non-capital cities similarly
            // ... (rest of your GetAdditionalCities logic, ensuring you use the correct CountryId from countriesWithDbIds)

            return additionalCities;
        }

        private List<Country> GetDefaultCountries()
        {
            // Your existing list of countries.
            // IMPORTANT: If your database auto-generates IDs, you should NOT set the Id property here.
            // EF Core will handle it. If you ARE setting IDs, ensure they are unique.
            // For this example, I'll assume DB generates IDs, so I'll remove Id = ...
            return new List<Country>
            {
                new Country { /*Id = 1,*/ Name = "Italia", Code = "IT", Continent = "Europa", Latitude = 41.9028, Longitude = 12.4964 },
                new Country { /*Id = 2,*/ Name = "Francia", Code = "FR", Continent = "Europa", Latitude = 48.8566, Longitude = 2.3522 },
                new Country { /*Id = 3,*/ Name = "Regno Unito", Code = "GB", Continent = "Europa", Latitude = 51.5074, Longitude = -0.1278 },
                new Country { /*Id = 4,*/ Name = "Germania", Code = "DE", Continent = "Europa", Latitude = 52.5200, Longitude = 13.4050 },
                new Country { /*Id = 5,*/ Name = "Spagna", Code = "ES", Continent = "Europa", Latitude = 40.4168, Longitude = -3.7038 },
                new Country { /*Id = 6,*/ Name = "Portogallo", Code = "PT", Continent = "Europa", Latitude = 38.7223, Longitude = -9.1393 },
                new Country { /*Id = 7,*/ Name = "Svizzera", Code = "CH", Continent = "Europa", Latitude = 46.9480, Longitude = 7.4474 },
                new Country { /*Id = 8,*/ Name = "Austria", Code = "AT", Continent = "Europa", Latitude = 48.2082, Longitude = 16.3738 },
                new Country { /*Id = 9,*/ Name = "Belgio", Code = "BE", Continent = "Europa", Latitude = 50.8503, Longitude = 4.3517 },
                new Country { /*Id = 10,*/ Name = "Paesi Bassi", Code = "NL", Continent = "Europa", Latitude = 52.3676, Longitude = 4.9041 },
                new Country { /*Id = 11,*/ Name = "Stati Uniti", Code = "US", Continent = "Nord America", Latitude = 38.9072, Longitude = -77.0369 },
                new Country { /*Id = 12,*/ Name = "Canada", Code = "CA", Continent = "Nord America", Latitude = 45.4215, Longitude = -75.6972 },
                new Country { /*Id = 13,*/ Name = "Giappone", Code = "JP", Continent = "Asia", Latitude = 35.6762, Longitude = 139.6503 },
                new Country { /*Id = 14,*/ Name = "Cina", Code = "CN", Continent = "Asia", Latitude = 39.9042, Longitude = 116.4074 },
                new Country { /*Id = 15,*/ Name = "Australia", Code = "AU", Continent = "Oceania", Latitude = -35.2809, Longitude = 149.1300 },
                new Country { /*Id = 16,*/ Name = "Russia", Code = "RU", Continent = "Europa/Asia", Latitude = 55.7558, Longitude = 37.6173 },
                new Country { /*Id = 17,*/ Name = "Brasile", Code = "BR", Continent = "Sud America", Latitude = -15.7801, Longitude = -47.9292 },
                new Country { /*Id = 18,*/ Name = "India", Code = "IN", Continent = "Asia", Latitude = 28.6139, Longitude = 77.2090 },
                new Country { /*Id = 19,*/ Name = "Sud Africa", Code = "ZA", Continent = "Africa", Latitude = -25.7461, Longitude = 28.1881 },
                new Country { /*Id = 20,*/ Name = "Messico", Code = "MX", Continent = "Nord America", Latitude = 19.4326, Longitude = -99.1332 },
                new Country { /*Id = 21,*/ Name = "Argentina", Code = "AR", Continent = "Sud America", Latitude = -34.6037, Longitude = -58.3816 },
                new Country { /*Id = 22,*/ Name = "Egitto", Code = "EG", Continent = "Africa", Latitude = 30.0444, Longitude = 31.2357 },
                new Country { /*Id = 23,*/ Name = "Grecia", Code = "GR", Continent = "Europa", Latitude = 37.9838, Longitude = 23.7275 },
                new Country { /*Id = 24,*/ Name = "Svezia", Code = "SE", Continent = "Europa", Latitude = 59.3293, Longitude = 18.0686 },
                new Country { /*Id = 25,*/ Name = "Norvegia", Code = "NO", Continent = "Europa", Latitude = 59.9139, Longitude = 10.7522 }
            };
        }
    }
}