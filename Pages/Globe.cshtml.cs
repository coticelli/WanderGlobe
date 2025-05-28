using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Models;
using WanderGlobe.Services;
using System.Text.Json;

namespace WanderGlobe.Pages
{
    public class VisitedCityViewModel
    {
        public int CityId { get; set; } 
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime VisitDate { get; set; }
        public string Description { get; set; }
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

        public GlobeModel(
            UserManager<ApplicationUser> userManager,
            ICountryService countryService,
            ICityService cityService)
        {
            _userManager = userManager;
            _countryService = countryService;
            _cityService = cityService;
        }
        public string VisitedCountriesJson { get; private set; } // This will be populated by VisitedCities
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Populate AllCitiesForDropdown
                var allCities = await _cityService.GetAllCitiesWithCountryAsync();
                AllCitiesForDropdown = allCities
                    .Select(city => new CityInfoForDropdown(
                        city.Id, 
                        $"{city.Name} ({city.Country.Name})", 
                        city.Country.Name, 
                        city.Country.Continent,
                        city.Country.Code))
                    .OrderBy(c => c.CityDisplayName)
                    .ToList();

                // Populate VisitedCities (which will also be used for VisitedCountriesJson)
                var visitedCountries = await _countryService.GetVisitedCountriesByUserAsync(user.Id);
                var visitedCitiesViewModels = new List<VisitedCityViewModel>();
                foreach (var vc in visitedCountries)
                {
                    if (vc.Country == null) continue;

                    var capitalCity = await _cityService.GetCapitalCityByCountryIdAsync(vc.CountryId);
                    if (capitalCity != null)
                    {
                        visitedCitiesViewModels.Add(new VisitedCityViewModel
                        {
                            CityId = capitalCity.Id,
                            CityName = capitalCity.Name,
                            CountryName = vc.Country.Name,
                            CountryCode = vc.Country.Code,
                            Latitude = capitalCity.Latitude ?? vc.Country.Latitude,
                            Longitude = capitalCity.Longitude ?? vc.Country.Longitude,
                            VisitDate = vc.VisitDate,
                            Description = $"Hai visitato {capitalCity.Name}, {vc.Country.Name}"
                        });
                    }
                    else // Fallback if no capital city is found (should ideally not happen if data is consistent)
                    {
                        visitedCitiesViewModels.Add(new VisitedCityViewModel
                        {
                            CityId = vc.CountryId, // Or a placeholder ID if CityId must be a city's ID
                            CityName = $"Capitale di {vc.Country.Name}", // Placeholder name
                            CountryName = vc.Country.Name,
                            CountryCode = vc.Country.Code,
                            Latitude = vc.Country.Latitude,
                            Longitude = vc.Country.Longitude,
                            VisitDate = vc.VisitDate,
                            Description = $"Hai visitato {vc.Country.Name}"
                        });
                    }
                }
                VisitedCities = visitedCitiesViewModels.OrderByDescending(vm => vm.VisitDate).ToList();
                VisitedCountriesJson = JsonSerializer.Serialize(VisitedCities);
                
                // Calculate VisitedPercentage based on unique countries visited
                var uniqueVisitedCountryIds = visitedCountries.Select(vc => vc.CountryId).Distinct().Count();
                var totalCountries = await _countryService.GetTotalCountryCountAsync(); // Assumes this method exists
                VisitedPercentage = totalCountries > 0 ? (double)uniqueVisitedCountryIds / totalCountries * 100 : 0;
            }
            else
            {
                VisitedCountriesJson = "[]";
                AllCitiesForDropdown = new List<CityInfoForDropdown>();
                VisitedCities = new List<VisitedCityViewModel>();
            }
        }

        // SerializeVisitedCitiesForMapAsync is no longer needed as VisitedCountriesJson is set in OnGetAsync

        public async Task<IActionResult> OnPostAddCountryAsync(int cityId, DateTime visitDate, string visitExperience)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Repopulate lists for the page
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var city = await _cityService.GetCityByIdAsync(cityId);
            if (city == null)
            {
                ModelState.AddModelError(string.Empty, "Città non trovata.");
                await OnGetAsync(); // Repopulate lists for the page
                return Page();
            }

            try
            {
                var visitedCountry = new VisitedCountry
                {
                    UserId = user.Id,
                    CountryId = city.CountryId, // Use CountryId from the selected city
                    VisitDate = visitDate,
                    Notes = visitExperience // Assuming VisitedCountry has a Notes/Experience field
                };

                await _countryService.AddVisitedCountryAsync(visitedCountry);
                return RedirectToPage();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await OnGetAsync(); // Repopulate lists for the page
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRemoveCountryAsync(int cityId) // Changed to cityId for consistency, will get countryId from city
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var city = await _cityService.GetCityByIdAsync(cityId);
            if (city == null)
            {
                 // Or handle as an error: return NotFound("City not found to determine country for removal.");
                 // For now, let's assume if city is not found, we can't proceed.
                 TempData["ErrorMessage"] = "Impossibile rimuovere la visita: città non trovata.";
                 return RedirectToPage();
            }

            await _countryService.RemoveVisitedCountryAsync(user.Id, city.CountryId);
            return RedirectToPage();
        }
    }
}