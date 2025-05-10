// Codice completo per ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WanderGlobe.Models;

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
                new Country { Id = 25, Name = "Norvegia", Code = "NO", Continent = "Europa", Latitude = 59.9139, Longitude = 10.7522 }
            };

            builder.Entity<Country>().HasData(countries);
            
            // Seed delle città capitali
            SeedCapitalCities(builder);
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
                new City { Id = 25, Name = "Oslo", IsCapital = true, CountryId = 25, Latitude = 59.9139, Longitude = 10.7522 }
            };

            builder.Entity<City>().HasData(capitals);
        }
    }
}