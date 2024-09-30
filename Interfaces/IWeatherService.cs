using OpenWeatherService.Models;

namespace OpenWeatherService.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherInfo> FetchWeatherData(string latitude, string longitude);
        Task<(double latitude, double longitude)?> GetLocationFromIP();
        (double latitude, double longitude) GetRandomLocation();
        void PrintWeatherData(WeatherInfo info);
    }
}
