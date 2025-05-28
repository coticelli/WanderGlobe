// Pages/Globe.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Models;
using WanderGlobe.Services;
using System.Text.Json;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;

namespace WanderGlobe.Pages
{
    // ViewModel per i dati dei pin sulla mappa (ora basato su VisitedCity)
    public class VisitedCityMapPinViewModel
    {
        public int VisitedCityId { get; set; } // PK della tabella VisitedCities
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string Continent { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime VisitDate { get; set; }
        public string? Description { get; set; } // Note della visita
    }


    [Authorize]
    public class GlobeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly ILogger<GlobeModel> _logger;
        private readonly ApplicationDbContext _context;
        public record CityInfoForDropdown(int CityId, string CityDisplayName, string CountryName, string Continent, string CountryCode);

        public List<CityInfoForDropdown> AllCitiesForDropdown { get; set; } = new List<CityInfoForDropdown>();

        // Questo ora conterrà i ViewModel basati su VisitedCity
        public List<VisitedCityMapPinViewModel> VisitedCityPins { get; set; } = new List<VisitedCityMapPinViewModel>();

        public double VisitedPercentageOfWorldCountries { get; set; } // Percentuale di paesi unici visitati
        public string VisitedCitiesJsonForMap { get; private set; } = "[]"; // JSON per i pin sulla mappa

        public GlobeModel(
            UserManager<ApplicationUser> userManager,
            ICountryService countryService,
            ICityService cityService,
            ILogger<GlobeModel> logger,
            ApplicationDbContext context) // Add context parameter
        {
            _userManager = userManager;
            _countryService = countryService;
            _cityService = cityService;
            _logger = logger;
            _context = context; // Initialize the context
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Dovrebbe essere gestito da [Authorize], ma per sicurezza
                VisitedCitiesJsonForMap = "[]";
                return;
            }

            // 1. Popola dropdown delle città
            var allCitiesFromDb = await _cityService.GetAllCitiesWithCountryAsync();
            AllCitiesForDropdown = allCitiesFromDb
                .Where(c => c.Country != null) // Assicurati che il paese esista
                .Select(city => new CityInfoForDropdown(
                    city.Id,
                    $"{city.Name} ({city.Country.Name})",
                    city.Country.Name,
                    city.Country.Continent ?? "Sconosciuto",
                    city.Country.Code ?? "??"))
                .OrderBy(c => c.CityDisplayName)
                .ToList();

            // 2. Ottieni le città visitate dall'utente (dalla nuova tabella VisitedCities)
            var userVisitedCities = await _cityService.GetVisitedCitiesByUserAsync(user.Id);

            VisitedCityPins = userVisitedCities
                .Where(vc => vc.City != null && vc.City.Country != null && vc.City.Latitude.HasValue && vc.City.Longitude.HasValue) // Filtra per dati validi
                .Select(vc => new VisitedCityMapPinViewModel
                {
                    VisitedCityId = vc.Id, // L'ID della riga VisitedCity
                    CityName = vc.City.Name,
                    CountryName = vc.City.Country.Name,
                    CountryCode = vc.City.Country.Code ?? "??",
                    Continent = vc.City.Country.Continent ?? "Sconosciuto",
                    Latitude = vc.City.Latitude!.Value, // Usa ! se hai filtrato i null
                    Longitude = vc.City.Longitude!.Value,
                    VisitDate = vc.VisitDate,
                    Description = vc.Notes
                })
                .OrderByDescending(vm => vm.VisitDate)
                .ToList();

            VisitedCitiesJsonForMap = JsonSerializer.Serialize(VisitedCityPins);

