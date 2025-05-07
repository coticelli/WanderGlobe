using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using WanderGlobe.Models;
using WanderGlobe.Services;

namespace WanderGlobe.Services
{
    public class UserProgressService : IUserProgressService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICountryService _countryService;

        public UserProgressService(
            ApplicationDbContext context,
            ICountryService countryService)
        {
            _context = context;
            _countryService = countryService;
        }

        public async Task<double> GetUserCompletionPercentageAsync(string userId)
        {
            return await _countryService.GetVisitedPercentageAsync(userId);
        }

        public async Task<int> GetVisitedCountriesCountAsync(string userId)
        {
            return await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .CountAsync();
        }

        public async Task<int> GetVisitedContinentsCountAsync(string userId)
        {
            return await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.Country.Continent)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<string>> GetVisitedContinentsAsync(string userId)
        {
            return await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.Country.Continent)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> CheckAndAssignBadgesAsync(string userId)
        {
            bool newBadgeAssigned = false;

            // Ottieni il conteggio dei paesi visitati
            int visitedCountriesCount = await GetVisitedCountriesCountAsync(userId);

            // Ottieni i continenti visitati
            var visitedContinents = await GetVisitedContinentsAsync(userId);

            // Controlla il badge per il numero di paesi visitati
            if (visitedCountriesCount >= 10)
            {
                newBadgeAssigned |= await AssignBadgeIfNotExistsAsync(userId, "traveler");
            }
            if (visitedCountriesCount >= 25)
            {
                newBadgeAssigned |= await AssignBadgeIfNotExistsAsync(userId, "explorer");
            }
            if (visitedCountriesCount >= 50)
            {
                newBadgeAssigned |= await AssignBadgeIfNotExistsAsync(userId, "globetrotter");
            }

            // Controlla il badge per tutti i continenti
            string[] allContinents = { "Europa", "Asia", "Africa", "Nord America", "Sud America", "Oceania", "Antartide" };
            bool allContinentsVisited = allContinents.All(c => visitedContinents.Contains(c));

            if (allContinentsVisited)
            {
                newBadgeAssigned |= await AssignBadgeIfNotExistsAsync(userId, "world_traveler");
            }

            return newBadgeAssigned;
        }

        private async Task<bool> AssignBadgeIfNotExistsAsync(string userId, string badgeCode)
        {
            var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == badgeCode);
            if (badge == null) return false;

            var userBadge = await _context.UserBadges
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BadgeId == badge.Id);

            if (userBadge == null)
            {
                _context.UserBadges.Add(new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.Id,
                    AchievedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<Badge>> GetUserBadgesAsync(string userId)
        {
            return await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.Badge)
                .ToListAsync();
        }
    }
}