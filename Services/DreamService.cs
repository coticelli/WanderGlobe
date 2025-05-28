// Services/DreamService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models; // Make sure this using statement is present and correct

namespace WanderGlobe.Services
{
    public class DreamService : IDreamService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DreamService> _logger;

        public DreamService(ApplicationDbContext context, ILogger<DreamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<DreamDestination>> GetUserWishlistAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<DreamDestination>();

            return await _context.DreamDestinations
                .Where(dd => dd.UserId == userId)
                .Include(dd => dd.City)
                    .ThenInclude(c => c!.Country) // Use ! if you expect City to not be null here due to filtering
                .Include(dd => dd.Country)
                .OrderByDescending(dd => dd.AddedDate)
                .ToListAsync();
        }

        public async Task AddToWishlistAsync(DreamDestination dream)
        {
            if (dream == null) throw new ArgumentNullException(nameof(dream));

            // Optional: Check for duplicates
            var existing = await _context.DreamDestinations
                .FirstOrDefaultAsync(dd => dd.UserId == dream.UserId &&
                                            (dd.CityId.HasValue && dd.CityId == dream.CityId ||
                                             dd.CountryId.HasValue && dd.CountryId == dream.CountryId && !dream.CityId.HasValue) && // Dream for same country (no city specified)
                                             dd.DestinationName == dream.DestinationName); // Or just by name if that should be unique per user

            if (existing != null)
            {
                _logger.LogWarning($"Dream destination '{dream.DestinationName}' already exists for user {dream.UserId}.");
                throw new ArgumentException("Questa destinazione è già nella tua lista dei sogni.");
            }

            _context.DreamDestinations.Add(dream);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Added dream '{dream.DestinationName}' for user {dream.UserId}.");
        }

        public async Task RemoveFromWishlistAsync(int dreamId, string userId)
        {
            var dream = await _context.DreamDestinations
                .FirstOrDefaultAsync(dd => dd.Id == dreamId && dd.UserId == userId);

            if (dream != null)
            {
                _context.DreamDestinations.Remove(dream);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Removed dream ID {dreamId} for user {userId}.");
            }
            else
            {
                _logger.LogWarning($"Attempted to remove non-existent or unauthorized dream ID {dreamId} for user {userId}.");
            }
        }

        public async Task<bool> IsCityInUserWishlistAsync(int cityId, string userId)
        {
            if (cityId <= 0 || string.IsNullOrEmpty(userId)) return false;

            return await _context.DreamDestinations
                .AnyAsync(dd => dd.UserId == userId && dd.CityId == cityId);
        }

        public async Task<DreamDestination?> GetDreamByIdAsync(int dreamId)
        {
            return await _context.DreamDestinations
                .Include(dd => dd.City)
                .Include(dd => dd.Country)
                .FirstOrDefaultAsync(dd => dd.Id == dreamId);
        }

        public async Task UpdateDreamAsync(DreamDestination dream)
        {
            if (dream == null) throw new ArgumentNullException(nameof(dream));
            _context.DreamDestinations.Update(dream);
            await _context.SaveChangesAsync();
             _logger.LogInformation($"Updated dream ID {dream.Id}.");
        }

        // Implementations for DreamCountry
        public async Task<List<DreamCountry>> GetUserDreamCountriesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<DreamCountry>();
            return await _context.DreamCountries
                .Where(dc => dc.UserId == userId)
                .Include(dc => dc.Country)
                .OrderByDescending(dc => dc.DateAdded)
                .ToListAsync();
        }

        public async Task AddDreamCountryAsync(DreamCountry dreamCountry)
        {
            if (dreamCountry == null) throw new ArgumentNullException(nameof(dreamCountry));

            var existing = await _context.DreamCountries
                .FirstOrDefaultAsync(dc => dc.UserId == dreamCountry.UserId && dc.CountryId == dreamCountry.CountryId);
            if(existing != null)
            {
                _logger.LogWarning($"Dream country ID {dreamCountry.CountryId} already exists for user {dreamCountry.UserId}.");
                throw new ArgumentException("Questo paese è già nella tua lista dei sogni.");
            }
            _context.DreamCountries.Add(dreamCountry);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Added dream country ID {dreamCountry.CountryId} for user {dreamCountry.UserId}.");
        }

        public async Task RemoveDreamCountryAsync(int dreamCountryId, string userId)
        {
             var dream = await _context.DreamCountries
                .FirstOrDefaultAsync(dc => dc.Id == dreamCountryId && dc.UserId == userId);
            if (dream != null)
            {
                _context.DreamCountries.Remove(dream);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Removed dream country ID {dreamCountryId} for user {userId}.");
            }
            else
            {
                 _logger.LogWarning($"Attempted to remove non-existent or unauthorized dream country ID {dreamCountryId} for user {userId}.");
            }
        }
    }
}