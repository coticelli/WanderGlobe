// Folder: Services
// File: VisitedCityService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models; // Or WanderGlobe.ViewModels if VisitedCityViewModel is there

namespace WanderGlobe.Services
{
    public class VisitedCityService : IVisitedCityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VisitedCityService> _logger;

        public VisitedCityService(ApplicationDbContext context, ILogger<VisitedCityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<VisitedCityViewModel>> GetVisitedCitiesForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<VisitedCityViewModel>();
            }

            try
            {
                var visitedCities = await _context.VisitedCities // Assuming your DbSet is named VisitedCities
                    .Where(vc => vc.UserId == userId)
                    .Include(vc => vc.City)                 // Eager load City
                        .ThenInclude(city => city.Country) // Then eager load Country from City
                    .OrderByDescending(vc => vc.VisitDate)
                    .ThenBy(vc => vc.City != null ? vc.City.Name : "") // Secondary sort by city name
                    .Select(vc => new VisitedCityViewModel // Project to ViewModel
                    {
                        VisitedCityRecordId = vc.Id, // This is the PK of VisitedCity table
                        CityId = vc.CityId,
                        CityName = vc.City != null ? vc.City.Name : "N/A",
                        CountryId = vc.City != null && vc.City.Country != null ? vc.City.Country.Id : 0,
                        CountryName = vc.City != null && vc.City.Country != null ? vc.City.Country.Name : "N/A",
                        CountryCode = vc.City != null && vc.City.Country != null ? vc.City.Country.Code : "N/A",
                        Continent = vc.City != null && vc.City.Country != null ? vc.City.Country.Continent : null,
                        Latitude = vc.City != null && vc.City.Latitude.HasValue ? vc.City.Latitude.Value : 0,
                        Longitude = vc.City != null && vc.City.Longitude.HasValue ? vc.City.Longitude.Value : 0,
                        VisitDate = vc.VisitDate,
                        Description = vc.Notes,
                        CreatedAt = vc.CreatedAt,
                        UpdatedAt = vc.UpdatedAt,
                        // CitySpecificImage logic would go here if you have it
                        // e.g., CitySpecificImage = vc.City != null ? DetermineCityImage(vc.City) : null,
                    })
                    .ToListAsync();

                return visitedCities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching visited cities for User ID {UserId}", userId);
                return new List<VisitedCityViewModel>(); // Return empty list on error
            }
        }

        public async Task<int> AddVisitedCityAsync(string userId, int cityId, DateTime visitDate, string? notes)
        {
            if (string.IsNullOrEmpty(userId) || cityId <= 0)
            {
                _logger.LogWarning("AddVisitedCityAsync: Invalid parameters. UserId: {UserId}, CityId: {CityId}", userId, cityId);
                return 0; // Indicate failure
            }

            var cityExists = await _context.Cities.AnyAsync(c => c.Id == cityId);
            if (!cityExists)
            {
                _logger.LogWarning("AddVisitedCityAsync: City with ID {CityId} not found.", cityId);
                throw new ArgumentException("Città specificata non trovata.");
            }

            // Optional: Check for duplicate exact visit (same user, city, date)
            bool alreadyVisitedOnDate = await _context.VisitedCities
               .AnyAsync(vc => vc.UserId == userId && vc.CityId == cityId && vc.VisitDate.Date == visitDate.Date);
            if (alreadyVisitedOnDate)
            {
                 _logger.LogInformation("AddVisitedCityAsync: User {UserId} already has a visit record for City {CityId} on {VisitDate}.", userId, cityId, visitDate.Date);
                 throw new InvalidOperationException("Hai già registrato una visita a questa città in questa data.");
            }

            var newVisitedCity = new VisitedCity
            {
                UserId = userId,
                CityId = cityId,
                VisitDate = visitDate,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.VisitedCities.Add(newVisitedCity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added visited city record ID {RecordId} for User {UserId}, City {CityId}", newVisitedCity.Id, userId, cityId);
                return newVisitedCity.Id; // Return the new record's ID
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error adding visited city for User {UserId}, City {CityId}", userId, cityId);
                throw new Exception("Errore del database durante il salvataggio della visita.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error adding visited city for User {UserId}, City {CityId}", userId, cityId);
                throw;
            }
        }

        public async Task<bool> UpdateVisitedCityAsync(int visitedCityRecordId, string userId, DateTime newVisitDate, string? newNotes)
        {
            if (visitedCityRecordId <= 0 || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateVisitedCityAsync: Invalid parameters. RecordId: {RecordId}, UserId: {UserId}", visitedCityRecordId, userId);
                return false;
            }

            try
            {
                var visit = await _context.VisitedCities
                                        .FirstOrDefaultAsync(vc => vc.Id == visitedCityRecordId && vc.UserId == userId);
                
                if (visit == null)
                {
                    _logger.LogWarning("UpdateVisitedCityAsync: Visit record ID {RecordId} not found for User {UserId}.", visitedCityRecordId, userId);
                    return false; // Not found or not authorized
                }

                visit.VisitDate = newVisitDate;
                visit.Notes = newNotes;
                visit.UpdatedAt = DateTime.UtcNow;

                _context.VisitedCities.Update(visit);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated visited city record ID {RecordId} for User {UserId}.", visitedCityRecordId, userId);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating visited city record ID {RecordId} for User {UserId}.", visitedCityRecordId, userId);
                // Handle concurrency issues, e.g., by checking if the entity still exists
                if (!await _context.VisitedCities.AnyAsync(e => e.Id == visitedCityRecordId))
                {
                    _logger.LogWarning("UpdateVisitedCityAsync: Record {RecordId} was deleted by another user during update attempt.", visitedCityRecordId);
                    return false; // Entity was deleted
                }
                else
                {
                    throw; // Other concurrency conflict
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visited city record ID {RecordId} for User {UserId}.", visitedCityRecordId, userId);
                return false;
            }
        }

        public async Task<bool> RemoveVisitedCityAsync(int visitedCityRecordId, string userId)
        {
             if (visitedCityRecordId <= 0 || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("RemoveVisitedCityAsync: Invalid parameters. RecordId: {RecordId}, UserId: {UserId}", visitedCityRecordId, userId);
                return false;
            }

            try
            {
                var visit = await _context.VisitedCities
                                    .FirstOrDefaultAsync(vc => vc.Id == visitedCityRecordId && vc.UserId == userId);

                if (visit == null)
                {
                    _logger.LogWarning("RemoveVisitedCityAsync: Visit record ID {RecordId} not found for User {UserId}.", visitedCityRecordId, userId);
                    return false; // Not found or not authorized
                }

                // Optional: Handle related entities like photos if they should be cascade deleted
                // or explicitly removed here.
                var photos = await _context.Photos.Where(p => p.VisitedCityId == visitedCityRecordId && p.UserId == userId).ToListAsync();
                if (photos.Any())
                {
                    _context.Photos.RemoveRange(photos);
                    // Note: Physical file deletion for photos would happen here or in PhotoService
                    // For simplicity, I'll assume PhotoService might handle file cleanup if photos are deleted,
                    // or you'd add that logic here.
                }

                _context.VisitedCities.Remove(visit);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully removed visited city record ID {RecordId} for User {UserId}.", visitedCityRecordId, userId);
                return true;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error removing visited city record ID {RecordId} for User {UserId}.", visitedCityRecordId, userId);
                return false;
            }
        }

        public async Task<VisitedCityViewModel?> GetVisitedCityByIdAsync(int visitedCityRecordId, string userId)
        {
            if (visitedCityRecordId <= 0 || string.IsNullOrEmpty(userId))
            {
                return null;
            }

            try
            {
                var visit = await _context.VisitedCities
                    .Where(vc => vc.Id == visitedCityRecordId && vc.UserId == userId)
                    .Include(vc => vc.City)
                        .ThenInclude(city => city.Country)
                    .Select(vc => new VisitedCityViewModel
                    {
                        VisitedCityRecordId = vc.Id,
                        CityId = vc.CityId,
                        CityName = vc.City != null ? vc.City.Name : "N/A",
                        CountryId = vc.City != null && vc.City.Country != null ? vc.City.Country.Id : 0,
                        CountryName = vc.City != null && vc.City.Country != null ? vc.City.Country.Name : "N/A",
                        CountryCode = vc.City != null && vc.City.Country != null ? vc.City.Country.Code : "N/A",
                        Continent = vc.City != null && vc.City.Country != null ? vc.City.Country.Continent : null,
                        Latitude = vc.City != null && vc.City.Latitude.HasValue ? vc.City.Latitude.Value : 0,
                        Longitude = vc.City != null && vc.City.Longitude.HasValue ? vc.City.Longitude.Value : 0,
                        VisitDate = vc.VisitDate,
                        Description = vc.Notes,
                        CreatedAt = vc.CreatedAt,
                        UpdatedAt = vc.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();
                
                return visit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching visited city ID {RecordId} for User ID {UserId}", visitedCityRecordId, userId);
                return null;
            }
        }
    }
}