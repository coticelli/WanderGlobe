using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WanderGlobe.Models.Custom;

namespace WanderGlobe.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IConfiguration _configuration;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = configuration["WeatherApi:ApiKey"] ?? "059cc76dfe7848562d1c1f7c43879e78"; // Fallback to provided key
        }

        public async Task<TimelineWeather> GetCurrentWeatherAsync(double latitude, double longitude)
        {
            try
            {
                // Use API version 2.5 which doesn't require subscription
                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={_apiKey}";
                
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(jsonResponse);
                    
                    if (weatherData != null)
                    {
                        return new TimelineWeather
                        {
                            Month = DateTime.Now.Month,
                            Temperature = (int)Math.Round(weatherData.Main.Temp), // Already in Celsius from API
                            Condition = MapWeatherCondition(weatherData.Weather[0].Main)
                        };
                    }
                }
                
                // Return default data if API call fails
                return GetDefaultWeather();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather data: {ex.Message}");
                return GetDefaultWeather();
            }
        }

        private string MapWeatherCondition(string apiCondition)
        {
            // Map OpenWeatherMap conditions to your application's conditions
            return apiCondition switch
            {
                "Clear" => "Soleggiato",
                "Clouds" => "Nuvoloso",
                "Rain" => "Piovoso",
                "Drizzle" => "Pioviggine",
                "Thunderstorm" => "Temporale",
                "Snow" => "Nevoso",
                "Mist" => "Nebbioso",
                "Fog" => "Nebbioso",
                "Haze" => "Foschia",
                "Dust" => "Polveroso",
                "Smoke" => "Fumoso",
                "Tornado" => "Tornado",
                _ => "Vario"
            };
        }

        private TimelineWeather GetDefaultWeather()
        {
            // Fallback weather data
            return new TimelineWeather
            {
                Month = DateTime.Now.Month,
                Temperature = 22, // Default temperature
                Condition = "Soleggiato" // Default condition
            };
        }

        // Classes to deserialize OpenWeatherMap API response
        private class WeatherResponse
        {
            public WeatherMain Main { get; set; } = new WeatherMain();
            public WeatherInfo[] Weather { get; set; } = Array.Empty<WeatherInfo>();
        }

        private class WeatherMain
        {
            public float Temp { get; set; }
        }

        private class WeatherInfo
        {
            public string Main { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
        }
    }
}