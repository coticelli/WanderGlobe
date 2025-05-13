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
using WanderGlobe.Models.Custom;
using WanderGlobe.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WanderGlobe.Pages
{
    public class TimelineModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICityService _cityService;
        private readonly IWeatherService _weatherService;
        private readonly IPhotoService _photoService;
        private readonly IWebHostEnvironment _environment;

        public TimelineModel(
            ApplicationDbContext context, 
            ICityService cityService,
            IWeatherService weatherService,
            IPhotoService photoService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _cityService = cityService;
            _weatherService = weatherService;
            _photoService = photoService;
            _environment = environment;
        }

        public List<VisitedCountry> Visits { get; set; } = new List<VisitedCountry>();
        public Dictionary<int, List<VisitedCountry>> GroupedVisits { get; set; } = new Dictionary<int, List<VisitedCountry>>();
        public List<int> VisitYears { get; set; } = new List<int>();
        public List<string> Continents { get; set; } = new List<string>();
        
        // Dictionary to cache weather data
        public Dictionary<int, TimelineWeather> WeatherData { get; set; } = new Dictionary<int, TimelineWeather>();

        public async Task OnGetAsync()
        {
            Visits = await _context.VisitedCountries
                .Include(v => v.Country) 
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();

            GroupedVisits = Visits
                .Where(v => v.Country != null) 
                .GroupBy(v => v.VisitDate.Year)
                .ToDictionary(g => g.Key, g => g.ToList());

            VisitYears = Visits.Select(v => v.VisitDate.Year).Distinct().OrderByDescending(y => y).ToList();

            Continents = await _context.Countries
                .Select(country => country.Continent) 
                .Where(continentName => !string.IsNullOrEmpty(continentName)) 
                .Distinct()
                .OrderBy(continentName => continentName)
                .ToListAsync();
                
            // Get weather data for visited countries
            await LoadWeatherDataAsync();
        }

        private async Task LoadWeatherDataAsync()
        {
            foreach (var visit in Visits)
            {
                if (visit.Country != null && !WeatherData.ContainsKey(visit.CountryId))
                {
                    var weather = await _weatherService.GetCurrentWeatherAsync(
                        visit.Country.Latitude, 
                        visit.Country.Longitude);
                    
                    WeatherData[visit.CountryId] = weather;
                }
            }
        }

        public async Task<IActionResult> OnPostEditVisitAsync(int visitId, DateTime visitDate, string visitNotes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var visit = await _context.VisitedCountries
                .FirstOrDefaultAsync(v => v.CountryId == visitId && v.UserId == userId);

            if (visit == null)
            {
                return NotFound();
            }

            visit.VisitDate = visitDate;
            visit.Notes = visitNotes;
            visit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
        
        public TimelineWeather GetWeatherForCountry(int countryId)
        {
            if (WeatherData.TryGetValue(countryId, out var weather))
            {
                return weather;
            }
            
            // Default weather if not found
            return new TimelineWeather
            {
                Month = DateTime.Now.Month,
                Temperature = 22,
                Condition = "Soleggiato"
            };
        }

        private string GetCurrentUserId()
        {
            return User.Identity?.Name ?? "UnknownUser";
        }
      
        public async Task<string> GetCapitalAsync(string countryCode)
        {
            string originalCountryCodeForDebug = countryCode ?? "NULL_INPUT";
            try
            {
                if (string.IsNullOrEmpty(countryCode))
                {
                    return "DBG:CodeNullOrEmpty";
                }

                string searchCode = countryCode.Trim();
                if (string.IsNullOrEmpty(searchCode))
                {
                    return $"DBG:CodeTrimmedEmpty(Orig:'{originalCountryCodeForDebug}')";
                }

                var italyTestCountry = await _context.Countries.FirstOrDefaultAsync(c => c.Code == "IT");
                string italyTestResult = italyTestCountry == null ? "IT_NotFoundInDB" : $"IT_Found(ID:{italyTestCountry.Id}, Name:{italyTestCountry.Name})";
                
                string searchCodeUpper = searchCode.ToUpper(); 

                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code != null && c.Code.ToUpper() == searchCodeUpper);

                if (country == null)
                {
                    var firstFiveCodes = await _context.Countries.Take(5).Select(c => c.Code ?? "NULL_CODE").ToListAsync();
                    string sampleCodes = string.Join(",", firstFiveCodes);
                    return $"DBG:CountryNull(S_UPPER:'{searchCodeUpper}',O:'{originalCountryCodeForDebug}',ITTest:'{italyTestResult}',Samples:'{sampleCodes}')";
                }
                
                if (_cityService == null)
                {
                    return $"DBG:CitySvcNull(Country:{country.Name},ITTest:'{italyTestResult}')";
                }
                
                var capital = await _cityService.GetCapitalCityAsync(country.Id);
                
                if (capital == null)
                {
                    return $"DBG:CapitalNull(Country:{country.Name}, ID:{country.Id})";
                }
                
                return capital.Name;
            }
            catch (Exception ex)
            {
                string excType = ex.GetType().Name;
                string excMsg = ex.Message;
                excMsg = excMsg.Length > 200 ? excMsg.Substring(0, 200) : excMsg;

                return $"DBG:EXC({excType}:'{excMsg}',Orig:'{originalCountryCodeForDebug}')";
            }
        }

        // Add photo upload handler
        [HttpPost]
        public async Task<IActionResult> OnPostUploadPhotoAsync(IFormFile photo, int countryId)
        {
            if (photo == null || photo.Length == 0)
                return new JsonResult(new { success = false, message = "Nessun file caricato" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, message = "Utente non autenticato" });

            try
            {
                // Create unique filename
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName)}";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "uploads");
                
                // Ensure directory exists
                Directory.CreateDirectory(uploadsFolder);
                
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                string urlPath = $"/images/uploads/{uniqueFileName}";

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Create photo record in database
                var newPhoto = new Photo
                {
                    UserId = userId,
                    FileName = photo.FileName,
                    FilePath = urlPath,
                    Url = urlPath,
                    Caption = Path.GetFileNameWithoutExtension(photo.FileName),
                    UploadDate = DateTime.UtcNow,
                    TravelJournalCountryId = countryId,
                    TravelJournalUserId = userId
                };

                _context.Photos.Add(newPhoto);
                await _context.SaveChangesAsync();

                return new JsonResult(new 
                { 
                    success = true, 
                    photoId = newPhoto.Id,
                    url = newPhoto.Url,
                    caption = newPhoto.Caption 
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Errore: {ex.Message}" });
            }
        }

        // Get photos for a visit
        [HttpGet]
        public async Task<IActionResult> OnGetVisitPhotosAsync(int countryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, message = "Utente non autenticato" });

            try
            {
                // Use the overloaded method that accepts both parameters
                var photos = await _photoService.GetPhotosByVisitAndUserAsync(countryId, userId);

                return new JsonResult(photos.Select(p => new { 
                    id = p.Id, 
                    url = p.Url, 
                    caption = p.Caption 
                }));
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Errore: {ex.Message}" });
            }
        }

        // Delete a photo
        [HttpPost]
        public async Task<IActionResult> OnPostDeletePhotoAsync(int photoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, message = "Utente non autenticato" });

            try
            {
                var photo = await _context.Photos.FindAsync(photoId);
                if (photo == null || photo.UserId != userId)
                    return new JsonResult(new { success = false, message = "Foto non trovata o non autorizzata" });

                // Delete the physical file if possible
                if (!string.IsNullOrEmpty(photo.FilePath))
                {
                    string filePath = Path.Combine(_environment.WebRootPath, photo.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Photos.Remove(photo);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Errore: {ex.Message}" });
            }
        }
    }
}