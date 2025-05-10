using WanderGlobe.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public interface ICityService
    {
        Task<List<City>> GetAllCitiesAsync();
        Task<List<City>> GetCitiesByCountryIdAsync(int countryId);
        Task<List<City>> GetCapitalCitiesAsync();
        Task<City> GetCityByIdAsync(int cityId);
        Task<City?> GetCapitalCityByCountryIdAsync(int countryId);
    }
}
