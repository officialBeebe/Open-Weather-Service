using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenWeatherService.Interfaces;
using OpenWeatherService.Models;

namespace OpenWeatherService.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly string _openWeatherApiKey;
        private readonly string _iPInfoApiKey;
        private readonly HttpClient _httpClient;

        public WeatherService(string openWeatherApiKey, string iPInfoApiKey, HttpClient httpClient)
        {
            _openWeatherApiKey = openWeatherApiKey;
            _iPInfoApiKey = iPInfoApiKey;
            _httpClient = httpClient;
        }

        public async Task<WeatherInfo> FetchWeatherData(string latitude, string longitude)
        {
            if (string.IsNullOrEmpty(_openWeatherApiKey))
            {
                Console.WriteLine("Invalid OpenWeather API key.");
                return null;
            }

            string requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_openWeatherApiKey}&units=metric";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic weatherData = JsonConvert.DeserializeObject(responseBody);

                string locationName = weatherData.name ?? "Unknown location";
                string weatherDescription = weatherData.weather[0].description;

                WeatherInfo weatherInfo = new WeatherInfo
                {
                    LocationName = locationName,
                    WeatherDescription = weatherDescription,
                    Celsius = (float)weatherData.main.temp,
                    Fahrenheit = (float)Math.Round((float)(weatherData.main.temp * 9 / 5) + 32),
                    SunriseMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunrise).ToLocalTime().DateTime,
                    SunsetMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunset).ToLocalTime().DateTime,
                    Latitude = Convert.ToDouble(latitude),
                    Longitude = Convert.ToDouble(longitude)
                };

                return weatherInfo;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error fetching weather data: {e.Message}");
                return null;
            }
        }
        

        public async Task<(double latitude, double longitude)?> GetLocationFromIP()
        {
            if (string.IsNullOrEmpty(_iPInfoApiKey))
            {
                Console.WriteLine("Invalid IPInfo API key.");
                return null;
            }

            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"https://ipinfo.io/json?token={_iPInfoApiKey}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic locationData = JsonConvert.DeserializeObject(responseBody);
                    string[] coords = locationData.loc.ToString().Split(',');

                    return (Convert.ToDouble(coords[0]), Convert.ToDouble(coords[1]));
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Error fetching location from IP: {e.Message}");
                    return null;
                }
            }
        }

        public (double latitude, double longitude) GetRandomLocation()
        {
            Random random = new Random();
            return (random.NextDouble() * 180 - 90, random.NextDouble() * 360 - 180);
        }

        public void PrintWeatherData(WeatherInfo info)
        {
            if (info == null)
            {
                Console.WriteLine("No weather data available.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Weather for {info.LocationName}");
            Console.WriteLine();
            Console.WriteLine($"\tWeather: {info.WeatherDescription}");
            Console.WriteLine($"\tTemperature: {info.Fahrenheit}°F");
            Console.WriteLine();
            Console.WriteLine($"\tSunrise: {info.SunriseMoment.ToShortTimeString()}");
            Console.WriteLine($"\tSunset: {info.SunsetMoment.ToShortTimeString()}");
            Console.WriteLine();
            //Console.WriteLine($"Coords: {Math.Abs(info.Latitude):F4}{(info.Latitude >= 0 ? " N" : " S")}, {Math.Abs(info.Longitude):F4}{(info.Longitude >= 0 ? " E" : " W")}");
        }
    }
}
