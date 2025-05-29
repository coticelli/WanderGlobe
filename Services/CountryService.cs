using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using WanderGlobe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Recommended for logging

namespace WanderGlobe.Services
{
    public class CountryService : ICountryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CountryService> _logger; // Recommended for better logging

        public CountryService(ApplicationDbContext context, ILogger<CountryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Country>> GetAllCountriesAsync()
        {
            var countries = await _context.Countries.OrderBy(c => c.Name).ToListAsync();

            // Seeding logic should ideally be run once at application startup,
            // not every time this method is called if the DB is empty.
            // Consider moving seeding to Program.cs or a dedicated seeder class.
            if (!countries.Any())
            {
                _logger.LogInformation("No countries found in the database. Attempting to seed countries and cities.");
                countries = await SeedCountriesAsync(); // This will also seed cities
            }

            return countries;
        }

        public async Task<int> GetTotalCountryCountAsync()
        {
            // This should return the count of all distinct countries in your database,
            // or a predefined number if you prefer.
            // For consistency with VisitedPercentage, if you use 193 there, consider that.
            // However, dynamically counting from your DB is more flexible.
            return await _context.Countries.CountAsync();
        }

        public async Task<List<VisitedCountry>> GetVisitedCountriesByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<VisitedCountry>();
            }
            return await _context.VisitedCountries
                .Include(vc => vc.Country) // Eager load the Country details
                .Where(vc => vc.UserId == userId)
                .OrderByDescending(vc => vc.VisitDate) // Good for display
                .ToListAsync();
        }

        public async Task<double> GetVisitedPercentageAsync(string userId)
        {
            var visitedCount = await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.CountryId) // Count distinct countries visited
                .Distinct()
                .CountAsync();

            var totalCountriesInDb = await _context.Countries.CountAsync();
            if (totalCountriesInDb == 0) return 0.0; // Avoid division by zero

            // Using total countries in DB for percentage, or use a fixed number like 193
            // const int recognizedWorldCountries = 193;
            // return Math.Round((double)visitedCount / recognizedWorldCountries * 100, 1);
            return Math.Round((double)visitedCount / totalCountriesInDb * 100, 1);
        }

        public async Task AddVisitedCountryAsync(VisitedCountry visitedCountry)
        {
            if (visitedCountry == null)
            {
                _logger.LogError("AddVisitedCountryAsync called with null visitedCountry object.");
                throw new ArgumentNullException(nameof(visitedCountry));
            }
            if (string.IsNullOrEmpty(visitedCountry.UserId))
            {
                 _logger.LogError("AddVisitedCountryAsync: UserId is null or empty.");
                throw new ArgumentException("UserId cannot be null or empty.", nameof(visitedCountry.UserId));
            }
            if (visitedCountry.CountryId <= 0)
            {
                _logger.LogError("AddVisitedCountryAsync: CountryId is invalid ({CountryId}).", visitedCountry.CountryId);
                throw new ArgumentException("CountryId must be a valid ID.", nameof(visitedCountry.CountryId));
            }


            // 1. Check if the country itself exists in the Countries table
            var countryExists = await _context.Countries.AnyAsync(c => c.Id == visitedCountry.CountryId);
            if (!countryExists)
            {
                _logger.LogWarning("Attempted to add visit for non-existent CountryId: {CountryId}", visitedCountry.CountryId);
                // This exception will be caught by GlobeModel and shown to the user if it's related to "Città non trovata" logic
                // or if the city's CountryId is somehow wrong.
                throw new ArgumentException($"Il paese con ID {visitedCountry.CountryId} non esiste nel database. Impossibile registrare la visita.");
            }

            // 2. Check if this specific visit (User + Country) already exists
            var existingVisit = await _context.VisitedCountries
                .FirstOrDefaultAsync(vc => vc.UserId == visitedCountry.UserId && vc.CountryId == visitedCountry.CountryId);

            if (existingVisit != null)
            {
                // If you want to allow updating an existing visit (e.g., change date or notes)
                // existingVisit.VisitDate = visitedCountry.VisitDate;
                // existingVisit.Notes = visitedCountry.Notes;
                // _context.VisitedCountries.Update(existingVisit);
                // _logger.LogInformation("Updating existing visit for User: {UserId}, Country: {CountryId}", visitedCountry.UserId, visitedCountry.CountryId);

                // If you want to strictly prevent adding if it exists and let PageModel handle the message:
                _logger.LogWarning("Attempted to add duplicate visit for User: {UserId}, Country: {CountryId}", visitedCountry.UserId, visitedCountry.CountryId);
                throw new ArgumentException("Questo paese è già stato visitato e registrato.");
            }
            else
            {
                _context.VisitedCountries.Add(visitedCountry);
                _logger.LogInformation("Adding new visit for User: {UserId}, Country: {CountryId}", visitedCountry.UserId, visitedCountry.CountryId);
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully saved changes for VisitedCountries.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving VisitedCountry to database. User: {UserId}, Country: {CountryId}", visitedCountry.UserId, visitedCountry.CountryId);
                // You might want to inspect ex.InnerException for more details (e.g., constraint violations)
                throw new Exception("Errore durante il salvataggio della visita nel database. Controlla i log per i dettagli.", ex);
            }
            catch (Exception ex) // Catch other potential errors during save
            {
                _logger.LogError(ex, "An unexpected error occurred while saving VisitedCountry. User: {UserId}, Country: {CountryId}", visitedCountry.UserId, visitedCountry.CountryId);
                throw; // Re-throw the original exception
            }
        }

        public async Task RemoveVisitedCountryAsync(string userId, int countryId)
        {
            if (string.IsNullOrEmpty(userId) || countryId <= 0)
            {
                _logger.LogWarning("RemoveVisitedCountryAsync called with invalid parameters. UserId: {UserId}, CountryId: {CountryId}", userId, countryId);
                return; // Or throw ArgumentException
            }

            var visitedCountry = await _context.VisitedCountries
                .FirstOrDefaultAsync(vc => vc.UserId == userId && vc.CountryId == countryId);

            if (visitedCountry != null)
            {
                _context.VisitedCountries.Remove(visitedCountry);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully removed visit for User: {UserId}, Country: {CountryId}", userId, countryId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error removing VisitedCountry from database. User: {UserId}, Country: {CountryId}", userId, countryId);
                    throw new Exception("Errore durante la rimozione della visita dal database.", ex);
                }
            }
            else
            {
                _logger.LogWarning("Attempted to remove a non-existent visit. User: {UserId}, Country: {CountryId}", userId, countryId);
            }
        }

        // --- SEEDING METHODS ---
        // IMPORTANT: Seeding should ideally be done ONCE during application startup.
        // Calling it from GetAllCountriesAsync can lead to repeated attempts and potential issues
        // if not handled carefully, especially with IDs.
        // Consider moving this to Program.cs or a dedicated seeder class that runs on startup.
        private async Task<List<Country>> SeedCountriesAsync()
        {
            _logger.LogInformation("Executing SeedCountriesAsync.");
            // Check if countries already exist to prevent duplication if this method is called multiple times
            if (await _context.Countries.AnyAsync())
            {
                _logger.LogInformation("Countries already exist in the database. Skipping country seeding.");
                return await _context.Countries.ToListAsync(); // Return existing countries
            }

            var countries = GetDefaultCountries();
            _context.Countries.AddRange(countries);

            try
            {
                // Save countries first to get their DB-generated IDs (if not manually set and PK is identity)
                // If IDs are manually set as in GetDefaultCountries, this is less critical for CountryId in City
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully seeded {Count} countries.", countries.Count);

                // Now that countries are saved (and have IDs if auto-generated), seed cities
                await SeedCitiesAsync(countries); // Pass the list of *saved* countries
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database seeding (Countries).");
                // Handle or rethrow, ensure data consistency
            }
            return countries; // Return the newly seeded countries
        }

        private async Task SeedCitiesAsync(List<Country> countriesInDb)
        {
            _logger.LogInformation("Executing SeedCitiesAsync.");
            if (await _context.Cities.AnyAsync())
            {
                _logger.LogInformation("Cities already exist in the database. Skipping city seeding.");
                return;
            }

            var citiesToAdd = new List<City>();

            foreach (var country in countriesInDb) // Iterate over countries that are confirmed in DB
            {
                // Find the matching country from the input list (which should have DB Id if seeding was correct)
                // This ensures we use the correct CountryId for the City.
                var dbCountry = _context.Countries.Local.FirstOrDefault(c => c.Code == country.Code) // Check local cache first
                                ?? await _context.Countries.FirstOrDefaultAsync(c => c.Code == country.Code); // Then query DB

                if (dbCountry == null)
                {
                    _logger.LogWarning("SeedCitiesAsync: Could not find country with code {CountryCode} in the database to associate with its capital. Skipping its capital.", country.Code);
                    continue;
                }

                var capital = GetCapitalCity(dbCountry); // Pass the country object with the DB ID
                if (capital != null) citiesToAdd.Add(capital);
            }

            // Add additional cities, ensuring their CountryId references existing countries
            var additionalCities = GetAdditionalCities();
            foreach (var city in additionalCities)
            {
                 // Find the country for this additional city by its temporary Id used in GetAdditionalCities
                // This mapping needs to be robust. Best to use CountryCode if available.
                // The current GetAdditionalCities uses hardcoded CountryId = 1, 2, etc. which matches GetDefaultCountries ID
                var countryForCity = countriesInDb.FirstOrDefault(c => c.Id == city.CountryId);
                if (countryForCity != null)
                {
                    city.CountryId = countryForCity.Id; // Ensure it uses the actual DB ID
                    citiesToAdd.Add(city);
                }
                else
                {
                    _logger.LogWarning("SeedCitiesAsync: Could not find country with Id {CountryId} for additional city {CityName}. Skipping this city.", city.CountryId, city.Name);
                }
            }

            if (citiesToAdd.Any())
            {
                _context.Cities.AddRange(citiesToAdd);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully seeded {Count} cities.", citiesToAdd.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during database seeding (Cities).");
                }
            }
            else
            {
                 _logger.LogInformation("No new cities to seed.");
            }
        }

        private City? GetCapitalCity(Country country) // country here should have its DB Id
        {
            if (country == null || country.Id == 0) return null; // Ensure country has a valid DB ID
            switch (country.Code)
            {
                case "IT": return new City { CountryId = country.Id, Name = "Roma", IsCapital = true, Latitude = 41.9028, Longitude = 12.4964 };
                case "FR": return new City { CountryId = country.Id, Name = "Parigi", IsCapital = true, Latitude = 48.8566, Longitude = 2.3522 };
                // ... (add all your cases, ensuring CountryId = country.Id)
                case "GB": return new City { CountryId = country.Id, Name = "Londra", IsCapital = true, Latitude = 51.5074, Longitude = -0.1278 };
                case "DE": return new City { CountryId = country.Id, Name = "Berlino", IsCapital = true, Latitude = 52.5200, Longitude = 13.4050 };
                case "ES": return new City { CountryId = country.Id, Name = "Madrid", IsCapital = true, Latitude = 40.4168, Longitude = -3.7038 };
                case "PT": return new City { CountryId = country.Id, Name = "Lisbona", IsCapital = true, Latitude = 38.7223, Longitude = -9.1393 };
                case "CH": return new City { CountryId = country.Id, Name = "Berna", IsCapital = true, Latitude = 46.9480, Longitude = 7.4474 };
                case "AT": return new City { CountryId = country.Id, Name = "Vienna", IsCapital = true, Latitude = 48.2082, Longitude = 16.3738 };
                case "BE": return new City { CountryId = country.Id, Name = "Bruxelles", IsCapital = true, Latitude = 50.8503, Longitude = 4.3517 };
                case "NL": return new City { CountryId = country.Id, Name = "Amsterdam", IsCapital = true, Latitude = 52.3676, Longitude = 4.9041 };
                case "US": return new City { CountryId = country.Id, Name = "Washington D.C.", IsCapital = true, Latitude = 38.9072, Longitude = -77.0369 };
                case "CA": return new City { CountryId = country.Id, Name = "Ottawa", IsCapital = true, Latitude = 45.4215, Longitude = -75.6972 };
                case "JP": return new City { CountryId = country.Id, Name = "Tokyo", IsCapital = true, Latitude = 35.6762, Longitude = 139.6503 };
                case "CN": return new City { CountryId = country.Id, Name = "Pechino", IsCapital = true, Latitude = 39.9042, Longitude = 116.4074 };
                case "AU": return new City { CountryId = country.Id, Name = "Canberra", IsCapital = true, Latitude = -35.2809, Longitude = 149.1300 };
                case "RU": return new City { CountryId = country.Id, Name = "Mosca", IsCapital = true, Latitude = 55.7558, Longitude = 37.6173 };
                case "BR": return new City { CountryId = country.Id, Name = "Brasilia", IsCapital = true, Latitude = -15.7801, Longitude = -47.9292 };
                case "IN": return new City { CountryId = country.Id, Name = "Nuova Delhi", IsCapital = true, Latitude = 28.6139, Longitude = 77.2090 };
                case "ZA": return new City { CountryId = country.Id, Name = "Pretoria", IsCapital = true, Latitude = -25.7461, Longitude = 28.1881 };
                case "MX": return new City { CountryId = country.Id, Name = "Città del Messico", IsCapital = true, Latitude = 19.4326, Longitude = -99.1332 };
                case "AR": return new City { CountryId = country.Id, Name = "Buenos Aires", IsCapital = true, Latitude = -34.6037, Longitude = -58.3816 };
                case "EG": return new City { CountryId = country.Id, Name = "Il Cairo", IsCapital = true, Latitude = 30.0444, Longitude = 31.2357 };
                case "GR": return new City { CountryId = country.Id, Name = "Atene", IsCapital = true, Latitude = 37.9838, Longitude = 23.7275 };
                case "SE": return new City { CountryId = country.Id, Name = "Stoccolma", IsCapital = true, Latitude = 59.3293, Longitude = 18.0686 };
                case "NO": return new City { CountryId = country.Id, Name = "Oslo", IsCapital = true, Latitude = 59.9139, Longitude = 10.7522 };
                default:
                    _logger.LogWarning("No specific capital defined for country code {CountryCode}. Using generic capital.", country.Code);
                    return new City { CountryId = country.Id, Name = $"Capitale di {country.Name}", IsCapital = true, Latitude = country.Latitude, Longitude = country.Longitude };
            }
        }

        private List<City> GetAdditionalCities()
        {
            // IMPORTANT: The CountryId here is a TEMPORARY ID that MUST match the Id you give
            // in GetDefaultCountries. This is fragile. A better approach is to look up
            // the Country by Code after GetDefaultCountries are saved, then use its real DB ID.
            // For now, I'll assume the IDs (1 for IT, 5 for ES, etc.) match GetDefaultCountries.
            return new List<City>
            {
                new City { CountryId = 1, Name = "Milano", IsCapital = false, Latitude = 45.4642, Longitude = 9.1900 },
                new City { CountryId = 1, Name = "Napoli", IsCapital = false, Latitude = 40.8518, Longitude = 14.2681 },
                new City { CountryId = 1, Name = "Firenze", IsCapital = false, Latitude = 43.7696, Longitude = 11.2558 },
                new City { CountryId = 1, Name = "Venezia", IsCapital = false, Latitude = 45.4408, Longitude = 12.3155 },
                new City { CountryId = 2, Name = "Marsiglia", IsCapital = false, Latitude = 43.2965, Longitude = 5.3698 },
                new City { CountryId = 2, Name = "Lione", IsCapital = false, Latitude = 45.7640, Longitude = 4.8357 },
                new City { CountryId = 2, Name = "Nizza", IsCapital = false, Latitude = 43.7102, Longitude = 7.2620 },
                new City { CountryId = 3, Name = "Manchester", IsCapital = false, Latitude = 53.4808, Longitude = -2.2426 },
                new City { CountryId = 3, Name = "Liverpool", IsCapital = false, Latitude = 53.4084, Longitude = -2.9916 },
                new City { CountryId = 3, Name = "Edimburgo", IsCapital = false, Latitude = 55.9533, Longitude = -3.1883 },
                new City { CountryId = 4, Name = "Monaco", IsCapital = false, Latitude = 48.1351, Longitude = 11.5820 },
                new City { CountryId = 4, Name = "Amburgo", IsCapital = false, Latitude = 53.5511, Longitude = 9.9937 },
                new City { CountryId = 4, Name = "Francoforte", IsCapital = false, Latitude = 50.1109, Longitude = 8.6821 },
                new City { CountryId = 5, Name = "Barcellona", IsCapital = false, Latitude = 41.3851, Longitude = 2.1734 }, // Spain
                new City { CountryId = 5, Name = "Valencia", IsCapital = false, Latitude = 39.4699, Longitude = -0.3763 },
                new City { CountryId = 5, Name = "Siviglia", IsCapital = false, Latitude = 37.3891, Longitude = -5.9845 },
                new City { CountryId = 11, Name = "New York", IsCapital = false, Latitude = 40.7128, Longitude = -74.0060 },
                new City { CountryId = 11, Name = "Los Angeles", IsCapital = false, Latitude = 34.0522, Longitude = -118.2437 },
                new City { CountryId = 11, Name = "Chicago", IsCapital = false, Latitude = 41.8781, Longitude = -87.6298 },
                new City { CountryId = 11, Name = "San Francisco", IsCapital = false, Latitude = 37.7749, Longitude = -122.4194 }
            };
        }

        private List<Country> GetDefaultCountries()
        {
            // IDs are explicitly set here. Make sure your DB schema allows this (i.e., PK is not identity if you set it)
            // Or, if PK is identity, remove the Id assignments and let the DB generate them.
            // If DB generates them, the SeedCitiesAsync logic needs to fetch the saved countries to get their IDs.
            return new List<Country>
            {
                new Country { Id = 1, Name = "Italia", Code = "IT", Continent = "Europa", Latitude = 41.9028, Longitude = 12.4964 },
                new Country { Id = 2, Name = "Francia", Code = "FR", Continent = "Europa", Latitude = 48.8566, Longitude = 2.3522 },
                new Country { Id = 3, Name = "Regno Unito", Code = "GB", Continent = "Europa", Latitude = 51.5074, Longitude = -0.1278 },
                new Country { Id = 4, Name = "Germania", Code = "DE", Continent = "Europa", Latitude = 52.5200, Longitude = 13.4050 },
                new Country { Id = 5, Name = "Spagna", Code = "ES", Continent = "Europa", Latitude = 40.4168, Longitude = -3.7038 },
                // ... (rest of your countries, ensuring IDs are unique if manually set)
                new Country { Id = 6, Name = "Portogallo", Code = "PT", Continent = "Europa", Latitude = 38.7223, Longitude = -9.1393 },
                new Country { Id = 7, Name = "Svizzera", Code = "CH", Continent = "Europa", Latitude = 46.9480, Longitude = 7.4474 },
                new Country { Id = 8, Name = "Austria", Code = "AT", Continent = "Europa", Latitude = 48.2082, Longitude = 16.3738 },
                new Country { Id = 9, Name = "Belgio", Code = "BE", Continent = "Europa", Latitude = 50.8503, Longitude = 4.3517 },
                new Country { Id = 10, Name = "Paesi Bassi", Code = "NL", Continent = "Europa", Latitude = 52.3676, Longitude = 4.9041 },
                new Country { Id = 11, Name = "Stati Uniti", Code = "US", Continent = "Nord America", Latitude = 38.9072, Longitude = -77.0369 },
                new Country { Id = 12, Name = "Canada", Code = "CA", Continent = "Nord America", Latitude = 45.4215, Longitude = -75.6972 },
                new Country { Id = 13, Name = "Giappone", Code = "JP", Continent = "Asia", Latitude = 35.6762, Longitude = 139.6503 },
                new Country { Id = 14, Name = "Cina", Code = "CN", Continent = "Asia", Latitude = 39.9042, Longitude = 116.4074 },
                new Country { Id = 15, Name = "Australia", Code = "AU", Continent = "Oceania", Latitude = -35.2809, Longitude = 149.1300 },
                new Country { Id = 16, Name = "Russia", Code = "RU", Continent = "Europa/Asia", Latitude = 55.7558, Longitude = 37.6173 },
                new Country { Id = 17, Name = "Brasile", Code = "BR", Continent = "Sud America", Latitude = -15.7801, Longitude = -47.9292 },
                new Country { Id = 18, Name = "India", Code = "IN", Continent = "Asia", Latitude = 28.6139, Longitude = 77.2090 },
                new Country { Id = 19, Name = "Sud Africa", Code = "ZA", Continent = "Africa", Latitude = -25.7461, Longitude = 28.1881 },
                new Country { Id = 20, Name = "Messico", Code = "MX", Continent = "Nord America", Latitude = 19.4326, Longitude = -99.1332 },
                new Country { Id = 21, Name = "Argentina", Code = "AR", Continent = "Sud America", Latitude = -34.6037, Longitude = -58.3816 },
                new Country { Id = 22, Name = "Egitto", Code = "EG", Continent = "Africa", Latitude = 30.0444, Longitude = 31.2357 },
                new Country { Id = 23, Name = "Grecia", Code = "GR", Continent = "Europa", Latitude = 37.9838, Longitude = 23.7275 },
                new Country { Id = 24, Name = "Svezia", Code = "SE", Continent = "Europa", Latitude = 59.3293, Longitude = 18.0686 },
                new Country { Id = 25, Name = "Norvegia", Code = "NO", Continent = "Europa", Latitude = 59.9139, Longitude = 10.7522 }
            };
        }
    }
}