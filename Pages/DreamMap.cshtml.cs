using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WanderGlobe.Models;
using WanderGlobe.Models.Custom;
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

namespace WanderGlobe.Pages
{
    [Authorize]
    public class DreamMapModel : PageModel
    {        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICountryService _countryService;
        private readonly IDreamService _dreamService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _geminiApiKey;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICityService _cityService;

        public List<DreamDestination> Wishlist { get; set; } = new List<DreamDestination>();
        public List<PlannedTrip> PlannedTrips { get; set; } = new List<PlannedTrip>();
        public List<RecommendedDestination> Recommendations { get; set; } = new List<RecommendedDestination>();
        public List<Country> Countries { get; set; } = new List<Country>();
        public MapDestinationsViewModel AllDestinations { get; set; } = new MapDestinationsViewModel();

        [BindProperty]
        public WishlistItemViewModel WishlistForm { get; set; } = new WishlistItemViewModel();        // Costruttore aggiornato con ICityService
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

            // Debug della chiave API
            System.Diagnostics.Debug.WriteLine($"GeminiApiKey: {(_geminiApiKey != null ? "PRESENTE" : "MANCANTE")}");
            if (_geminiApiKey != null && _geminiApiKey.Length > 5)
            {
                System.Diagnostics.Debug.WriteLine($"Primi 5 caratteri della chiave: {_geminiApiKey.Substring(0, 5)}");
            }
        }

        // Test API semplice
        [IgnoreAntiforgeryToken]
        public JsonResult OnGetSimpletest()
        {
            return new JsonResult(new { success = true, message = "Test riuscito!" });
        }        // Caricamento iniziale della pagina
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

                    // Recupera le destinazioni dei sogni dell'utente
                    Wishlist = await _dreamService.GetUserWishlistAsync(user.Id);

                    // Recupera i viaggi pianificati dell'utente
                    PlannedTrips = await _dreamService.GetUserPlannedTripsAsync(user.Id);

                    // Recupera i suggerimenti personalizzati
                    Recommendations = await _dreamService.GetRecommendationsAsync(user.Id);

                    // Prepara i dati per la mappa
                    var visitedCountries = await _countryService.GetVisitedCountriesByUserAsync(user.Id);

                    // Prepara la lista delle città visitate con operazioni asincrone
                    var visitedCitiesItems = new List<MapDestinationItem>();
                    foreach (var v in visitedCountries)
                    {
                        string capitalName = await GetCapitalAsync(v.Country.Code);
                        visitedCitiesItems.Add(new MapDestinationItem
                        {
                            Id = NormalizeNameToId(capitalName, v.Country.Code),
                            CityName = capitalName,
                            CountryName = v.Country.Name,
                            CountryCode = v.Country.Code,
                            Latitude = v.Country.Latitude,
                            Longitude = v.Country.Longitude
                        });
                    }

