using WanderGlobe.Models;

namespace WanderGlobe.Services
{
    public interface ICountryService
    {
        Task<List<Country>> GetAllCountriesAsync();
        Task<List<VisitedCountry>> GetVisitedCountriesByUserAsync(string userId);
        Task<double> GetVisitedPercentageAsync(string userId);
        Task AddVisitedCountryAsync(VisitedCountry visitedCountry);
        Task RemoveVisitedCountryAsync(string userId, int countryId);
    }
}