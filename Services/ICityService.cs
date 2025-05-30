// File: Services/ICityService.cs
using WanderGlobe.Models;
using System; // For DateTime
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public interface ICityService
    {
        Task<List<City>> GetAllCitiesAsync();
        Task<List<City>> GetCitiesByCountryIdAsync(int countryId);
        Task<List<City>> GetCapitalCitiesAsync();
        Task<City> GetCityByIdAsync(int cityId); // Already exists
        Task<City?> GetCapitalCityAsync(int countryId); // Already exists (likely same as GetCapitalCityByCountryIdAsync)
        Task<City?> GetCapitalCityByCountryIdAsync(int countryId); // Already exists
        Task<List<City>> GetAvailableCitiesForUserAsync(string userId);
        Task<List<City>> GetCitiesNotInWishlistAsync(string userId);
        Task<bool> IsCityVisitedByUserAsync(int cityId, string userId); // This likely needs to change to check VisitedCities table
        Task<bool> IsCityInWishlistAsync(int cityId, string userId);
        Task<bool> MarkCityAsVisitedAsync(int cityId, string userId, DateTime visitDate); // This likely needs to change to add to VisitedCities
        Task<List<City>> GetAllCitiesWithCountryAsync(); // Already exists

        // --- Methods for the new VisitedCity entity ---
        Task AddVisitedCityAsync(VisitedCity visitedCity); // Add this if not already present for GlobeModel
        Task<List<VisitedCity>> GetVisitedCitiesByUserAsync(string userId); // Add this if not already present for GlobeModel & TimelineModel
        Task<VisitedCity?> GetVisitedCityByIdAsync(int visitedCityRecordId, string userId); // To fetch for editing/verification

        // !!! ADD THIS METHOD SIGNATURE !!!
        Task<bool> UpdateVisitedCityAsync(int visitedCityRecordId, string userId, DateTime visitDate, string? notes);

        Task RemoveVisitedCityAsync(int visitedCityRecordId); // Add this if not already present for GlobeModel
    }
}