// Services/IDreamService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using WanderGlobe.Models; // Make sure this using statement is present and correct

namespace WanderGlobe.Services
{
    public interface IDreamService
    {
        Task<List<DreamDestination>> GetUserWishlistAsync(string userId);
        Task AddToWishlistAsync(DreamDestination dream);
        Task RemoveFromWishlistAsync(int dreamId, string userId);
        Task<bool> IsCityInUserWishlistAsync(int cityId, string userId);
        Task<DreamDestination?> GetDreamByIdAsync(int dreamId); // Added for completeness
        Task UpdateDreamAsync(DreamDestination dream); // Added for completeness
        // Add other methods as needed, e.g., for DreamCountry
        Task<List<DreamCountry>> GetUserDreamCountriesAsync(string userId);
        Task AddDreamCountryAsync(DreamCountry dreamCountry);
        Task RemoveDreamCountryAsync(int dreamCountryId, string userId);

    }
}