// Codice completo per ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using WanderGlobe.Models;
using WanderGlobe.Models.Custom;

namespace WanderGlobe.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<VisitedCountry> VisitedCountries { get; set; } = null!;
        public DbSet<TravelJournal> TravelJournals { get; set; } = null!;
        public DbSet<Badge> Badges { get; set; } = null!;
        public DbSet<UserBadge> UserBadges { get; set; } = null!;
        public DbSet<Photo> Photos { get; set; } = null!;
        public DbSet<DreamDestination> DreamDestinations { get; set; } = null!;
        public DbSet<PlannedTrip> PlannedTrips { get; set; } = null!;
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurazione relazioni
            builder.Entity<VisitedCountry>()
                .HasKey(vc => new { vc.UserId, vc.CountryId });

            builder.Entity<VisitedCountry>()
                .HasOne(vc => vc.User)
                .WithMany(u => u.VisitedCountries)
                .HasForeignKey(vc => vc.UserId);

            builder.Entity<VisitedCountry>()
                .HasOne(vc => vc.Country)
                .WithMany(c => c.VisitedByUsers)
                .HasForeignKey(vc => vc.CountryId);

            builder.Entity<UserBadge>()
                .HasKey(ub => new { ub.UserId, ub.BadgeId });
                
            // Ignora le proprietà di tipo List<string> per evitare che EF le consideri come entità
            builder.Ignore<List<string>>();

            // Seed dei dati dei paesi
            SeedCountries(builder);
        }

        private void SeedCountries(ModelBuilder builder)
        {
            // Seed dei dati dei paesi con coordinate complete
            var countries = new List<Country>
            {
                new Country { Id = 1, Name = "Italia", Code = "IT", Continent = "Europa", Latitude = 41.9028, Longitude = 12.4964 },
                new Country { Id = 2, Name = "Francia", Code = "FR", Continent = "Europa", Latitude = 48.8566, Longitude = 2.3522 },
                new Country { Id = 3, Name = "Stati Uniti", Code = "US", Continent = "Nord America", Latitude = 38.9072, Longitude = -77.0369 },
                new Country { Id = 4, Name = "Germania", Code = "DE", Continent = "Europa", Latitude = 52.5200, Longitude = 13.4050 },
                new Country { Id = 5, Name = "Spagna", Code = "ES", Continent = "Europa", Latitude = 40.4168, Longitude = -3.7038 },
                new Country { Id = 6, Name = "Portogallo", Code = "PT", Continent = "Europa", Latitude = 38.7223, Longitude = -9.1393 },
                new Country { Id = 7, Name = "Svizzera", Code = "CH", Continent = "Europa", Latitude = 46.9480, Longitude = 7.4474 },
                new Country { Id = 8, Name = "Austria", Code = "AT", Continent = "Europa", Latitude = 48.2082, Longitude = 16.3738 },
                new Country { Id = 9, Name = "Belgio", Code = "BE", Continent = "Europa", Latitude = 50.8503, Longitude = 4.3517 },
                new Country { Id = 10, Name = "Paesi Bassi", Code = "NL", Continent = "Europa", Latitude = 52.3676, Longitude = 4.9041 },
                new Country { Id = 11, Name = "Regno Unito", Code = "GB", Continent = "Europa", Latitude = 51.5074, Longitude = -0.1278 },
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
                new Country { Id = 22, Name = "Grecia", Code = "GR", Continent = "Europa", Latitude = 37.9838, Longitude = 23.7275 },
                new Country { Id = 23, Name = "Egitto", Code = "EG", Continent = "Africa", Latitude = 30.0444, Longitude = 31.2357 },
                new Country { Id = 24, Name = "Svezia", Code = "SE", Continent = "Europa", Latitude = 59.3293, Longitude = 18.0686 },
                new Country { Id = 25, Name = "Norvegia", Code = "NO", Continent = "Europa", Latitude = 59.9139, Longitude = 10.7522 },
                new Country { Id = 26, Name = "Danimarca", Code = "DK", Continent = "Europa", Latitude = 55.6761, Longitude = 12.5683 },
                new Country { Id = 27, Name = "Finlandia", Code = "FI", Continent = "Europa", Latitude = 60.1699, Longitude = 24.9384 },
                new Country { Id = 28, Name = "Irlanda", Code = "IE", Continent = "Europa", Latitude = 53.3498, Longitude = -6.2603 },
                new Country { Id = 29, Name = "Nuova Zelanda", Code = "NZ", Continent = "Oceania", Latitude = -41.2865, Longitude = 174.7762 },
                new Country { Id = 30, Name = "Singapore", Code = "SG", Continent = "Asia", Latitude = 1.3521, Longitude = 103.8198 },
                new Country { Id = 31, Name = "Thailandia", Code = "TH", Continent = "Asia", Latitude = 13.7563, Longitude = 100.5018 },
                new Country { Id = 32, Name = "Vietnam", Code = "VN", Continent = "Asia", Latitude = 21.0278, Longitude = 105.8342 },
                new Country { Id = 33, Name = "Indonesia", Code = "ID", Continent = "Asia", Latitude = -6.2088, Longitude = 106.8456 },
                new Country { Id = 34, Name = "Malesia", Code = "MY", Continent = "Asia", Latitude = 3.1390, Longitude = 101.6869 },
                new Country { Id = 35, Name = "Turchia", Code = "TR", Continent = "Europa/Asia", Latitude = 39.9334, Longitude = 32.8597 }
            };
            
            builder.Entity<Country>().HasData(countries);
            
            // Seed delle città capitali
            SeedCapitalCities(builder);
            
            // Seed delle città principali
            SeedMajorCities(builder);
        }

        private void SeedCapitalCities(ModelBuilder builder)
        {
            var capitals = new List<City>
            {
                new City { Id = 1, Name = "Roma", IsCapital = true, CountryId = 1, Latitude = 41.9028, Longitude = 12.4964 },
                new City { Id = 2, Name = "Parigi", IsCapital = true, CountryId = 2, Latitude = 48.8566, Longitude = 2.3522 },
                new City { Id = 3, Name = "Washington D.C.", IsCapital = true, CountryId = 3, Latitude = 38.9072, Longitude = -77.0369 },
                new City { Id = 4, Name = "Berlino", IsCapital = true, CountryId = 4, Latitude = 52.5200, Longitude = 13.4050 },
                new City { Id = 5, Name = "Madrid", IsCapital = true, CountryId = 5, Latitude = 40.4168, Longitude = -3.7038 },
                new City { Id = 6, Name = "Lisbona", IsCapital = true, CountryId = 6, Latitude = 38.7223, Longitude = -9.1393 },
                new City { Id = 7, Name = "Berna", IsCapital = true, CountryId = 7, Latitude = 46.9480, Longitude = 7.4474 },
                new City { Id = 8, Name = "Vienna", IsCapital = true, CountryId = 8, Latitude = 48.2082, Longitude = 16.3738 },
                new City { Id = 9, Name = "Bruxelles", IsCapital = true, CountryId = 9, Latitude = 50.8503, Longitude = 4.3517 },
                new City { Id = 10, Name = "Amsterdam", IsCapital = true, CountryId = 10, Latitude = 52.3676, Longitude = 4.9041 },
                new City { Id = 11, Name = "Londra", IsCapital = true, CountryId = 11, Latitude = 51.5074, Longitude = -0.1278 },
                new City { Id = 12, Name = "Ottawa", IsCapital = true, CountryId = 12, Latitude = 45.4215, Longitude = -75.6972 },
                new City { Id = 13, Name = "Tokyo", IsCapital = true, CountryId = 13, Latitude = 35.6762, Longitude = 139.6503 },
                new City { Id = 14, Name = "Pechino", IsCapital = true, CountryId = 14, Latitude = 39.9042, Longitude = 116.4074 },
                new City { Id = 15, Name = "Canberra", IsCapital = true, CountryId = 15, Latitude = -35.2809, Longitude = 149.1300 },
                new City { Id = 16, Name = "Mosca", IsCapital = true, CountryId = 16, Latitude = 55.7558, Longitude = 37.6173 },
                new City { Id = 17, Name = "Brasilia", IsCapital = true, CountryId = 17, Latitude = -15.7801, Longitude = -47.9292 },
                new City { Id = 18, Name = "Nuova Delhi", IsCapital = true, CountryId = 18, Latitude = 28.6139, Longitude = 77.2090 },
                new City { Id = 19, Name = "Pretoria", IsCapital = true, CountryId = 19, Latitude = -25.7461, Longitude = 28.1881 },
                new City { Id = 20, Name = "Città del Messico", IsCapital = true, CountryId = 20, Latitude = 19.4326, Longitude = -99.1332 },
                new City { Id = 21, Name = "Buenos Aires", IsCapital = true, CountryId = 21, Latitude = -34.6037, Longitude = -58.3816 },
                new City { Id = 22, Name = "Atene", IsCapital = true, CountryId = 22, Latitude = 37.9838, Longitude = 23.7275 },
                new City { Id = 23, Name = "Il Cairo", IsCapital = true, CountryId = 23, Latitude = 30.0444, Longitude = 31.2357 },
                new City { Id = 24, Name = "Stoccolma", IsCapital = true, CountryId = 24, Latitude = 59.3293, Longitude = 18.0686 },
                new City { Id = 25, Name = "Oslo", IsCapital = true, CountryId = 25, Latitude = 59.9139, Longitude = 10.7522 },
                new City { Id = 26, Name = "Copenhagen", IsCapital = true, CountryId = 26, Latitude = 55.6761, Longitude = 12.5683 },
                new City { Id = 27, Name = "Helsinki", IsCapital = true, CountryId = 27, Latitude = 60.1699, Longitude = 24.9384 },
                new City { Id = 28, Name = "Dublino", IsCapital = true, CountryId = 28, Latitude = 53.3498, Longitude = -6.2603 },
                new City { Id = 29, Name = "Wellington", IsCapital = true, CountryId = 29, Latitude = -41.2865, Longitude = 174.7762 },
                new City { Id = 30, Name = "Singapore", IsCapital = true, CountryId = 30, Latitude = 1.3521, Longitude = 103.8198 },
                new City { Id = 31, Name = "Bangkok", IsCapital = true, CountryId = 31, Latitude = 13.7563, Longitude = 100.5018 },
                new City { Id = 32, Name = "Hanoi", IsCapital = true, CountryId = 32, Latitude = 21.0278, Longitude = 105.8342 },
                new City { Id = 33, Name = "Jakarta", IsCapital = true, CountryId = 33, Latitude = -6.2088, Longitude = 106.8456 },
                new City { Id = 34, Name = "Kuala Lumpur", IsCapital = true, CountryId = 34, Latitude = 3.1390, Longitude = 101.6869 },
                new City { Id = 35, Name = "Ankara", IsCapital = true, CountryId = 35, Latitude = 39.9334, Longitude = 32.8597 }
            };
            
            builder.Entity<City>().HasData(capitals);
        }
        
        private void SeedMajorCities(ModelBuilder builder)
        {
            // ID inizia da 36 perché abbiamo già 35 capitali
            int id = 36;
            
            var majorCities = new List<City>
            {
                // Italia (ID: 1)
                new City { Id = id++, Name = "Milano", IsCapital = false, CountryId = 1, Latitude = 45.4642, Longitude = 9.1900 },
                new City { Id = id++, Name = "Napoli", IsCapital = false, CountryId = 1, Latitude = 40.8518, Longitude = 14.2681 },
                new City { Id = id++, Name = "Firenze", IsCapital = false, CountryId = 1, Latitude = 43.7696, Longitude = 11.2558 },
                new City { Id = id++, Name = "Venezia", IsCapital = false, CountryId = 1, Latitude = 45.4408, Longitude = 12.3155 },
                new City { Id = id++, Name = "Bologna", IsCapital = false, CountryId = 1, Latitude = 44.4949, Longitude = 11.3426 },
                new City { Id = id++, Name = "Torino", IsCapital = false, CountryId = 1, Latitude = 45.0703, Longitude = 7.6869 },
                new City { Id = id++, Name = "Palermo", IsCapital = false, CountryId = 1, Latitude = 38.1157, Longitude = 13.3615 },
                
                // Francia (ID: 2)
                new City { Id = id++, Name = "Marsiglia", IsCapital = false, CountryId = 2, Latitude = 43.2965, Longitude = 5.3698 },
                new City { Id = id++, Name = "Lione", IsCapital = false, CountryId = 2, Latitude = 45.7640, Longitude = 4.8357 },
                new City { Id = id++, Name = "Nizza", IsCapital = false, CountryId = 2, Latitude = 43.7102, Longitude = 7.2620 },
                new City { Id = id++, Name = "Bordeaux", IsCapital = false, CountryId = 2, Latitude = 44.8378, Longitude = -0.5792 },
                new City { Id = id++, Name = "Tolosa", IsCapital = false, CountryId = 2, Latitude = 43.6047, Longitude = 1.4442 },
                new City { Id = id++, Name = "Strasburgo", IsCapital = false, CountryId = 2, Latitude = 48.5734, Longitude = 7.7521 },
                
                // Stati Uniti (ID: 3)
                new City { Id = id++, Name = "New York", IsCapital = false, CountryId = 3, Latitude = 40.7128, Longitude = -74.0060 },
                new City { Id = id++, Name = "Los Angeles", IsCapital = false, CountryId = 3, Latitude = 34.0522, Longitude = -118.2437 },
                new City { Id = id++, Name = "Chicago", IsCapital = false, CountryId = 3, Latitude = 41.8781, Longitude = -87.6298 },
                new City { Id = id++, Name = "Miami", IsCapital = false, CountryId = 3, Latitude = 25.7617, Longitude = -80.1918 },
                new City { Id = id++, Name = "San Francisco", IsCapital = false, CountryId = 3, Latitude = 37.7749, Longitude = -122.4194 },
                new City { Id = id++, Name = "Las Vegas", IsCapital = false, CountryId = 3, Latitude = 36.1699, Longitude = -115.1398 },
                new City { Id = id++, Name = "Boston", IsCapital = false, CountryId = 3, Latitude = 42.3601, Longitude = -71.0589 },
                
                // Germania (ID: 4)
                new City { Id = id++, Name = "Monaco", IsCapital = false, CountryId = 4, Latitude = 48.1351, Longitude = 11.5820 },
                new City { Id = id++, Name = "Amburgo", IsCapital = false, CountryId = 4, Latitude = 53.5511, Longitude = 9.9937 },
                new City { Id = id++, Name = "Francoforte", IsCapital = false, CountryId = 4, Latitude = 50.1109, Longitude = 8.6821 },
                new City { Id = id++, Name = "Colonia", IsCapital = false, CountryId = 4, Latitude = 50.9375, Longitude = 6.9603 },
                new City { Id = id++, Name = "Düsseldorf", IsCapital = false, CountryId = 4, Latitude = 51.2277, Longitude = 6.7735 },
                
                // Spagna (ID: 5)
                new City { Id = id++, Name = "Barcellona", IsCapital = false, CountryId = 5, Latitude = 41.3851, Longitude = 2.1734 },
                new City { Id = id++, Name = "Valencia", IsCapital = false, CountryId = 5, Latitude = 39.4699, Longitude = -0.3763 },
                new City { Id = id++, Name = "Siviglia", IsCapital = false, CountryId = 5, Latitude = 37.3891, Longitude = -5.9845 },
                new City { Id = id++, Name = "Bilbao", IsCapital = false, CountryId = 5, Latitude = 43.2630, Longitude = -2.9350 },
                new City { Id = id++, Name = "Malaga", IsCapital = false, CountryId = 5, Latitude = 36.7213, Longitude = -4.4213 },
                
                // Regno Unito (ID: 11)
                new City { Id = id++, Name = "Manchester", IsCapital = false, CountryId = 11, Latitude = 53.4808, Longitude = -2.2426 },
                new City { Id = id++, Name = "Birmingham", IsCapital = false, CountryId = 11, Latitude = 52.4862, Longitude = -1.8904 },
                new City { Id = id++, Name = "Glasgow", IsCapital = false, CountryId = 11, Latitude = 55.8642, Longitude = -4.2518 },
                new City { Id = id++, Name = "Liverpool", IsCapital = false, CountryId = 11, Latitude = 53.4084, Longitude = -2.9916 },
                new City { Id = id++, Name = "Edimburgo", IsCapital = false, CountryId = 11, Latitude = 55.9533, Longitude = -3.1883 },
                
                // Canada (ID: 12)
                new City { Id = id++, Name = "Toronto", IsCapital = false, CountryId = 12, Latitude = 43.6532, Longitude = -79.3832 },
                new City { Id = id++, Name = "Montreal", IsCapital = false, CountryId = 12, Latitude = 45.5017, Longitude = -73.5673 },
                new City { Id = id++, Name = "Vancouver", IsCapital = false, CountryId = 12, Latitude = 49.2827, Longitude = -123.1207 },
                new City { Id = id++, Name = "Calgary", IsCapital = false, CountryId = 12, Latitude = 51.0447, Longitude = -114.0719 },
                
                // Giappone (ID: 13)
                new City { Id = id++, Name = "Osaka", IsCapital = false, CountryId = 13, Latitude = 34.6937, Longitude = 135.5023 },
                new City { Id = id++, Name = "Kyoto", IsCapital = false, CountryId = 13, Latitude = 35.0116, Longitude = 135.7681 },
                new City { Id = id++, Name = "Hiroshima", IsCapital = false, CountryId = 13, Latitude = 34.3853, Longitude = 132.4553 },
                new City { Id = id++, Name = "Nagoya", IsCapital = false, CountryId = 13, Latitude = 35.1815, Longitude = 136.9066 },
                
                // Australia (ID: 15)
                new City { Id = id++, Name = "Sydney", IsCapital = false, CountryId = 15, Latitude = -33.8688, Longitude = 151.2093 },
                new City { Id = id++, Name = "Melbourne", IsCapital = false, CountryId = 15, Latitude = -37.8136, Longitude = 144.9631 },
                new City { Id = id++, Name = "Brisbane", IsCapital = false, CountryId = 15, Latitude = -27.4698, Longitude = 153.0251 },
                new City { Id = id++, Name = "Perth", IsCapital = false, CountryId = 15, Latitude = -31.9505, Longitude = 115.8605 },
                
                // Brasile (ID: 17)
                new City { Id = id++, Name = "Rio de Janeiro", IsCapital = false, CountryId = 17, Latitude = -22.9068, Longitude = -43.1729 },
                new City { Id = id++, Name = "São Paulo", IsCapital = false, CountryId = 17, Latitude = -23.5505, Longitude = -46.6333 },
                new City { Id = id++, Name = "Salvador", IsCapital = false, CountryId = 17, Latitude = -12.9714, Longitude = -38.5014 },
                
                // Italia (aggiunta)
                new City { Id = id++, Name = "Verona", IsCapital = false, CountryId = 1, Latitude = 45.4384, Longitude = 10.9916 },
                new City { Id = id++, Name = "Genova", IsCapital = false, CountryId = 1, Latitude = 44.4056, Longitude = 8.9463 },
                
                // Turchia (ID: 35)
                new City { Id = id++, Name = "Istanbul", IsCapital = false, CountryId = 35, Latitude = 41.0082, Longitude = 28.9784 },
                new City { Id = id++, Name = "Antalya", IsCapital = false, CountryId = 35, Latitude = 36.8969, Longitude = 30.7133 },
                new City { Id = id++, Name = "Izmir", IsCapital = false, CountryId = 35, Latitude = 38.4237, Longitude = 27.1428 },
                
                // Thailandia (ID: 31)
                new City { Id = id++, Name = "Phuket", IsCapital = false, CountryId = 31, Latitude = 7.9519, Longitude = 98.3381 },
                new City { Id = id++, Name = "Chiang Mai", IsCapital = false, CountryId = 31, Latitude = 18.7883, Longitude = 98.9853 },
                new City { Id = id++, Name = "Pattaya", IsCapital = false, CountryId = 31, Latitude = 12.9236, Longitude = 100.8824 }
            };
            
            builder.Entity<City>().HasData(majorCities);
        }
    }
}