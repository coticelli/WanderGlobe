// Services/ICityService.cs
using WanderGlobe.Models;
using System; // Per DateTime
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public interface ICityService
    {
        Task<List<City>> GetAllCitiesAsync();
        Task<List<City>> GetCitiesByCountryIdAsync(int countryId);
        Task<List<City>> GetCapitalCitiesAsync();
        Task<City?> GetCityByIdAsync(int cityId); // Modificato per ritornare nullable
        Task<City?> GetCapitalCityAsync(int countryId); // Già nullable
        Task<City?> GetCapitalCityByCountryIdAsync(int countryId); // Già nullable
        Task<List<City>> GetAvailableCitiesForUserAsync(string userId); // Questo potrebbe cambiare logica
        Task<List<City>> GetCitiesNotInWishlistAsync(string userId);
        Task<bool> IsCityVisitedByUserAsync(int cityId, string userId); // Questo sarà rimpiazzato o modificato
        Task<bool> MarkCityAsVisitedAsync(int cityId, string userId, DateTime visitDate); // Questo sarà rimpiazzato
        Task<List<City>> GetAllCitiesWithCountryAsync();
        Task<bool> IsCityInWishlistAsync(int cityId, string userId); // Nuovo metodo

        // --- NUOVI METODI PER VISITEDCITY ---
        Task AddVisitedCityAsync(VisitedCity visitedCity);
        Task<List<VisitedCity>> GetVisitedCitiesByUserAsync(string userId);
        Task<bool> HasUserVisitedCityOnDateAsync(string userId, int cityId, DateTime visitDate);
        Task RemoveVisitedCityAsync(int visitedCityId); // Rimuovi per ID della visita specifica
        Task<VisitedCity?> GetVisitedCityByIdAsync(int visitedCityId); // Per la rimozione
    }
}