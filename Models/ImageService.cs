using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json;

namespace WanderGlobe.Services
{
    public class ImageService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _unsplashAccessKey;

        public ImageService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _unsplashAccessKey = configuration["UnsplashApi:AccessKey"];
            
            // Configure HttpClient for reuse
            _httpClient.DefaultRequestHeaders.Add("Accept-Version", "v1");
        }

        public async Task<string> GetCityImageUrlAsync(string cityName, string countryName)
        {
            // Create unique cache key for this city
            string cacheKey = $"CityImage_{cityName}_{countryName}";

            // Check if image URL is already in cache
            if (_cache.TryGetValue(cacheKey, out string cachedImageUrl))
            {
                return cachedImageUrl;
            }

            try
            {
                // Create search query (city + country for more accurate results)
                string query = $"{cityName} {countryName} city skyline";

                // Call Unsplash API with appropriate parameters
                string apiUrl = $"https://api.unsplash.com/search/photos?query={Uri.EscapeDataString(query)}&per_page=1&orientation=landscape&content_filter=high";
                
                // Set authorization header for each request to avoid duplicates
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_unsplashAccessKey}");
                
                var response = await _httpClient.GetAsync(apiUrl);
                
                // Handle rate limits and other errors
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error from Unsplash API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return GetDefaultImageUrl(cityName, countryName);
                }
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                var searchResponse = JsonSerializer.Deserialize<UnsplashSearchResponse>(jsonContent);

                if (searchResponse?.Results?.Count > 0)
                {
                    // Get image URL (regular size is a good balance between quality and size)
                    string imageUrl = searchResponse.Results[0].Urls.Regular;
                    
                    // Add attribution query param as required by Unsplash API terms
                    imageUrl += $"&utm_source=WanderGlobe&utm_medium=referral";

                    // Cache for 24 hours
                    _cache.Set(cacheKey, imageUrl, TimeSpan.FromHours(24));

                    return imageUrl;
                }

                return GetDefaultImageUrl(cityName, countryName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving image for {cityName}: {ex.Message}");
                return GetDefaultImageUrl(cityName, countryName);
            }
        }

        public async Task<string> GetCountryFlagUrlAsync(string countryCode)
        {
            // You may want to implement this method to get flag images
            // For now, use local flags
            return $"/images/flags/{countryCode.ToLower()}.png";
        }

        private string GetDefaultImageUrl(string cityName, string countryName)
        {
            // Map to default images by continent or return generic placeholder
            var continentMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Europa", "/images/default/europe.jpg"},
                {"Asia", "/images/default/asia.jpg"},
                {"Africa", "/images/default/africa.jpg"},
                {"Nord America", "/images/default/north-america.jpg"},
                {"Sud America", "/images/default/south-america.jpg"},
                {"Oceania", "/images/default/oceania.jpg"},
                {"Antartide", "/images/default/antarctica.jpg"}
            };

            // Logic to determine continent from country name could be added here
            // For now, return a generic placeholder
            return "/images/placeholder-city.jpg";
        }
    }

    public class UnsplashSearchResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("results")]
        public List<UnsplashPhoto> Results { get; set; }
    }

    public class UnsplashPhoto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("urls")]
        public UnsplashUrls Urls { get; set; }

        [JsonPropertyName("user")]
        public UnsplashUser User { get; set; }
    }

    public class UnsplashUrls
    {
        [JsonPropertyName("raw")]
        public string Raw { get; set; }

        [JsonPropertyName("full")]
        public string Full { get; set; }

        [JsonPropertyName("regular")]
        public string Regular { get; set; }

        [JsonPropertyName("small")]
        public string Small { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }
    }

    public class UnsplashUser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}