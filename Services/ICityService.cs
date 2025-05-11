using WanderGlobe.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{    public interface ICityService
    {        Task<List<City>> GetAllCitiesAsync();
        Task<List<City>> GetCitiesByCountryIdAsync(int countryId);
        Task<List<City>> GetCapitalCitiesAsync();
        Task<City> GetCityByIdAsync(int cityId);
        Task<City?> GetCapitalCityByCountryIdAsync(int countryId);
        Task<List<City>> GetAvailableCitiesForUserAsync(string userId);
        Task<List<City>> GetCitiesNotInWishlistAsync(string userId);
        Task<bool> IsCityVisitedByUserAsync(int cityId, string userId);
        Task<bool> IsCityInWishlistAsync(int cityId, string userId);
        Task<bool> MarkCityAsVisitedAsync(int cityId, string userId, DateTime visitDate);
        // Nuovo metodo per ottenere tutte le città con il loro paese
        Task<List<City>> GetAllCitiesWithCountryAsync();
    }
}
