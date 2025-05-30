// File: Pages/Timeline.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;
using WanderGlobe.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace WanderGlobe.Pages
{
    // TimelineDisplayEntryViewModel and TimelineWeather remain the same as your last provided version

    public class TimelineDisplayEntryViewModel
    {
        public int VisitedCityRecordId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string? Continent { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime VisitDate { get; set; }
        public string? Description { get; set; }
        public string? CitySpecificImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PhotoViewModel> Photos { get; set; } = new List<PhotoViewModel>();
        public TimelineWeather? Weather { get; set; }
    }

    public class TimelineWeather
    {
        public string Condition { get; set; } = "N/D";
        public double Temperature { get; set; } = 20;
        public int Month { get; set; }
        public string? IconUrl { get; set; }
    }

    public class TimelineModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICityService _cityService;
        private readonly IWeatherService _weatherService;
        private readonly IPhotoService _photoService;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TimelineModel> _logger;

        public Dictionary<int, List<TimelineDisplayEntryViewModel>> GroupedVisits { get; set; } = new Dictionary<int, List<TimelineDisplayEntryViewModel>>();
        public List<int> VisitYears { get; set; } = new List<int>();
        public List<string> Continents { get; set; } = new List<string>();

        [BindProperty] public int Edit_VisitedCityRecordId { get; set; }
        [BindProperty] public DateTime Edit_VisitDate { get; set; }
        [BindProperty] public string? Edit_VisitNotes { get; set; }
        [BindProperty] public IFormFile? Upload_PhotoFile { get; set; }
        [BindProperty] public int Upload_VisitedCityRecordId { get; set; }

        public TimelineModel(
            ApplicationDbContext context, ICityService cityService, IWeatherService weatherService,
            IPhotoService photoService, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager,
            ILogger<TimelineModel> logger)
        {
            _context = context; _cityService = cityService; _weatherService = weatherService;
            _photoService = photoService; _environment = environment; _userManager = userManager;
            _logger = logger;
        }

        public async Task OnGetAsync(string? highlightVisitedCityRecordId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { _logger.LogWarning("Timeline OnGetAsync: User not authenticated."); return; }

            _logger.LogInformation("Timeline OnGetAsync: User {UserId} authenticated. Fetching visited cities.", user.Id);
            var userVisitedCities = await _cityService.GetVisitedCitiesByUserAsync(user.Id); // Returns List<VisitedCity>

            _logger.LogInformation("Timeline OnGetAsync: _cityService.GetVisitedCitiesByUserAsync returned {Count} entries.", userVisitedCities.Count);

            if (!userVisitedCities.Any())
            {
                _logger.LogInformation("Timeline OnGetAsync: No VisitedCity records found for user {UserId}. Timeline will be empty.", user.Id);
                GroupedVisits = new Dictionary<int, List<TimelineDisplayEntryViewModel>>();
                VisitYears = new List<int>();
                Continents = new List<string>();
                return;
            }

            var allTimelineEntries = new List<TimelineDisplayEntryViewModel>();
            int skippedEntries = 0;
            foreach (var vc in userVisitedCities) // vc is a VisitedCity entity
            {
                if (vc.City == null) { _logger.LogWarning("Timeline OnGetAsync: Skipping VisitedCity record ID {RecordId} because vc.City is null.", vc.Id); skippedEntries++; continue; }
                if (vc.City.Country == null) { _logger.LogWarning("Timeline OnGetAsync: Skipping VisitedCity record ID {RecordId} for City '{CityName}' because vc.City.Country is null.", vc.Id, vc.City.Name); skippedEntries++; continue; }
                if (!vc.City.Latitude.HasValue) { _logger.LogWarning("Timeline OnGetAsync: Skipping VisitedCity record ID {RecordId} for City '{CityName}' because vc.City.Latitude is null.", vc.Id, vc.City.Name); skippedEntries++; continue; }
                if (!vc.City.Longitude.HasValue) { _logger.LogWarning("Timeline OnGetAsync: Skipping VisitedCity record ID {RecordId} for City '{CityName}' because vc.City.Longitude is null.", vc.Id, vc.City.Name); skippedEntries++; continue; }

                _logger.LogDebug("Timeline OnGetAsync: Processing VisitedCity ID {VcId}, City: {CityName}, Country: {CountryName}", vc.Id, vc.City.Name, vc.City.Country.Name);

                allTimelineEntries.Add(new TimelineDisplayEntryViewModel
                {
                    VisitedCityRecordId = vc.Id,
                    CityId = vc.CityId,
                    CityName = vc.City.Name,
                    CountryId = vc.City.CountryId,
                    CountryName = vc.City.Country.Name,
                    CountryCode = vc.City.Country.Code ?? "??",
                    Continent = vc.City.Country.Continent ?? "Sconosciuto",
                    Latitude = vc.City.Latitude.Value,
                    Longitude = vc.City.Longitude.Value,
                    VisitDate = vc.VisitDate,
                    Description = vc.Notes,
                    CitySpecificImage = GenerateCitySpecificImagePath(vc.City),
                    CreatedAt = vc.CreatedAt,
                    UpdatedAt = vc.UpdatedAt,
                    Photos = await _photoService.GetPhotosForVisitedCityAsync(vc.Id, user.Id),
                    Weather = await FetchAndMapWeatherAsync(vc.City.Latitude.Value, vc.City.Longitude.Value, vc.VisitDate)
                });
            }

            if (skippedEntries > 0) _logger.LogWarning("Timeline OnGetAsync: Skipped {SkippedCount} entries due to missing data.", skippedEntries);
            _logger.LogInformation("Timeline OnGetAsync: Mapped to {Count} TimelineDisplayEntryViewModel entries.", allTimelineEntries.Count);


            if (allTimelineEntries.Any())
            {
                allTimelineEntries = allTimelineEntries.OrderByDescending(t => t.VisitDate).ToList();
                GroupedVisits = allTimelineEntries.GroupBy(v => v.VisitDate.Year).ToDictionary(g => g.Key, g => g.ToList());
                VisitYears = allTimelineEntries.Select(v => v.VisitDate.Year).Distinct().OrderByDescending(y => y).ToList();
                Continents = allTimelineEntries.Where(v => !string.IsNullOrEmpty(v.Continent)).Select(v => v.Continent!).Distinct().OrderBy(c => c).ToList();

                _logger.LogInformation("Timeline OnGetAsync: GroupedVisits contains {Count} years. First year: {FirstYear}", GroupedVisits.Count, VisitYears.FirstOrDefault());
                
                // Additional debug logging
                foreach (var group in GroupedVisits)
                {
                    _logger.LogInformation("Year {Year} has {Count} visits", group.Key, group.Value.Count);
                    foreach (var visit in group.Value)
                    {
                        _logger.LogInformation("  - Visit: {CityName}, {CountryName}, RecordId: {RecordId}", 
                            visit.CityName, visit.CountryName, visit.VisitedCityRecordId);
                    }
                }
            }
            else
            {
                _logger.LogInformation("Timeline OnGetAsync: No valid timeline entries to display after mapping and filtering.");
                GroupedVisits = new Dictionary<int, List<TimelineDisplayEntryViewModel>>();
                VisitYears = new List<int>();
                Continents = new List<string>();
            }


            if (!string.IsNullOrEmpty(highlightVisitedCityRecordId) && int.TryParse(highlightVisitedCityRecordId, out int recordIdToHighlight))
            {
                ViewData["HighlightVisitedCityRecordId"] = recordIdToHighlight;
            }
        }

        private string? GenerateCitySpecificImagePath(City city)
        {
            // Matches Globe.cshtml logic for consistency, adjust as needed
            return $"~/images/cities/{city.Country?.Code?.ToLowerInvariant() ?? "unknown"}-city.jpg";
        }

        private async Task<TimelineWeather?> FetchAndMapWeatherAsync(double latitude, double longitude, DateTime visitDate)
        {
            try
            {
                var weatherServiceResponse = await _weatherService.GetCurrentWeatherAsync(latitude, longitude);
                if (weatherServiceResponse != null)
                {
                    return new TimelineWeather
                    {
                        Condition = weatherServiceResponse.Condition,
                        Temperature = weatherServiceResponse.Temperature,
                        Month = visitDate.Month,
                        IconUrl = weatherServiceResponse.IconUrl
                    };
                }
                _logger.LogWarning("FetchAndMapWeatherAsync: Weather data not found via service for Lat {Lat}, Lng {Lng}", latitude, longitude);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather for Lat {Lat}, Lng {Lng}, Date {Date}", latitude, longitude, visitDate);
            }
            return new TimelineWeather { Condition = "Meteo N/D", Temperature = 20, Month = visitDate.Month };
        }

        public TimelineWeather GetWeatherForCityVisit(int cityId, DateTime visitDate) // This might be redundant if weather is preloaded into ViewModel
        {
            _logger.LogDebug("GetWeatherForCityVisit called for CityId {CityId}, Date {VisitDate}", cityId, visitDate);
            // This method is less efficient if called repeatedly from Razor. Prefer pre-loading.
            // For now, it tries to find an already processed entry.
            var entry = GroupedVisits.SelectMany(kvp => kvp.Value)
                                     .FirstOrDefault(e => e.CityId == cityId && e.VisitDate.Date == visitDate.Date);
            if (entry?.Weather != null)
            {
                return entry.Weather;
            }
            _logger.LogWarning("GetWeatherForCityVisit: Weather not pre-loaded or found for CityId {CityId} on {Date}", cityId, visitDate.ToShortDateString());
            return new TimelineWeather { Condition = "N/D (lookup)", Temperature = 20, Month = visitDate.Month };
        }

        // ... (OnPostEditVisitAsync, Photo handlers remain the same) ...
        public async Task<IActionResult> OnPostEditVisitAsync(
            int visitedCityRecordId, // Renamed Edit_VisitedCityRecordId to match parameter
            DateTime visitDate,      // Renamed Edit_VisitDate
            string? visitNotes)      // Renamed Edit_VisitNotes
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            _logger.LogInformation("OnPostEditVisitAsync: Attempting to update VisitedCity record {RecordId} for user {UserId}", visitedCityRecordId, user.Id);
            var success = await _cityService.UpdateVisitedCityAsync(visitedCityRecordId, user.Id, visitDate, visitNotes);

            if (!success)
            {
                _logger.LogWarning("OnPostEditVisitAsync failed for RecordId {RecordId}", visitedCityRecordId);
                TempData["ErrorMessage"] = "Impossibile aggiornare la visita.";
                await OnGetAsync(null);
                return Page();
            }
            _logger.LogInformation("OnPostEditVisitAsync succeeded for RecordId {RecordId}", visitedCityRecordId);
            TempData["SuccessMessage"] = "Visita aggiornata con successo!";
            return RedirectToPage(new { highlightVisitedCityRecordId = visitedCityRecordId.ToString() });
        }

        public async Task<List<PhotoViewModel>> GetPhotosForVisitedCityAsync(int visitedCityRecordId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new List<PhotoViewModel>();
            return await _photoService.GetPhotosForVisitedCityAsync(visitedCityRecordId, user.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUploadPhotoAsync() // Parameters come from BindProperty
        {
            if (Upload_PhotoFile == null || Upload_PhotoFile.Length == 0)
                return new JsonResult(new { success = false, message = "Nessun file caricato." });
            if (Upload_VisitedCityRecordId <= 0)
                return new JsonResult(new { success = false, message = "ID visita non valido." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return new JsonResult(new { success = false, message = "Utente non autenticato." });

            var visit = await _context.VisitedCities.AsNoTracking()
                                  .FirstOrDefaultAsync(vc => vc.Id == Upload_VisitedCityRecordId && vc.UserId == user.Id);
            if (visit == null)
                return new JsonResult(new { success = false, message = "Visita non trovata o non autorizzata." });

            try
            {
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(Upload_PhotoFile.FileName)}";
                string userVisitPhotoFolder = Path.Combine("images", "user_photos", user.Id, $"visited_city_{Upload_VisitedCityRecordId}");
                string uploadsFolderServerPath = Path.Combine(_environment.WebRootPath, userVisitPhotoFolder);
                Directory.CreateDirectory(uploadsFolderServerPath);
                string filePathOnServer = Path.Combine(uploadsFolderServerPath, uniqueFileName);
                string urlPathForDb = $"/{userVisitPhotoFolder}/{uniqueFileName}".Replace(Path.DirectorySeparatorChar, '/');

                using (var stream = new FileStream(filePathOnServer, FileMode.Create))
                {
                    await Upload_PhotoFile.CopyToAsync(stream);
                }

                var newPhoto = new Photo
                {
                    UserId = user.Id,
                    FileName = Upload_PhotoFile.FileName,
                    Url = urlPathForDb,
                    Caption = Path.GetFileNameWithoutExtension(Upload_PhotoFile.FileName),
                    UploadDate = DateTime.UtcNow,
                    VisitedCityId = Upload_VisitedCityRecordId,
                };
                _context.Photos.Add(newPhoto);
                await _context.SaveChangesAsync();

                return new JsonResult(new
                {
                    success = true,
                    photoId = newPhoto.Id,
                    photoUrl = newPhoto.Url,
                    caption = newPhoto.Caption
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostUploadPhotoAsync error for visit {VisitId}", Upload_VisitedCityRecordId);
                return new JsonResult(new { success = false, message = $"Errore server durante l'upload: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetVisitPhotosAsync(int visitedCityRecordId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new JsonResult(new List<PhotoViewModel>());
            if (visitedCityRecordId <= 0) return new JsonResult(new List<PhotoViewModel>());
            var photos = await _photoService.GetPhotosForVisitedCityAsync(visitedCityRecordId, user.Id);
            return new JsonResult(photos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeletePhotoAsync(int photoId) // Assuming photoId is passed in form/AJAX
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new JsonResult(new { success = false, message = "Utente non autenticato." });

            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == user.Id);
            if (photo == null) return new JsonResult(new { success = false, message = "Foto non trovata o non autorizzata." });

            if (!string.IsNullOrEmpty(photo.Url))
            {
                string webRootPath = _environment.WebRootPath;
                string fullFilePath = Path.Combine(webRootPath, photo.Url.TrimStart('/'));
                if (System.IO.File.Exists(fullFilePath))
                {
                    try { System.IO.File.Delete(fullFilePath); }
                    catch (IOException ex) { _logger.LogError(ex, "Error deleting physical file {FilePath}", fullFilePath); }
                }
            }
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }
    }
}