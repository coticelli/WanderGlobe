// File: Services/IWeatherService.cs
using WanderGlobe.Models.Custom; // Points to the standard TimelineWeather
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public interface IWeatherService
    {
        Task<TimelineWeather?> GetCurrentWeatherAsync(double latitude, double longitude);
    }
}