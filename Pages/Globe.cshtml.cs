using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Models;
using WanderGlobe.Services;
using System.Text.Json;

namespace WanderGlobe.Pages
{
    [Authorize]
    public class GlobeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICountryService _countryService;

        public List<Country> AllCountries { get; set; } = new List<Country>();
        public List<VisitedCountry> VisitedCountries { get; set; } = new List<VisitedCountry>();
        public double VisitedPercentage { get; set; }

        public GlobeModel(
            UserManager<ApplicationUser> userManager,
            ICountryService countryService)
        {
            _userManager = userManager;
            _countryService = countryService;
        }
        public string VisitedCountriesJson { get; private set; }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                AllCountries = await _countryService.GetAllCountriesAsync();
                VisitedCountries = await _countryService.GetVisitedCountriesByUserAsync(user.Id);
                VisitedPercentage = await _countryService.GetVisitedPercentageAsync(user.Id);
            }
            VisitedCountriesJson = SerializeVisitedCountriesForMap();
        }

        private string SerializeVisitedCountriesForMap()
        {
            // Se non abbiamo visite, restituiamo un array vuoto
            if (VisitedCountries == null || !VisitedCountries.Any())
                return "[]";

            var simplifiedData = VisitedCountries.Select((vc, index) => new
            {
                // Usa l'indice come ID se non hai una proprietà ID
                id = index + 1,
                name = vc.Country?.Name ?? "Paese sconosciuto",
                countryName = vc.Country?.Name ?? "Paese sconosciuto",
                countryCode = vc.Country?.Code ?? "unknown",
                lat = vc.Country?.Latitude ?? 0,
                lng = vc.Country?.Longitude ?? 0,
                // Controlla se è DateTime default (01/01/0001)
                visitDate = (vc.VisitDate == default(DateTime)) ? DateTime.UtcNow : vc.VisitDate,
                // Messaggio di default
                description = "Hai visitato questo paese"
            }).ToList();

            return JsonSerializer.Serialize(simplifiedData);
        }

        public async Task<IActionResult> OnPostAddCountryAsync(int countryId, DateTime visitDate)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var visitedCountry = new VisitedCountry
                {
                    UserId = user.Id,
                    CountryId = countryId,
                    VisitDate = visitDate
                };

                await _countryService.AddVisitedCountryAsync(visitedCountry);
                return RedirectToPage();
            }
            catch (ArgumentException ex)
            {
                // Se il paese non esiste, mostriamo un messaggio di errore
                ModelState.AddModelError(string.Empty, ex.Message);

                // Ricarichiamo i dati per mostrare la pagina con l'errore
                AllCountries = await _countryService.GetAllCountriesAsync();
                VisitedCountries = await _countryService.GetVisitedCountriesByUserAsync(user.Id);
                VisitedPercentage = await _countryService.GetVisitedPercentageAsync(user.Id);

                return Page();
            }
        }

        public async Task<IActionResult> OnPostRemoveCountryAsync(int countryId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            await _countryService.RemoveVisitedCountryAsync(user.Id, countryId);
            return RedirectToPage();
        }
    }
}