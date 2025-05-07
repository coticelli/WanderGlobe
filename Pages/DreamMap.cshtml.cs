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

namespace WanderGlobe.Pages
{
    [Authorize]
    public class DreamMapModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICountryService _countryService;
        private readonly IDreamService _dreamService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _geminiApiKey = "AIzaSyDdEmi-z2ltv89z3WMA5t5HEkVQ5N5NP58";
        private readonly IWebHostEnvironment _webHostEnvironment;

        public List<DreamDestination> Wishlist { get; set; } = new List<DreamDestination>();
        public List<PlannedTrip> PlannedTrips { get; set; } = new List<PlannedTrip>();
        public List<RecommendedDestination> Recommendations { get; set; } = new List<RecommendedDestination>();
        public List<Country> Countries { get; set; } = new List<Country>();
        public MapDestinationsViewModel AllDestinations { get; set; } = new MapDestinationsViewModel();
        public WishlistItemViewModel WishlistForm { get; set; } = new WishlistItemViewModel();

        // Costruttore unificato che combina tutte le dipendenze
        public DreamMapModel(
            UserManager<ApplicationUser> userManager,
            ICountryService countryService,
            IDreamService dreamService,
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _countryService = countryService;
            _dreamService = dreamService;
            _clientFactory = clientFactory;
            _geminiApiKey = configuration["GeminiApiKey"];
            _webHostEnvironment = webHostEnvironment;

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
        }

        // Caricamento iniziale della pagina
        public async Task OnGetAsync()
        {
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

                        VisitedCities = visitedCountries.Select(v => new MapDestinationItem
                        {
                            Id = NormalizeNameToId(GetCapital(v.Country.Code), v.Country.Code),
                            CityName = GetCapital(v.Country.Code),
                            CountryName = v.Country.Name,
                            CountryCode = v.Country.Code,
                            Latitude = v.Country.Latitude,
                            Longitude = v.Country.Longitude
                        }).ToList()
                    };

