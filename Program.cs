using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenWeatherService.Models;
using Spectre.Console;

namespace OpenWeatherService
{
    class Program
    {
        // Configuration variables
        private static string openWeatherApiKey = System.Configuration.ConfigurationManager.AppSettings["OpenWeather:ApiKey"];
        private static string iPInfoApiKey = System.Configuration.ConfigurationManager.AppSettings["IPInfo:ApiKey"];
        private static string latitude = System.Configuration.ConfigurationManager.AppSettings["Latitude"];
        private static string longitude = System.Configuration.ConfigurationManager.AppSettings["Longitude"];

        static async Task Main(string[] args)
        {
            // Validate API keys
            if (string.IsNullOrEmpty(openWeatherApiKey) || string.IsNullOrEmpty(iPInfoApiKey))
            {
                Console.WriteLine("Something is wrong. No API keys detected in App.Config...");
            }

            // Process command-line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--location":
                        if (i + 2 < args.Length)
                        {
                            latitude = args[i + 1];
                            longitude = args[i + 2];
                            i += 2;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Usage: --location <latitude> <longitude>");
                            break;
                        }

                    case "--random":
                        var randomLocation = GetRandomLocation();
                        latitude = randomLocation.latitude.ToString();
                        longitude = randomLocation.longitude.ToString();
                        Console.WriteLine($"Using random Latitude: {latitude}, Longitude: {longitude}");
                        break;

                    case "--help":
                        Console.WriteLine("Usage: OpenWeatherService [--location <latitude> <longitude>] [--random] [--help]");
                        break;

                    default:
                        Console.WriteLine("Usage: OpenWeatherService [--location <latitude> <longitude>] [--random] [--help]");
                        break;
                }
            }

            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
            {
                var locationFromIP = await GetLocationFromIP();
                if (locationFromIP != null)
                {
                    latitude = locationFromIP.Value.latitude.ToString();
                    longitude = locationFromIP.Value.longitude.ToString();
                    Console.WriteLine($"Empty or Invalid Latitude or Longitude. Requesting data from IP-based Latitude: {latitude}, Longitude: {longitude}");
                }
                else
                {
                    var randomLocation = GetRandomLocation();
                    latitude = randomLocation.latitude.ToString();
                    longitude = randomLocation.longitude.ToString();
                    Console.WriteLine($"Empty or Invalid Latitude or Longitude. Request for IP-based Latitude and Longitude failed. Requesting data from random Latitude: {latitude}, Longitude: {longitude}");
                }
            }

            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude) || string.IsNullOrEmpty(openWeatherApiKey))
            {
                Console.WriteLine("Oops! Something went terribly wrong...");
                return;
            }

            WeatherInfo weatherInfo = await FetchWeatherData(openWeatherApiKey, latitude, longitude);
            PrintWeatherData(weatherInfo);
        }

        public static async Task<(double latitude, double longitude)?> GetLocationFromIP()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string requestUrl = $"https://ipinfo.io/json?token={iPInfoApiKey}";

                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    dynamic locationData;
                    string _location;
                    string[] coords;
                    double latitude;
                    double longitude;

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        locationData = JsonConvert.DeserializeObject(responseBody);
                        _location = locationData.loc;
                        coords = _location.Split(',');
                        latitude = Convert.ToDouble(coords[0]);
                        longitude = Convert.ToDouble(coords[1]);
                        return (latitude, longitude);
                    }

                    throw new Exception("Failed to get location from IP");
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

        public static async Task<WeatherInfo> FetchWeatherData(string apiKey, string latitude, string longitude)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("Bad key");
                return null;
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric";
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic weatherData = JsonConvert.DeserializeObject(responseBody);

                    string locationName = !string.IsNullOrEmpty(weatherData.name.ToString()) ? weatherData.name.ToString() : "unknown locale";
                    string weatherDescription = weatherData.weather[0].description.ToString();

                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    string titleCasedWeatherDescription = textInfo.ToTitleCase(weatherDescription.ToLower());

                    float celsius = (float)weatherData.main.temp;
                    float fahrenheit = (int)Math.Round(celsius * 9 / 5 + 32);

                    DateTime sunriseMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunrise).ToLocalTime().DateTime;
                    DateTime sunsetMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunset).ToLocalTime().DateTime;

                    double lat = Convert.ToDouble(latitude);
                    double lon = Convert.ToDouble(longitude);

                    WeatherInfo weatherInfo = new WeatherInfo
                    {
                        LocationName = locationName,
                        WeatherDescription = titleCasedWeatherDescription,
                        Celsius = celsius,
                        Fahrenheit = fahrenheit,
                        SunriseMoment = sunriseMoment,
                        SunsetMoment = sunsetMoment,
                        Latitude = lat,
                        Longitude = lon
                    };

                    return weatherInfo;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                    return null;
                }
                catch (JsonException e)
                {
                    Console.WriteLine($"JSON parsing error: {e.Message}");
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An unexpected error occurred: {e.Message}");
                    return null;
                }
            }
        }

        public static void PrintWeatherData(WeatherInfo info)
        {
            if (info == null)
            {
                Console.WriteLine("No weather data to display.");
                return;
            }

            string _locationName = info.LocationName;
            string _latitude = info.Latitude.ToString("F4");
            string _longitude = info.Longitude.ToString("F4");
            string _weatherDescription = info.WeatherDescription;
            string _fahrenheit = info.Fahrenheit.ToString();
            string _sunriseMoment = info.SunriseMoment.ToShortTimeString();
            string _sunsetMoment = info.SunsetMoment.ToShortTimeString();

            Console.WriteLine();
            Console.WriteLine($"Weather for {_locationName} at {Math.Abs(Convert.ToDouble(_latitude))}{(Convert.ToDouble(_latitude) >= 0 ? "N" : "S")}, {Math.Abs(Convert.ToDouble(_longitude))}{(Convert.ToDouble(_longitude) >= 0 ? "E" : "W")}: ");
            Console.WriteLine();
            Console.WriteLine($"\tWeather: {_weatherDescription}");
            Console.WriteLine($"\tTemperature: {_fahrenheit}°F");
            Console.WriteLine();
            Console.WriteLine($"\tSunrise: {_sunriseMoment}");
            Console.WriteLine($"\tSunset: {_sunsetMoment}");
            Console.WriteLine();
        }

        static void UpdateAppSetting(string key, string value)
        {
            try
            {
                // Explicitly use System.Configuration.ConfigurationManager to avoid ambiguity
                var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = config.AppSettings.Settings;

                if (settings[key] == null)
                {
                    settings.Add(key, value);
                    Console.WriteLine($"Added setting: {key} = {value}");
                }
                else
                {
                    settings[key].Value = value;
                    Console.WriteLine($"Updated setting: {key} = {value}");
                }

                config.Save(ConfigurationSaveMode.Modified);
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
            catch (ConfigurationErrorsException ex)
            {
                Console.WriteLine("Error writing app settings: " + ex.Message);
            }
        }
    }
}
