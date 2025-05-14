using WanderGlobe.Models.Custom;
using WanderGlobe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace WanderGlobe.Services
{
    public class DreamService : IDreamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DreamService(
            ApplicationDbContext context, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<DreamDestination>> GetUserWishlistAsync(string userId)
        {
            // Recupera la wishlist dal database
            return await _context.DreamDestinations
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<PlannedTrip>> GetUserPlannedTripsAsync(string userId)
        {
            // Recupera i viaggi pianificati dal database
            // *** MODIFICA QUI: Aggiungi Include per la Checklist ***
            System.Diagnostics.Debug.WriteLine($"[DreamService.GetUserPlannedTripsAsync] Recupero PlannedTrips per UserId: {userId} CON Inclusione Checklist.");
            var plannedTrips = await _context.PlannedTrips
                .Include(p => p.Checklist) // <= AGGIUNGI QUESTO!
                .Where(p => p.UserId == userId)
                .ToListAsync();
            System.Diagnostics.Debug.WriteLine($"[DreamService.GetUserPlannedTripsAsync] Trovati {plannedTrips.Count} piani. Il primo piano ha {plannedTrips.FirstOrDefault()?.Checklist?.Count ?? 0} item nella checklist.");
            return plannedTrips;
        }
        
        public Task<List<RecommendedDestination>> GetRecommendationsAsync(string userId)
        {
            // Per le raccomandazioni, potremmo implementare una logica più complessa in futuro
            // Per ora, restituisci una lista vuota per non mostrare località non richieste
            return Task.FromResult(new List<RecommendedDestination>());
        }

        // Implementazione di altri metodi
        public async Task<DreamDestination> AddToWishlistAsync(DreamDestination destination)
        {
            // Aggiunge la destinazione alla wishlist nel database
            _context.DreamDestinations.Add(destination);
            await _context.SaveChangesAsync();
            return destination;
        }

        public async Task<bool> RemoveFromWishlistAsync(int dreamId, string userId)
        {
            try
            {
                // Trova la destinazione nella wishlist dell'utente
                var destination = await _context.DreamDestinations
                    .FirstOrDefaultAsync(d => d.Id == dreamId && d.UserId == userId);

                if (destination != null)
                {
                    _context.DreamDestinations.Remove(destination);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in RemoveFromWishlistAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<PlannedTrip> CreatePlannedTripAsync(PlannedTrip trip)
        {
            // Aggiunge un nuovo viaggio pianificato al database
            _context.PlannedTrips.Add(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<bool> UpdatePlannedTripAsync(PlannedTrip trip)
        {
            try
            {
                _context.PlannedTrips.Update(trip);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in UpdatePlannedTripAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePlannedTripAsync(string tripId, string userId)
        {
            try
            {
                var trip = await _context.PlannedTrips
                    .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

                if (trip != null)
                {
                    _context.PlannedTrips.Remove(trip);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in DeletePlannedTripAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkTripAsVisitedAsync(string tripId, string userId)
        {
            try
            {
                return await DeletePlannedTripAsync(tripId, userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in MarkTripAsVisitedAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsCityInUserWishlistAsync(int cityId, string userId)
        {
            try
            {
                // Recupera la città dal database
                var city = await _context.Cities.FindAsync(cityId);
                if (city == null)
                    return false;

                // Controlla se la città è nella wishlist dell'utente
                return await _context.DreamDestinations
                    .AnyAsync(d => d.UserId == userId &&
                             d.CityName.Equals(city.Name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in IsCityInUserWishlistAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RecommendedDestination>> GetAIRecommendationsAsync(string userId, string recommendationType)
        {
            try
            {
                // Get visited places for this user
                var visitedCities = await _context.TravelJournals
                    .Where(v => v.UserId == userId)
                    .Include(v => v.Country) // Only include Country, not City
                    .Select(v => new {
                        // Use the Country name as location indicator
                        CityName = "Unknown", // Placeholder since we don't have city info
                        CountryName = v.Country.Name 
                    })
                    .ToListAsync();
                
                // If the user hasn't visited any places, return popular destinations
                if (!visitedCities.Any())
                {
                    return await GetPopularDestinationsAsync();
                }
                
                // Also get cities from the wishlist to exclude them (user already knows about these)
                var wishlistCities = await _context.DreamDestinations
                    .Where(d => d.UserId == userId)
                    .Select(d => new { d.CityName, d.CountryName })
                    .ToListAsync();
                    
                // Prepare the prompt for Gemini based on the recommendation type
                string prompt = BuildRecommendationPrompt(visitedCities, wishlistCities, recommendationType);
                
                // Get recommendations from Gemini API
                var recommendationsJson = await CallGeminiForRecommendationsAsync(prompt);
                
                // Enrich the data with information from the database
                return await EnrichRecommendationsWithDataAsync(recommendationsJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAIRecommendationsAsync: {ex.Message}");
                // Fallback: return a list of predefined popular destinations
                return await GetPopularDestinationsAsync();
            }
        }

        private string BuildRecommendationPrompt(IEnumerable<dynamic> visitedCities, IEnumerable<dynamic> wishlistCities, string recommendationType)
        {
            var visitedCitiesText = string.Join(", ", visitedCities.Select(c => $"{c.CityName}, {c.CountryName}"));
            var wishlistCitiesText = string.Join(", ", wishlistCities.Select(c => $"{c.CityName}, {c.CountryName}"));
            
            // Base prompt template
            string basePrompt = $"Sono un viaggiatore che ha già visitato queste città: {visitedCitiesText}. ";
            
            // Add wishlist cities if present
            if (!string.IsNullOrEmpty(wishlistCitiesText))
            {
                basePrompt += $"Ho già pianificato di visitare: {wishlistCitiesText}. ";
            }
            
            // Customize the prompt based on the recommendation type
            switch (recommendationType.ToLower())
            {
                case "simili":
                    return basePrompt + "Suggeriscimi 5 città simili a quelle che ho visitato, ma che non ho ancora visto. Per ogni città fornisci: nome, paese, una breve descrizione (max 100 caratteri), e un motivo per cui potrebbe piacermi basandoti sulle città che ho già visitato. Rispondi in formato JSON strutturato come un array di oggetti con campi: cityName, countryName, description, reason.";
                    
                case "popolari":
                    return basePrompt + "Suggeriscimi 5 città popolari e iconiche che non ho ancora visitato e che ogni viaggiatore dovrebbe vedere almeno una volta. Per ogni città fornisci: nome, paese, una breve descrizione (max 100 caratteri), e la sua attrazione principale. Rispondi in formato JSON strutturato come un array di oggetti con campi: cityName, countryName, description, mainAttraction.";
                    
                case "nuove":
                    return basePrompt + "Suggeriscimi 5 destinazioni emergenti o fuori dai percorsi turistici tradizionali che potrei apprezzare in base alle città che ho già visitato. Per ogni città fornisci: nome, paese, una breve descrizione (max 100 caratteri), e cosa la rende unica. Rispondi in formato JSON strutturato come un array di oggetti con campi: cityName, countryName, description, uniqueFeature.";
                    
                default: // "tutte" o qualsiasi altro valore
                    return basePrompt + "Suggeriscimi 5 città diverse che non ho ancora visitato e che potrebbero piacermi, con una buona varietà di esperienze. Per ogni città fornisci: nome, paese, una breve descrizione (max 100 caratteri), e perché dovrei visitarla. Rispondi in formato JSON strutturato come un array di oggetti con campi: cityName, countryName, description, whyVisit.";
            }
        }

        private async Task<List<JsonElement>> CallGeminiForRecommendationsAsync(string prompt)
        {
            // Use IHttpClientFactory to get a configured HttpClient
            using var httpClient = _httpClientFactory.CreateClient();
            
            // Gemini API address (use configuration variable)
            string apiKey = _configuration["GeminiApiKey"];
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";
            
            // Prepare the request
            var requestData = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };
            
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestData),
                System.Text.Encoding.UTF8,
                "application/json");
            
            // Send request
            var response = await httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    // Extract text from response
                    var responseDoc = JsonDocument.Parse(responseContent);
                    var responseRoot = responseDoc.RootElement;
                    string responseText = "";
                    
                    if (responseRoot.TryGetProperty("candidates", out var candidates) && 
                        candidates.GetArrayLength() > 0 &&
                        candidates[0].TryGetProperty("content", out var content1) &&
                        content1.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0 &&
                        parts[0].TryGetProperty("text", out var textElement))
                    {
                        responseText = textElement.GetString() ?? "[]";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid Gemini API response format");
                        return new List<JsonElement>();
                    }
                    
                    // Parse the JSON text to get recommendations
                    var recommendationsDoc = JsonDocument.Parse(responseText);
                    var recommendations = new List<JsonElement>();
                    
                    foreach (var item in recommendationsDoc.RootElement.EnumerateArray())
                    {
                        recommendations.Add(item.Clone());
                    }
                    
                    return recommendations;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing Gemini response: {ex.Message}");
                    return new List<JsonElement>();
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"Gemini API Error: {response.StatusCode}");
            return new List<JsonElement>();
        }

        private async Task<List<RecommendedDestination>> EnrichRecommendationsWithDataAsync(List<JsonElement> recommendations)
        {
            var result = new List<RecommendedDestination>();
            
            foreach (var rec in recommendations)
            {
                try
                {
                    if (!rec.TryGetProperty("cityName", out var cityNameProp) ||
                        !rec.TryGetProperty("countryName", out var countryNameProp))
                    {
                        continue;
                    }
                    
                    string cityName = cityNameProp.GetString() ?? "";
                    string countryName = countryNameProp.GetString() ?? "";
                    
                    // Look for matches in the database
                    var cityInfo = await _context.Cities
                        .Include(c => c.Country)
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower() || 
                                                c.Country.Name.ToLower() == countryName.ToLower());
                    
                    // Create RecommendedDestination object
                    var destination = new RecommendedDestination
                    {
                        Id = Guid.NewGuid().ToString(),
                        CityName = cityName,
                        CountryName = countryName,
                        // Use lat/lng from DB if available, otherwise default values
                        Latitude = cityInfo?.Latitude ?? 0,
                        Longitude = cityInfo?.Longitude ?? 0,
                        Description = rec.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "",
                        ReasonToVisit = GetReasonToVisit(rec),
                        ImageUrl = await GetCityImageUrlAsync(cityName, countryName)
                    };
                    
                    result.Add(destination);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing recommendation: {ex.Message}");
                    // Continue with the next recommendation
                    continue;
                }
            }
            
            return result;
        }

        private string GetReasonToVisit(JsonElement recommendation)
        {
            // Extract the reason from the Gemini response (can have different field names)
            if (recommendation.TryGetProperty("reason", out var reason)) return reason.GetString() ?? "";
            if (recommendation.TryGetProperty("whyVisit", out var whyVisit)) return whyVisit.GetString() ?? "";
            if (recommendation.TryGetProperty("mainAttraction", out var mainAttraction)) return mainAttraction.GetString() ?? "";
            if (recommendation.TryGetProperty("uniqueFeature", out var uniqueFeature)) return uniqueFeature.GetString() ?? "";
            
            return "Destinazione consigliata in base alle tue preferenze";
        }

        private async Task<string> GetCityImageUrlAsync(string cityName, string countryName)
        {
            try
            {
                // First look for an image already in the database
                var city = await _context.Cities
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());
                
                if (city != null && !string.IsNullOrEmpty(city.ImageUrl))
                {
                    return city.ImageUrl;
                }
                
                // If not found, use a fallback image based on the first letter of the country
                string countryFirstLetter = countryName.Substring(0, 1).ToLower();
                return $"/images/destinations/{countryFirstLetter}.jpg";
            }
            catch
            {
                // Default fallback
                return "/images/destinations/default.jpg";
            }
        }

        private async Task<List<RecommendedDestination>> GetPopularDestinationsAsync()
        {
            // Predefined popular destinations as fallback
            return new List<RecommendedDestination>
            {
                new RecommendedDestination { 
                    Id = Guid.NewGuid().ToString(), 
                    CityName = "Parigi", 
                    CountryName = "Francia", 
                    Description = "La città dell'amore con la sua iconica Torre Eiffel",
                    ReasonToVisit = "Arte, cultura e cucina di fama mondiale",
                    ImageUrl = "/images/destinations/f.jpg",
                    Latitude = 48.8566,
                    Longitude = 2.3522
                },
                // More popular destinations...
                new RecommendedDestination { 
                    Id = Guid.NewGuid().ToString(), 
                    CityName = "New York", 
                    CountryName = "Stati Uniti", 
                    Description = "La Grande Mela, la città che non dorme mai",
                    ReasonToVisit = "Skyline iconico e vita culturale vibrante",
                    ImageUrl = "/images/destinations/s.jpg",
                    Latitude = 40.7128,
                    Longitude = -74.0060
                },
                new RecommendedDestination { 
                    Id = Guid.NewGuid().ToString(), 
                    CityName = "Tokyo", 
                    CountryName = "Giappone", 
                    Description = "Metropoli ultramoderna con un mix di tradizione e futuro",
                    ReasonToVisit = "Tecnologia all'avanguardia e cultura tradizionale",
                    ImageUrl = "/images/destinations/j.jpg",
                    Latitude = 35.6762,
                    Longitude = 139.6503
                },
                new RecommendedDestination { 
                    Id = Guid.NewGuid().ToString(), 
                    CityName = "Roma", 
                    CountryName = "Italia", 
                    Description = "La Città Eterna con migliaia di anni di storia",
                    ReasonToVisit = "Arte, architettura e cucina italiana autentica",
                    ImageUrl = "/images/destinations/i.jpg",
                    Latitude = 41.9028,
                    Longitude = 12.4964
                },
                new RecommendedDestination { 
                    Id = Guid.NewGuid().ToString(), 
                    CityName = "Sydney", 
                    CountryName = "Australia", 
                    Description = "Splendida città costiera con la sua Opera House",
                    ReasonToVisit = "Spiagge bellissime e stile di vita rilassato",
                    ImageUrl = "/images/destinations/a.jpg",
                    Latitude = -33.8688,
                    Longitude = 151.2093
                }
            };
        }
    }
}