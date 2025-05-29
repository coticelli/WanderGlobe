// File: Services/WeatherService.cs
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WanderGlobe.Models.Custom; // Uses the standard TimelineWeather
using Microsoft.Extensions.Logging;

namespace WanderGlobe.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WeatherApi:ApiKey"] ?? "YOUR_SECURE_API_KEY";
            _logger = logger;
        }

        public async Task<TimelineWeather?> GetCurrentWeatherAsync(double latitude, double longitude)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={_apiKey}&lang=it";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(jsonResponse);

                    if (weatherData != null && weatherData.Weather != null && weatherData.Weather.Length > 0)
                    {
                        return new TimelineWeather // Returns Models.Custom.TimelineWeather
                        {
                            Month = DateTime.Now.Month,
                            Temperature = weatherData.Main.Temp, // Temp is double, matches TimelineWeather.Temperature
                            Condition = MapWeatherCondition(weatherData.Weather[0].Main, weatherData.Weather[0].Description),
                            IconUrl = $"https://openweathermap.org/img/wn/{weatherData.Weather[0].Icon}@2x.png"
                        };
                    }
                    _logger.LogWarning("Weather API response OK but data parsing failed for lat:{lat},lon:{lon}. Response: {json}", latitude, longitude, jsonResponse);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error from Weather API: {StatusCode} - {ErrorContent} for lat:{lat},lon:{lon}", response.StatusCode, errorContent, latitude, longitude);
                }
                return null; // Indicate failure to get weather
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching weather data for lat:{lat},lon:{lon}", latitude, longitude);
                return null; // Indicate failure
            }
        }

        private string MapWeatherCondition(string apiMainCondition, string apiDescription)
        {
            // ... (your existing MapWeatherCondition logic - ensure it's robust) ...
            string mainLower = apiMainCondition.ToLowerInvariant();
            string descLower = apiDescription.ToLowerInvariant();

            switch (mainLower)
            {
                case "clear": return "Sereno";
                case "clouds":
                    if (descLower.Contains("few clouds")) return "Poco nuvoloso";
                    if (descLower.Contains("scattered clouds")) return "Parzialmente nuvoloso";
                    if (descLower.Contains("broken clouds")) return "Nuvoloso";
                    if (descLower.Contains("overcast clouds")) return "Coperto";
                    return "Nuvoloso";
                case "rain":
                    if (descLower.Contains("light rain")) return "Pioggia debole";
                    if (descLower.Contains("moderate rain")) return "Pioggia moderata";
                    if (descLower.Contains("heavy intensity rain")) return "Pioggia forte";
                    if (descLower.Contains("freezing rain")) return "Pioggia gelata";
                    return "Piovoso";
                case "drizzle": return "Pioviggine";
                case "thunderstorm": return "Temporale";
                case "snow": return "Nevoso";
                case "mist": return "Foschia";
                case "fog": return "Nebbioso";
                case "smoke": return "Fumoso";
                case "haze": return "Foschia";
                case "dust": return "Polveroso";
                case "sand": return "Sabbioso";
                case "ash": return "Cenere vulcanica";
                case "squall": return "Raffica"; // Corrected from Raffica di vento
                case "tornado": return "Tornado";
                default:
                    _logger.LogWarning("Unmapped weather condition from API: Main='{Main}', Description='{Description}'", apiMainCondition, apiDescription);
                    return apiMainCondition;
            }
        }

        // Default weather if API call fails or data is incomplete
        private TimelineWeather GetDefaultWeather() // This method is no longer used if GetCurrentWeatherAsync returns null on failure
        {
            return new TimelineWeather
            {
                Month = DateTime.Now.Month,
                Temperature = 20,
                Condition = "N/D",
                IconUrl = null
            };
        }

        private class WeatherResponse { public WeatherMain Main { get; set; } = new(); public WeatherInfo[] Weather { get; set; } = Array.Empty<WeatherInfo>(); }
        private class WeatherMain { public double Temp { get; set; } } // API returns temp as double
        private class WeatherInfo { public string Main { get; set; } = ""; public string Description { get; set; } = ""; public string Icon { get; set; } = ""; }
    }
}