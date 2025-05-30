using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Models; // Assicurati che DreamDestination sia qui e non ambiguo
using WanderGlobe.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using WanderGlobe.Models.Custom;

namespace WanderGlobe.Pages
{
    [Authorize]
    public class DreamMapModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICountryService _countryService;
        private readonly IDreamService _dreamService;
        private readonly ICityService _cityService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _geminiApiKey;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DreamMapModel> _logger;

        public List<DreamDestination> Wishlist { get; set; } = new List<DreamDestination>();
        public List<PlannedTrip> PlannedTrips { get; set; } = new List<PlannedTrip>();
        public List<RecommendedDestination> Recommendations { get; set; } = new List<RecommendedDestination>();
        public List<RecommendedDestination> RecommendedDestinations { get; set; } = new List<RecommendedDestination>();
        public List<Country> Countries { get; set; } = new List<Country>();
        public MapDestinationsViewModel AllDestinations { get; set; } = new MapDestinationsViewModel();

        [BindProperty]
        public WishlistItemViewModel WishlistForm { get; set; } = new WishlistItemViewModel();

        public DreamMapModel(
            UserManager<ApplicationUser> userManager,
            ICountryService countryService,
            IDreamService dreamService,
            ICityService cityService,
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext dbContext,
            ILogger<DreamMapModel> logger)
        {
            _userManager = userManager;
            _countryService = countryService;
            _dreamService = dreamService;
            _cityService = cityService;
            _clientFactory = clientFactory;
            _geminiApiKey = configuration["GeminiApiKey"] ?? string.Empty;
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            VerifyRequiredImages(); // Assicurati che questa funzione esista e sia corretta
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("OnGetAsync: User not found or not authenticated.");
                // Potresti voler reindirizzare alla pagina di login o mostrare un errore
                return;
            }

