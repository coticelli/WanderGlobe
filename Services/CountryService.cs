using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
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

            // Ora che abbiamo i paesi con ID assegnati, aggiungiamo le città
            await SeedCitiesAsync(countries);

            return countries;
        }

        // Metodo per popolare il database con le città principali
        private async Task SeedCitiesAsync(List<Country> countries)
        {
            var cities = new List<City>();
            
            // Prima aggiungiamo le capitali per ogni paese
            foreach (var country in countries)
            {
                cities.Add(GetCapitalCity(country));
            }
            
            // Poi aggiungiamo altre città importanti
            cities.AddRange(GetAdditionalCities());
            
            _context.Cities.AddRange(cities);
            await _context.SaveChangesAsync();
        }
        
        // Metodo per ottenere la capitale di un paese
        private City GetCapitalCity(Country country)
        {
            // Definizioni delle capitali per ogni paese
            switch (country.Code)
            {
                case "IT":
                    return new City { CountryId = country.Id, Name = "Roma", IsCapital = true, Latitude = 41.9028, Longitude = 12.4964 };
                case "FR":
                    return new City { CountryId = country.Id, Name = "Parigi", IsCapital = true, Latitude = 48.8566, Longitude = 2.3522 };
                case "GB":
                    return new City { CountryId = country.Id, Name = "Londra", IsCapital = true, Latitude = 51.5074, Longitude = -0.1278 };
                case "DE":
                    return new City { CountryId = country.Id, Name = "Berlino", IsCapital = true, Latitude = 52.5200, Longitude = 13.4050 };
                case "ES":
                    return new City { CountryId = country.Id, Name = "Madrid", IsCapital = true, Latitude = 40.4168, Longitude = -3.7038 };
                case "PT":
                    return new City { CountryId = country.Id, Name = "Lisbona", IsCapital = true, Latitude = 38.7223, Longitude = -9.1393 };
                case "CH":
                    return new City { CountryId = country.Id, Name = "Berna", IsCapital = true, Latitude = 46.9480, Longitude = 7.4474 };
                case "AT":
                    return new City { CountryId = country.Id, Name = "Vienna", IsCapital = true, Latitude = 48.2082, Longitude = 16.3738 };
                case "BE":
                    return new City { CountryId = country.Id, Name = "Bruxelles", IsCapital = true, Latitude = 50.8503, Longitude = 4.3517 };
                case "NL":
                    return new City { CountryId = country.Id, Name = "Amsterdam", IsCapital = true, Latitude = 52.3676, Longitude = 4.9041 };
                case "US":
                    return new City { CountryId = country.Id, Name = "Washington D.C.", IsCapital = true, Latitude = 38.9072, Longitude = -77.0369 };
                case "CA":
                    return new City { CountryId = country.Id, Name = "Ottawa", IsCapital = true, Latitude = 45.4215, Longitude = -75.6972 };
                case "JP":
                    return new City { CountryId = country.Id, Name = "Tokyo", IsCapital = true, Latitude = 35.6762, Longitude = 139.6503 };
                case "CN":
                    return new City { CountryId = country.Id, Name = "Pechino", IsCapital = true, Latitude = 39.9042, Longitude = 116.4074 };
                case "AU":
                    return new City { CountryId = country.Id, Name = "Canberra", IsCapital = true, Latitude = -35.2809, Longitude = 149.1300 };
                case "RU":
                    return new City { CountryId = country.Id, Name = "Mosca", IsCapital = true, Latitude = 55.7558, Longitude = 37.6173 };
                case "BR":
                    return new City { CountryId = country.Id, Name = "Brasilia", IsCapital = true, Latitude = -15.7801, Longitude = -47.9292 };
                case "IN":
                    return new City { CountryId = country.Id, Name = "Nuova Delhi", IsCapital = true, Latitude = 28.6139, Longitude = 77.2090 };
                case "ZA":
                    return new City { CountryId = country.Id, Name = "Pretoria", IsCapital = true, Latitude = -25.7461, Longitude = 28.1881 };
                case "MX":
                    return new City { CountryId = country.Id, Name = "Città del Messico", IsCapital = true, Latitude = 19.4326, Longitude = -99.1332 };
                case "AR":
                    return new City { CountryId = country.Id, Name = "Buenos Aires", IsCapital = true, Latitude = -34.6037, Longitude = -58.3816 };
                case "EG":
                    return new City { CountryId = country.Id, Name = "Il Cairo", IsCapital = true, Latitude = 30.0444, Longitude = 31.2357 };
                case "GR":
                    return new City { CountryId = country.Id, Name = "Atene", IsCapital = true, Latitude = 37.9838, Longitude = 23.7275 };
                case "SE":
                    return new City { CountryId = country.Id, Name = "Stoccolma", IsCapital = true, Latitude = 59.3293, Longitude = 18.0686 };
                case "NO":
                    return new City { CountryId = country.Id, Name = "Oslo", IsCapital = true, Latitude = 59.9139, Longitude = 10.7522 };
                default:
                    return new City { CountryId = country.Id, Name = "Capitale", IsCapital = true, Latitude = country.Latitude, Longitude = country.Longitude };
            }
        }
        
        // Altre città importanti oltre alle capitali
        private List<City> GetAdditionalCities()
        {
            return new List<City>
            {
                // Italia
                new City { CountryId = 1, Name = "Milano", IsCapital = false, Latitude = 45.4642, Longitude = 9.1900 },
                new City { CountryId = 1, Name = "Napoli", IsCapital = false, Latitude = 40.8518, Longitude = 14.2681 },
                new City { CountryId = 1, Name = "Firenze", IsCapital = false, Latitude = 43.7696, Longitude = 11.2558 },
                new City { CountryId = 1, Name = "Venezia", IsCapital = false, Latitude = 45.4408, Longitude = 12.3155 },
                
                // Francia
                new City { CountryId = 2, Name = "Marsiglia", IsCapital = false, Latitude = 43.2965, Longitude = 5.3698 },
                new City { CountryId = 2, Name = "Lione", IsCapital = false, Latitude = 45.7640, Longitude = 4.8357 },
                new City { CountryId = 2, Name = "Nizza", IsCapital = false, Latitude = 43.7102, Longitude = 7.2620 },
                
                // Regno Unito
                new City { CountryId = 3, Name = "Manchester", IsCapital = false, Latitude = 53.4808, Longitude = -2.2426 },
                new City { CountryId = 3, Name = "Liverpool", IsCapital = false, Latitude = 53.4084, Longitude = -2.9916 },
                new City { CountryId = 3, Name = "Edimburgo", IsCapital = false, Latitude = 55.9533, Longitude = -3.1883 },
                
                // Germania
                new City { CountryId = 4, Name = "Monaco", IsCapital = false, Latitude = 48.1351, Longitude = 11.5820 },
                new City { CountryId = 4, Name = "Amburgo", IsCapital = false, Latitude = 53.5511, Longitude = 9.9937 },
                new City { CountryId = 4, Name = "Francoforte", IsCapital = false, Latitude = 50.1109, Longitude = 8.6821 },
                
                // Spagna
                new City { CountryId = 5, Name = "Barcellona", IsCapital = false, Latitude = 41.3851, Longitude = 2.1734 },
                new City { CountryId = 5, Name = "Valencia", IsCapital = false, Latitude = 39.4699, Longitude = -0.3763 },
                new City { CountryId = 5, Name = "Siviglia", IsCapital = false, Latitude = 37.3891, Longitude = -5.9845 },
                
                // Stati Uniti
                new City { CountryId = 11, Name = "New York", IsCapital = false, Latitude = 40.7128, Longitude = -74.0060 },
                new City { CountryId = 11, Name = "Los Angeles", IsCapital = false, Latitude = 34.0522, Longitude = -118.2437 },
                new City { CountryId = 11, Name = "Chicago", IsCapital = false, Latitude = 41.8781, Longitude = -87.6298 },
                new City { CountryId = 11, Name = "San Francisco", IsCapital = false, Latitude = 37.7749, Longitude = -122.4194 }
            };
        }

        // Lista di paesi di default per popolare il database
        private List<Country> GetDefaultCountries()
        {
            return new List<Country>
            {
                new Country { Id = 1, Name = "Italia", Code = "IT", Continent = "Europa", Latitude = 41.9028, Longitude = 12.4964 },
                new Country { Id = 2, Name = "Francia", Code = "FR", Continent = "Europa", Latitude = 48.8566, Longitude = 2.3522 },
                new Country { Id = 3, Name = "Regno Unito", Code = "GB", Continent = "Europa", Latitude = 51.5074, Longitude = -0.1278 },
                new Country { Id = 4, Name = "Germania", Code = "DE", Continent = "Europa", Latitude = 52.5200, Longitude = 13.4050 },
                new Country { Id = 5, Name = "Spagna", Code = "ES", Continent = "Europa", Latitude = 40.4168, Longitude = -3.7038 },
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