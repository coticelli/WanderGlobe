// File: Services/TravelJournalService.cs
using WanderGlobe.Data;
using WanderGlobe.Models;          // For PhotoViewModel, VisitedCityViewModel
using WanderGlobe.Models.Custom; // For TimelineEntry, TimelinePhoto, TimelineWeather, TimelineNote
using WanderGlobe.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace WanderGlobe.Services
{
    public class TravelJournalService : ITravelJournalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVisitedCityService _visitedCityService;
        private readonly IWeatherService _weatherService;
        private readonly IPhotoService _photoService;
        private readonly ILogger<TravelJournalService> _logger;

        public TravelJournalService(
            ApplicationDbContext context,
            IVisitedCityService visitedCityService,
            IWeatherService weatherService,
            IPhotoService photoService,
            ILogger<TravelJournalService> logger)
        {
            _context = context;
            _visitedCityService = visitedCityService;
            _weatherService = weatherService;
            _photoService = photoService;
            _logger = logger;
        }

        public async Task<List<TimelineEntry>> GetTimelineByUserAsync(string userId, string sort = "desc")
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetTimelineByUserAsync called with null or empty userId.");
                return new List<TimelineEntry>();
            }
            _logger.LogInformation("Fetching real timeline entries for user {UserId}", userId);

            List<VisitedCityViewModel> visitedCities = await _visitedCityService.GetVisitedCitiesForUserAsync(userId);

            if (visitedCities == null || !visitedCities.Any())
            {
                _logger.LogInformation("No visited cities found for user {UserId} to build timeline.", userId);
                return new List<TimelineEntry>();
            }

            var timelineEntries = new List<TimelineEntry>();

            foreach (var visitVM in visitedCities)
            {
                var entry = new TimelineEntry
                {
                    Id = visitVM.VisitedCityRecordId,
                    UserId = userId,
                    CountryId = visitVM.CountryId,
                    CountryName = visitVM.CountryName,
                    CountryCode = visitVM.CountryCode,
                    CityName = visitVM.CityName,
                    VisitDate = visitVM.VisitDate,
                    Notes = visitVM.Description ?? string.Empty,
                    Photos = new List<TimelinePhoto>()
                };

                List<PhotoViewModel> photosForVisit = await _photoService.GetPhotosForVisitedCityAsync(visitVM.VisitedCityRecordId, userId);
                if (photosForVisit != null)
                {
                    entry.Photos = photosForVisit.Select(p => new TimelinePhoto
                    {
                        Id = p.Id,
                        Url = p.Url,
                        Caption = p.Caption ?? string.Empty,
                        UploadDate = DateTime.MinValue // Placeholder: Add UploadDate to PhotoViewModel if needed
                    }).ToList();
                }

                TimelineWeather? weatherResponse = await _weatherService.GetCurrentWeatherAsync(visitVM.Latitude, visitVM.Longitude);
                if (weatherResponse != null)
                {
                    entry.Weather = weatherResponse; // Direct assignment as types match
                }
                else
                {
                    entry.Weather = new TimelineWeather // Default Models.Custom.TimelineWeather
                    {
                        Month = visitVM.VisitDate.Month,
                        Temperature = 20,
                        Condition = "N/D",
                        IconUrl = null
                    };
                }
                timelineEntries.Add(entry);
            }

            timelineEntries = sort.ToLower() == "asc"
                ? timelineEntries.OrderBy(e => e.VisitDate).ToList()
                : timelineEntries.OrderByDescending(e => e.VisitDate).ToList();

            _logger.LogInformation("Returning {Count} real timeline entries for user {UserId}", timelineEntries.Count, userId);
            return timelineEntries;
        }

        public async Task<List<int>> GetVisitedYearsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<int>();
            var years = await _context.VisitedCities
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.VisitDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            return years;
        }

        public async Task<bool> AddJournalNoteAsync(TimelineNote note)
        {
            _logger.LogInformation("AddJournalNoteAsync called (placeholder) for User: {UserId}, CountryId: {CountryId}", note.UserId, note.CountryId);
            // TODO: Implement actual DB saving for notes, likely linked to VisitedCityId
            return await Task.FromResult(false);
        }

        public async Task<bool> AddPhotoAsync(int countryId, string userId, string caption, string imageUrl)
        {
            _logger.LogWarning("AddPhotoAsync in TravelJournalService is likely obsolete. Photo uploads via PageModel. Called for CountryId: {CountryId}", countryId);
            // TODO: Review if this method is needed; photo uploads are in TimelineModel.cs
            return await Task.FromResult(false);
        }
    }
}