                    // Inizializza il form per l'aggiunta alla wishlist con le città disponibili
                    WishlistForm = new WishlistItemViewModel
                    {
                        AvailableCities = GetAvailableCapitals()
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

        // Nuovo metodo per gestire l'aggiunta alla wishlist
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostSaveToWishlistAsync(WishlistItemViewModel model)
        {
            try
            {
                // Log per debug
                System.Diagnostics.Debug.WriteLine($"OnPostSaveToWishlistAsync chiamato - Città: {model?.City}, Paese: {model?.Country}");

                if (model == null)
                {
                    ModelState.AddModelError("", "I dati del modulo non sono validi.");
                    return Page();
                }

                if (!ModelState.IsValid)
                {
                    // Log degli errori di validazione
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Errore validazione: {state.Key} - {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }

                    // Ricarica le città disponibili e ritorna la vista con gli errori
                    model.AvailableCities = GetAvailableCapitals();
                    return Page();
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Trova le coordinate della città selezionata
                var cityInfo = GetAvailableCapitals().FirstOrDefault(c => c.Name == model.City);
                double latitude = 0, longitude = 0;

                // Se troviamo una corrispondenza con la città, usiamo le coordinate dal database dei paesi
                if (cityInfo != null)
                {
                    var country = Countries.FirstOrDefault(c => c.Code == cityInfo.CountryCode);
                    if (country != null)
                    {
                        latitude = country.Latitude;
                        longitude = country.Longitude;
                    }
                }

                // Converti la priorità da stringa a enum
                DreamPriority priority;
                Enum.TryParse(model.Priority, out priority);
                if (priority == 0) priority = DreamPriority.Medium; // Default se la conversione fallisce

                // Salva il file immagine se presente
                string imageUrl = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageUrl = await SaveWishlistImageAsync(model.ImageFile);
                }

                // Crea una lista di tag dalla stringa di input
                List<string> tagList = new List<string>();
                if (!string.IsNullOrEmpty(model.Tags))
                {
                    tagList = model.Tags.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }

                // Crea un nuovo oggetto DreamDestination
                var newDream = new DreamDestination
                {
                    UserId = user.Id,
                    CityName = model.City,
                    CountryName = model.Country,
                    CountryCode = cityInfo?.CountryCode,
                    Note = model.Notes,
                    Tags = tagList,
                    Priority = priority,
                    Latitude = latitude,
                    Longitude = longitude,
                    ImageUrl = imageUrl ?? $"/images/cities/{(cityInfo?.CountryCode ?? "default").ToLower()}-city.jpg",
                    CreatedAt = DateTime.UtcNow
                };

                // Salva nel database tramite il servizio
                var savedDream = await _dreamService.AddToWishlistAsync(newDream);

                if (savedDream != null)
                {
                    // Redirect con messaggio di successo
                    TempData["SuccessMessage"] = $"{model.City} aggiunta alla tua wishlist!";
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio. Riprova.");
                    model.AvailableCities = GetAvailableCapitals();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Log dell'errore
                System.Diagnostics.Debug.WriteLine($"Eccezione in OnPostSaveToWishlistAsync: {ex.Message}");
                ModelState.AddModelError("", $"Errore: {ex.Message}");
                model.AvailableCities = GetAvailableCapitals();
                return Page();
            }
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
                    System.Diagnostics.Debug.WriteLine($"Errore API: {response.StatusCode} - {responseContent}");

                    // Se il modello non è disponibile, prova con modelli alternativi
                    if (apiUrl.Contains("gemini-2.0-flash") && (response.StatusCode == System.Net.HttpStatusCode.NotFound || responseContent.Contains("not found")))
                    {
                        // Prova con gemini-1.5-flash
                        System.Diagnostics.Debug.WriteLine("Modello non trovato, tentativo con modello alternativo: gemini-1.5-flash");
                        string alternativeUrl = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";

                        var alternativeResponse = await client.PostAsync(alternativeUrl, content);
                        if (alternativeResponse.IsSuccessStatusCode)
                        {
                            string altContent = await alternativeResponse.Content.ReadAsStringAsync();
                            dynamic altParsed = JsonConvert.DeserializeObject(altContent);

                            if (altParsed?.candidates != null &&
                                altParsed.candidates.Count > 0 &&
                                altParsed.candidates[0].content != null &&
                                altParsed.candidates[0].content.parts != null &&
                                altParsed.candidates[0].content.parts.Count > 0)
                            {
                                string htmlContent = altParsed.candidates[0].content.parts[0].text;
                                htmlContent = CleanupMarkdownCodeDelimiters(htmlContent);

                                return new JsonResult(new
                                {
                                    success = true,
                                    html = htmlContent,
                                    note = "Utilizzato modello alternativo"
                                });
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
            );

            return content.Trim();
        }

        // Metodo per ottenere le capitali disponibili
        private List<CityInfo> GetAvailableCapitals()
        {
            return new List<CityInfo>
            {
                new CityInfo { Name = "Roma", Country = "Italia", CountryCode = "IT" },
                new CityInfo { Name = "Parigi", Country = "Francia", CountryCode = "FR" },
                new CityInfo { Name = "Londra", Country = "Regno Unito", CountryCode = "GB" },
                new CityInfo { Name = "Madrid", Country = "Spagna", CountryCode = "ES" },
                new CityInfo { Name = "Berlino", Country = "Germania", CountryCode = "DE" },
                new CityInfo { Name = "Vienna", Country = "Austria", CountryCode = "AT" },
                new CityInfo { Name = "Bruxelles", Country = "Belgio", CountryCode = "BE" },
                new CityInfo { Name = "Amsterdam", Country = "Paesi Bassi", CountryCode = "NL" },
                new CityInfo { Name = "Atene", Country = "Grecia", CountryCode = "GR" },
                new CityInfo { Name = "Lisbona", Country = "Portogallo", CountryCode = "PT" },
                new CityInfo { Name = "Budapest", Country = "Ungheria", CountryCode = "HU" },
                new CityInfo { Name = "Praga", Country = "Repubblica Ceca", CountryCode = "CZ" },
                new CityInfo { Name = "Varsavia", Country = "Polonia", CountryCode = "PL" },
                new CityInfo { Name = "Dublino", Country = "Irlanda", CountryCode = "IE" },
                new CityInfo { Name = "Copenhagen", Country = "Danimarca", CountryCode = "DK" },
                new CityInfo { Name = "Stoccolma", Country = "Svezia", CountryCode = "SE" },
                new CityInfo { Name = "Helsinki", Country = "Finlandia", CountryCode = "FI" },
                new CityInfo { Name = "Oslo", Country = "Norvegia", CountryCode = "NO" },
                new CityInfo { Name = "Washington D.C.", Country = "Stati Uniti", CountryCode = "US" },
                new CityInfo { Name = "Ottawa", Country = "Canada", CountryCode = "CA" },
                new CityInfo { Name = "Tokyo", Country = "Giappone", CountryCode = "JP" },
                new CityInfo { Name = "Pechino", Country = "Cina", CountryCode = "CN" },
                new CityInfo { Name = "Seoul", Country = "Corea del Sud", CountryCode = "KR" },
                new CityInfo { Name = "Mosca", Country = "Russia", CountryCode = "RU" },
                new CityInfo { Name = "Brasilia", Country = "Brasile", CountryCode = "BR" },
                new CityInfo { Name = "Buenos Aires", Country = "Argentina", CountryCode = "AR" },
                new CityInfo { Name = "Città del Messico", Country = "Messico", CountryCode = "MX" },
                new CityInfo { Name = "Il Cairo", Country = "Egitto", CountryCode = "EG" },
                new CityInfo { Name = "Bangkok", Country = "Thailandia", CountryCode = "TH" },
                new CityInfo { Name = "Nuova Delhi", Country = "India", CountryCode = "IN" },
                new CityInfo { Name = "Sydney", Country = "Australia", CountryCode = "AU" },
                new CityInfo { Name = "Wellington", Country = "Nuova Zelanda", CountryCode = "NZ" }
            };
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

        // Helper per ottenere il nome della capitale
        private string GetCapital(string countryCode)
        {
            var capitals = new Dictionary<string, string>
            {
                {"IT", "Roma"}, {"GB", "Londra"}, {"FR", "Parigi"}, {"DE", "Berlino"},
                {"ES", "Madrid"}, {"PT", "Lisbona"}, {"NL", "Amsterdam"}, {"BE", "Bruxelles"},
                {"GR", "Atene"}, {"US", "Washington D.C."}, {"JP", "Tokyo"}, {"CN", "Pechino"},
                {"AT", "Vienna"}, {"HU", "Budapest"}, {"CZ", "Praga"}, {"PL", "Varsavia"},
                {"IE", "Dublino"}, {"DK", "Copenhagen"}, {"SE", "Stoccolma"}, {"FI", "Helsinki"},
                {"NO", "Oslo"}, {"CA", "Ottawa"}, {"KR", "Seoul"}, {"RU", "Mosca"},
                {"BR", "Brasilia"}, {"AR", "Buenos Aires"}, {"MX", "Città del Messico"},
                {"EG", "Il Cairo"}, {"TH", "Bangkok"}, {"IN", "Nuova Delhi"},
                {"AU", "Sydney"}, {"NZ", "Wellington"}
            };

            return capitals.ContainsKey(countryCode) ? capitals[countryCode] : $"Capitale di {countryCode}";
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

                // In un'implementazione reale, qui chiameresti i metodi del servizio
                // per spostare l'elemento dalla wishlist alla pianificazione

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult OnPostUpdatePlanDetailsAsync([FromBody] UpdatePlan request)
        {
            try
            {
                var plan = PlannedTrips.FirstOrDefault(p => p.Id.ToString() == request.PlanId);
                if (plan != null)
                {
                    plan.Notes = request.Notes;
                    plan.StartDate = DateTime.Parse(request.StartDate);
                    plan.EndDate = DateTime.Parse(request.EndDate);

                    // In un'implementazione reale, qui salveresti le modifiche nel database
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
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
        }

        // Classi per i modelli di richieste e form
        public class MoveToPlanning
        {
            public string DreamId { get; set; }
        }

        public class MarkAsVisited
        {
            public string PlanId { get; set; }
        }

        public class RemovePlan
        {
            public string PlanId { get; set; }
        }

        public class UpdatePlan
        {
            public string PlanId { get; set; }
            public string Notes { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }

        public class WishlistItemViewModel
        {
            public string City { get; set; }
            public string Country { get; set; }
            public string CountryCode { get; set; }
            public string Notes { get; set; }
            public string Tags { get; set; }
            public string Priority { get; set; } = "Media";
            public IFormFile ImageFile { get; set; }
            public List<CityInfo> AvailableCities { get; set; } = new List<CityInfo>();
        }

        public class CityInfo
        {
            public string Name { get; set; }
            public string Country { get; set; }
            public string CountryCode { get; set; }
        }
    }
}