            // 3. Calcola la percentuale di paesi UNICI visitati (usando VisitedCountries)
            // Questa logica può rimanere se vuoi ancora tracciare la % di paesi.
            var visitedUserCountries = await _countryService.GetVisitedCountriesByUserAsync(user.Id);
            var uniqueVisitedCountryIdsCount = visitedUserCountries.Select(vc => vc.CountryId).Distinct().Count();
            var totalWorldCountries = await _countryService.GetTotalCountryCountAsync(); // O un numero fisso come 193
            VisitedPercentageOfWorldCountries = totalWorldCountries > 0 ? (double)uniqueVisitedCountryIdsCount / totalWorldCountries * 100 : 0;

            // Log per debug
            _logger.LogInformation("OnGetAsync: Popolati {Count} pin di città visitate per l'utente {UserId}", VisitedCityPins.Count, user.Id);
            _logger.LogInformation("OnGetAsync: JSON per la mappa: {Json}", VisitedCitiesJsonForMap);
        }

        // Handler per aggiungere una NUOVA VISITA A UNA CITTÀ
        public async Task<IActionResult> OnPostAddCityVisitAsync(int cityId, DateTime visitDate, string? visitExperience)
        {
            _logger.LogInformation("OnPostAddCityVisitAsync chiamato con cityId: {CityId}, visitDate: {VisitDate}", cityId, visitDate);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("OnPostAddCityVisitAsync: ModelState non valido.");
                await PreparePageModelDataAsync(); // Ricarica i dati necessari per la view
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var city = await _cityService.GetCityByIdAsync(cityId);
            if (city == null || city.Country == null) // Assicurati che la città e il suo paese siano validi
            {
                _logger.LogWarning("OnPostAddCityVisitAsync: Città con ID {CityId} non trovata o senza paese associato.", cityId);
                ModelState.AddModelError(string.Empty, "Città selezionata non valida o informazioni paese mancanti.");
                await PreparePageModelDataAsync();
                return Page();
            }

            try
            {
                var newVisitedCity = new VisitedCity
                {
                    UserId = user.Id,
                    CityId = city.Id,
                    VisitDate = visitDate,
                    Notes = visitExperience
                };
                await _cityService.AddVisitedCityAsync(newVisitedCity);
                _logger.LogInformation("Visita alla città {CityName} aggiunta con successo per l'utente {UserId}", city.Name, user.Id);

                // Logica aggiuntiva: Assicurati che il paese sia marcato come visitato in VisitedCountries
                // Questo mantiene la coerenza se vuoi ancora usare la tabella VisitedCountries per statistiche sui paesi
                var existingVisitedCountry = await _context.VisitedCountries // Assumendo che _context sia disponibile o tramite _countryService
                    .FirstOrDefaultAsync(vc => vc.UserId == user.Id && vc.CountryId == city.CountryId);

                if (existingVisitedCountry == null)
                {
                    var visitedCountryEntry = new VisitedCountry
                    {
                        UserId = user.Id,
                        CountryId = city.CountryId,
                        VisitDate = visitDate, // Potresti usare la data della prima visita a una città di quel paese
                        Notes = $"Visitato tramite {city.Name}" // Nota opzionale
                    };
                    await _countryService.AddVisitedCountryAsync(visitedCountryEntry); // Usa il tuo servizio esistente
                    _logger.LogInformation("Paese {CountryName} aggiunto/aggiornato in VisitedCountries.", city.Country.Name);
                }
                else if (visitDate < existingVisitedCountry.VisitDate) // Aggiorna la data della prima visita al paese se questa è precedente
                {
                    existingVisitedCountry.VisitDate = visitDate;
                    // _context.VisitedCountries.Update(existingVisitedCountry); // Se AddVisitedCountryAsync non gestisce l'update
                    // await _context.SaveChangesAsync();
                    // Oppure modifica AddVisitedCountryAsync per fare un AddOrUpdate
                }


                TempData["SuccessMessage"] = $"Visita a {city.Name} aggiunta con successo!";
                return RedirectToPage();
            }
            catch (ArgumentException ex) // Es. "già visitata in questa data" da AddVisitedCityAsync
            {
                _logger.LogWarning(ex, "ArgumentException durante OnPostAddCityVisitAsync per CityId {CityId}: {Message}", cityId, ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generico durante OnPostAddCityVisitAsync per CityId {CityId}: {Message}", cityId, ex.Message);
                ModelState.AddModelError(string.Empty, "Si è verificato un errore imprevisto durante l'aggiunta della visita.");
            }

            await PreparePageModelDataAsync();
            return Page();
        }

        // Handler per rimuovere una VISITA SPECIFICA A UNA CITTÀ
        public async Task<IActionResult> OnPostRemoveCityVisitAsync(int visitedCityId) // Ora prende l'ID della riga VisitedCity
        {
            _logger.LogInformation("OnPostRemoveCityVisitAsync chiamato con visitedCityId: {VisitedCityId}", visitedCityId);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var visitedCityEntry = await _cityService.GetVisitedCityByIdAsync(visitedCityId);
            if (visitedCityEntry == null || visitedCityEntry.UserId != user.Id)
            {
                _logger.LogWarning("Tentativo di rimozione visita città non trovata o non autorizzata. ID: {VisitedCityId}, Utente: {UserId}", visitedCityId, user.Id);
                TempData["ErrorMessage"] = "Impossibile trovare la visita da rimuovere o non autorizzato.";
                return RedirectToPage();
            }

            try
            {
                await _cityService.RemoveVisitedCityAsync(visitedCityId);
                _logger.LogInformation("Visita città ID {VisitedCityId} rimossa con successo.", visitedCityId);
                TempData["SuccessMessage"] = $"Visita a {visitedCityEntry.City.Name} rimossa con successo.";

                // Opzionale: Controlla se era l'ultima città visitata in quel paese.
                // Se sì, potresti voler rimuovere la riga da VisitedCountries.
                // Questa logica dipende da come vuoi gestire le statistiche dei paesi.
                bool otherCitiesInCountry = await _context.VisitedCities
                    .AnyAsync(vc => vc.UserId == user.Id &&
                                   vc.City.CountryId == visitedCityEntry.City.CountryId &&
                                   vc.Id != visitedCityId);
                if (!otherCitiesInCountry)
                {
                    await _countryService.RemoveVisitedCountryAsync(user.Id, visitedCityEntry.City.CountryId);
                    _logger.LogInformation("Rimosso anche VisitedCountry per CountryId {CountryId} poiché non ci sono altre città visitate.", visitedCityEntry.City.CountryId);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la rimozione della visita città ID {VisitedCityId}", visitedCityId);
                TempData["ErrorMessage"] = "Errore durante la rimozione della visita.";
            }

            return RedirectToPage();
        }


        // Metodo helper per caricare i dati necessari quando si ritorna Page() da un handler POST
        private async Task PreparePageModelDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var allCitiesFromDb = await _cityService.GetAllCitiesWithCountryAsync();
            AllCitiesForDropdown = allCitiesFromDb
                .Where(c => c.Country != null)
                .Select(city => new CityInfoForDropdown(
                    city.Id,
                    $"{city.Name} ({city.Country.Name})",
                    city.Country.Name,
                    city.Country.Continent ?? "Sconosciuto",
                    city.Country.Code ?? "??"))
                .OrderBy(c => c.CityDisplayName)
                .ToList();

            // Potresti voler ricaricare anche VisitedCityPins e VisitedPercentageOfWorldCountries se necessario
            // per mostrare gli errori sulla pagina con i dati attuali.
            // Questo è già fatto in OnGetAsync, che viene chiamato implicitamente da RedirectToPage(),
            // ma se ritorni Page() direttamente, OnGetAsync non viene rieseguito per quel POST.
        }

        // Manteniamo il riferimento a _context per la logica opzionale in OnPostAddCityVisitAsync
        // e OnPostRemoveCityVisitAsync. Idealmente, questa logica sarebbe incapsulata nei servizi.
    }
}