using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Models; // Assuming ApplicationUser, VisitedCountry, City, Country models are here
using WanderGlobe.Services;
using System.Text.Json;
using System.Linq;       // Required for LINQ methods like .Select, .GroupBy, .Any, etc.
using System;            // Required for DateTime
using System.Collections.Generic; // Required for List, Dictionary
using System.Threading.Tasks; // Required for Task

namespace WanderGlobe.Pages
{
    public class VisitedCityViewModel
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string Continent { get; set; } // Added Continent
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime VisitDate { get; set; }
        public string Description { get; set; } // Optional: if you want to pass visit notes
    }

    [Authorize]
    public class GlobeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;

        public record CityInfoForDropdown(int CityId, string CityDisplayName, string CountryName, string Continent, string CountryCode);
        public List<CityInfoForDropdown> AllCitiesForDropdown { get; set; } = new List<CityInfoForDropdown>();
        public List<VisitedCityViewModel> VisitedCities { get; set; } = new List<VisitedCityViewModel>();
        public double VisitedPercentage { get; set; }
        public string VisitedCountriesJson { get; private set; }

        public GlobeModel(
            UserManager<ApplicationUser> userManager,
            ICountryService countryService,
            ICityService cityService)
        {
            _userManager = userManager;
            _countryService = countryService;
            _cityService = cityService;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var allCities = await _cityService.GetAllCitiesWithCountryAsync();
                AllCitiesForDropdown = allCities
                    .Select(city => new CityInfoForDropdown(
                        city.Id,
                        $"{city.Name} ({city.Country.Name})",
                        city.Country.Name,
                        city.Country.Continent, // Assuming City.Country has a Continent property
                        city.Country.Code))
                    .OrderBy(c => c.CityDisplayName)
                    .ToList();

                var visitedUserCountries = await _countryService.GetVisitedCountriesByUserAsync(user.Id);
                var visitedCitiesViewModels = new List<VisitedCityViewModel>();

                foreach (var vc in visitedUserCountries)
                {
                    if (vc.Country == null) continue;

                    // Prefer capital city data if available, otherwise use country data
                    var capitalCity = await _cityService.GetCapitalCityByCountryIdAsync(vc.CountryId);
                    if (capitalCity != null && capitalCity.Country != null) // Ensure capitalCity's country is loaded for continent
                    {
                        visitedCitiesViewModels.Add(new VisitedCityViewModel
                        {
                            CityId = capitalCity.Id,
                            CityName = capitalCity.Name,
                            CountryName = vc.Country.Name, // Or capitalCity.Country.Name
                            CountryCode = vc.Country.Code, // Or capitalCity.Country.Code
                            Continent = capitalCity.Country.Continent, // Get continent from capital's country
                            Latitude = capitalCity.Latitude ?? vc.Country.Latitude,
                            Longitude = capitalCity.Longitude ?? vc.Country.Longitude,
                            VisitDate = vc.VisitDate,
                            Description = vc.Notes // Assuming VisitedCountry has Notes
                        });
                    }
                    else // Fallback: use country data directly if capital is not found or lacks details
                    {
                        visitedCitiesViewModels.Add(new VisitedCityViewModel
                        {
                            // CityId might be problematic if it MUST be a city's ID.
                            // Consider how you handle "country visits" not tied to a specific city for map pins.
                            // For now, assuming a "visited country" implies its capital or a primary city for map pin.
                            CityId = vc.CountryId, // Placeholder or handle differently if CityId must be an actual city
                            CityName = $"Visita a {vc.Country.Name}", // Generic name
                            CountryName = vc.Country.Name,
                            CountryCode = vc.Country.Code,
                            Continent = vc.Country.Continent, // Get continent from the visited country
                            Latitude = vc.Country.Latitude,
                            Longitude = vc.Country.Longitude,
                            VisitDate = vc.VisitDate,
                            Description = vc.Notes
                        });
                    }
                }
                VisitedCities = visitedCitiesViewModels.OrderByDescending(vm => vm.VisitDate).ToList();
                VisitedCountriesJson = JsonSerializer.Serialize(VisitedCities);

                var uniqueVisitedCountryIds = visitedUserCountries.Select(vc => vc.CountryId).Distinct().Count();
                var totalCountries = await _countryService.GetTotalCountryCountAsync();
                VisitedPercentage = totalCountries > 0 ? (double)uniqueVisitedCountryIds / totalCountries * 100 : 0;
            }
            else
            {
                VisitedCountriesJson = "[]";
                // Initialize other properties to avoid null issues if needed
                AllCitiesForDropdown = new List<CityInfoForDropdown>();
                VisitedCities = new List<VisitedCityViewModel>();
                VisitedPercentage = 0;
            }
        }

        public async Task<IActionResult> OnPostAddCountryAsync(int cityId, DateTime visitDate, string visitExperience)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var city = await _cityService.GetCityByIdAsync(cityId);
            if (city == null)
            {
                ModelState.AddModelError(string.Empty, "Città non trovata.");
                await OnGetAsync();
                return Page();
            }

            try
            {
                var visitedCountry = new VisitedCountry
                {
                    UserId = user.Id,
                    CountryId = city.CountryId,
                    VisitDate = visitDate,
                    Notes = visitExperience
                };
                await _countryService.AddVisitedCountryAsync(visitedCountry);
                return RedirectToPage();
            }
            catch (ArgumentException ex) // Catch specific exception if AddVisitedCountryAsync throws it for duplicates
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await OnGetAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRemoveCountryAsync(int cityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // This logic assumes removing a city visit means removing the entire country visit.
            // If you want to remove only a specific city visit record (and VisitedCountry stores per-city visits),
            // this needs adjustment. Current _countryService.RemoveVisitedCountryAsync suggests country-level removal.
            var city = await _cityService.GetCityByIdAsync(cityId);
            if (city == null)
            {
                TempData["ErrorMessage"] = "Impossibile rimuovere la visita: città non trovata.";
                return RedirectToPage();
            }

            await _countryService.RemoveVisitedCountryAsync(user.Id, city.CountryId);
            return RedirectToPage();
        }
    }
}