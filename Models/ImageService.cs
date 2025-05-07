using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

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
        }

        public async Task<string> GetCityImageUrlAsync(string cityName, string countryName)
        {
            // Crea una chiave di cache unica per questa città
            string cacheKey = $"CityImage_{cityName}_{countryName}";

            // Controlla se l'URL dell'immagine è già in cache
            if (_cache.TryGetValue(cacheKey, out string cachedImageUrl))
            {
                return cachedImageUrl;
            }

            try
            {
                // Crea la query di ricerca (città + paese per risultati più accurati)
                string query = $"{cityName} {countryName} city skyline";

                // Chiamata all'API di Unsplash
                string apiUrl = $"https://api.unsplash.com/search/photos?query={Uri.EscapeDataString(query)}&per_page=1&orientation=landscape";
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_unsplashAccessKey}");

                var response = await _httpClient.GetFromJsonAsync<UnsplashSearchResponse>(apiUrl);

                if (response?.Results?.Count > 0)
                {
                    // Ottieni l'URL dell'immagine (versione regular è un buon compromesso tra qualità e dimensione)
                    string imageUrl = response.Results[0].Urls.Regular;

                    // Salva in cache per 24 ore
                    _cache.Set(cacheKey, imageUrl, TimeSpan.FromHours(24));

                    return imageUrl;
                }

                return GetDefaultImageUrl(cityName, countryName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel recupero dell'immagine per {cityName}: {ex.Message}");
                return GetDefaultImageUrl(cityName, countryName);
            }
        }

        public async Task<string> GetCountryFlagUrlAsync(string countryCode)
        {
            // Le bandiere possono essere recuperate da servizi come Flagpedia
            return $"https://flagcdn.com/w160/{countryCode.ToLower()}.png";
        }

        private string GetDefaultImageUrl(string cityName, string countryName)
        {
            // Controlla prima se esiste un'immagine locale
            string localImagePath = $"/images/cities/{countryName.ToLower()}-{cityName.ToLower()}.jpg";

            // Se non esiste, usa un'immagine generica basata sulla prima lettera della città
            string fallbackPath = $"/images/cities/default-{cityName.Substring(0, 1).ToLower()}.jpg";

            return fallbackPath;
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