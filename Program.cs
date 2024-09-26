using Newtonsoft.Json;
using System.Globalization;
using OpenWeatherService.Models;
using Microsoft.Extensions.Configuration;

namespace OpenWeatherService
{
    class Program
    {
        private static IConfiguration config;

        static async Task Main(string[] args)
        {
            // Load configuration
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Bind config to OpenWeatherServiceConfig
            var openWeatherServiceConfig = config.GetSection("OpenWeatherService").Get<OpenWeatherSettings>();
            string openWeatherApiKey = openWeatherServiceConfig.OpenWeatherApiKey;
            string latitude = openWeatherServiceConfig.Latitude;
            string longitude = openWeatherServiceConfig.Longitude;

            // Argument to update latitude and longitude
            if (args.Length > 0 && args[0] == "--update-lat-long")
            {
                if (args.Length == 3)
                {
                    UpdateConfigLatLong(args[1], args[2]);
                    Console.WriteLine($"Updated appsettings.json with Latitude: {args[1]}, Longitude: {args[2]}");
                    return;
                }
                else
                {
                    Console.WriteLine("Usage: --update-lat-long <latitude> <longitude>");
                    return;
                }
            }

            // Argument to use config latitude and longitude
            if (args.Length > 0 && args[0] == "--config")
            {
                Console.WriteLine("Using Latitude and Longitude from appsettings.json");
            }
            else
            {
                // Get latitude and longitude from IP or generate random
                var locationFromIP = await GetLocationFromIP();
                if (locationFromIP == null)
                {
                    Console.WriteLine("Failed to get location from IP; using random...");
                    var randomLocation = GetRandomLocation();
                    latitude = randomLocation.latitude.ToString();
                    longitude = randomLocation.longitude.ToString();
                }
                else
                {
                    latitude = locationFromIP.Value.latitude.ToString();
                    longitude = locationFromIP.Value.longitude.ToString();
                }
            }

            if (args.Length > 0 && args[0] == "--random")
            {
                var randomLocation = GetRandomLocation();
                latitude = randomLocation.latitude.ToString();
                longitude = randomLocation.longitude.ToString();
                Console.WriteLine($"Using random Latitude: {latitude}, Longitude: {longitude}");
            }

            // Validate configuration
            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude) || string.IsNullOrEmpty(openWeatherApiKey))
            {
                Console.WriteLine("Invalid configuration");
                return;
            }

            // Fetch weather data using HttpClient
            await FetchWeatherData(openWeatherApiKey, latitude, longitude);
        }

        public static async Task FetchWeatherData(string apiKey, string latitude, string longitude)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic weatherData = JsonConvert.DeserializeObject(responseBody);

                    // Extract weather data
                    string locationName = weatherData.name != "" ? weatherData.name : "unknown locale";
                    string weatherDescription = weatherData.weather[0].description;

                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    string titleCasedWeatherDescription = textInfo.ToTitleCase(weatherDescription.ToLower());

                    float celsius = (float)weatherData.main.temp;
                    Func<float, float> convertToFahrenheit = c => (int)Math.Round(c * 9 / 5 + 32);

                    DateTime sunriseMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunrise).DateTime.ToLocalTime();
                    DateTime sunsetMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunset).DateTime.ToLocalTime();

                    Console.WriteLine();
                    Console.WriteLine($"Hand-picked weather data for {locationName}:");
                    Console.WriteLine();
                    Console.WriteLine($"\tWeather: {titleCasedWeatherDescription}");
                    Console.WriteLine($"\tTemperature: {convertToFahrenheit(celsius)}°F");
                    Console.WriteLine($"\tSunrise: {sunriseMoment.ToShortTimeString()}");
                    Console.WriteLine($"\tSunset: {sunsetMoment.ToShortTimeString()}");
                    Console.WriteLine($"\tLatitude: {Convert.ToDouble(latitude).ToString("F4")}");
                    Console.WriteLine($"\tLongitude: {Convert.ToDouble(longitude).ToString("F4")}");
                    Console.WriteLine();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }

        public static void UpdateConfigLatLong(string latitude, string longitude)
        {
            var jsonFile = "appsettings.json";
            var json = File.ReadAllText(jsonFile);
            dynamic jsonObj = JsonConvert.DeserializeObject(json);

            // Update Latitude and Longitude in the config file
            jsonObj["OpenWeatherService"]["Latitude"] = latitude;
            jsonObj["OpenWeatherService"]["Longitude"] = longitude;

            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(jsonFile, output);
        }

        public static async Task<(double latitude, double longitude)?> GetLocationFromIP()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var openWeatherServiceConfig = config.GetSection("OpenWeatherService").Get<OpenWeatherSettings>();
                    string iPInfoApiKey = openWeatherServiceConfig.IPInfoApiKey;
                    string requestUrl = $"https://ipinfo.io/json?token={iPInfoApiKey}";

                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic locationData = JsonConvert.DeserializeObject(responseBody);
                    string loc = locationData.loc;

                    var coords = loc.Split(',');
                    double latitude = Convert.ToDouble(coords[0]);
                    double longitude = Convert.ToDouble(coords[1]);

                    return (latitude, longitude);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }

        public static (double latitude, double longitude) GetRandomLocation()
        {
            Random random = new Random();
            double latitude = random.NextDouble() * 180 - 90;
            double longitude = random.NextDouble() * 360 - 180;
            return (latitude, longitude);
        }
    }
}
 