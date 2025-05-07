using WanderGlobe.Models;

namespace WanderGlobe.Services
{
    public interface IUserProgressService
    {
        Task<double> GetUserCompletionPercentageAsync(string userId);
        Task<int> GetVisitedCountriesCountAsync(string userId);
        Task<int> GetVisitedContinentsCountAsync(string userId);
        Task<List<string>> GetVisitedContinentsAsync(string userId);
        Task<bool> CheckAndAssignBadgesAsync(string userId);
        Task<List<Badge>> GetUserBadgesAsync(string userId);
    }
}