                    // Popola il modello con tutti i dati per la mappa
                    AllDestinations = new MapDestinationsViewModel
                    {
                        Wishlist = Wishlist.Select(d => new MapDestinationItem
                        {
                            Id = NormalizeNameToId(d.CityName, d.CountryCode),
                            CityName = d.CityName,
                            CountryName = d.CountryName,
                            CountryCode = d.CountryCode,
                            Latitude = d.Latitude,
                            Longitude = d.Longitude,
                            Priority = (int)d.Priority
                        }).ToList(),

                        PlannedTrips = PlannedTrips.Select(p => new MapDestinationItem
                        {
                            Id = NormalizeNameToId(p.CityName, p.CountryCode),
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
                        AvailableCities = await GetAvailableCapitalsAsync()
                    };
                }
            }
            catch (Exception ex)
            {
                // Log dell'errore
                System.Diagnostics.Debug.WriteLine($"Errore in OnGetAsync: {ex.Message}");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento della pagina.";
            }
        }

               [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostSaveToWishlistAsync()
        {
            try
            {
                if (WishlistForm == null)
                {
                    return new JsonResult(new { success = false, message = "Model binding failed (model is null)." });
                }
        
                // Correct key for the City property in ModelState
                string cityFieldKeyInModelState = $"{nameof(this.WishlistForm)}.{nameof(WishlistItemViewModel.City)}";
        
                // Log ModelState before processing
                System.Diagnostics.Debug.WriteLine("--- ModelState Before Processing ---");
                foreach (var state in ModelState)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {state.Key}, Errors: {state.Value.Errors.Count}");
                    foreach (var error in state.Value.Errors) 
                        System.Diagnostics.Debug.WriteLine($"  Error: {error.ErrorMessage}");
                }
        
                // Clear any existing errors on City field to prevent validation issues
                ModelState.Remove(cityFieldKeyInModelState);
        
                // Validate model manually
                if (string.IsNullOrEmpty(WishlistForm.City))
                {
                    return new JsonResult(new { success = false, message = "Seleziona una città valida." });
                }
        
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "Utente non autenticato" });
                }
        
                // Get city information from available cities
                var cityInfo = (await GetAvailableCapitalsAsync()).FirstOrDefault(c => c.Name == WishlistForm.City);
                
                // Debug for city info
                System.Diagnostics.Debug.WriteLine($"CityInfo found: {(cityInfo != null ? "Yes" : "No")} for city: {WishlistForm.City}");
                
                // Make sure we have country information
                if (cityInfo == null)
                {
                    System.Diagnostics.Debug.WriteLine($"CityInfo not found for selected city: {WishlistForm.City}");
                    
                    // Create default city info if not found
                    cityInfo = new CityInfo
                    {
                        Name = WishlistForm.City,
                        Country = WishlistForm.Country ?? "Paese sconosciuto",
                        CountryCode = WishlistForm.CountryCode ?? "XX"
                    };
                }
                else
                {
                    // Ensure the model has country data from cityInfo
                    WishlistForm.Country = cityInfo.Country;
                    WishlistForm.CountryCode = cityInfo.CountryCode;
                }
                
                // Debug country data
                System.Diagnostics.Debug.WriteLine($"Country data: {WishlistForm.Country}, Code: {WishlistForm.CountryCode}");
        
                // Ensure we have non-null values for required fields
                WishlistForm.Tags = WishlistForm.Tags ?? "";
                WishlistForm.Notes = WishlistForm.Notes ?? "";
                WishlistForm.Priority = WishlistForm.Priority ?? "Media";
        
                // Parse priority enum
                DreamPriority priority;
                if (!Enum.TryParse(WishlistForm.Priority, true, out priority))
                {
                    priority = DreamPriority.Medium;
                }
        
                // Process image if available
                string imageUrl = null;
                if (WishlistForm.ImageFile != null && WishlistForm.ImageFile.Length > 0)
                {
                    imageUrl = await SaveWishlistImageAsync(WishlistForm.ImageFile);
                }
        
                // Process tags
                List<string> tagList = new List<string>();
                if (!string.IsNullOrEmpty(WishlistForm.Tags))
                {
                    tagList = WishlistForm.Tags.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }
        
                // Set default latitude/longitude if not available
                double latitude = 0, longitude = 0;
                
                // Find the city in database to get proper coordinates
                var cityFromDb = await _dbContext.Cities
                    .Include(c => c.Country)
                    .FirstOrDefaultAsync(c => 
                        c.Name.ToLower() == WishlistForm.City.ToLower() && 
                        c.Country.Name.ToLower() == WishlistForm.Country.ToLower());
                if (cityFromDb != null)
                {
                    latitude = cityFromDb.Latitude  ?? 0.0;
                    longitude = cityFromDb.Longitude  ?? 0.0;
                    System.Diagnostics.Debug.WriteLine($"Found city in DB: {cityFromDb.Name}, Lat: {latitude}, Lon: {longitude}");
                }
                else
                {
                    // Use default coordinates - better than zeros
                    System.Diagnostics.Debug.WriteLine($"City not found in DB, using default lat/long");
                    latitude = 45.0; // Default latitude
                    longitude = 9.0; // Default longitude
                }
        
                // Create new dream destination with all fields properly set
                var newDream = new DreamDestination
                {
                    UserId = user.Id,
                    CityName = WishlistForm.City ?? "Città sconosciuta",
                    CountryName = WishlistForm.Country ?? "Paese non specificato",
                    CountryCode = WishlistForm.CountryCode ?? "XX",
                    Note = WishlistForm.Notes,
                    Tags = tagList,
                    Priority = priority,
                    Latitude = latitude,
                    Longitude = longitude,
                    ImageUrl = imageUrl ?? $"/images/cities/{(WishlistForm.CountryCode ?? "default").ToLower()}-city.jpg",
                    CreatedAt = DateTime.UtcNow
                };
        
                // Extra debug logging before save
                System.Diagnostics.Debug.WriteLine($"About to save new dream: City={newDream.CityName}, Country={newDream.CountryName}, Code={newDream.CountryCode}");
        
                try
                {
                    var savedDream = await _dreamService.AddToWishlistAsync(newDream);
        
                    if (savedDream != null)
                    {
                        return new JsonResult(new
                        {
                            success = true,
                            message = $"{WishlistForm.City} aggiunta alla tua wishlist!",
                            newItem = savedDream
                        });
                    }
                    else
                    {
                        return new JsonResult(new
                        {
                            success = false,
                            message = "Errore nel salvataggio della destinazione."
                        });
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    // Log the detailed inner exception for debugging
                    var innerMsg = dbEx.InnerException?.Message ?? "No inner exception";
                    System.Diagnostics.Debug.WriteLine($"Database error: {dbEx.Message}. Inner: {innerMsg}");
                    
                    return new JsonResult(new
                    {
                        success = false,
                        message = $"Errore database: {dbEx.Message}. Dettagli: {innerMsg}"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in OnPostSaveToWishlistAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                
                // Include inner exception details if available
                var innerMsg = ex.InnerException?.Message ?? "Nessun dettaglio aggiuntivo";
                
                return new JsonResult(new
                {
                    success = false,
                    message = $"Errore server: {ex.Message}. Dettagli: {innerMsg}"
                });
            }
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetCityinfoAsync(string city)
        {
            var cityInfo = (await GetAvailableCapitalsAsync()).FirstOrDefault(c => c.Name == city);

            if (cityInfo == null)
            {
                return new JsonResult(new { success = false });
            }

            return new JsonResult(new
            {
                success = true,
                city = cityInfo.Name,
                country = cityInfo.Country,
                countryCode = cityInfo.CountryCode
            });
        }

        // Metodo ausiliario per salvare l'immagine della wishlist
        private async Task<string> SaveWishlistImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;

            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "wishlist");

                // Crea la directory se non esiste
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Genera un nome file univoco
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                // Salva il file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Restituisci il percorso relativo
                return $"/images/wishlist/{fileName}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio dell'immagine: {ex.Message}");
                return null;
            }
        }

        // Helper per normalizzare nomi di città in ID
        private string NormalizeNameToId(string cityName, string countryCode)
        {
            if (string.IsNullOrEmpty(cityName))
                return "unknown";

            // Rimuovi spazi e caratteri speciali, converti in minuscolo
            string normalizedName = cityName.ToLower()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "");

            // Aggiungi il codice paese per unicità (in minuscolo)
            if (!string.IsNullOrEmpty(countryCode))
                return normalizedName + "_" + countryCode.ToLower();

            return normalizedName;
        }

        [HttpGet]
        public IActionResult OnGetApiTest()
        {
            return new JsonResult(new { success = true, message = "API funzionante" });
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetTravelsuggestionsAsync(string cityName, string suggestionType)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Handler chiamato - Città: {cityName}, Tipo: {suggestionType}");

                var client = _clientFactory.CreateClient();

                if (string.IsNullOrEmpty(_geminiApiKey))
                {
                    System.Diagnostics.Debug.WriteLine("Errore: Chiave API Gemini mancante");
                    return new JsonResult(new { error = "Chiave API Gemini non configurata" });
                }

                string prompt = BuildPrompt(cityName, suggestionType);

                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                // Usa il modello gemini-2.0-flash, che dovrebbe essere disponibile con una API key gratuita
                string apiUrl = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";

                System.Diagnostics.Debug.WriteLine($"URL API: {apiUrl.Substring(0, apiUrl.IndexOf('?'))}?key=XXXXX");

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json");

                client.Timeout = TimeSpan.FromSeconds(30);

                // Log della richiesta JSON per debug
                System.Diagnostics.Debug.WriteLine($"Richiesta JSON: {JsonConvert.SerializeObject(requestData)}");

                var response = await client.PostAsync(apiUrl, content);

                string responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Risposta HTTP: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Contenuto risposta: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}...");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        dynamic parsedResponse = JsonConvert.DeserializeObject(responseContent);

                        if (parsedResponse?.candidates != null &&
                            parsedResponse.candidates.Count > 0 &&
                            parsedResponse.candidates[0].content != null &&
                            parsedResponse.candidates[0].content.parts != null &&
                            parsedResponse.candidates[0].content.parts.Count > 0)
                        {
                            string htmlContent = parsedResponse.candidates[0].content.parts[0].text;

                            // Pulizia dei delimitatori markdown
                            htmlContent = CleanupMarkdownCodeDelimiters(htmlContent);

                            return new JsonResult(new
                            {
                                success = true,
                                html = htmlContent
                            });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Risposta API valida ma struttura non corretta");
                            return new JsonResult(new
                            {
                                error = "Risposta API valida ma struttura non corretta",
                                debug = responseContent
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Errore parsing JSON: {ex.Message}");
                        return new JsonResult(new
                        {
                            error = $"Errore nel parsing della risposta: {ex.Message}",
                            debug = responseContent
                        });
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Errore API: {response.StatusCode} - {responseContent}");                    // Se il modello non è disponibile, prova con modelli alternativi
                    if (apiUrl.Contains("gemini-2.0-flash") && (response.StatusCode == System.Net.HttpStatusCode.NotFound || responseContent.Contains("not found")))
                    {
                        // Prova con gemini-1.5-flash
                        System.Diagnostics.Debug.WriteLine("Modello non trovato, tentativo con modello alternativo: gemini-1.5-flash");
                        string alternativeUrl = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";                        var alternativeResponse = await client.PostAsync(alternativeUrl, content);
                        if (alternativeResponse.IsSuccessStatusCode)
                        {
                            string altContent = await alternativeResponse.Content.ReadAsStringAsync();
                            dynamic? altParsed = JsonConvert.DeserializeObject(altContent);                            if (altParsed?.candidates != null &&
                                altParsed.candidates.Count > 0 &&
                                altParsed.candidates[0]?.content != null &&
                                altParsed.candidates[0].content?.parts != null &&
                                altParsed.candidates[0].content.parts.Count > 0 &&
                                altParsed.candidates[0].content.parts[0]?.text != null)
                            {
                                string htmlContent = altParsed.candidates[0].content.parts[0]?.text;
                                if (htmlContent != null)
                                {                                    htmlContent = CleanupMarkdownCodeDelimiters(htmlContent);

                                    return new JsonResult(new
                                    {
                                        success = true,
                                        html = htmlContent,
                                        note = "Utilizzato modello alternativo"
                                    });
                                }
                            }
                        }
                    }

                    return new JsonResult(new
                    {
                        error = $"Errore API Gemini: {response.StatusCode}",
                        debug = responseContent
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eccezione: {ex.Message}");
                return new JsonResult(new { error = $"Errore: {ex.Message}" });
            }
        }

        // Metodo helper per pulire i delimitatori Markdown dalla risposta
        private string CleanupMarkdownCodeDelimiters(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            // Rimuovi ```html o ```HTML all'inizio (case insensitive)
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"^\s*```(?:html|HTML)?\s*\n?",
                "",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            // Rimuovi ``` alla fine
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"\s*```\s*$",
                "",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            // Rimuovi anche eventuali <pre> e </pre> inseriti erroneamente
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"^\s*<pre>\s*|\s*</pre>\s*$",
                "",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );            return content.Trim();
        }          // Metodo per ottenere tutte le città disponibili dal database
        private async Task<List<CityInfo>> GetAvailableCapitalsAsync()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Utilizziamo un approccio diretto con Entity Framework
                var allCities = await _dbContext.Cities
                    .Include(c => c.Country)
                    .Where(c => c.Country != null)
                    .ToListAsync();
                
                // Debug - quante città abbiamo trovato nel database?
                System.Diagnostics.Debug.WriteLine($"Trovate {allCities.Count} città totali nel database");
                
                var availableCities = allCities
                    .Select(city => new CityInfo
                    {
                        Name = city.Name,
                        Country = city.Country.Name,
                        CountryCode = city.Country.Code
                    })
                    .ToList();
                
                // Debug output
                System.Diagnostics.Debug.WriteLine($"Totale città disponibili per il dropdown: {availableCities.Count}");
                
                return availableCities.OrderBy(c => c.Country).ThenBy(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in GetAvailableCapitalsAsync: {ex.Message}");
                return new List<CityInfo>();
            }
        }
        
        // Metodo per ottenere il nome della capitale dal database tramite ICityService
        private async Task<string> GetCapitalAsync(string countryCode)
        {
            try
            {
                if (string.IsNullOrEmpty(countryCode))
                    return "Capitale sconosciuta";

                var country = await _dbContext.Countries
                    .FirstOrDefaultAsync(c => c.Code.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

                if (country == null)
                    return $"Capitale di {countryCode}";

                var capital = await _cityService.GetCapitalCityByCountryIdAsync(country.Id);
                
                return capital?.Name ?? $"Capitale di {country.Name}";
            }            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel recupero della capitale per {countryCode}: {ex.Message}");
                return $"Capitale di {countryCode}";
            }
        }

        // Metodo helper per costruire i prompt
        private string BuildPrompt(string cityName, string suggestionType)
        {
            switch (suggestionType)
            {
                case "attractions":
                    return $"Sei un esperto di viaggi. Fornisci 5 attrazioni turistiche principali da visitare a {cityName}. " +
                           $"Per ciascuna attrazione, indica il nome e una breve descrizione di 10-15 parole. " +
                           $"Formatta la risposta come lista HTML con tag <ul> e <li>. Per ogni attrazione, metti il nome in un tag <strong>. " +
                           $"La risposta deve contenere SOLO codice HTML, senza delimitatori di codice come ```html o ```. " +
                           $"Non inserire commenti, prefazioni o conclusioni. Inizia direttamente con i tag HTML.";

                case "gastronomy":
                    return $"Sei un esperto gastronomico. Descrivi 5 piatti o esperienze gastronomiche da provare a {cityName}. " +
                           $"Per ciascun piatto, indica il nome e una breve descrizione di 10-15 parole. " +
                           $"Inizia con un breve paragrafo introduttivo sulla cucina locale di {cityName}. " +
                           $"Formatta la risposta come HTML con un tag <p> per l'introduzione e una lista <ul> per i piatti. " +
                           $"Per ogni piatto, metti il nome in un tag <strong>. " +
                           $"La risposta deve contenere SOLO codice HTML, senza delimitatori di codice come ```html o ```. " +
                           $"Non inserire commenti, prefazioni o conclusioni. Inizia direttamente con i tag HTML.";

                case "history":
                    return $"Sei uno storico. Descrivi 6 eventi storici significativi di {cityName} in ordine cronologico. " +
                           $"Per ciascun evento, indica l'anno o periodo e un fatto interessante in 10-15 parole. " +
                           $"Inizia con un breve paragrafo introduttivo sulla storia generale di {cityName}. " +
                           $"Formatta la risposta come HTML con un tag <p> per l'introduzione e una lista <ul> per gli eventi storici. " +
                           $"La risposta deve contenere SOLO codice HTML, senza delimitatori di codice come ```html o ```. " +
                           $"Non inserire commenti, prefazioni o conclusioni. Inizia direttamente con i tag HTML.";

                case "tips":
                    return $"Sei un consulente di viaggi. Fornisci 6 consigli pratici per i turisti che visitano {cityName}. " +
                           $"Includi suggerimenti su trasporti, sicurezza, risparmio, cultura locale, periodo migliore per visitare e comunicazione. " +
                           $"Formatta la risposta come HTML con un paragrafo introduttivo <p> seguito da una lista <ul>. " +
                           $"Per ogni consiglio, metti il titolo (es. 'Trasporti', 'Sicurezza') in un tag <strong>. " +
                           $"La risposta deve contenere SOLO codice HTML, senza delimitatori di codice come ```html o ```. " +
                           $"Non inserire commenti, prefazioni o conclusioni. Inizia direttamente con i tag HTML.";

                default:
                    return $"Sei un esperto di viaggi. Fornisci informazioni generali su {cityName} come meta turistica. " +
                           $"Formatta la risposta come paragrafi HTML con tag <p>. " +
                           $"La risposta deve contenere SOLO codice HTML, senza delimitatori di codice come ```html o ```. " +
                           $"Non inserire commenti, prefazioni o conclusioni. Inizia direttamente con i tag HTML.";
            }
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult OnGetDebug()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var path = Request.Path.ToString();
            var method = Request.Method;

            return new JsonResult(new
            {
                message = "Debug info",
                path = path,
                method = method,
                headers = headers,
                queryParams = queryParams,
                hasGeminiKey = !string.IsNullOrEmpty(_geminiApiKey),
                geminiKeyFirstChars = !string.IsNullOrEmpty(_geminiApiKey) ? _geminiApiKey.Substring(0, 5) + "..." : null
            });
        }

        [IgnoreAntiforgeryToken]
        public IActionResult OnGetTest()
        {
            return new JsonResult(new { status = "ok", message = "Test riuscito!" });
        }

        // HANDLERS PER LE OPERAZIONI AJAX
                [HttpPost]
                [IgnoreAntiforgeryToken]
                public async Task<IActionResult> OnPostMoveToPlanningAsync([FromBody] MoveToPlanning request)
                {
                    if (string.IsNullOrEmpty(request?.DreamId))
                    {
                        return new JsonResult(new { success = false, message = "ID destinazione non valido" });
                    }
                
                    try
                    {
                        var user = await _userManager.GetUserAsync(User);
                        if (user == null)
                        {
                            return new JsonResult(new { success = false, message = "Utente non autenticato" });
                        }
                
                        // Converti l'ID da string a int se necessario
                        if (!int.TryParse(request.DreamId, out int dreamId))
                        {
                            return new JsonResult(new { success = false, message = "Formato ID non valido" });
                        }
                
                        // Ottieni la destinazione wishlist
                        var dreamItem = await _dbContext.DreamDestinations
                            .FirstOrDefaultAsync(d => d.Id == dreamId && d.UserId == user.Id);
                
                        if (dreamItem == null)
                        {
                            return new JsonResult(new { success = false, message = "Destinazione non trovata" });
                        }
                
                        // Crea una nuova entità per il viaggio pianificato
                        var plannedTrip = new PlannedTrip
                        {
                            UserId = user.Id,
                            CityName = dreamItem.CityName,
                            CountryName = dreamItem.CountryName,
                            CountryCode = dreamItem.CountryCode,
                            Notes = dreamItem.Note,
                            Latitude = dreamItem.Latitude,
                            Longitude = dreamItem.Longitude,
                            ImageUrl = dreamItem.ImageUrl,
                            StartDate = DateTime.UtcNow.AddDays(30), // Default a 30 giorni da oggi
                            EndDate = DateTime.UtcNow.AddDays(37),   // Default a 37 giorni da oggi
                            CompletionPercentage = 0,
                            CreatedAt = DateTime.UtcNow
                        };
                
                        // Salva nel database
                        _dbContext.PlannedTrips.Add(plannedTrip);
                        _dbContext.DreamDestinations.Remove(dreamItem);
                        await _dbContext.SaveChangesAsync();
                
                        return new JsonResult(new { 
                            success = true,
                            plannedTrip = new {
                                id = plannedTrip.Id,
                                cityName = plannedTrip.CityName,
                                countryName = plannedTrip.CountryName,
                                countryCode = plannedTrip.CountryCode,
                                startDate = plannedTrip.StartDate,
                                endDate = plannedTrip.EndDate,
                                completionPercentage = plannedTrip.CompletionPercentage,
                                notes = plannedTrip.Notes,
                                latitude = plannedTrip.Latitude,
                                longitude = plannedTrip.Longitude,
                                imageUrl = plannedTrip.ImageUrl
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Eccezione in MoveToPlanningAsync: {ex.Message}");
                        return new JsonResult(new { success = false, message = ex.Message });
                    }
                }  
                
                [HttpPost]
            [IgnoreAntiforgeryToken]
            public async Task<IActionResult> OnPostUpdatePlanDetailsAsync([FromBody] UpdatePlan request)
            {
                try
                {
                    if (string.IsNullOrEmpty(request?.PlanId))
                    {
                        return new JsonResult(new { success = false, message = "ID piano non valido" });
                    }
            
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return new JsonResult(new { success = false, message = "Utente non autenticato" });
                    }
            
                    // Converti l'ID da string a int se necessario
                    if (!int.TryParse(request.PlanId, out int planId))
                    {
                        return new JsonResult(new { success = false, message = "Formato ID non valido" });
                    }
            
                    // Trova il viaggio pianificato nel database
                    var plan = await _dbContext.PlannedTrips
                        .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == user.Id);
            
                    if (plan == null)
                    {
                        return new JsonResult(new { success = false, message = "Piano non trovato" });
                    }
            
                    // Aggiorna i campi
                    if (!string.IsNullOrEmpty(request.StartDate) && DateTime.TryParse(request.StartDate, out DateTime startDate))
                    {
                        plan.StartDate = startDate;
                    }
            
                    if (!string.IsNullOrEmpty(request.EndDate) && DateTime.TryParse(request.EndDate, out DateTime endDate))
                    {
                        plan.EndDate = endDate;
                    }
            
                    plan.Notes = request.Notes ?? plan.Notes;
            
                    // Se hai una tabella specifica per la checklist
                    if (request.Checklist != null)
                    {
                        // Aggiorna la percentuale di completamento
                        plan.CompletionPercentage = request.CompletionPercentage ?? plan.CompletionPercentage;
                        
                        // In una implementazione completa, qui dovresti aggiornare anche gli elementi della checklist
                        // ad esempio:
                        // await UpdateChecklistItems(planId, request.Checklist);
                    }
            
                    // Salva le modifiche
                    await _dbContext.SaveChangesAsync();
            
                    return new JsonResult(new { 
                        success = true,
                        updatedPlan = new {
                            id = plan.Id,
                            startDate = plan.StartDate,
                            endDate = plan.EndDate,
                            notes = plan.Notes,
                            completionPercentage = plan.CompletionPercentage
                            // Includi altri campi se necessario
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Eccezione in UpdatePlanDetailsAsync: {ex.Message}");
                    return new JsonResult(new { success = false, message = ex.Message });
                }
            }
        
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult OnPostRemovePlanAsync([FromBody] RemovePlan request)
        {
            try
            {
                // In un'implementazione reale, qui rimuoveresti l'elemento dal database
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult OnPostRemoveDreamAsync([FromBody] RemovePlan request)
        {
            try
            {
                // In un'implementazione reale, qui rimuoveresti l'elemento dal database
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostMarkAsVisitedAsync([FromBody] MarkAsVisited request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"MarkAsVisitedAsync chiamato con planId: {request?.PlanId}");

                if (string.IsNullOrEmpty(request?.PlanId))
                {
                    return new JsonResult(new { success = false, message = "ID piano non valido" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "Utente non autenticato" });
                }

                // Simulazione: indicare che l'operazione è stata completata con successo
                // In un'implementazione reale, qui chiameresti i metodi appropriati di _dreamService e _countryService
                System.Diagnostics.Debug.WriteLine($"Simulazione: Viaggio {request.PlanId} contrassegnato come visitato");

                // Ritorna una risposta di successo simulata
                return new JsonResult(new
                {
                    success = true,
                    message = "La funzione è in fase di implementazione. Le modifiche saranno visibili al prossimo aggiornamento."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eccezione in MarkAsVisitedAsync: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }        // Classi per i modelli di richieste e form
        public class MoveToPlanning
        {
            public string? DreamId { get; set; }
        }

        public class MarkAsVisited
        {
            public string? PlanId { get; set; }
        }

        public class RemovePlan
        {
            public string? PlanId { get; set; }
        }

                         public class UpdatePlan
            {
                public string? PlanId { get; set; }
                public string? Notes { get; set; }
                public string? StartDate { get; set; }
                public string? EndDate { get; set; }
                public int? CompletionPercentage { get; set; }
                public List<ChecklistItem>? Checklist { get; set; }
            }

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

        public class CityInfo
        {
            public string? Name { get; set; }
            public string? Country { get; set; }
            public string? CountryCode { get; set; }
        }

    public async Task<IActionResult> OnPostMarkAsVisitedAsync(int destinationId)
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

            var userId = _userManager.GetUserId(User);
            
            try
            {
                // Prima otteniamo i dettagli della destinazione
                var userWishlist = await _dreamService.GetUserWishlistAsync(userId);
                var wishlistItem = userWishlist.FirstOrDefault(d => d.Id == destinationId);
                
                if (wishlistItem == null)
                {
                    TempData["ErrorMessage"] = "Destinazione non trovata nella wishlist.";
                    return RedirectToPage();
                }
                
                // Cerchiamo la città nel database
                var cities = await _cityService.GetAllCitiesAsync();
                var city = cities.FirstOrDefault(c => 
                    c.Name.Equals(wishlistItem.CityName, StringComparison.OrdinalIgnoreCase) &&
                    c.Country.Name.Equals(wishlistItem.CountryName, StringComparison.OrdinalIgnoreCase));
                
                if (city == null)
                {
                    TempData["ErrorMessage"] = $"Non è stato possibile trovare la città {wishlistItem.CityName} nel database.";
                    return RedirectToPage();
                }
                
                // Segna la città come visitata e rimuovila dalla wishlist
                var result = await _cityService.MarkCityAsVisitedAsync(city.Id, userId, DateTime.Now);
                
                if (result)
                {
                    TempData["SuccessMessage"] = $"La città {wishlistItem.CityName} è stata segnata come visitata e rimossa dalla wishlist.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Si è verificato un errore durante il salvataggio.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Si è verificato un errore: {ex.Message}";
            }
            
            return RedirectToPage();
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnGetDiagnosticAsync()
        {
            try
            {
                // Recupera tutte le città e i paesi
                var userId = _userManager.GetUserId(User);
                
                // Otteni tutti i paesi
                var allCountries = await _countryService.GetAllCountriesAsync();
                
                // Ottieni tutte le città
                var allCities = await _dbContext.Cities
                    .Include(c => c.Country)
                    .ToListAsync();

                // Verifica l'integrità della relazione
                var citiesWithoutCountry = allCities.Count(c => c.Country == null);
                
                // Ottiene il numero di città per paese
                var citiesPerCountry = allCities
                    .Where(c => c.Country != null)
                    .GroupBy(c => c.Country.Name)
                    .Select(g => new { Country = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

                // Prova ad usare il metodo GetAllCitiesWithCountryAsync
                List<City> citiesWithCountry = new List<City>();
                try
                {
                    citiesWithCountry = await _cityService.GetAllCitiesWithCountryAsync();
                }
                catch (Exception ex)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        error = $"Errore in GetAllCitiesWithCountryAsync: {ex.Message}",
                        stack = ex.StackTrace
                    });
                }

                return new JsonResult(new
                {
                    success = true,
                    countriesCount = allCountries.Count,
                    citiesCount = allCities.Count,
                    citiesWithoutCountry = citiesWithoutCountry,
                    citiesWithCountryCount = citiesWithCountry.Count,
                    citiesPerCountry = citiesPerCountry,
                    citiesWithoutCountryIds = allCities.Where(c => c.Country == null).Select(c => c.Id).ToList()
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    error = ex.Message,
                    stack = ex.StackTrace
                });
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
                    Path.Combine(_webHostEnvironment.WebRootPath, "images", "placeholder-destination.jpg")
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
                            // Crea un semplice SVG
                            string svgContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""200px"" height=""200px"" viewBox=""0 0 200 200"" xmlns=""http://www.w3.org/2000/svg"">
  <rect width=""200"" height=""200"" fill=""#f8f9fa"" />
  <text x=""50"" y=""100"" font-family=""Arial"" font-size=""14"" fill=""#6c757d"">
    " + filename + @"
  </text>
</svg>";
                            System.IO.File.WriteAllText(path, svgContent);
                        }
                        else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            // Trova un'immagine predefinita da copiare
                            string defaultImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "default-city.jpg");
                            if (System.IO.File.Exists(defaultImagePath))
                            {
                                System.IO.File.Copy(defaultImagePath, path);
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
    }
}