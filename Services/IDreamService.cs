using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WanderGlobe.Models.Custom;

namespace WanderGlobe.Services
{
    public interface IDreamService
    {
        Task<List<DreamDestination>> GetUserWishlistAsync(string userId);
        Task<List<PlannedTrip>> GetUserPlannedTripsAsync(string userId);
        Task<List<RecommendedDestination>> GetRecommendationsAsync(string userId);
        Task<DreamDestination> AddToWishlistAsync(DreamDestination destination);
        Task<bool> RemoveFromWishlistAsync(int dreamId, string userId);
        Task<PlannedTrip> CreatePlannedTripAsync(PlannedTrip trip);
        Task<bool> UpdatePlannedTripAsync(PlannedTrip trip);
        Task<bool> DeletePlannedTripAsync(string tripId, string userId);
        Task<bool> MarkTripAsVisitedAsync(string tripId, string userId);
        Task<bool> IsCityInUserWishlistAsync(int cityId, string userId);
        Task<List<RecommendedDestination>> GetAIRecommendationsAsync(string userId, string recommendationType);
    }
}