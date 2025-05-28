using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data; // Assuming ApplicationDbContext is here
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
        private readonly ILogger<CountryService> _logger; // Recommended

        public CountryService(ApplicationDbContext context, ILogger<CountryService> logger)
        {
            _context = context;
            _logger = logger; // Initialize logger
        }

        public async Task<List<Country>> GetAllCountriesAsync()
        {
            var countries = await _context.Countries.OrderBy(c => c.Name).ToListAsync();

            // Seeding logic should ideally be done once during application startup,
            // for example, in Program.cs or a dedicated DbInitializer class.
            // Calling it here on every request if the table is empty can be inefficient
            // and lead to issues if multiple requests hit this simultaneously before seeding completes.
            if (!countries.Any())
            {
                _logger.LogInformation("Countries table is empty. Seeding default countries and cities.");
                countries = await SeedCountriesAsync(); // This will also seed cities
            }

            return countries;
        }

        public async Task<int> GetTotalCountryCountAsync()
        {
            // This counts countries in your DB. If you want a fixed "world" count,
            // you might return a constant or fetch from a configuration.
            return await _context.Countries.CountAsync();
        }

        public async Task<List<VisitedCountry>> GetVisitedCountriesByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<VisitedCountry>(); // Or throw ArgumentNullException
            }
            return await _context.VisitedCountries
                .Include(vc => vc.Country) // Ensure Country details are loaded
                .Where(vc => vc.UserId == userId)
                .OrderByDescending(vc => vc.VisitDate) // Often useful to order by visit date
                .ToListAsync();
        }

        public async Task<double> GetVisitedPercentageAsync(string userId)
        {
            var visitedCount = await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.CountryId) // Count distinct countries visited
                .Distinct()
                .CountAsync();

            var totalCountriesInDb = await GetTotalCountryCountAsync();
            if (totalCountriesInDb == 0) return 0; // Avoid division by zero

            // If you want percentage based on a fixed number (e.g., 193 UN countries)
            // const int totalWorldCountries = 193;
            // return Math.Round((double)visitedCount / totalWorldCountries * 100, 1);

            // Current logic is based on total countries in your DB
            return Math.Round((double)visitedCount / totalCountriesInDb * 100, 1);
        }

        public async Task AddVisitedCountryAsync(VisitedCountry visitedCountry)
        {
            if (visitedCountry == null)
            {
                throw new ArgumentNullException(nameof(visitedCountry));
            }

            // 1. Verify the country itself exists
            var countryExists = await _context.Countries.AnyAsync(c => c.Id == visitedCountry.CountryId);
            if (!countryExists)
            {
                _logger.LogError($"Attempted to add visit for non-existent CountryId: {visitedCountry.CountryId}");
                throw new ArgumentException($"Il paese con ID {visitedCountry.CountryId} non esiste nel database.");
            }

            // 2. Check if this specific visit (user + country) already exists
            var existingVisit = await _context.VisitedCountries
                .FirstOrDefaultAsync(vc => vc.UserId == visitedCountry.UserId && vc.CountryId == visitedCountry.CountryId);

            if (existingVisit != null)
            {
                // The country has already been visited by this user.
                // Your PageModel catches ArgumentException for this.
                // You could also choose to update the existingVisit.Notes or existingVisit.VisitDate here
                // existingVisit.VisitDate = visitedCountry.VisitDate;
                // existingVisit.Notes = visitedCountry.Notes;
                // _context.VisitedCountries.Update(existingVisit);
                _logger.LogWarning($"Attempted to re-add an existing visited country. UserId: {visitedCountry.UserId}, CountryId: {visitedCountry.CountryId}");
                throw new ArgumentException("Questo paese è già stato registrato come visitato.");
            }
            else
            {
                // 3. Add the new visit
                _context.VisitedCountries.Add(visitedCountry);
            }

            // 4. Save changes to the database
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully added visited country. UserId: {visitedCountry.UserId}, CountryId: {visitedCountry.CountryId}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database update error while adding visited country. UserId: {visitedCountry.UserId}, CountryId: {visitedCountry.CountryId}");
                // This could happen if there's a database-level constraint (e.g., unique index)
                // that wasn't caught by the C# logic, or other DB issues.
                throw new Exception("Errore durante il salvataggio della visita nel database. Controlla i log per i dettagli.", ex);
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                _logger.LogError(ex, $"Unexpected error while adding visited country. UserId: {visitedCountry.UserId}, CountryId: {visitedCountry.CountryId}");
                throw; // Re-throw the original exception
            }
        }

        public async Task RemoveVisitedCountryAsync(string userId, int countryId)
        {
            var visitedCountry = await _context.VisitedCountries
                .FirstOrDefaultAsync(vc => vc.UserId == userId && vc.CountryId == countryId);

            if (visitedCountry != null)
            {
                _context.VisitedCountries.Remove(visitedCountry);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully removed visited country. UserId: {userId}, CountryId: {countryId}");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, $"Database update error while removing visited country. UserId: {userId}, CountryId: {countryId}");
                    throw new Exception("Errore durante la rimozione della visita dal database.", ex);
                }
            }
            else
            {
                _logger.LogWarning($"Attempted to remove a non-existent visited country. UserId: {userId}, CountryId: {countryId}");
                // Optionally, you could throw an exception or just do nothing if it's not found.
            }
        }

        // --- Seeding Methods ---
        // It's generally better to have seeding as a one-time setup process
        // (e.g., in Program.cs after context creation or using a dedicated DbInitializer class)
        // rather than in a service method called on every request if data is missing.
        // This avoids race conditions and performance overhead.

        private async Task<List<Country>> SeedCountriesAsync()
        {
            var countries = GetDefaultCountries();

            // Check if countries already exist by Code to avoid duplicates if this method is called multiple times
            var existingCountryCodes = await _context.Countries.Select(c => c.Code).ToListAsync();
            var countriesToAdd = countries.Where(c => !existingCountryCodes.Contains(c.Code)).ToList();

            if (countriesToAdd.Any())
            {
                _context.Countries.AddRange(countriesToAdd);
                await _context.SaveChangesAsync(); // Save countries to get their IDs
                _logger.LogInformation($"Seeded {countriesToAdd.Count} new countries.");

                // Seed cities for the newly added countries
                await SeedCitiesAsync(countriesToAdd); // Pass only newly added countries
            }
            else
            {
                _logger.LogInformation("No new countries to seed.");
            }
            // Return all countries from DB now, including any pre-existing ones plus newly seeded.
            return await _context.Countries.OrderBy(c => c.Name).ToListAsync();
        }

        private async Task SeedCitiesAsync(List<Country> countriesToSeedFor)
        {
            var citiesToAdd = new List<City>();
            var existingCityNamesForCountry = new Dictionary<int, HashSet<string>>();

            foreach (var country in countriesToSeedFor)
            {
                if (!existingCityNamesForCountry.ContainsKey(country.Id))
                {
                    existingCityNamesForCountry[country.Id] = (await _context.Cities
                        .Where(c => c.CountryId == country.Id)
                        .Select(c => c.Name)
                        .ToListAsync())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                }

                var capital = GetCapitalCity(country);
                if (capital != null && !existingCityNamesForCountry[country.Id].Contains(capital.Name))
                {
                    citiesToAdd.Add(capital);
                    existingCityNamesForCountry[country.Id].Add(capital.Name);
                }
            }

            // Get additional cities and ensure they are linked to existing, seeded countries
            // and are not duplicates within those countries.
            var allDbCountries = await _context.Countries.ToDictionaryAsync(c => c.Id, c => c);
            var additionalCities = GetAdditionalCities(allDbCountries); // Pass DB countries for correct ID linking

            foreach (var city in additionalCities)
            {
                if (!existingCityNamesForCountry.ContainsKey(city.CountryId))
                {
                     existingCityNamesForCountry[city.CountryId] = (await _context.Cities
                        .Where(c => c.CountryId == city.CountryId)
                        .Select(c => c.Name)
                        .ToListAsync())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                }

                if (!existingCityNamesForCountry[city.CountryId].Contains(city.Name))
                {
                    citiesToAdd.Add(city);
                    existingCityNamesForCountry[city.CountryId].Add(city.Name);
                }
            }

            if (citiesToAdd.Any())
            {
                _context.Cities.AddRange(citiesToAdd);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Seeded {citiesToAdd.Count} new cities.");
            }
            else
            {
                _logger.LogInformation("No new cities to seed.");
            }
        }

        private City GetCapitalCity(Country country)
        {
            // Definitions of capitals (ensure CountryId matches the actual ID from the DB after seeding countries)
            // This is safer if Country IDs are not hardcoded but fetched/passed.
            switch (country.Code)
            {
                case "IT": return new City { CountryId = country.Id, Name = "Roma", IsCapital = true, Latitude = 41.9028, Longitude = 12.4964 };
                case "FR": return new City { CountryId = country.Id, Name = "Parigi", IsCapital = true, Latitude = 48.8566, Longitude = 2.3522 };
                case "GB": return new City { CountryId = country.Id, Name = "Londra", IsCapital = true, Latitude = 51.5074, Longitude = -0.1278 };
                case "DE": return new City { CountryId = country.Id, Name = "Berlino", IsCapital = true, Latitude = 52.5200, Longitude = 13.4050 };
                case "ES": return new City { CountryId = country.Id, Name = "Madrid", IsCapital = true, Latitude = 40.4168, Longitude = -3.7038 };
                // ... (add all other capitals from your original list, ensuring country.Id is used) ...
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
                    // Fallback, but ideally all your seeded countries should have a capital defined
                    _logger.LogWarning($"No specific capital defined for country code {country.Code}. Using generic capital.");
                    return new City { CountryId = country.Id, Name = $"Capitale di {country.Name}", IsCapital = true, Latitude = country.Latitude, Longitude = country.Longitude };
            }
        }

        // Modified to take a dictionary of DB countries for correct ID linking
        private List<City> GetAdditionalCities(Dictionary<int, Country> dbCountries)
        {
            var additionalCities = new List<City>();

            // Helper to find country ID by code from the provided dbCountries dictionary
            int? GetCountryIdByCode(string code)
            {
                return dbCountries.Values.FirstOrDefault(c => c.Code == code)?.Id;
            }

            // Italia
            int? italyId = GetCountryIdByCode("IT");
            if (italyId.HasValue)
            {
                additionalCities.Add(new City { CountryId = italyId.Value, Name = "Milano", IsCapital = false, Latitude = 45.4642, Longitude = 9.1900 });
                additionalCities.Add(new City { CountryId = italyId.Value, Name = "Napoli", IsCapital = false, Latitude = 40.8518, Longitude = 14.2681 });
                additionalCities.Add(new City { CountryId = italyId.Value, Name = "Firenze", IsCapital = false, Latitude = 43.7696, Longitude = 11.2558 });
                additionalCities.Add(new City { CountryId = italyId.Value, Name = "Venezia", IsCapital = false, Latitude = 45.4408, Longitude = 12.3155 });
            }

            // Francia
            int? franceId = GetCountryIdByCode("FR");
            if (franceId.HasValue)
            {
                additionalCities.Add(new City { CountryId = franceId.Value, Name = "Marsiglia", IsCapital = false, Latitude = 43.2965, Longitude = 5.3698 });
                additionalCities.Add(new City { CountryId = franceId.Value, Name = "Lione", IsCapital = false, Latitude = 45.7640, Longitude = 4.8357 });
                additionalCities.Add(new City { CountryId = franceId.Value, Name = "Nizza", IsCapital = false, Latitude = 43.7102, Longitude = 7.2620 });
            }
            // ... (Continue for all other countries and their additional cities from your list) ...
            // Ensure you use GetCountryIdByCode to link correctly

            // Regno Unito
            int? gbId = GetCountryIdByCode("GB");
            if (gbId.HasValue)
            {
                additionalCities.Add(new City { CountryId = gbId.Value, Name = "Manchester", IsCapital = false, Latitude = 53.4808, Longitude = -2.2426 });
                additionalCities.Add(new City { CountryId = gbId.Value, Name = "Liverpool", IsCapital = false, Latitude = 53.4084, Longitude = -2.9916 });
                additionalCities.Add(new City { CountryId = gbId.Value, Name = "Edimburgo", IsCapital = false, Latitude = 55.9533, Longitude = -3.1883 });
            }

            // Germania
            int? deId = GetCountryIdByCode("DE");
            if (deId.HasValue)
            {
                additionalCities.Add(new City { CountryId = deId.Value, Name = "Monaco", IsCapital = false, Latitude = 48.1351, Longitude = 11.5820 });
                additionalCities.Add(new City { CountryId = deId.Value, Name = "Amburgo", IsCapital = false, Latitude = 53.5511, Longitude = 9.9937 });
                additionalCities.Add(new City { CountryId = deId.Value, Name = "Francoforte", IsCapital = false, Latitude = 50.1109, Longitude = 8.6821 });
            }

            // Spagna
            int? esId = GetCountryIdByCode("ES");
            if (esId.HasValue)
            {
                additionalCities.Add(new City { CountryId = esId.Value, Name = "Barcellona", IsCapital = false, Latitude = 41.3851, Longitude = 2.1734 });
                additionalCities.Add(new City { CountryId = esId.Value, Name = "Valencia", IsCapital = false, Latitude = 39.4699, Longitude = -0.3763 });
                additionalCities.Add(new City { CountryId = esId.Value, Name = "Siviglia", IsCapital = false, Latitude = 37.3891, Longitude = -5.9845 });
            }

            // Stati Uniti
            int? usId = GetCountryIdByCode("US");
            if (usId.HasValue)
            {
                additionalCities.Add(new City { CountryId = usId.Value, Name = "New York", IsCapital = false, Latitude = 40.7128, Longitude = -74.0060 });
                additionalCities.Add(new City { CountryId = usId.Value, Name = "Los Angeles", IsCapital = false, Latitude = 34.0522, Longitude = -118.2437 });
                additionalCities.Add(new City { CountryId = usId.Value, Name = "Chicago", IsCapital = false, Latitude = 41.8781, Longitude = -87.6298 });
                additionalCities.Add(new City { CountryId = usId.Value, Name = "San Francisco", IsCapital = false, Latitude = 37.7749, Longitude = -122.4194 });
            }
            return additionalCities;
        }

        private List<Country> GetDefaultCountries()
        {
            // The IDs here are only initial suggestions. EF Core will assign actual IDs upon insertion
            // if the Id property is configured as identity (which is typical).
            // It's better to rely on Codes for uniqueness if re-seeding.
            return new List<Country>
            {
                // IDs are removed here because EF Core will assign them.
                // Ensure Code is unique.
                new Country { Name = "Italia", Code = "IT", Continent = "Europa", Latitude = 41.9028, Longitude = 12.4964 },
                new Country { Name = "Francia", Code = "FR", Continent = "Europa", Latitude = 48.8566, Longitude = 2.3522 },
                new Country { Name = "Regno Unito", Code = "GB", Continent = "Europa", Latitude = 51.5074, Longitude = -0.1278 },
                new Country { Name = "Germania", Code = "DE", Continent = "Europa", Latitude = 52.5200, Longitude = 13.4050 },
                new Country { Name = "Spagna", Code = "ES", Continent = "Europa", Latitude = 40.4168, Longitude = -3.7038 },
                new Country { Name = "Portogallo", Code = "PT", Continent = "Europa", Latitude = 38.7223, Longitude = -9.1393 },
                new Country { Name = "Svizzera", Code = "CH", Continent = "Europa", Latitude = 46.9480, Longitude = 7.4474 },
                new Country { Name = "Austria", Code = "AT", Continent = "Europa", Latitude = 48.2082, Longitude = 16.3738 },
                new Country { Name = "Belgio", Code = "BE", Continent = "Europa", Latitude = 50.8503, Longitude = 4.3517 },
                new Country { Name = "Paesi Bassi", Code = "NL", Continent = "Europa", Latitude = 52.3676, Longitude = 4.9041 },
                new Country { Name = "Stati Uniti", Code = "US", Continent = "Nord America", Latitude = 38.9072, Longitude = -77.0369 },
                new Country { Name = "Canada", Code = "CA", Continent = "Nord America", Latitude = 45.4215, Longitude = -75.6972 },
                new Country { Name = "Giappone", Code = "JP", Continent = "Asia", Latitude = 35.6762, Longitude = 139.6503 },
                new Country { Name = "Cina", Code = "CN", Continent = "Asia", Latitude = 39.9042, Longitude = 116.4074 },
                new Country { Name = "Australia", Code = "AU", Continent = "Oceania", Latitude = -35.2809, Longitude = 149.1300 },
                new Country { Name = "Russia", Code = "RU", Continent = "Europa/Asia", Latitude = 55.7558, Longitude = 37.6173 },
                new Country { Name = "Brasile", Code = "BR", Continent = "Sud America", Latitude = -15.7801, Longitude = -47.9292 },
                new Country { Name = "India", Code = "IN", Continent = "Asia", Latitude = 28.6139, Longitude = 77.2090 },
                new Country { Name = "Sud Africa", Code = "ZA", Continent = "Africa", Latitude = -25.7461, Longitude = 28.1881 },
                new Country { Name = "Messico", Code = "MX", Continent = "Nord America", Latitude = 19.4326, Longitude = -99.1332 },
                new Country { Name = "Argentina", Code = "AR", Continent = "Sud America", Latitude = -34.6037, Longitude = -58.3816 },
                new Country { Name = "Egitto", Code = "EG", Continent = "Africa", Latitude = 30.0444, Longitude = 31.2357 },
                new Country { Name = "Grecia", Code = "GR", Continent = "Europa", Latitude = 37.9838, Longitude = 23.7275 },
                new Country { Name = "Svezia", Code = "SE", Continent = "Europa", Latitude = 59.3293, Longitude = 18.0686 },
                new Country { Name = "Norvegia", Code = "NO", Continent = "Europa", Latitude = 59.9139, Longitude = 10.7522 }
            };
        }
    }
}