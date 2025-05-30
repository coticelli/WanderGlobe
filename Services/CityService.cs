// File: Services/CityService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // If you keep the lazy-loaded DreamService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;
// using WanderGlobe.Models.Custom; // Only if you use specific custom models here

namespace WanderGlobe.Services
{
    public class CityService : ICityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider; // For lazy loading DreamService
        private IDreamService? _dreamService; // For lazy loading

        public CityService(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        private IDreamService DreamService => _dreamService ??= _serviceProvider.GetRequiredService<IDreamService>();

        // ... (your existing methods like GetAllCitiesAsync, GetCityByIdAsync, etc.) ...

        public async Task AddVisitedCityAsync(VisitedCity visitedCity)
        {
            if (visitedCity == null)
            {
                throw new ArgumentNullException(nameof(visitedCity));
            }

            // Optional: Check for duplicates if needed (e.g., user + city + date)
            // For now, assume GlobeModel.cs handles pre-checks or simple add is fine.
            // var existing = await _context.VisitedCities.FirstOrDefaultAsync(vc =>
            //    vc.UserId == visitedCity.UserId &&
            //    vc.CityId == visitedCity.CityId &&
            //    vc.VisitDate.Date == visitedCity.VisitDate.Date); // Example duplicate check
            // if (existing != null)
            // {
            //    throw new ArgumentException("Questa città è già stata registrata come visitata in questa data.");
            // }

            _context.VisitedCities.Add(visitedCity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<VisitedCity>> GetVisitedCitiesByUserAsync(string userId)
        {
            return await _context.VisitedCities
                .Include(vc => vc.City)
                    .ThenInclude(city => city.Country) // Crucial for getting Country Name, Code, Continent
                .Where(vc => vc.UserId == userId)
                .OrderByDescending(vc => vc.VisitDate)
                .ToListAsync();
        }

        public async Task<VisitedCity?> GetVisitedCityByIdAsync(int visitedCityRecordId, string userId)
        {
            return await _context.VisitedCities
                .Include(vc => vc.City)
                    .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(vc => vc.Id == visitedCityRecordId && vc.UserId == userId);
        }


        // !!! IMPLEMENT THIS METHOD !!!
        public async Task<bool> UpdateVisitedCityAsync(int visitedCityRecordId, string userId, DateTime visitDate, string? notes)
        {
            var visitedCity = await _context.VisitedCities
                                      .FirstOrDefaultAsync(vc => vc.Id == visitedCityRecordId && vc.UserId == userId);

            if (visitedCity == null)
            {
                return false; // Visit not found or user not authorized
            }

            visitedCity.VisitDate = visitDate;
            visitedCity.Notes = notes;
            // visitedCity.CityId cannot be changed here, as this method updates an *existing* visit to a city.
            // If you needed to change the city itself, it would be a delete + add new scenario.

            try
            {
                _context.VisitedCities.Update(visitedCity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency issues if necessary, e.g., log and return false
                // For now, just rethrow or log
                Console.WriteLine($"Concurrency error updating VisitedCity: {ex.Message}"); // Replace with proper logging
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error updating VisitedCity: {ex.Message}"); // Replace with proper logging
                return false;
            }
        }

        public async Task RemoveVisitedCityAsync(int visitedCityRecordId) // Assuming userId check is done by caller or here
        {
            var visitedCity = await _context.VisitedCities.FindAsync(visitedCityRecordId);
            if (visitedCity != null)
            {
                // Also remove associated photos if you have a cascade delete set up,
                // or do it manually here before removing the VisitedCity record.
                var photosToDelete = await _context.Photos.Where(p => p.VisitedCityId == visitedCityRecordId).ToListAsync();
                if (photosToDelete.Any())
                {
                    _context.Photos.RemoveRange(photosToDelete);
                    // Note: Physical file deletion for photos would also need to happen here or be triggered.
                }

                _context.VisitedCities.Remove(visitedCity);
                await _context.SaveChangesAsync();
            }
        }


        // --- Your other existing ICityService methods ---
        // Make sure to review MarkCityAsVisitedAsync and IsCityVisitedByUserAsync
        // to ensure they align with the new VisitedCity table logic.

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Include(c => c.Country)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .Where(c => c.CountryId == countryId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<City>> GetCapitalCitiesAsync()
        {
            return await _context.Cities
                .Where(c => c.IsCapital)
                .Include(c => c.Country)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        public async Task<City> GetCityByIdAsync(int cityId)
        {
            // Return new City() is problematic if city not found, can lead to NullReferenceExceptions later.
            // Better to return City? and handle null in the calling code.
            var city = await _context.Cities
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == cityId);
            return city ?? new City { Id = 0, Name = "Sconosciuta" }; // Or throw NotFound, or return null
        }

        public async Task<City?> GetCapitalCityByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .Include(c => c.Country) // Good to include country if you need its details
                .FirstOrDefaultAsync(c => c.CountryId == countryId && c.IsCapital);
        }

        // This method might need adjustment if "visited" means an entry in VisitedCities
        public async Task<List<City>> GetAvailableCitiesForUserAsync(string userId)
        {
            var visitedCityIds = await _context.VisitedCities
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.CityId)
                .Distinct()
                .ToListAsync();

            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => !visitedCityIds.Contains(c.Id)) // Show cities not in VisitedCities for this user
                .OrderBy(c => c.Country != null ? c.Country.Name : "")
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesNotInWishlistAsync(string userId)
        {
            var wishlistCityIds = await _context.DreamDestinations // Assuming DreamDestinations stores CityId
                .Where(dd => dd.UserId == userId && dd.CityId.HasValue)
                .Select(dd => dd.CityId!.Value) // Use !.Value as we filtered for HasValue
                .Distinct()
                .ToListAsync();

            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => !wishlistCityIds.Contains(c.Id))
                .OrderBy(c => c.Country != null ? c.Country.Name : "")
                .ThenBy(c => c.Name)
                .ToListAsync();
        }
        public async Task<City?> GetCapitalCityAsync(int countryId) // Duplicate of GetCapitalCityByCountryIdAsync?
        {
            return await GetCapitalCityByCountryIdAsync(countryId);
        }

        // This should now check the VisitedCities table
        public async Task<bool> IsCityVisitedByUserAsync(int cityId, string userId)
        {
            return await _context.VisitedCities
                .AnyAsync(vc => vc.CityId == cityId && vc.UserId == userId);
        }

        public async Task<bool> IsCityInWishlistAsync(int cityId, string userId)
        {
            return await DreamService.IsCityInUserWishlistAsync(cityId, userId);
        }

        // This method should now directly add to VisitedCities and handle wishlist removal
        public async Task<bool> MarkCityAsVisitedAsync(int cityId, string userId, DateTime visitDate)
        {
            try
            {
                var city = await _context.Cities.FindAsync(cityId);
                if (city == null) return false;

                // Check if this specific city visit already exists for this user
                bool alreadyVisited = await _context.VisitedCities
                    .AnyAsync(vc => vc.UserId == userId && vc.CityId == cityId);

                if (!alreadyVisited)
                {
                    var newVisitedCity = new VisitedCity
                    {
                        UserId = userId,
                        CityId = cityId,
                        VisitDate = visitDate,
                        Notes = $"Visitata il {visitDate:dd/MM/yyyy}" // Example note
                    };
                    _context.VisitedCities.Add(newVisitedCity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Optionally, update the existing visit's date/notes if desired
                    // For now, if already visited, we just proceed to check wishlist
                }

                // Remove from wishlist if it was there
                if (await DreamService.IsCityInUserWishlistAsync(cityId, userId))
                {
                    var dreamDestination = await _context.DreamDestinations
                        .FirstOrDefaultAsync(d => d.UserId == userId && d.CityId == cityId);
                    if (dreamDestination != null)
                    {
                        await DreamService.RemoveFromWishlistAsync(dreamDestination.Id, userId);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in MarkCityAsVisitedAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<City>> GetAllCitiesWithCountryAsync()
        {
            try
            {
                return await _context.Cities
                    .Include(c => c.Country)
                    .Where(c => c.Country != null) // Ensure Country is not null
                    .OrderBy(c => c.Country!.Name) // Use ! if you've filtered for non-null
                    .ThenBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in GetAllCitiesWithCountryAsync: {ex.Message}");
                return new List<City>();
            }
        }
    }
}