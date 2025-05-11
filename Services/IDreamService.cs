using WanderGlobe.Models.Custom;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public interface IDreamService
    {
        Task<List<DreamDestination>> GetUserWishlistAsync(string userId);
        Task<List<PlannedTrip>> GetUserPlannedTripsAsync(string userId);
        Task<List<RecommendedDestination>> GetRecommendationsAsync(string userId);
        Task<DreamDestination> AddToWishlistAsync(DreamDestination destination);
        Task<bool> RemoveFromWishlistAsync(int destinationId, string userId);
        Task<PlannedTrip> CreatePlannedTripAsync(PlannedTrip trip);        Task<bool> UpdatePlannedTripAsync(PlannedTrip trip);
        Task<bool> DeletePlannedTripAsync(int tripId, string userId);
        Task<bool> MarkTripAsVisitedAsync(int tripId, string userId);
        Task<bool> IsCityInUserWishlistAsync(int cityId, string userId);
    }
}