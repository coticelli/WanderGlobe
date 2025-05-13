using WanderGlobe.Models.Custom;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public interface IWeatherService
    {
        Task<TimelineWeather> GetCurrentWeatherAsync(double latitude, double longitude);
    }
}