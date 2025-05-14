using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Models;
using WanderGlobe.Models.Custom; // Assicurati che questo using ci sia
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
using Microsoft.EntityFrameworkCore; // Necessario per Entity Framework Core
using WanderGlobe.Data; // Necessario per ApplicationDbContext
using Newtonsoft.Json.Linq;

namespace WanderGlobe.Pages
{
    [Authorize]
    public class DreamMapModel : PageModel
    {
        // ... (proprietà esistenti: _userManager, _countryService, etc.) ...
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICountryService _countryService;
        private readonly IDreamService _dreamService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _geminiApiKey; // <- Keep this one
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICityService _cityService;

        public List<DreamDestination> Wishlist { get; set; } = new List<DreamDestination>();
        public List<PlannedTrip> PlannedTrips { get; set; } = new List<PlannedTrip>();
        public List<RecommendedDestination> Recommendations { get; set; } = new List<RecommendedDestination>();
        // ADD THIS PROPERTY for AI recommendations
        public List<RecommendedDestination> RecommendedDestinations { get; set; } = new List<RecommendedDestination>();
        public List<Country> Countries { get; set; } = new List<Country>(); // Added missing property
        public MapDestinationsViewModel AllDestinations { get; set; } = new MapDestinationsViewModel(); // Added missing property

        [BindProperty]
        public WishlistItemViewModel WishlistForm { get; set; } = new WishlistItemViewModel();

        // --- COSTRUTTORE (invariato) ---
        public DreamMapModel(
            UserManager<ApplicationUser> userManager,
            ICountryService countryService,
            IDreamService dreamService,
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext dbContext,
            ICityService cityService)
        {
            _userManager = userManager;
            _countryService = countryService;
            _dreamService = dreamService;
            _clientFactory = clientFactory;
            _geminiApiKey = configuration["GeminiApiKey"] ?? string.Empty;
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
            _cityService = cityService;
            // ... (eventuali altri log nel costruttore) ...
        }