            try
            {
                _logger.LogInformation("OnGetAsync: Fetching data for user {UserId}", user.Id);
                Countries = await _countryService.GetAllCountriesAsync();
                Wishlist = await _dreamService.GetUserWishlistAsync(user.Id);
                PlannedTrips = await _dbContext.PlannedTrips
                                    .Where(pt => pt.UserId == user.Id)
                                    .OrderByDescending(pt => pt.CreatedAt)
                                    .ToListAsync();
                // Recommendations = ...; // Se hai un'altra fonte per queste
                RecommendedDestinations = await GetAIRecommendationsAsync(user.Id, "tutte");

                var visitedCities = await _cityService.GetVisitedCitiesByUserAsync(user.Id);

                AllDestinations = new MapDestinationsViewModel
                {
                    Wishlist = Wishlist.Select(d => new MapDestinationItem
                    {
                        Id = $"wishlist_{d.Id}",
                        CityName = d.City?.Name ?? d.DestinationName,
                        CountryName = d.Country?.Name ?? d.City?.Country?.Name ?? "Sconosciuto",
                        CountryCode = d.Country?.Code ?? d.City?.Country?.Code ?? "XX",
                        Latitude = d.City?.Latitude ?? d.Country?.Latitude ?? 0,
                        Longitude = d.City?.Longitude ?? d.Country?.Longitude ?? 0,
                        Priority = (DreamPriority)d.Priority,
                        Type = "wishlist",
                        ImageUrl = GetImageUrlForDream(d) // Helper per immagine
                    }).ToList(),

                    PlannedTrips = PlannedTrips.Select(p => new MapDestinationItem
                    {
                        Id = $"planned_{p.Id}",
                        CityName = p.CityName,
                        CountryName = p.CountryName,
                        CountryCode = p.CountryCode,
                        Latitude = p.Latitude != 0 ? p.Latitude : 0.0,  // Use direct access instead
                        Longitude = p.Longitude != 0 ? p.Longitude : 0.0, // Use direct access instead
                        CompletionPercentage = p.CompletionPercentage,
                        Type = "planned",
                        ImageUrl = p.ImageUrl
                    }).ToList(),

                    VisitedCities = visitedCities
                        .Where(vc => vc.City != null && vc.City.Country != null && vc.City.Latitude.HasValue && vc.City.Longitude.HasValue)
                        .Select(vc => new MapDestinationItem
                        {
                            Id = $"visited_city_{vc.Id}",
                            CityName = vc.City.Name,
                            CountryName = vc.City.Country.Name,
                            CountryCode = vc.City.Country.Code,
                            Latitude = vc.City.Latitude.Value,
                            Longitude = vc.City.Longitude.Value,
                            Type = "visited",
                            ImageUrl = vc.City.ImageUrl ?? vc.City.Country.FlagUrl
                        }).ToList()
                };

                var allCitiesWithCountry = await _cityService.GetAllCitiesWithCountryAsync();
                WishlistForm = new WishlistItemViewModel
                {
                    AvailableCities = allCitiesWithCountry
                        .Where(c => c.Country != null)
                        .Select(city => new CityInfo
                        {
                            Name = city.Name,
                            Country = city.Country.Name,
                            CountryCode = city.Country.Code
                        }).ToList()
                };
                _logger.LogInformation("OnGetAsync: Data loaded successfully for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnGetAsync of DreamMapModel for user {UserId}.", user?.Id ?? "Unknown");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento della mappa dei sogni.";
            }
        }

        private string GetImageUrlForDream(DreamDestination dream)
        {
            // Logica per determinare l'ImageUrl per un DreamDestination
            // Potrebbe essere una proprietà diretta di DreamDestination se la aggiungi,
            // o derivata da City/Country come prima.
            if (dream.City?.ImageUrl != null) return dream.City.ImageUrl;
            if (dream.Country?.FlagUrl != null) return dream.Country.FlagUrl; // Esempio di fallback
            return $"/images/cities/{(dream.Country?.Code ?? dream.City?.Country?.Code ?? "default").ToLower()}-city.jpg";
        }


        public async Task<IActionResult> OnPostSaveToWishlistAsync()
        {
            if (WishlistForm == null || string.IsNullOrWhiteSpace(WishlistForm.City))
            {
                return new JsonResult(new { success = false, message = "Seleziona una città valida." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "Utente non autenticato." });
            }
            _logger.LogInformation("OnPostSaveToWishlistAsync: User {UserId} attempting to save city {CityName} (Country: {CountryName}) to wishlist.", user.Id, WishlistForm.City, WishlistForm.Country);


            try
            {
                City? cityFromDb = null;
                if (!string.IsNullOrWhiteSpace(WishlistForm.Country))
                {
                    cityFromDb = await _dbContext.Cities
                        .Include(c => c.Country)
                        .FirstOrDefaultAsync(c => c.Name == WishlistForm.City && c.Country.Name == WishlistForm.Country);
                }
                else
                {
                    cityFromDb = await _dbContext.Cities
                       .Include(c => c.Country)
                       .FirstOrDefaultAsync(c => c.Name == WishlistForm.City);
                }
                _logger.LogInformation("City lookup result: {CityFoundStatus}", cityFromDb != null ? $"Found '{cityFromDb.Name}'" : "Not found in DB");


                string? determinedImageUrl = null;
                if (WishlistForm.ImageFile != null && WishlistForm.ImageFile.Length > 0)
                {
                    determinedImageUrl = await SaveWishlistImageAsync(WishlistForm.ImageFile);
                    _logger.LogInformation("Custom image uploaded: {ImageUrl}", determinedImageUrl);
                }
                else
                {
                    if (cityFromDb?.ImageUrl != null) determinedImageUrl = cityFromDb.ImageUrl;
                    else if (cityFromDb?.Country?.FlagUrl != null) determinedImageUrl = cityFromDb.Country.FlagUrl;
                    else
                    {
                        var tempCountryCode = cityFromDb?.Country?.Code ?? WishlistForm.CountryCode ?? "default";
                        determinedImageUrl = $"/images/cities/{tempCountryCode.ToLower()}-city.jpg";
                        if (!System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, determinedImageUrl.TrimStart('/'))))
                        {
                            determinedImageUrl = "/images/placeholder-destination.jpg";
                        }
                    }
                    _logger.LogInformation("Determined image URL (no upload): {ImageUrl}", determinedImageUrl);
                }


                var newDream = new DreamDestination
                {
                    UserId = user.Id,
                    DestinationName = WishlistForm.City,
                    CityId = cityFromDb?.Id,
                    CountryId = cityFromDb?.CountryId,
                    Notes = WishlistForm.Notes,
                    Tags = WishlistForm.Tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>(),
                    Priority = Enum.TryParse<DreamPriority>(WishlistForm.Priority, true, out var priority) ? (int)priority : 1,
                    AddedDate = DateTime.UtcNow
                };

                await _dreamService.AddToWishlistAsync(newDream); // Il servizio dovrebbe salvare e popolare newDream.Id
                _logger.LogInformation("DreamDestination added via service. ID: {DreamId}", newDream.Id);


                var responseDto = new
                {
                    id = newDream.Id.ToString(),
                    userId = newDream.UserId,
                    cityName = cityFromDb?.Name ?? newDream.DestinationName,
                    countryName = cityFromDb?.Country?.Name,
                    countryCode = cityFromDb?.Country?.Code,
                    latitude = cityFromDb?.Latitude,
                    longitude = cityFromDb?.Longitude,
                    priority = (int)newDream.Priority,
                    imageUrl = determinedImageUrl, // Usiamo l'immagine determinata
                    notes = newDream.Notes,
                    addedDate = newDream.AddedDate, // o CreatedAt
                    tags = newDream.Tags
                };

                return new JsonResult(new { success = true, message = $"{WishlistForm.City} aggiunta alla wishlist!", newItem = responseDto });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to add to wishlist (ArgumentException): {Message}", ex.Message);
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnPostSaveToWishlistAsync for user {UserId}.", user.Id);
                return new JsonResult(new { success = false, message = "Errore durante il salvataggio della destinazione." });
            }
        }

        public async Task<IActionResult> OnPostMoveToPlanningAsync([FromBody] MoveToPlanningRequest request)
        {
            _logger.LogInformation("OnPostMoveToPlanningAsync received DreamIdString: {DreamIdString}", request?.dreamId);
            if (request == null || !int.TryParse(request.dreamId, out int dreamId))
            {
                _logger.LogWarning("OnPostMoveToPlanningAsync: Invalid DreamIdString received: {DreamIdString}", request?.dreamId);
                return new JsonResult(new { success = false, message = "ID destinazione non valido." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("OnPostMoveToPlanningAsync: User not authenticated.");
                return new JsonResult(new { success = false, message = "Utente non autenticato." });
            }

            try
            {
                var dreamItem = await _dbContext.DreamDestinations
                                    .Include(d => d.City)
                                        .ThenInclude(c => c!.Country)
                                    .Include(d => d.Country)
                                    .FirstOrDefaultAsync(d => d.Id == dreamId && d.UserId == user.Id);

                if (dreamItem == null)
                {
                    _logger.LogWarning("OnPostMoveToPlanningAsync: DreamDestination not found or user mismatch. Requested DreamId: {DreamId}, UserId: {UserId}", dreamId, user.Id);
                    return new JsonResult(new { success = false, message = "Destinazione non trovata nella tua wishlist." });
                }

                _logger.LogInformation("Moving DreamDestination '{DestinationName}' (ID: {DreamId}) to planning for user {UserId}.", dreamItem.DestinationName, dreamItem.Id, user.Id);

                var plannedTrip = new PlannedTrip
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    DestinationName = dreamItem.DestinationName,
                    CityName = dreamItem.City?.Name,
                    CountryName = dreamItem.Country?.Name ?? dreamItem.City?.Country?.Name,
                    CountryCode = dreamItem.Country?.Code ?? dreamItem.City?.Country?.Code,
                    Notes = dreamItem.Notes, // Deve corrispondere a DreamDestination.Notes
                    Latitude = (double)(dreamItem.City?.Latitude ?? dreamItem.Country?.Latitude ?? 0.0),
                    Longitude = (double)(dreamItem.City?.Longitude ?? dreamItem.Country?.Longitude ?? 0.0),
                    ImageUrl = GetImageUrlForDream(dreamItem), // Usa helper per coerenza
                    StartDate = DateTime.UtcNow.Date.AddDays(30),
                    EndDate = DateTime.UtcNow.Date.AddDays(37),
                    CompletionPercentage = 0,
                    CreatedAt = DateTime.UtcNow, // PlannedTrip ha CreatedAt
                    Checklist = new List<ChecklistItem>()
                };

                _dbContext.PlannedTrips.Add(plannedTrip);
                _dbContext.DreamDestinations.Remove(dreamItem); // Rimosso dal DbContext

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully moved dream to planned trip. PlannedTrip ID: {PlannedTripId}", plannedTrip.Id);

                var plannedTripDto = new
                {
                    id = plannedTrip.Id,
                    cityName = plannedTrip.CityName,
                    countryName = plannedTrip.CountryName,
                    countryCode = plannedTrip.CountryCode,
                    startDate = plannedTrip.StartDate.ToString("yyyy-MM-dd"),
                    endDate = plannedTrip.EndDate.ToString("yyyy-MM-dd"),
                    completionPercentage = plannedTrip.CompletionPercentage,
                    notes = plannedTrip.Notes,
                    latitude = plannedTrip.Latitude,
                    longitude = plannedTrip.Longitude,
                    imageUrl = plannedTrip.ImageUrl,
                    checklist = new List<object>()
                };

                return new JsonResult(new
                {
                    success = true,
                    message = $"{plannedTrip.DestinationName} spostato in pianificazione!",
                    plannedTrip = plannedTripDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnPostMoveToPlanningAsync for DreamId {DreamId}, User {UserId}", dreamId, user.Id);
                return new JsonResult(new { success = false, message = "Errore durante lo spostamento della destinazione." });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostUpdatePlanDetailsAsync([FromBody] UpdatePlanDetailsRequest request)
        {
            _logger.LogInformation("OnPostUpdatePlanDetailsAsync called for PlanId: {PlanId}", request?.PlanId);
            if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
            {
                _logger.LogWarning("[UpdatePlan] Invalid PlanId.");
                return new JsonResult(new { success = false, message = "ID piano non valido o mancante." });
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("[UpdatePlan] User not authenticated.");
                return new JsonResult(new { success = false, message = "Utente non autenticato." });
            }

            var plan = await _dbContext.PlannedTrips
                .Include(p => p.Checklist)
                .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.UserId == user.Id);

            if (plan == null)
            {
                _logger.LogWarning("[UpdatePlan] Plan not found with ID {PlanId} for user {UserId}.", request.PlanId, user.Id);
                return new JsonResult(new { success = false, message = $"Piano non trovato." });
            }

            _logger.LogInformation("[UpdatePlan] Updating plan '{PlanId}'. Current notes: '{CurrentNotes}'", plan.Id, plan.Notes);

            plan.Notes = request.Notes;
            if (DateTime.TryParse(request.StartDate, out var startDate)) plan.StartDate = startDate;
            if (DateTime.TryParse(request.EndDate, out var endDate)) plan.EndDate = endDate;

            plan.Checklist.Clear(); // Rimuovi i vecchi e aggiungi i nuovi
            if (request.Checklist != null)
            {
                foreach (var itemDto in request.Checklist)
                {
                    if (!string.IsNullOrWhiteSpace(itemDto.Title))
                    {
                        plan.Checklist.Add(new ChecklistItem { Title = itemDto.Title, Category = itemDto.Category ?? "Generale", DueDate = itemDto.DueDate, IsCompleted = itemDto.IsCompleted, PlannedTripId = plan.Id });
                    }
                }
            }
            plan.CompletionPercentage = plan.Checklist.Any() ? (int)Math.Round((double)plan.Checklist.Count(c => c.IsCompleted) * 100 / plan.Checklist.Count) : 0;
            plan.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[UpdatePlan] Plan '{PlanId}' updated successfully.", plan.Id);
                // Ritorna il piano aggiornato per aggiornare la UI
                var updatedPlanDto = new
                {
                    id = plan.Id,
                    startDate = plan.StartDate.ToString("yyyy-MM-dd"),
                    endDate = plan.EndDate.ToString("yyyy-MM-dd"),
                    notes = plan.Notes,
                    completionPercentage = plan.CompletionPercentage,
                    checklist = plan.Checklist.Select(c => new { id = c.Id.ToString(), title = c.Title, category = c.Category, dueDate = c.DueDate, isCompleted = c.IsCompleted }).ToList()
                };
                return new JsonResult(new { success = true, message = "Piano aggiornato.", updatedPlan = updatedPlanDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdatePlan] Error updating plan '{PlanId}'.", plan.Id);
                return new JsonResult(new { success = false, message = "Errore durante l'aggiornamento del piano." });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostRemovePlanAsync([FromBody] RemovePlanRequest request)
        {
            _logger.LogInformation("OnPostRemovePlanAsync called for PlanId: {PlanId}", request?.PlanId);
            if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
            {
                _logger.LogWarning("[RemovePlan] Invalid PlanId.");
                return new JsonResult(new { success = false, message = "ID piano non valido." });
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("[RemovePlan] User not authenticated.");
                return new JsonResult(new { success = false, message = "Utente non autenticato." });
            }

            var plan = await _dbContext.PlannedTrips
                .Include(p => p.Checklist) // Include checklist per la rimozione a cascata (o manuale)
                .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.UserId == user.Id);

            if (plan == null)
            {
                _logger.LogWarning("[RemovePlan] Plan not found with ID {PlanId} for user {UserId}.", request.PlanId, user.Id);
                return new JsonResult(new { success = false, message = "Piano non trovato." });
            }

            _dbContext.ChecklistItems.RemoveRange(plan.Checklist); // Rimuovi prima gli item dipendenti se non c'è cascade delete
            _dbContext.PlannedTrips.Remove(plan);

            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[RemovePlan] Plan '{PlanId}' removed successfully.", plan.Id);
                return new JsonResult(new { success = true, message = "Piano eliminato." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RemovePlan] Error removing plan '{PlanId}'.", plan.Id);
                return new JsonResult(new { success = false, message = "Errore durante l'eliminazione del piano." });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostRemoveDreamAsync([FromBody] RemoveDreamRequest request)
        {
            _logger.LogInformation("OnPostRemoveDreamAsync called for DreamId (string from JS): {DreamId}", request?.DreamId);
            if (request == null || !int.TryParse(request.DreamId, out int dreamIdToInt))
            {
                _logger.LogWarning("[RemoveDream] Invalid DreamId format: {DreamId}", request?.DreamId);
                return new JsonResult(new { success = false, message = "ID Sogno non valido." });
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("[RemoveDream] User not authenticated.");
                return new JsonResult(new { success = false, message = "Utente non autenticato." });
            }

            var dream = await _dbContext.DreamDestinations
                .FirstOrDefaultAsync(d => d.Id == dreamIdToInt && d.UserId == user.Id);

            if (dream == null)
            {
                _logger.LogWarning("[RemoveDream] Dream not found with ID {DreamIdToInt} for user {UserId}.", dreamIdToInt, user.Id);
                return new JsonResult(new { success = false, message = "Sogno non trovato." });
            }

            _dbContext.DreamDestinations.Remove(dream);
            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[RemoveDream] Dream ID {DreamIdToInt} removed successfully.", dreamIdToInt);
                return new JsonResult(new { success = true, message = "Sogno rimosso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RemoveDream] Error removing dream ID {DreamIdToInt}.", dreamIdToInt);
                return new JsonResult(new { success = false, message = "Errore durante la rimozione del sogno." });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostMarkAsVisitedAsync([FromBody] MarkAsVisitedRequest request)
        {
            _logger.LogInformation("OnPostMarkAsVisitedAsync called for PlanId: {PlanId}", request?.PlanId);
            if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
            {
                return new JsonResult(new { success = false, message = "ID piano non valido." });
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new JsonResult(new { success = false, message = "Utente non autenticato." });

            var plan = await _dbContext.PlannedTrips
                .Include(p => p.Checklist) // Per rimuoverli
                .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.UserId == user.Id);

            if (plan == null) return new JsonResult(new { success = false, message = "Piano non trovato." });

            try
            {
                // Trova la città e il paese del piano
                City? cityOfPlan = null;
                if (!string.IsNullOrWhiteSpace(plan.CityName))
                {
                    cityOfPlan = await _dbContext.Cities
                        .Include(c => c.Country)
                        .FirstOrDefaultAsync(c => c.Name == plan.CityName && (plan.CountryName == null || c.Country.Name == plan.CountryName));
                }

                Country? countryOfPlan = cityOfPlan?.Country ??
                                       await _dbContext.Countries.FirstOrDefaultAsync(c => c.Name == plan.CountryName || c.Code == plan.CountryCode);

                if (countryOfPlan == null)
                {
                    _logger.LogWarning("Paese non trovato per il piano {PlanId} ({PlanCountryName})", plan.Id, plan.CountryName);
                    return new JsonResult(new { success = false, message = "Paese del piano non trovato. Impossibile marcare come visitato." });
                }

                // Aggiungi a VisitedCities se la città è specificata
                if (cityOfPlan != null)
                {
                    var existingCityVisit = await _dbContext.VisitedCities
                        .FirstOrDefaultAsync(vc => vc.UserId == user.Id && vc.CityId == cityOfPlan.Id && vc.VisitDate.Date == plan.EndDate.Date);
                    if (existingCityVisit == null)
                    {
                        _dbContext.VisitedCities.Add(new VisitedCity
                        {
                            UserId = user.Id,
                            CityId = cityOfPlan.Id,
                            VisitDate = plan.EndDate,
                            Notes = plan.Notes
                        });
                        _logger.LogInformation("Aggiunta nuova VisitedCity per {CityName}, data {VisitDate}", cityOfPlan.Name, plan.EndDate);
                    }
                }

                // Aggiungi/Aggiorna VisitedCountries
                var existingCountryVisit = await _dbContext.VisitedCountries
                    .FirstOrDefaultAsync(vc => vc.UserId == user.Id && vc.CountryId == countryOfPlan.Id);
                if (existingCountryVisit == null)
                {
                    _dbContext.VisitedCountries.Add(new VisitedCountry
                    {
                        UserId = user.Id,
                        CountryId = countryOfPlan.Id,
                        VisitDate = plan.EndDate,
                        Notes = $"Visitato tramite piano per {plan.DestinationName}"
                    });
                    _logger.LogInformation("Aggiunta nuova VisitedCountry per {CountryName}, data {VisitDate}", countryOfPlan.Name, plan.EndDate);
                }
                else if (plan.EndDate < existingCountryVisit.VisitDate) // Se questa visita è precedente, aggiorna la data della prima visita al paese
                {
                    existingCountryVisit.VisitDate = plan.EndDate;
                    _dbContext.VisitedCountries.Update(existingCountryVisit);
                    _logger.LogInformation("Aggiornata VisitDate per VisitedCountry {CountryName} a {VisitDate}", countryOfPlan.Name, plan.EndDate);
                }


                _dbContext.ChecklistItems.RemoveRange(plan.Checklist);
                _dbContext.PlannedTrips.Remove(plan);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Piano {PlanId} marcato come visitato e rimosso.", plan.Id);
                return new JsonResult(new { success = true, message = $"{plan.DestinationName} marcato come visitato!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in OnPostMarkAsVisitedAsync per PlanId {PlanId}", request.PlanId);
                return new JsonResult(new { success = false, message = "Errore durante la marcatura come visitato." });
            }
        }

        private async Task<string?> SaveWishlistImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0) return null;
            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "wishlist_uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetRandomFileName() + Path.GetExtension(image.FileName); // Più unico
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                _logger.LogInformation("Immagine salvata in: {FilePath}", filePath);
                return $"/images/wishlist_uploads/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il salvataggio dell'immagine della wishlist.");
                return null;
            }
        }

        private void VerifyRequiredImages()
        {
            var imagePaths = new[]
            {
                Path.Combine(_webHostEnvironment.WebRootPath, "images", "empty-wishlist.svg"),
                Path.Combine(_webHostEnvironment.WebRootPath, "images", "empty-planning.svg"),
                Path.Combine(_webHostEnvironment.WebRootPath, "images", "placeholder-destination.jpg"),
                Path.Combine(_webHostEnvironment.WebRootPath, "images", "default-city.jpg")
            };

            foreach (var path in imagePaths)
            {
                if (!System.IO.File.Exists(path))
                {
                    _logger.LogWarning("Immagine richiesta mancante: {Path}. Tentativo di creazione/copia.", path);
                    string? directoryName = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    // Logica di creazione/copia placeholder (semplificata)
                    if (Path.GetExtension(path).Equals(".svg", StringComparison.OrdinalIgnoreCase))
                    {
                        System.IO.File.WriteAllText(path, "<svg width=\"100\" height=\"100\"><rect width=\"100\" height=\"100\" fill=\"#eee\"/></svg>");
                    }
                    // Per JPG, potresti copiare un placeholder se esiste, o loggare un avviso più forte
                }
            }
        }

        private async Task<List<RecommendedDestination>> GetAIRecommendationsAsync(string userId, string type)
        {
            _logger.LogInformation("GetAIRecommendationsAsync per utente {UserId}, tipo '{Type}'", userId, type);
            if (string.IsNullOrEmpty(_geminiApiKey))
            {
                _logger.LogWarning("Chiave API Gemini non configurata. Restituisco fallback.");
                return MapRecommendationItemsToDestinations(GetFallbackRecommendations());
            }

            // Qui potresti personalizzare il prompt ulteriormente basandoti su userId e type
            string prompt = $"Fornisci 5 suggerimenti di viaggio per la categoria '{type}'. " +
                            "Restituisci solo un array JSON di destinazioni. Ogni oggetto JSON deve avere le seguenti chiavi (usa esattamente questi nomi e questo casing): " +
                            "\"id\" (stringa univoca), \"cityName\" (stringa), \"countryName\" (stringa), \"description\" (stringa, 1-2 frasi), " +
                            "\"reasonToVisit\" (stringa, 1 frase), \"latitude\" (numero), \"longitude\" (numero), e \"imageUrl\" (stringa, un URL a un'immagine placeholder come \"/images/placeholder-destination.jpg\"). " +
                            "Assicurati che il JSON sia valido. Non includere testo al di fuori dell'array JSON.";

            var geminiItems = await CallGeminiForRecommendationsAsync(prompt);
            if (!geminiItems.Any())
            {
                _logger.LogWarning("Nessuna raccomandazione da Gemini, restituisco fallback.");
                return MapRecommendationItemsToDestinations(GetFallbackRecommendations());
            }
            return MapRecommendationItemsToDestinations(geminiItems);
        }

        private List<RecommendedDestination> MapRecommendationItemsToDestinations(List<RecommendationItem> items)
        {
            return items.Select(g => new RecommendedDestination
            {
                Id = g.Id,
                CityName = g.CityName,
                CountryName = g.CountryName,
                CountryCode = g.CountryCode ?? "XX",
                Description = g.Description,
                ReasonToVisit = g.ReasonToVisit,
                ImageUrl = g.ImageUrl,
                Latitude = g.Latitude,
                Longitude = g.Longitude,
                MatchPercentage = g.MatchPercentage,
                Tags = g.Tags ?? new List<string>(),
                Weather = g.Weather ?? "Varies",
                CostLevel = g.CostLevel ?? "Medium",
                Accommodations = g.Accommodations ?? "Hotels, hostels"
            }).ToList();
        }


        private async Task<List<RecommendationItem>> CallGeminiForRecommendationsAsync(string prompt)
        {
            using var httpClient = _clientFactory.CreateClient("GeminiClient"); // Potresti configurare un named client
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_geminiApiKey}";
            var requestData = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            var requestJson = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            _logger.LogInformation("Invio richiesta a Gemini. Prompt (inizio): {PromptStart}", prompt.Substring(0, Math.Min(prompt.Length, 100)));

            try
            {
                var response = await httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Errore API Gemini: {StatusCode} - {ResponseContent}", response.StatusCode, responseContent);
                    return new List<RecommendationItem>();
                }

                _logger.LogInformation("Risposta ricevuta da Gemini. Lunghezza: {Length}", responseContent.Length);
                JObject? responseJObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                string? textResult = responseJObject?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                if (string.IsNullOrWhiteSpace(textResult))
                {
                    _logger.LogWarning("Testo mancante o vuoto nella risposta Gemini. Raw: {RawResponse}", responseContent);
                    return new List<RecommendationItem>();
                }

                textResult = CleanupMarkdownCodeDelimiters(textResult);
                _logger.LogDebug("Testo Gemini pulito (inizio): {CleanTextStart}", textResult.Substring(0, Math.Min(textResult.Length, 200)));
                return JsonConvert.DeserializeObject<List<RecommendationItem>>(textResult) ?? new List<RecommendationItem>();
            }
            catch (JsonReaderException jsonEx)
            {
                _logger.LogError(jsonEx, "Errore parsing JSON da Gemini.");
                return new List<RecommendationItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eccezione durante la chiamata o l'elaborazione della risposta Gemini.");
                return new List<RecommendationItem>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetTravelsuggestions(string cityName, string suggestionType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cityName))
                {
                    return new JsonResult(new { success = false, error = "Nome città mancante" });
                }

                var prompt = BuildPrompt(cityName, suggestionType);
                // Create a specialized method to get HTML content instead of recommendation items
                var htmlContent = await CallGeminiForHtmlContentAsync(prompt);

                return new JsonResult(new { success = true, html = htmlContent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generando suggerimenti di viaggio per {City}, tipo {Type}", cityName, suggestionType);
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        // Add this new method to get HTML content from Gemini
        private async Task<string> CallGeminiForHtmlContentAsync(string prompt)
        {
            using var httpClient = _clientFactory.CreateClient("GeminiClient");
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_geminiApiKey}";
            var requestData = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            var requestJson = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            _logger.LogInformation("Invio richiesta HTML a Gemini. Prompt (inizio): {PromptStart}", prompt.Substring(0, Math.Min(prompt.Length, 100)));

            try
            {
                var response = await httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Errore API Gemini: {StatusCode} - {ResponseContent}", response.StatusCode, responseContent);
                    return string.Empty;
                }

                JObject? responseJObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                string? textResult = responseJObject?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                if (string.IsNullOrWhiteSpace(textResult))
                {
                    _logger.LogWarning("Testo mancante o vuoto nella risposta Gemini. Raw: {RawResponse}", responseContent);
                    return string.Empty;
                }

                // Clean up any markdown or code delimiters and return the HTML content
                return CleanupMarkdownCodeDelimiters(textResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eccezione durante la chiamata o l'elaborazione della risposta HTML Gemini.");
                return string.Empty; // Return empty string instead of throwing to avoid 500 errors
            }
        }


        private string BuildPrompt(string cityName, string suggestionType)
        {
            string basePrompt = $"Sei un esperto di viaggi conciso. Fornisci SOLO il contenuto richiesto in formato HTML valido, senza prefazioni, conclusioni, commenti HTML (<!-- -->) o delimitatori di codice markdown come ```html.";
            switch (suggestionType.ToLowerInvariant())
            {
                case "attractions": return $"{basePrompt} Lista 5 attrazioni principali di {cityName}. Per ogni attrazione, usa il formato: <ul><li><strong>Nome Attrazione:</strong> Breve descrizione (10-15 parole).</li></ul>";
                case "gastronomy": return $"{basePrompt} Breve introduzione <p>sulla cucina locale di {cityName} (max 20 parole).</p> Lista 5 piatti/esperienze culinarie, formato: <ul><li><strong>Nome Piatto:</strong> Breve descrizione (10-15 parole).</li></ul>";
                case "history": return $"{basePrompt} Breve introduzione <p>sulla storia di {cityName} (max 20 parole).</p> Lista 5-6 eventi storici significativi in ordine cronologico, formato: <ul><li><strong>Anno/Periodo:</strong> Fatto (10-15 parole).</li></ul>";
                case "tips": return $"{basePrompt} Lista 5-6 consigli pratici per visitare {cityName} (es. trasporti, sicurezza), formato: <ul><li><strong>Consiglio:</strong> Dettaglio (15-20 parole).</li></ul>";
                default: return $"{basePrompt} Fornisci una descrizione generale di {cityName} come meta turistica in un singolo paragrafo <p> (max 50 parole).</p>";
            }
        }

        private string CleanupMarkdownCodeDelimiters(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            content = System.Text.RegularExpressions.Regex.Replace(content, @"^\s*```(json|html|HTML)?\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
            content = System.Text.RegularExpressions.Regex.Replace(content, @"\s*```\s*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            return content.Trim();
        }

        private List<RecommendationItem> GetFallbackRecommendations()
        {
            return new List<RecommendationItem> {
                new RecommendationItem { Id="fb1", CityName="Roma", CountryName="Italia", Description="Storia e cultura.", ReasonToVisit="Colosseo.", Latitude=41.9, Longitude=12.5, ImageUrl="/images/placeholder-destination.jpg" },
                new RecommendationItem { Id="fb2", CityName="Parigi", CountryName="Francia", Description="Amore e luci.", ReasonToVisit="Torre Eiffel.", Latitude=48.85, Longitude=2.35, ImageUrl="/images/placeholder-destination.jpg" }
            };
        }

    } // Chiusura classe DreamMapModel


    // *** Le classi DTO devono essere fuori dalla classe DreamMapModel se vuoi usarle come parametri [FromBody] o tipi di ritorno complessi ***
    // *** O definiscile come classi pubbliche innestate se preferisci ***

    public class MoveToPlanningRequest { public string? dreamId { get; set; } }
    public class RemoveDreamRequest { public string? DreamId { get; set; } }
    public class MarkAsVisitedRequest { public string? PlanId { get; set; } }
    public class RemovePlanRequest { public string? PlanId { get; set; } }
    public class UpdatePlanDetailsRequest
    {
        public string? PlanId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Notes { get; set; }
        public List<ChecklistItemDto>? Checklist { get; set; }
    }
    public class ChecklistItemDto
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "Il titolo è obbligatorio")]
        public string Title { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
    public class WishlistItemViewModel // Già definita come proprietà, ma se usata come parametro [FromBody] meglio esterna o public nested
    {
        [Required(ErrorMessage = "Seleziona una città")] public string? City { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? Notes { get; set; }
        public string? Tags { get; set; }
        public string? Priority { get; set; } = "Medium";
        public IFormFile? ImageFile { get; set; }
        public List<CityInfo> AvailableCities { get; set; } = new List<CityInfo>();
    }

    public class MapDestinationsViewModel
    {
        public List<MapDestinationItem> Wishlist { get; set; } = new List<MapDestinationItem>();
        public List<MapDestinationItem> PlannedTrips { get; set; } = new List<MapDestinationItem>();
        public List<MapDestinationItem> VisitedCities { get; set; } = new List<MapDestinationItem>();
    }
    public class MapDestinationItem
    {
        public string Id { get; set; } = string.Empty;
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Type { get; set; } = string.Empty;
        public DreamPriority Priority { get; set; } // Usa l'enum se disponibile
        public int CompletionPercentage { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class RecommendedDestination // Modello per la UI
    {
        public string Id { get; set; } = string.Empty; public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? Description { get; set; }
        public string? ReasonToVisit { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
        
        // Add these new properties
        public string? CountryCode { get; set; } = "XX";
        public int MatchPercentage { get; set; } = 0;
        public List<string>? Tags { get; set; } = new List<string>();
        public string? Weather { get; set; } = "Varies";
        public string? CostLevel { get; set; } = "Medium";
        public string? Accommodations { get; set; } = "Hotels, hostels";
        
        // Keep the existing ToString method
        public override string ToString() => $"Id: {Id}, City: {CityName}, Country: {CountryName}, Desc: {Description?.Substring(0, Math.Min(Description?.Length ?? 0, 20))}...";
    }
    public class RecommendationItem // Modello per la risposta Gemini
    {
        [JsonProperty("id")] public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("cityName")] public string CityName { get; set; } = string.Empty;
        [JsonProperty("countryName")] public string CountryName { get; set; } = string.Empty;
        [JsonProperty("description")] public string Description { get; set; } = string.Empty;
        [JsonProperty("reasonToVisit")] public string ReasonToVisit { get; set; } = string.Empty;
        [JsonProperty("latitude")] public double Latitude { get; set; }
        [JsonProperty("longitude")] public double Longitude { get; set; }
        [JsonProperty("imageUrl")] public string ImageUrl { get; set; } = "/images/placeholder-destination.jpg";
        
        // Add these new properties
        [JsonProperty("countryCode")] public string? CountryCode { get; set; }
        [JsonProperty("matchPercentage")] public int MatchPercentage { get; set; }
        [JsonProperty("tags")] public List<string>? Tags { get; set; }
        [JsonProperty("weather")] public string? Weather { get; set; }
        [JsonProperty("costLevel")] public string? CostLevel { get; set; }
        [JsonProperty("accommodations")] public string? Accommodations { get; set; }
        
        // Keep existing toString method
        public override string ToString() => $"Id: {Id}, City: {CityName}, Country: {CountryName}, Desc: {Description?.Substring(0, Math.Min(Description?.Length ?? 0, 20))}...";
    }

} // Chiusura namespace WanderGlobe.Pages