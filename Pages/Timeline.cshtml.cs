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
using WanderGlobe.Models;          // For ViewModels like VisitedCityViewModel, PhotoViewModel
using WanderGlobe.Models.Custom; // For TimelineEntry, TimelinePhoto, AND our standard TimelineWeather
using WanderGlobe.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace WanderGlobe.Pages
{
    public class TimelineModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICityService _cityService;
        private readonly IWeatherService _weatherService;
        private readonly IPhotoService _photoService;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVisitedCityService _visitedCityService;
        private readonly ILogger<TimelineModel> _logger;

        public TimelineModel(
            ApplicationDbContext context,
            ICityService cityService,
            IWeatherService weatherService,
            IPhotoService photoService,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager,
            IVisitedCityService visitedCityService,
            ILogger<TimelineModel> logger)
        {
            _context = context;
            _cityService = cityService;
            _weatherService = weatherService;
            _photoService = photoService;
            _environment = environment;
            _userManager = userManager;
            _visitedCityService = visitedCityService;
            _logger = logger;
        }

        public List<VisitedCityViewModel> VisitedCityEntries { get; set; } = new List<VisitedCityViewModel>();
        public Dictionary<int, List<VisitedCityViewModel>> GroupedVisits { get; set; } = new Dictionary<int, List<VisitedCityViewModel>>();
        public List<int> VisitYears { get; set; } = new List<int>();
        public List<string> Continents { get; set; } = new List<string>();
        public Dictionary<string, TimelineWeather> WeatherData { get; set; } = new Dictionary<string, TimelineWeather>();

        public async Task OnGetAsync(string? highlightCityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Timeline: User not authenticated. Displaying empty timeline.");
                return;
            }

            _logger.LogInformation("Timeline: Fetching visited cities for user {UserId}", user.Id);
            VisitedCityEntries = await _visitedCityService.GetVisitedCitiesForUserAsync(user.Id);

            if (VisitedCityEntries.Any())
            {
                _logger.LogInformation("Timeline: Found {Count} visited city entries for user {UserId}", VisitedCityEntries.Count, user.Id);
                GroupedVisits = VisitedCityEntries
                    .GroupBy(v => v.VisitDate.Year)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(v => v.VisitDate).ToList());

                VisitYears = VisitedCityEntries
                    .Select(v => v.VisitDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();

                Continents = VisitedCityEntries
                    .Where(v => !string.IsNullOrEmpty(v.Continent))
                    .Select(v => v.Continent!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                await LoadWeatherDataForCitiesAsync();
            }
            else
            {
                _logger.LogInformation("Timeline: No visited city entries found for user {UserId}", user.Id);
            }

            if (!string.IsNullOrEmpty(highlightCityId) && int.TryParse(highlightCityId, out int cityIdToHighlight))
            {
                ViewData["HighlightCityId"] = cityIdToHighlight;
            }
        }

        private async Task LoadWeatherDataForCitiesAsync()
        {
            _logger.LogInformation("Timeline: Loading weather data for {Count} visited city entries.", VisitedCityEntries.Count);
            foreach (var visit in VisitedCityEntries)
            {
                string weatherKey = $"{visit.CityId}_{visit.VisitDate:yyyyMMdd}";
                if (!WeatherData.ContainsKey(weatherKey))
                {
                    try
                    {
                        TimelineWeather? weatherResponse = await _weatherService.GetCurrentWeatherAsync(visit.Latitude, visit.Longitude);
                        if (weatherResponse != null)
                        {
                            WeatherData[weatherKey] = weatherResponse;
                        }
                        else
                        {
                            WeatherData[weatherKey] = new TimelineWeather { Condition = "Meteo N/D", Temperature = 20, Month = visit.VisitDate.Month };
                            _logger.LogWarning("Timeline: Weather data not found for CityId {CityId} on {Date}", visit.CityId, visit.VisitDate.ToShortDateString());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Timeline: Error fetching weather for CityId {CityId}, Lat: {Latitude}, Lng: {Longitude}", visit.CityId, visit.Latitude, visit.Longitude);
                        WeatherData[weatherKey] = new TimelineWeather { Condition = "Errore Meteo", Temperature = 20, Month = visit.VisitDate.Month };
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostEditVisitAsync(
            int visitedCityRecordId,
            DateTime visitDate,
            string? visitNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("OnPostEditVisitAsync: User not authenticated.");
                return Unauthorized();
            }

            _logger.LogInformation("OnPostEditVisitAsync: Attempting to update visit {RecordId} for user {UserId}", visitedCityRecordId, user.Id);

            var success = await _visitedCityService.UpdateVisitedCityAsync(visitedCityRecordId, user.Id, visitDate, visitNotes);

            if (!success)
            {
                _logger.LogWarning("OnPostEditVisitAsync: Failed to update visit {RecordId} for user {UserId}", visitedCityRecordId, user.Id);
                TempData["ErrorMessage"] = "Impossibile aggiornare la visita. Potrebbe essere stata rimossa o si è verificato un errore.";
                await OnGetAsync(null);
                return Page();
            }

            _logger.LogInformation("OnPostEditVisitAsync: Successfully updated visit {RecordId} for user {UserId}", visitedCityRecordId, user.Id);
            TempData["SuccessMessage"] = "Visita aggiornata con successo!";

            var editedVisit = await _visitedCityService.GetVisitedCityByIdAsync(visitedCityRecordId, user.Id);
            return RedirectToPage(new { highlightCity = editedVisit?.CityId.ToString() });
        }

        public TimelineWeather GetWeatherForCityVisit(int cityId, DateTime visitDate)
        {
            string weatherKey = $"{cityId}_{visitDate:yyyyMMdd}";
            if (WeatherData.TryGetValue(weatherKey, out var weather))
            {
                return weather;
            }
            return new TimelineWeather { Condition = "N/D", Temperature = 20, Month = visitDate.Month };
        }

        public async Task<List<PhotoViewModel>> GetPhotosForVisitedCityAsync(int visitedCityRecordId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new List<PhotoViewModel>();

            return await _photoService.GetPhotosForVisitedCityAsync(visitedCityRecordId, user.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUploadPhotoAsync(IFormFile photoFile, int visitedCityRecordId)
        {
            if (photoFile == null || photoFile.Length == 0)
                return new JsonResult(new { success = false, message = "Nessun file caricato." });

            if (visitedCityRecordId <= 0)
                return new JsonResult(new { success = false, message = "ID visita non valido." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return new JsonResult(new { success = false, message = "Utente non autenticato." });

            _logger.LogInformation("OnPostUploadPhotoAsync: Attempting to upload photo for visit {RecordId}, user {UserId}", visitedCityRecordId, user.Id);

            var visit = await _context.VisitedCities.AsNoTracking()
                                  .FirstOrDefaultAsync(vc => vc.Id == visitedCityRecordId && vc.UserId == user.Id);
            if (visit == null)
            {
                _logger.LogWarning("OnPostUploadPhotoAsync: User {UserId} does not own visit record {RecordId} or it does not exist.", user.Id, visitedCityRecordId);
                return new JsonResult(new { success = false, message = "Visita non trovata o non autorizzata." });
            }

            try
            {
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(photoFile.FileName)}";
                string userVisitPhotoFolder = Path.Combine("images", "user_photos", user.Id, $"visit_{visitedCityRecordId}");
                string uploadsFolderServerPath = Path.Combine(_environment.WebRootPath, userVisitPhotoFolder);

                Directory.CreateDirectory(uploadsFolderServerPath);

                string filePathOnServer = Path.Combine(uploadsFolderServerPath, uniqueFileName);
                string urlPathForDb = $"/{userVisitPhotoFolder}/{uniqueFileName}".Replace(Path.DirectorySeparatorChar, '/');

                using (var stream = new FileStream(filePathOnServer, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }
                _logger.LogInformation("OnPostUploadPhotoAsync: Photo saved to {FilePath}", filePathOnServer);

                var newPhoto = new Photo
                {
                    UserId = user.Id,
                    FileName = photoFile.FileName,
                    Url = urlPathForDb,
                    Caption = Path.GetFileNameWithoutExtension(photoFile.FileName),
                    UploadDate = DateTime.UtcNow,
                    VisitedCityId = visitedCityRecordId,
                };

                _context.Photos.Add(newPhoto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("OnPostUploadPhotoAsync: Photo record ID {PhotoId} created and linked to visit {RecordId}.", newPhoto.Id, visitedCityRecordId);

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
                _logger.LogError(ex, "OnPostUploadPhotoAsync: Error uploading photo for visit {RecordId}, user {UserId}", visitedCityRecordId, user.Id);
                return new JsonResult(new { success = false, message = $"Errore server durante l'upload: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetVisitPhotosAsync(int visitedCityRecordId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new JsonResult(new List<PhotoViewModel>());

            if (visitedCityRecordId <= 0) return new JsonResult(new List<PhotoViewModel>());

            _logger.LogInformation("OnGetVisitPhotosAsync: Fetching photos for visit {RecordId}, user {UserId}", visitedCityRecordId, user.Id);
            var photos = await _photoService.GetPhotosForVisitedCityAsync(visitedCityRecordId, user.Id);

            return new JsonResult(photos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeletePhotoAsync(int photoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return new JsonResult(new { success = false, message = "Utente non autenticato." });

            _logger.LogInformation("OnPostDeletePhotoAsync: Attempting to delete photo {PhotoId} for user {UserId}", photoId, user.Id);
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == user.Id);

            if (photo == null)
            {
                _logger.LogWarning("OnPostDeletePhotoAsync: Photo {PhotoId} not found or user {UserId} not authorized.", photoId, user.Id);
                return new JsonResult(new { success = false, message = "Foto non trovata o non autorizzata." });
            }

            if (!string.IsNullOrEmpty(photo.Url))
            {
                string webRootPath = _environment.WebRootPath;
                string fullFilePath = Path.Combine(webRootPath, photo.Url.TrimStart('/'));

                if (System.IO.File.Exists(fullFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(fullFilePath);
                        _logger.LogInformation("OnPostDeletePhotoAsync: Successfully deleted physical file {FilePath}", fullFilePath);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "OnPostDeletePhotoAsync: Error deleting physical file {FilePath}", fullFilePath);
                    }
                }
                else
                {
                    _logger.LogWarning("OnPostDeletePhotoAsync: Physical file not found at {FilePath} for photo {PhotoId}", fullFilePath, photoId);
                }
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
            _logger.LogInformation("OnPostDeletePhotoAsync: Successfully deleted photo record {PhotoId} for user {UserId}", photoId, user.Id);
            return new JsonResult(new { success = true });
        }
    }
}