        // --- OnGetAsync e altri metodi GET (probabilmente invariati) ---
        public async Task OnGetAsync()
        {
            // Verifica se le immagini richieste esistono
            VerifyRequiredImages();

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    // Recupera i paesi disponibili
                    Countries = await _countryService.GetAllCountriesAsync();

                    // Recupera le destinazioni dei sogni dell'utente (DreamDestination.Id è int)
                    Wishlist = await _dreamService.GetUserWishlistAsync(user.Id);

                    // Recupera i viaggi pianificati dell'utente (PlannedTrip.Id è string)
                    PlannedTrips = await _dreamService.GetUserPlannedTripsAsync(user.Id);

                    // Recupera i suggerimenti personalizzati
                    Recommendations = await _dreamService.GetRecommendationsAsync(user.Id);

                    // Prepara i dati per la mappa
                    var visitedCountries = await _countryService.GetVisitedCountriesByUserAsync(user.Id);

                    // Prepara la lista delle città visitate con operazioni asincrone
                    var visitedCitiesItems = new List<MapDestinationItem>();
                    foreach (var v in visitedCountries)
                    {
                        // Assumendo che GetCapitalAsync restituisca un oggetto City o null
                        var capital = await _cityService.GetCapitalCityByCountryIdAsync(v.CountryId);
                        if (capital != null)
                        {
                            visitedCitiesItems.Add(new MapDestinationItem
                            {
                                // Visited ID potrebbe essere basato sulla città o sul paese
                                Id = $"visited_city_{capital.Id}", // Esempio ID
                                CityName = capital.Name,
                                CountryName = v.Country.Name,
                                CountryCode = v.Country.Code,
                                Latitude = capital.Latitude ?? v.Country.Latitude, // Usa lat/lon città se disponibili
                                Longitude = capital.Longitude ?? v.Country.Longitude
                            });
                        }
                    }

                    // Popola il modello con tutti i dati per la mappa
                    AllDestinations = new MapDestinationsViewModel
                    {
                        Wishlist = Wishlist.Select(d => new MapDestinationItem
                        {
                            Id = d.Id.ToString(), // Converti l'ID int a string per MapDestinationItem
                            CityName = d.CityName,
                            CountryName = d.CountryName,
                            CountryCode = d.CountryCode,
                            Latitude = d.Latitude,
                            Longitude = d.Longitude,
                            Priority = (int)d.Priority
                        }).ToList(),

                        PlannedTrips = PlannedTrips.Select(p => new MapDestinationItem
                        {
                            Id = p.Id, // L'ID di PlannedTrip è già string
                            CityName = p.CityName,
                            CountryName = p.CountryName,
                            CountryCode = p.CountryCode,
                            Latitude = p.Latitude,
                            Longitude = p.Longitude,
                            CompletionPercentage = p.CompletionPercentage
                        }).ToList(),

                        VisitedCities = visitedCitiesItems
                    };

                    // Inizializza il form per l'aggiunta alla wishlist con le città disponibili
                    WishlistForm = new WishlistItemViewModel
                    {
                        AvailableCities = await GetAvailableCapitalsAsync() // Assumi che questo funzioni
                    };

                    // ADD THIS LINE to load AI recommendations
                    RecommendedDestinations = await _dreamService.GetAIRecommendationsAsync(user.Id, "tutte");
                }
            }
            catch (Exception ex)
            {
                // Log dell'errore
                System.Diagnostics.Debug.WriteLine($"Errore in OnGetAsync: {ex.Message}");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento della pagina.";
            }
        }

                private async Task<List<RecommendationItem>> CallGeminiForRecommendationsAsync(string prompt)
        {
            using var httpClient = _clientFactory.CreateClient();
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_geminiApiKey}";
        
            var requestData = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };
        
            var content = new StringContent(
                JsonConvert.SerializeObject(requestData),
                System.Text.Encoding.UTF8,
                "application/json");
        
            System.Diagnostics.Debug.WriteLine($"Sending request to Gemini API: {apiUrl.Substring(0, apiUrl.IndexOf('?'))}?key=API_KEY_HIDDEN");
            System.Diagnostics.Debug.WriteLine($"Prompt: {prompt.Substring(0, Math.Min(prompt.Length, 100))}...");
            
            var response = await httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
        
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // First extract the text from the response structure
                    JObject responseJson = JsonConvert.DeserializeObject<JObject>(responseContent);
                    string responseText = responseJson?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
        
                    if (string.IsNullOrEmpty(responseText))
                    {
                        System.Diagnostics.Debug.WriteLine("Empty or missing recommendations text in Gemini response.");
                        return new List<RecommendationItem>();
                    }
        
                    // Clean up any markdown code delimiters
                    if (responseText.StartsWith("```json") || responseText.StartsWith("```"))
                    {
                        responseText = responseText
                            .Replace("```json", "")
                            .Replace("```", "")
                            .Trim();
                    }
        
                    System.Diagnostics.Debug.WriteLine($"Cleaned response text: {responseText.Substring(0, Math.Min(responseText.Length, 200))}...");
        
                    // Parse the cleaned text as JSON
                    try
                    {
                        var recommendations = JsonConvert.DeserializeObject<List<RecommendationItem>>(responseText);
                        System.Diagnostics.Debug.WriteLine($"Successfully parsed {recommendations.Count} recommendations.");
                        return recommendations ?? new List<RecommendationItem>();
                    }
                    catch (JsonReaderException jsonEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON parsing error: {jsonEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Content that failed to parse: {responseText}");
                        return new List<RecommendationItem>();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing Gemini response: {ex.Message}\n{ex.StackTrace}");
                    return new List<RecommendationItem>();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Gemini API Error: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Error Response: {responseContent}");
                return new List<RecommendationItem>();
            }
        }

        // --- OnPostSaveToWishlistAsync (Aggiornato per gestire meglio ID e coordinate) ---
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostSaveToWishlistAsync()
        {
            try
            {
                if (WishlistForm == null)
                {
                    return new JsonResult(new { success = false, message = "Dati del form non ricevuti." });
                }

                if (string.IsNullOrWhiteSpace(WishlistForm.City))
                {
                    return new JsonResult(new { success = false, message = "Seleziona una città valida." });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "Utente non autenticato" });
                }

                // Ottieni informazioni sulla città selezionata (DAL DATABASE)
                var cityFromDb = await _dbContext.Cities
                    .Include(c => c.Country) // Include il paese per avere tutte le info
                    .FirstOrDefaultAsync(c => c.Name == WishlistForm.City && c.Country.Name == WishlistForm.Country); // Assicurati che WishlistForm.Country sia popolato correttamente dal dropdown

                if (cityFromDb == null)
                {
                    // Potrebbe essere una città non ancora nel DB o un problema nel form
                    // Prova a cercare solo per nome città se il paese non era nel form
                    if (string.IsNullOrWhiteSpace(WishlistForm.Country))
                    {
                        cityFromDb = await _dbContext.Cities.Include(c => c.Country).FirstOrDefaultAsync(c => c.Name == WishlistForm.City);
                    }

                    if (cityFromDb == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"WARN: Città '{WishlistForm.City}' (Paese: '{WishlistForm.Country}') non trovata nel DB.");
                        // Potresti decidere di non permettere l'aggiunta o usare valori di default
                        // Per ora, usiamo valori di default se non trovata
                        WishlistForm.Country = WishlistForm.Country ?? "Sconosciuto";
                        WishlistForm.CountryCode = WishlistForm.CountryCode ?? "XX"; // Assicurati che il code arrivi dal form o dalla logica del dropdown
                    }
                }

                // Se la città è stata trovata nel DB, usa i suoi dati
                string countryName = cityFromDb?.Country?.Name ?? WishlistForm.Country ?? "Sconosciuto";
                string countryCode = cityFromDb?.Country?.Code ?? WishlistForm.CountryCode ?? "XX";
                double latitude = cityFromDb?.Latitude ?? 0.0; // Usa Lat/Lon dal DB se disponibili
                double longitude = cityFromDb?.Longitude ?? 0.0;


                // Parse priority enum
                if (!Enum.TryParse<DreamPriority>(WishlistForm.Priority, true, out var priority))
                {
                    priority = DreamPriority.Medium; // Default
                }

                // Process image if available
                string? imageUrl = null;
                if (WishlistForm.ImageFile != null && WishlistForm.ImageFile.Length > 0)
                {
                    imageUrl = await SaveWishlistImageAsync(WishlistForm.ImageFile); // Assumi che questa funzione esista e funzioni
                }
                else // Fallback image
                {
                    // Potresti avere immagini di default per paese o città
                    imageUrl = $"/images/cities/{countryCode.ToLower()}-city.jpg"; // Esempio
                                                                                   // Verifica se esiste, altrimenti usa un placeholder generico
                    if (!System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'))))
                    {
                        imageUrl = "/images/placeholder-destination.jpg";
                    }
                }


                // Process tags
                List<string> tagList = string.IsNullOrWhiteSpace(WishlistForm.Tags)
                    ? new List<string>()
                    : WishlistForm.Tags.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();

                // Crea nuova DreamDestination (Id è int, sarà generato dal DB)
                var newDream = new DreamDestination
                {
                    // Id sarà generato da EF Core
                    UserId = user.Id,
                    CityName = WishlistForm.City,
                    CountryName = countryName,
                    CountryCode = countryCode,
                    Note = WishlistForm.Notes ?? "", // Assicura non sia null
                    Tags = tagList,
                    Priority = priority,
                    Latitude = latitude,
                    Longitude = longitude,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                System.Diagnostics.Debug.WriteLine($"Tentativo salvataggio Wishlist: City={newDream.CityName}, Country={newDream.CountryName}, Lat={newDream.Latitude}, Lon={newDream.Longitude}");

                var savedDream = await _dreamService.AddToWishlistAsync(newDream); // Assumi che il servizio funzioni

                if (savedDream != null && savedDream.Id > 0) // Controlla che l'ID sia stato generato
                {
                    System.Diagnostics.Debug.WriteLine($"Salvataggio Wishlist riuscito. Nuovo ID: {savedDream.Id}");
                    // Restituisci l'oggetto salvato, assicurandoti che l'ID sia stringa per il JS
                    return new JsonResult(new
                    {
                        success = true,
                        message = $"{WishlistForm.City} aggiunta alla tua wishlist!",
                        newItem = new // Crea un DTO per la risposta
                        {
                            id = savedDream.Id.ToString(), // Converti int ID a stringa
                            userId = savedDream.UserId,
                            cityName = savedDream.CityName,
                            countryName = savedDream.CountryName,
                            countryCode = savedDream.CountryCode,
                            latitude = savedDream.Latitude,
                            longitude = savedDream.Longitude,
                            priority = (int)savedDream.Priority, // Invia come int
                            imageUrl = savedDream.ImageUrl,
                            note = savedDream.Note,
                            createdAt = savedDream.CreatedAt,
                            tags = savedDream.Tags
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Errore nel salvataggio della destinazione tramite DreamService.");
                    return new JsonResult(new { success = false, message = "Errore nel salvataggio della destinazione." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in OnPostSaveToWishlistAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new JsonResult(new { success = false, message = $"Errore server: {ex.Message}" });
            }
        }

        // --- OnPostMoveToPlanningAsync (Aggiornato per ID int di DreamDestination) ---
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostMoveToPlanningAsync([FromBody] MoveToPlanning request)
        {
            System.Diagnostics.Debug.WriteLine($"--- [OnPostMoveToPlanningAsync] INIZIO ---");
            System.Diagnostics.Debug.WriteLine($"Richiesta ricevuta. DreamId: '{request?.DreamId ?? "NULL"}'");

            if (request == null || string.IsNullOrWhiteSpace(request.DreamId))
            {
                System.Diagnostics.Debug.WriteLine("[OnPostMoveToPlanningAsync] ERRORE: Richiesta o DreamId nullo/vuoto.");
                return new JsonResult(new { success = false, message = "ID destinazione non valido o mancante." });
            }

            var requestedDreamIdStr = request.DreamId.Trim();
            System.Diagnostics.Debug.WriteLine($"DreamId normalizzato: '{requestedDreamIdStr}'");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("[OnPostMoveToPlanningAsync] ERRORE: Utente non autenticato.");
                return new JsonResult(new { success = false, message = "Utente non autenticato" });
            }
            System.Diagnostics.Debug.WriteLine($"Utente autenticato: Id='{user.Id}', UserName='{user.UserName}'");

            DreamDestination? dreamItem = null;
            try
            {
                System.Diagnostics.Debug.WriteLine($"Tentativo di trovare DreamDestination (Id = int) per UserId='{user.Id}'...");

                // CORREZIONE: Usa int.TryParse perché DreamDestination.Id è INT
                if (int.TryParse(requestedDreamIdStr, out int dreamIdAsInt))
                {
                    System.Diagnostics.Debug.WriteLine($"ID interpretato come INT: {dreamIdAsInt}. Esecuzione query con confronto INT...");
                    dreamItem = await _dbContext.DreamDestinations
                        .FirstOrDefaultAsync(d => d.UserId == user.Id && d.Id == dreamIdAsInt); // Confronto INT

                    if (dreamItem != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Trovato (come INT)! ID DB: {dreamItem.Id}, Città: {dreamItem.CityName}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Non trovato (come INT).");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[OnPostMoveToPlanningAsync] ERRORE: ID '{requestedDreamIdStr}' non è un INT valido come richiesto da DreamDestination.Id.");
                    return new JsonResult(new { success = false, message = "Formato ID destinazione non valido." });
                }

                if (dreamItem == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnPostMoveToPlanningAsync] ERRORE FINALE: Destinazione con ID (INT) '{dreamIdAsInt}' non trovata per l'utente '{user.Id}'.");
                    var existingDreamIds = await _dbContext.DreamDestinations
                                               .Where(d => d.UserId == user.Id)
                                               .Select(d => d.Id) // Seleziona gli ID int
                                               .ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"ID DreamDestination (int) esistenti per l'utente: [{string.Join(", ", existingDreamIds)}]");
                    return new JsonResult(new { success = false, message = "Destinazione non trovata nella tua wishlist." });
                }

                System.Diagnostics.Debug.WriteLine($"Destinazione trovata nel DB. ID: {dreamItem.Id}, Città: {dreamItem.CityName}. Procedo con lo spostamento...");

                // Creazione PlannedTrip (PlannedTrip.Id è string)
                var plannedTrip = new PlannedTrip
                {
                    Id = Guid.NewGuid().ToString(), // Genera un ID stringa univoco
                    UserId = user.Id,
                    CityName = dreamItem.CityName,
                    CountryName = dreamItem.CountryName,
                    CountryCode = dreamItem.CountryCode,
                    Notes = dreamItem.Note,
                    Latitude = dreamItem.Latitude,
                    Longitude = dreamItem.Longitude,
                    ImageUrl = dreamItem.ImageUrl,
                    StartDate = DateTime.UtcNow.Date.AddDays(30),
                    EndDate = DateTime.UtcNow.Date.AddDays(37),
                    CompletionPercentage = 0,
                    CreatedAt = DateTime.UtcNow,
                    Checklist = new List<ChecklistItem>()
                };
                System.Diagnostics.Debug.WriteLine($"Nuovo PlannedTrip creato. ID (string): {plannedTrip.Id}, Città: {plannedTrip.CityName}");

                _dbContext.PlannedTrips.Add(plannedTrip);
                System.Diagnostics.Debug.WriteLine("PlannedTrip aggiunto al DbContext.");

                _dbContext.DreamDestinations.Remove(dreamItem);
                System.Diagnostics.Debug.WriteLine("DreamDestination rimosso dal DbContext.");

                int changes = await _dbContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"SaveChangesAsync completato. Righe modificate: {changes}");

                // Preparazione Risposta DTO
                var plannedTripDto = new
                {
                    id = plannedTrip.Id, // Già stringa
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
                    checklist = new List<object>() // Checklist è vuota all'inizio
                };

                System.Diagnostics.Debug.WriteLine($"Spostamento completato. Invio risposta di successo.");
                System.Diagnostics.Debug.WriteLine($"--- [OnPostMoveToPlanningAsync] FINE ---");
                return new JsonResult(new
                {
                    success = true,
                    message = $"{plannedTrip.CityName} spostata correttamente in pianificazione.",
                    plannedTrip = plannedTripDto
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OnPostMoveToPlanningAsync] ECCEZIONE NON GESTITA: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"--- [OnPostMoveToPlanningAsync] FINE CON ERRORE ---");
                return new JsonResult(new { success = false, message = "Si è verificato un errore interno durante lo spostamento." });
            }
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostUpdatePlanDetailsAsync([FromBody] UpdatePlan request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"--- [OnPostUpdatePlanDetailsAsync] INIZIO ---");
                System.Diagnostics.Debug.WriteLine($"Richiesta ricevuta. PlanId: '{request?.PlanId ?? "NULL"}', StartDate: '{request?.StartDate ?? "NULL"}', EndDate: '{request?.EndDate ?? "NULL"}', Notes: '{request?.Notes ?? "NULL"}', Checklist DTO Count: {request?.Checklist?.Count ?? 0}");

                if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
                {
                    System.Diagnostics.Debug.WriteLine("[OnPostUpdatePlanDetailsAsync] ERRORE: Richiesta o PlanId nullo/vuoto.");
                    return new JsonResult(new { success = false, message = "ID piano non valido o mancante." });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("[OnPostUpdatePlanDetailsAsync] ERRORE: Utente non autenticato.");
                    return new JsonResult(new { success = false, message = "Utente non autenticato." });
                }
                System.Diagnostics.Debug.WriteLine($"Utente autenticato: Id='{user.Id}', UserName='{user.UserName}'");

                var requestedPlanId = request.PlanId.Trim();
                System.Diagnostics.Debug.WriteLine($"Tentativo ricerca piano (Id = string) con ID: '{requestedPlanId}' per UserId: '{user.Id}'");

                var plan = await _dbContext.PlannedTrips
                    .Include(p => p.Checklist)
                    .FirstOrDefaultAsync(p => p.Id == requestedPlanId && p.UserId == user.Id);

                if (plan == null)
                {
                    var userPlans = await _dbContext.PlannedTrips.Where(p => p.UserId == user.Id).Select(p => p.Id).ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"[OnPostUpdatePlanDetailsAsync] ERRORE: Piano con ID '{requestedPlanId}' non trovato per l'utente '{user.Id}'. Piani (string ID) esistenti: [{string.Join(", ", userPlans)}]");
                    return new JsonResult(new { success = false, message = $"Piano non trovato (ID: {requestedPlanId})" });
                }

                System.Diagnostics.Debug.WriteLine($"Piano trovato (ID string: {plan.Id}). Original StartDate: {plan.StartDate:O}, Original EndDate: {plan.EndDate:O}, Original Notes: '{plan.Notes}', Original Completion%: {plan.CompletionPercentage}, Stato EF Iniziale: {_dbContext.Entry(plan).State}. Checklist caricate: {plan.Checklist.Count}");

                bool planModified = false; // Flag per tracciare modifiche dirette al piano
                DateTime parsedStartDate, parsedEndDate;

                // Confronta e aggiorna StartDate
                if (DateTime.TryParse(request.StartDate, out parsedStartDate))
                {
                    if (plan.StartDate.Date != parsedStartDate.Date)
                    {
                        plan.StartDate = parsedStartDate.Date; // Assegna solo la parte Date
                        planModified = true;
                        System.Diagnostics.Debug.WriteLine($"[Modifica Plan] StartDate AGGIORNATA a: {plan.StartDate:yyyy-MM-dd}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Modifica Plan] StartDate (request: {request.StartDate}, parsata: {parsedStartDate.Date:yyyy-MM-dd}) NON DIVERSA da plan.StartDate ({plan.StartDate.Date:yyyy-MM-dd}). Nessun aggiornamento per StartDate.");
                    }
                }
                else if (!string.IsNullOrEmpty(request.StartDate)) { System.Diagnostics.Debug.WriteLine($"[Modifica Plan] StartDate fornita ('{request.StartDate}') non è una data valida. Non aggiornata."); }


                // Confronta e aggiorna EndDate
                if (DateTime.TryParse(request.EndDate, out parsedEndDate))
                {
                    if (plan.EndDate.Date != parsedEndDate.Date)
                    {
                        plan.EndDate = parsedEndDate.Date; // Assegna solo la parte Date
                        planModified = true;
                        System.Diagnostics.Debug.WriteLine($"[Modifica Plan] EndDate AGGIORNATA a: {plan.EndDate:yyyy-MM-dd}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Modifica Plan] EndDate (request: {request.EndDate}, parsata: {parsedEndDate.Date:yyyy-MM-dd}) NON DIVERSA da plan.EndDate ({plan.EndDate.Date:yyyy-MM-dd}). Nessun aggiornamento per EndDate.");
                    }
                }
                else if (!string.IsNullOrEmpty(request.EndDate)) { System.Diagnostics.Debug.WriteLine($"[Modifica Plan] EndDate fornita ('{request.EndDate}') non è una data valida. Non aggiornata."); }


                // Confronta e aggiorna Notes
                string newNotes = request.Notes ?? ""; // Se request.Notes è null, confronta con stringa vuota
                if (plan.Notes != newNotes)
                {
                    plan.Notes = newNotes;
                    planModified = true;
                    System.Diagnostics.Debug.WriteLine($"[Modifica Plan] Notes AGGIORNATE. Nuove Notes: '{plan.Notes}'");
                } else { System.Diagnostics.Debug.WriteLine($"[Modifica Plan] Notes NON DIVERSE. Request Notes: '{newNotes}', Plan Notes Correnti: '{plan.Notes}'"); }

                // Gestione Checklist
                List<ChecklistItem> itemsToRemove = new List<ChecklistItem>(plan.Checklist);
                List<ChecklistItem> itemsToAdd = new List<ChecklistItem>();
                List<ChecklistItemDto> validDtosForPercentage = new List<ChecklistItemDto>();

                if (request.Checklist != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Modifica Checklist] Checklist fornita nel DTO ({request.Checklist.Count} elementi).");
                    foreach (var itemDto in request.Checklist)
                    {
                        if (!string.IsNullOrWhiteSpace(itemDto.Title))
                        {
                            var newItem = new ChecklistItem { PlannedTripId = plan.Id, Title = itemDto.Title, Category = itemDto.Category ?? "other", DueDate = itemDto.DueDate, IsCompleted = itemDto.IsCompleted };
                            itemsToAdd.Add(newItem);
                            validDtosForPercentage.Add(itemDto);
                        } else { System.Diagnostics.Debug.WriteLine("[Modifica Checklist] Saltato DTO checklist con titolo vuoto."); }
                    }
                    if (itemsToRemove.Any()) { _dbContext.ChecklistItems.RemoveRange(itemsToRemove); System.Diagnostics.Debug.WriteLine($"[Modifica Checklist] Marcati {itemsToRemove.Count} checklist items ESISTENTI per RIMOZIONE."); }
                    else { System.Diagnostics.Debug.WriteLine("[Modifica Checklist] Nessun checklist item esistente da rimuovere."); }
                    if (itemsToAdd.Any()) { _dbContext.ChecklistItems.AddRange(itemsToAdd); System.Diagnostics.Debug.WriteLine($"[Modifica Checklist] Marcati {itemsToAdd.Count} checklist items NUOVI per AGGIUNTA."); }
                    else { System.Diagnostics.Debug.WriteLine("[Modifica Checklist] Nessun checklist item nuovo valido da aggiungere."); }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Modifica Checklist] Checklist NON fornita nella richiesta (null). Rimuovo solo gli esistenti.");
                    if (itemsToRemove.Any()) { _dbContext.ChecklistItems.RemoveRange(itemsToRemove); System.Diagnostics.Debug.WriteLine($"[Modifica Checklist] Marcati {itemsToRemove.Count} checklist items ESISTENTI per RIMOZIONE."); }
                    else { System.Diagnostics.Debug.WriteLine("[Modifica Checklist] Nessun checklist item esistente da rimuovere."); }
                    validDtosForPercentage.Clear();
                }

                int newPercentage = validDtosForPercentage.Any() ? (int)Math.Round((double)validDtosForPercentage.Count(dto => dto.IsCompleted) * 100 / validDtosForPercentage.Count) : 0;
                if (plan.CompletionPercentage != newPercentage)
                {
                    plan.CompletionPercentage = newPercentage;
                    planModified = true; // Il piano è stato modificato anche per la percentuale
                    System.Diagnostics.Debug.WriteLine($"[Modifica Plan] CompletionPercentage AGGIORNATA a: {plan.CompletionPercentage}%");
                } else { System.Diagnostics.Debug.WriteLine($"[Modifica Plan] CompletionPercentage non cambiata ({plan.CompletionPercentage}%).");}


                System.Diagnostics.Debug.WriteLine($"--- [UpdatePlanDetails] VERIFICA PRIMA DI SAVE ---");
                System.Diagnostics.Debug.WriteLine($"Flag 'planModified': {planModified}");
                var planEntryState = _dbContext.Entry(plan).State;
                System.Diagnostics.Debug.WriteLine($"Stato entità Plan ('{plan.Id}'): {planEntryState}");

                // Forza lo stato a Modified se il nostro flag planModified è true ma EF non l'ha rilevato
                // (potrebbe succedere se l'unica modifica fosse alla CompletionPercentage e EF non la tracciasse come modifica all'entità Plan)
                if (planModified && planEntryState == EntityState.Unchanged) {
                     _dbContext.Entry(plan).State = EntityState.Modified;
                     planEntryState = _dbContext.Entry(plan).State; // Rileggi lo stato
                     System.Diagnostics.Debug.WriteLine($"Stato entità Plan FORZATO a Modified. Nuovo stato: {planEntryState}");
                }


                var trackedChecklistItems = _dbContext.ChangeTracker.Entries<ChecklistItem>().Where(e => e.Entity.PlannedTripId == plan.Id).ToList();
                System.Diagnostics.Debug.WriteLine($"Elementi Checklist tracciati da EF ({trackedChecklistItems.Count}) per Plan ID '{plan.Id}':");
                bool checklistChangesDetectedInTracker = false;
                foreach(var entry in trackedChecklistItems) {
                    System.Diagnostics.Debug.WriteLine($"  - '{entry.Entity.Title}' (ID DB: {entry.Entity.Id}) -> Stato EF: {entry.State}");
                    if (entry.State == EntityState.Added || entry.State == EntityState.Deleted || entry.State == EntityState.Modified) {
                        checklistChangesDetectedInTracker = true;
                    }
                }

                bool hasPendingChanges = _dbContext.ChangeTracker.HasChanges();
                System.Diagnostics.Debug.WriteLine($"DbContext.ChangeTracker.HasChanges(): {hasPendingChanges}");

                if (!hasPendingChanges && (planModified || checklistChangesDetectedInTracker)) {
                     System.Diagnostics.Debug.WriteLine("WARN: Modifiche rilevate manualmente (planModified o checklistChangesDetectedInTracker) ma HasChanges() è false! Questo non dovrebbe accadere se lo stato del piano è Modified.");
                }


                int changes = 0;
                if (hasPendingChanges)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Tentativo di eseguire SaveChangesAsync...");
                        changes = await _dbContext.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"SaveChangesAsync ESEGUITO. Righe modificate nel DB: {changes}");
                        
                        var postSaveState = _dbContext.Entry(plan).State;
                        System.Diagnostics.Debug.WriteLine($"Stato entità Plan DOPO SaveChanges: {postSaveState}");
                        if (postSaveState != EntityState.Unchanged && changes > 0) System.Diagnostics.Debug.WriteLine("WARN: Stato Plan non è Unchanged dopo un salvataggio che ha affetto righe!");
                        
                        var postSaveChecklistEntries = _dbContext.ChangeTracker.Entries<ChecklistItem>()
                                                        .Where(e => e.Entity.PlannedTripId == plan.Id && e.State != EntityState.Detached)
                                                        .ToList();
                        System.Diagnostics.Debug.WriteLine($"Numero ChecklistItems tracciati DOPO SaveChanges: {postSaveChecklistEntries.Count}");
                        foreach(var entry in postSaveChecklistEntries) {
                            System.Diagnostics.Debug.WriteLine($"  - ChecklistItem ID {entry.Entity.Id} (titolo: '{entry.Entity.Title}') -> Stato EF: {entry.State}");
                        }
                    }
                    catch (DbUpdateException dbEx) { /* ... gestione errore ... */ return new JsonResult(new { success = false, message = "Errore database." }); }
                    catch (Exception saveEx) { /* ... gestione errore ... */ return new JsonResult(new { success = false, message = "Errore salvataggio." }); }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Nessuna modifica rilevata dal Change Tracker, SaveChangesAsync NON eseguito.");
                    changes = 0;
                }

                System.Diagnostics.Debug.WriteLine($"[Risposta] Tentativo di rileggere finalPlanState con ID: {requestedPlanId}");
                var finalPlanState = await _dbContext.PlannedTrips
                   .Include(p => p.Checklist)
                   .AsNoTracking()
                   .FirstOrDefaultAsync(p => p.Id == requestedPlanId);

                if (finalPlanState == null) { /* ... errore ... */ return new JsonResult(new { success = false, message = "Errore recupero stato finale." }); }
                System.Diagnostics.Debug.WriteLine($"[Risposta] finalPlanState recuperato. Notes: '{finalPlanState.Notes}', Checklist è null? {finalPlanState.Checklist == null}");

                List<ChecklistItemDto> finalChecklistDto = new List<ChecklistItemDto>();
                if (finalPlanState.Checklist != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Risposta] finalPlanState.Checklist NON è null. Elementi: {finalPlanState.Checklist.Count}");
                    try {
                        finalChecklistDto = finalPlanState.Checklist.OrderBy(c => c.Id)
                            .Select(c => new ChecklistItemDto { Id = c.Id.ToString(), Title = c.Title, Category = c.Category, DueDate = c.DueDate, IsCompleted = c.IsCompleted }).ToList();
                        System.Diagnostics.Debug.WriteLine($"[Risposta] finalChecklistDto mappata: {finalChecklistDto.Count} elementi.");
                    } catch (Exception linqEx) { System.Diagnostics.Debug.WriteLine($"[Risposta] ECCEZIONE LINQ: {linqEx.Message}"); }
                } else { System.Diagnostics.Debug.WriteLine($"[Risposta] ATTENZIONE: finalPlanState.Checklist è NULL."); }

                System.Diagnostics.Debug.WriteLine($"Invio risposta. Modifiche DB: {changes > 0}.");
                System.Diagnostics.Debug.WriteLine($"--- [OnPostUpdatePlanDetailsAsync] FINE ---");

                return new JsonResult(new
                {
                    success = true,
                    message = changes > 0 ? "Modifiche salvate." : "Nessuna modifica da salvare.",
                    updatedPlan = new { id = finalPlanState.Id, startDate = finalPlanState.StartDate.ToString("yyyy-MM-dd"), endDate = finalPlanState.EndDate.ToString("yyyy-MM-dd"), notes = finalPlanState.Notes, completionPercentage = finalPlanState.CompletionPercentage, checklist = finalChecklistDto }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ECCEZIONE BLOCCO PRINCIPALE: {ex.Message}\n{ex.StackTrace}");
                return new JsonResult(new { success = false, message = "Errore server." });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostRemovePlanAsync([FromBody] RemovePlan request) // Usa il modello corretto
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"OnPostRemovePlanAsync chiamato con PlanId (string): {request?.PlanId}");
                if (string.IsNullOrWhiteSpace(request?.PlanId))
                {
                    System.Diagnostics.Debug.WriteLine("ERRORE: ID piano non valido.");
                    return new JsonResult(new { success = false, message = "ID piano non valido" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERRORE: Utente non autenticato.");
                    return new JsonResult(new { success = false, message = "Utente non autenticato" });
                }

                var planIdToRemove = request.PlanId.Trim(); // ID string

                // Trova il piano (confronto string)
                var plan = await _dbContext.PlannedTrips
                    .FirstOrDefaultAsync(p => p.Id == planIdToRemove && p.UserId == user.Id); // Confronto string

                if (plan == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERRORE: Piano con ID '{planIdToRemove}' non trovato.");
                    return new JsonResult(new { success = false, message = "Piano non trovato" });
                }
                System.Diagnostics.Debug.WriteLine($"Piano trovato (ID: {plan.Id}). Procedo con rimozione.");

                // Rimuovi gli elementi della checklist associati (ChecklistItem.PlannedTripId è string)
                var checklistItems = await _dbContext.ChecklistItems
                    .Where(c => c.PlannedTripId == plan.Id) // Filtra per ID string del piano
                    .ToListAsync();

                if (checklistItems.Any())
                {
                    _dbContext.ChecklistItems.RemoveRange(checklistItems);
                    System.Diagnostics.Debug.WriteLine($"Rimossi {checklistItems.Count} elementi checklist associati.");
                }

                // Rimuovi il piano
                _dbContext.PlannedTrips.Remove(plan);
                System.Diagnostics.Debug.WriteLine($"Piano (ID: {plan.Id}) rimosso dal DbContext.");

                await _dbContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Rimozione piano completata.");

                return new JsonResult(new { success = true, message = "Piano eliminato con successo." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eccezione in RemovePlanAsync: {ex.Message}");
                return new JsonResult(new { success = false, message = "Errore durante l'eliminazione del piano." });
            }
        }

        // --- OnPostRemoveDreamAsync (Aggiornato per ID int di DreamDestination) ---
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostRemoveDreamAsync([FromBody] RemoveDreamRequest request) // Usa il modello corretto
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"OnPostRemoveDreamAsync chiamato con DreamId (string from JS): {request?.DreamId}");
                if (string.IsNullOrWhiteSpace(request?.DreamId))
                {
                    System.Diagnostics.Debug.WriteLine("ERRORE: ID destinazione non valido.");
                    return new JsonResult(new { success = false, message = "ID destinazione non valido" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERRORE: Utente non autenticato.");
                    return new JsonResult(new { success = false, message = "Utente non autenticato" });
                }

                // CORREZIONE: Converti ID stringa in INT per la query
                if (!int.TryParse(request.DreamId.Trim(), out int dreamIdAsInt))
                {
                    System.Diagnostics.Debug.WriteLine($"ERRORE: Formato ID destinazione non valido ('{request.DreamId}').");
                    return new JsonResult(new { success = false, message = "Formato ID destinazione non valido." });
                }
                System.Diagnostics.Debug.WriteLine($"Tentativo rimozione DreamDestination (ID int: {dreamIdAsInt})");

                // Trova il sogno (confronto int)
                var dream = await _dbContext.DreamDestinations
                    .FirstOrDefaultAsync(d => d.Id == dreamIdAsInt && d.UserId == user.Id); // Confronto int

                if (dream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERRORE: Destinazione con ID (int) {dreamIdAsInt} non trovata.");
                    return new JsonResult(new { success = false, message = "Destinazione non trovata" });
                }
                System.Diagnostics.Debug.WriteLine($"Destinazione trovata (ID: {dream.Id}). Procedo con rimozione.");

                // Rimuovi la destinazione dalla wishlist
                _dbContext.DreamDestinations.Remove(dream);
                System.Diagnostics.Debug.WriteLine($"Destinazione (ID: {dream.Id}) rimossa dal DbContext.");

                await _dbContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Rimozione destinazione completata.");

                return new JsonResult(new { success = true, message = "Destinazione rimossa con successo." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eccezione in RemoveDreamAsync: {ex.Message}");
                return new JsonResult(new { success = false, message = "Errore durante la rimozione della destinazione." });
            }
        }

        // --- OnPostMarkAsVisitedAsync (Aggiornato per ID string di PlannedTrip) ---
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostMarkAsVisitedAsync([FromBody] MarkAsVisited request) // Usa il modello corretto
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"MarkAsVisitedAsync chiamato con PlanId (string): {request?.PlanId}");

                if (string.IsNullOrWhiteSpace(request?.PlanId))
                {
                    System.Diagnostics.Debug.WriteLine("ERRORE: ID piano non valido.");
                    return new JsonResult(new { success = false, message = "ID piano non valido" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERRORE: Utente non autenticato.");
                    return new JsonResult(new { success = false, message = "Utente non autenticato" });
                }

                var planIdToMark = request.PlanId.Trim(); // ID string

                // Recupera il piano dal database (confronto string)
                var plan = await _dbContext.PlannedTrips
                    .FirstOrDefaultAsync(p => p.Id == planIdToMark && p.UserId == user.Id); // Confronto string

                if (plan == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERRORE: Piano con ID '{planIdToMark}' non trovato.");
                    return new JsonResult(new { success = false, message = "Piano di viaggio non trovato" });
                }
                System.Diagnostics.Debug.WriteLine($"Piano trovato (ID: {plan.Id}). Procedo con Marcatura Visitato.");

                // Trova il paese corrispondente nel database
                var country = await _dbContext.Countries
                    .FirstOrDefaultAsync(c => c.Name == plan.CountryName || c.Code == plan.CountryCode);

                if (country == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERRORE: Paese '{plan.CountryName}' (Code: '{plan.CountryCode}') non trovato nel DB.");
                    // Potresti voler gestire questo caso diversamente, ma per ora restituisci errore
                    return new JsonResult(new { success = false, message = "Paese di destinazione non trovato nel database. Impossibile segnare come visitato." });
                }
                System.Diagnostics.Debug.WriteLine($"Paese trovato (ID: {country.Id}, Name: {country.Name}).");

                // Verifica se l'utente ha già visitato questo paese
                var existingVisit = await _dbContext.VisitedCountries
                    .FirstOrDefaultAsync(vc => vc.UserId == user.Id && vc.CountryId == country.Id);

                if (existingVisit == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Nessuna visita esistente per Paese ID {country.Id}. Aggiungo nuova VisitedCountry.");
                    // Crea un nuovo record di visita
                    var visitedCountry = new VisitedCountry
                    {
                        UserId = user.Id,
                        CountryId = country.Id,
                        VisitDate = plan.EndDate // Usa la data di fine viaggio come data della visita
                    };
                    _dbContext.VisitedCountries.Add(visitedCountry);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Visita già esistente per Paese ID {country.Id}. Non aggiungo duplicato.");
                    // Potresti aggiornare la VisitDate se lo desideri: existingVisit.VisitDate = plan.EndDate;
                }

                // Crea opzionalmente un TravelJournal
                if (!string.IsNullOrEmpty(plan.Notes))
                {
                    System.Diagnostics.Debug.WriteLine($"Note presenti nel piano. Aggiungo/Aggiorno TravelJournal.");
                    // Verifica se esiste già un diario per questo utente/paese
                    var existingJournal = await _dbContext.TravelJournals
                        .FirstOrDefaultAsync(tj => tj.UserId == user.Id && tj.CountryId == country.Id);

                    if (existingJournal == null)
                    {
                        var travelJournal = new TravelJournal
                        {
                            UserId = user.Id,
                            CountryId = country.Id,
                            Notes = plan.Notes,
                            VisitDate = plan.EndDate,
                            Rating = 5 // Rating di default
                        };
                        _dbContext.TravelJournals.Add(travelJournal);
                        System.Diagnostics.Debug.WriteLine($"Nuovo TravelJournal creato.");
                    }
                    else
                    {
                        // Aggiorna il diario esistente (es. aggiungi note o aggiorna data)
                        existingJournal.Notes += $"\n\nNote aggiunte dal viaggio del {plan.EndDate:dd/MM/yyyy}:\n{plan.Notes}";
                        existingJournal.VisitDate = plan.EndDate; // Aggiorna all'ultima data
                        System.Diagnostics.Debug.WriteLine($"TravelJournal esistente aggiornato.");
                    }

                }

                // Rimuovi gli elementi della checklist associati al piano
                var checklistItems = await _dbContext.ChecklistItems
                    .Where(c => c.PlannedTripId == plan.Id) // Filtra per ID string piano
                    .ToListAsync();

                if (checklistItems.Any())
                {
                    _dbContext.ChecklistItems.RemoveRange(checklistItems);
                    System.Diagnostics.Debug.WriteLine($"Rimossi {checklistItems.Count} elementi checklist associati.");
                }

                // Rimuovi il piano
                _dbContext.PlannedTrips.Remove(plan);
                System.Diagnostics.Debug.WriteLine($"Piano (ID: {plan.Id}) rimosso dal DbContext.");

                // Salva tutte le modifiche al database
                await _dbContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Marcatura visitato completata.");

                return new JsonResult(new
                {
                    success = true,
                    message = $"{plan.CityName}, {plan.CountryName} contrassegnata come visitata!",
                    visitedCountry = new // DTO della visita (opzionale)
                    {
                        id = country.Id,
                        name = country.Name,
                        code = country.Code,
                        visitDate = plan.EndDate.ToString("yyyy-MM-dd")
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eccezione in MarkAsVisitedAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new JsonResult(new { success = false, message = "Errore durante la marcatura come visitato." });
            }
        }


        // --- Classi Modello per le Richieste JSON ---
        // (Assicurati che siano definite UNA SOLA VOLTA nel file)

        // Per spostare da Wishlist (int ID) a Planning (string ID)
        public class MoveToPlanning
        {
            public string? DreamId { get; set; } // Riceve ID int come stringa
        }

        // Per marcare un piano (string ID) come visitato
        public class MarkAsVisited
        {
            public string? PlanId { get; set; } // Riceve ID stringa
        }

        // Per rimuovere un piano (string ID)
        public class RemovePlan
        {
            public string? PlanId { get; set; } // Riceve ID stringa
        }

        // Per rimuovere un sogno dalla wishlist (int ID)
        public class RemoveDreamRequest // Nome specifico
        {
            public string? DreamId { get; set; } // Riceve ID int come stringa
        }

        // Per aggiornare i dettagli di un piano (string ID)
        public class UpdatePlan
        {
            public string? PlanId { get; set; } // Riceve ID stringa
            public string? Notes { get; set; }
            public string? StartDate { get; set; } // Riceve come stringa
            public string? EndDate { get; set; }   // Riceve come stringa
            // CompletionPercentage non è più necessaria qui, viene calcolata dal server
            public List<ChecklistItemDto>? Checklist { get; set; } // Usa DTO
        }

        // DTO per ChecklistItem (ricevuto/inviato via JSON)
        public class ChecklistItemDto
        {
            // L'ID ricevuto dal client può essere null (nuovo) o una stringa (esistente o temporaneo client)
            // L'ID inviato nella risposta sarà l'ID int del DB convertito in stringa.
            public string? Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Category { get; set; }
            public DateTime? DueDate { get; set; }
            public bool IsCompleted { get; set; }
        }

        // Modello per il Form Wishlist (invariato)
        public class WishlistItemViewModel
        {
            [Required(ErrorMessage = "Seleziona una città")]
            public string? City { get; set; }

            public string? Country { get; set; }
            public string? CountryCode { get; set; }
            public string? Notes { get; set; }
            public string? Tags { get; set; }
            public string? Priority { get; set; } = "Media";
            public IFormFile? ImageFile { get; set; }

            public List<CityInfo> AvailableCities { get; set; } = new List<CityInfo>();
        }

        // Helper per il dropdown città (invariato)
        public class CityInfo
        {
            public string? Name { get; set; }
            public string? Country { get; set; }
            public string? CountryCode { get; set; }
        }


        // --- Altri metodi helper (GetAvailableCapitalsAsync, SaveWishlistImageAsync, BuildPrompt, ecc.) ---
        // ... (assicurati che esistano e siano corretti) ...
        // Metodo ausiliario per salvare l'immagine della wishlist
        private async Task<string?> SaveWishlistImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;

            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "wishlist");
                Directory.CreateDirectory(uploadsFolder); // Crea se non esiste

                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return $"/images/wishlist/{uniqueFileName}"; // Restituisce percorso relativo web
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore salvataggio immagine wishlist: {ex.Message}");
                return null; // O restituisci un'immagine di default
            }
        }

        private async Task<List<CityInfo>> GetAvailableCapitalsAsync()
        {
            try
            {
                // Ottieni tutte le città che sono capitali, includendo il paese
                var capitalCities = await _dbContext.Cities
                   .Where(c => c.IsCapital == true) // Filtra per capitali
                   .Include(c => c.Country) // Carica i dati del paese associato
                   .OrderBy(c => c.Country.Name).ThenBy(c => c.Name) // Ordina per paese, poi per città
                   .ToListAsync();

                // Mappa le città trovate nel formato CityInfo
                var cityInfoList = capitalCities
                   .Where(city => city.Country != null) // Assicurati che il paese esista
                   .Select(city => new CityInfo
                   {
                       Name = city.Name,
                       Country = city.Country.Name,
                       CountryCode = city.Country.Code
                   })
                   .ToList();

                System.Diagnostics.Debug.WriteLine($"Trovate {cityInfoList.Count} capitali per il dropdown.");
                return cityInfoList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in GetAvailableCapitalsAsync: {ex.Message}");
                return new List<CityInfo>(); // Restituisci lista vuota in caso di errore
            }
        }

        private void VerifyRequiredImages()
        {
            try
            {
                // Percorsi delle immagini da verificare
                var imagePaths = new[]
                {
                    Path.Combine(_webHostEnvironment.WebRootPath, "images", "empty-wishlist.svg"),
                    Path.Combine(_webHostEnvironment.WebRootPath, "images", "empty-planning.svg"),
                    Path.Combine(_webHostEnvironment.WebRootPath, "images", "placeholder-destination.jpg"),
                    Path.Combine(_webHostEnvironment.WebRootPath, "images", "default-city.jpg") // Assicurati che esista anche questa
                };

                // Verifica se le immagini esistono e creale se mancano
                foreach (var path in imagePaths)
                {
                    if (!System.IO.File.Exists(path))
                    {
                        string filename = Path.GetFileName(path);
                        string extension = Path.GetExtension(path).ToLowerInvariant();
                        // Crea la directory se non esiste
                        string? directoryName = Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                        if (extension == ".svg")
                        {
                            // Crea un semplice SVG placeholder
                            string svgContent = @"<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg""><rect width=""100%"" height=""100%"" fill=""#eee""/><text x=""50%"" y=""50%"" dominant-baseline=""middle"" text-anchor=""middle"" fill=""#aaa"" font-size=""10"">" + filename + @"</text></svg>";
                            System.IO.File.WriteAllText(path, svgContent);
                            System.Diagnostics.Debug.WriteLine($"Creato file SVG placeholder: {path}");
                        }
                        else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            // Prova a copiare default-city.jpg se esiste
                            string defaultImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "default-city.jpg");
                            if (System.IO.File.Exists(defaultImagePath) && path != defaultImagePath)
                            {
                                try
                                {
                                    System.IO.File.Copy(defaultImagePath, path);
                                    System.Diagnostics.Debug.WriteLine($"Copiata immagine di default in: {path}");
                                }
                                catch (IOException ioEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Errore IO durante la copia dell'immagine di default per {path}: {ioEx.Message}");
                                }
                            }
                            else if (!System.IO.File.Exists(defaultImagePath) && path != defaultImagePath)
                            {
                                System.Diagnostics.Debug.WriteLine($"WARN: Immagine di default '{defaultImagePath}' non trovata per creare {path}.");
                                // Potresti creare un JPG/PNG placeholder programmaticamente qui se necessario
                            }
                            else if (path == defaultImagePath && !System.IO.File.Exists(defaultImagePath))
                            {
                                System.Diagnostics.Debug.WriteLine($"WARN: Immagine di default '{defaultImagePath}' mancante.");
                                // Crea un placeholder anche per default-city.jpg
                                // (Logica per creare JPG/PNG placeholder non inclusa qui per brevità)

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nella verifica delle immagini: {ex.Message}");
            }
}
        
        // Nuovo handler per ottenere consigli in base alla categoria
          [HttpGet]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> OnGetRecommendationsAsync(string type)
{
    try
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) 
            return new JsonResult(new { success = false, error = "Utente non autenticato." });
        
        if (string.IsNullOrEmpty(_geminiApiKey))
        {
            return new JsonResult(new { success = false, error = "Chiave API Gemini non configurata" });
        }
        
        // Build a prompt for recommendations based on type
        string prompt = $"Fornisci 5 suggerimenti di viaggio per la categoria '{type}'. " +
                       "Restituisci solo un array JSON di destinazioni con id, cityName, countryName, description, " +
                       "reasonToVisit, latitude, longitude e imageUrl. " +
                       "Non aggiungere altro testo o commenti, solo JSON.";
        
        // Call the dedicated method to handle Gemini API request
        var recommendations = await CallGeminiForRecommendationsAsync(prompt);
        
        if (recommendations.Any())
        {
            //***  ADD LOGGING HERE  ***
            System.Diagnostics.Debug.WriteLine($"[OnGetRecommendationsAsync] Dati ottenuti da CallGeminiForRecommendationsAsync");
            foreach (var rec in recommendations)
            {
                System.Diagnostics.Debug.WriteLine($"[OnGetRecommendationsAsync] ---- {rec.ToString()}");
            }
                
            return new JsonResult(new { 
                success = true, 
                recommendations = recommendations 
            });
        }
        else
        {
            // Use a fallback set of recommendations if the API returns nothing
            var fallbackRecommendations = GetFallbackRecommendations();
            return new JsonResult(new { 
                success = true, 
                recommendations = fallbackRecommendations,
                message = "Using fallback recommendations due to API issue" 
            });
        }
    }
    catch (Exception ex) 
    {
        System.Diagnostics.Debug.WriteLine($"Error in recommendations handler: {ex.Message}");
        return new JsonResult(new { 
            success = false, 
            message = $"Error processing recommendations: {ex.Message}"
        });
    }
}  
                // Helper method for fallback recommendations
                private List<RecommendationItem> GetFallbackRecommendations()
                {
                    return new List<RecommendationItem>
                    {
                        new RecommendationItem 
                        { 
                            Id = "fallback1",
                            CityName = "Roma", 
                            CountryName = "Italia", 
                            Description = "La città eterna con una storia millenaria",
                            ReasonToVisit = "Perfetta per gli amanti della storia e della cultura",
                            ImageUrl = "/images/destinations/rome.jpg",
                            Latitude = 41.9028,
                            Longitude = 12.4964
                        },
                        new RecommendationItem 
                        { 
                            Id = "fallback2",
                            CityName = "Parigi", 
                            CountryName = "Francia", 
                            Description = "La città dell'amore e delle luci",
                            ReasonToVisit = "Romantica e piena di arte",
                            ImageUrl = "/images/destinations/paris.jpg",
                            Latitude = 48.8566,
                            Longitude = 2.3522
                        },
                        // Add a few more fallback recommendations if you wish
                    };
                }
        
        
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetTravelsuggestionsAsync(string cityName, string suggestionType)
        {
            // ... (Codice esistente per chiamare Gemini API) ...
            // Assicurati che la logica interna sia corretta
            try
            {
                System.Diagnostics.Debug.WriteLine($"Handler GetTravelsuggestionsAsync chiamato - Città: {cityName}, Tipo: {suggestionType}");

                var client = _clientFactory.CreateClient();

                if (string.IsNullOrEmpty(_geminiApiKey))
                {
                    System.Diagnostics.Debug.WriteLine("Errore: Chiave API Gemini mancante");
                    return new JsonResult(new { success = false, error = "Chiave API Gemini non configurata" });
                }

                string prompt = BuildPrompt(cityName, suggestionType); // Assicurati che BuildPrompt esista

                var requestData = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } }
                };

                // Prova prima con 1.5 Flash che sembra più permissivo per account gratuiti
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_geminiApiKey}";

                System.Diagnostics.Debug.WriteLine($"URL API: {apiUrl.Substring(0, apiUrl.IndexOf('?'))}?key=API_KEY_HIDDEN");
                System.Diagnostics.Debug.WriteLine($"Prompt inviato: {prompt.Substring(0, Math.Min(prompt.Length, 100))}...");

                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(45); // Timeout leggermente più lungo

                HttpResponseMessage response;
                string responseContent;

                try
                {
                    response = await client.PostAsync(apiUrl, content);
                    responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Risposta da {apiUrl}: {response.StatusCode}");
                }
                catch (TaskCanceledException timeoutEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Timeout API Gemini: {timeoutEx.Message}");
                    return new JsonResult(new { success = false, error = "Timeout durante la richiesta all'API Gemini." });
                }
                catch (HttpRequestException httpEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore HTTP API Gemini: {httpEx.Message}");
                    return new JsonResult(new { success = false, error = $"Errore di rete API Gemini: {httpEx.StatusCode}" });
                }


                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore API ({response.StatusCode}): {responseContent}");
                    // Qui potresti provare altri modelli se 1.5-flash fallisce, ma per ora restituisci errore
                    return new JsonResult(new { success = false, error = $"Errore dall'API Gemini ({response.StatusCode})", debugInfo = responseContent });
                }

                // Parsing della risposta (più robusto)
                try
                {
                    dynamic? parsedResponse = JsonConvert.DeserializeObject(responseContent);

                    // Navigazione sicura nell'oggetto JSON
                    string? textResult = parsedResponse?.candidates?[0]?.content?.parts?[0]?.text?.ToString();

                    if (!string.IsNullOrEmpty(textResult))
                    {
                        System.Diagnostics.Debug.WriteLine($"Testo ricevuto da Gemini: {textResult.Substring(0, Math.Min(textResult.Length, 200))}...");
                        string htmlContent = CleanupMarkdownCodeDelimiters(textResult); // Assicurati che CleanupMarkdownCodeDelimiters esista
                        return new JsonResult(new { success = true, html = htmlContent });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Risposta API valida ma struttura JSON inattesa o testo mancante.");
                        System.Diagnostics.Debug.WriteLine($"Raw Response: {responseContent}");
                        return new JsonResult(new { success = false, error = "Struttura risposta API inattesa.", debugInfo = responseContent });
                    }
                }
                catch (JsonReaderException jsonEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore parsing JSON risposta: {jsonEx.Message}");
                    return new JsonResult(new { success = false, error = "Errore nel leggere la risposta dall'API.", debugInfo = responseContent });
                }
                catch (Exception parseEx) // Altre eccezioni durante il parsing
                {
                    System.Diagnostics.Debug.WriteLine($"Errore generico parsing risposta: {parseEx.Message}");
                    return new JsonResult(new { success = false, error = "Errore nell'elaborare la risposta dall'API.", debugInfo = responseContent });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eccezione in OnGetTravelsuggestionsAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new JsonResult(new { success = false, error = $"Errore interno del server: {ex.Message}" });
            }
        }
        private string BuildPrompt(string cityName, string suggestionType)
        {
            // Implementazione di BuildPrompt come fornita precedentemente
            // ... (assicurati che generi prompt corretti) ...
            string basePrompt = $"Sei un esperto di viaggi conciso e focalizzato. Fornisci SOLO il contenuto richiesto in formato HTML valido, senza prefazioni, conclusioni, commenti HTML (<!-- -->) o delimitatori di codice come ```html.";

            switch (suggestionType.ToLowerInvariant())
            {
                case "attractions":
                    return $"{basePrompt} Lista 5 attrazioni principali di {cityName}. Per ogni attrazione: <ul><li><strong>Nome Attrazione:</strong> Breve descrizione (10-15 parole).</li></ul>";
                case "gastronomy":
                    return $"{basePrompt} Breve introduzione <p>sulla cucina locale di {cityName} (20 parole max).</p> Lista 5 piatti/esperienze culinarie: <ul><li><strong>Nome Piatto:</strong> Breve descrizione (10-15 parole).</li></ul>";
                case "history":
                    return $"{basePrompt} Breve introduzione <p>sulla storia di {cityName} (20 parole max).</p> Lista 5-6 eventi storici significativi in ordine cronologico: <ul><li><strong>Anno/Periodo:</strong> Fatto interessante (10-15 parole).</li></ul>";
                case "tips":
                    return $"{basePrompt} Lista 5-6 consigli pratici per visitare {cityName} (es. trasporti, sicurezza, periodo migliore, etc.): <ul><li><strong>Consiglio (es. Trasporti):</strong> Dettaglio pratico (15-20 parole).</li></ul>";
                default:
                    return $"{basePrompt} Fornisci una descrizione generale di {cityName} come meta turistica in un singolo paragrafo <p>.</p>";
            }
        }

        private string CleanupMarkdownCodeDelimiters(string content)
        {
            // Implementazione di CleanupMarkdownCodeDelimiters come fornita precedentemente
            // ... (assicurati che rimuova ```html e ```) ...
            if (string.IsNullOrEmpty(content)) return content;

            // Rimuovi ```html, ```HTML, ``` all'inizio, con eventuali spazi/newline
            content = System.Text.RegularExpressions.Regex.Replace(content, @"^\s*```(html|HTML)?\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Rimuovi ``` alla fine, con eventuali spazi/newline
            content = System.Text.RegularExpressions.Regex.Replace(content, @"\s*```\s*$", "");
            // Rimuovi eventuali tag <pre> o </pre> rimasti (meno probabile con i prompt aggiornati)
            content = System.Text.RegularExpressions.Regex.Replace(content, @"^\s*<pre>\s*|\s*</pre>\s*$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);


            return content.Trim();
        }

        public class RecommendationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CityName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReasonToVisit { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ImageUrl { get; set; } = "/images/placeholder-destination.jpg";
}

    } // Chiusura classe DreamMapModel
} // Chiusura namespace