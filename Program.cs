using System;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Globalization;
using System.Threading.Tasks;
using OpenWeatherService.Models;
using Microsoft.Extensions.Configuration;

namespace OpenWeatherService
{

    class Program
    {


        static async Task Main(string[] args)
        {
            // using Microsoft.Extensions.Configuration
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Bind config to OpenWeatherServiceConfig
            var openWeatherServiceConfig = config.GetSection("OpenWeather").Get<OpenWeatherSettings>();

            // Destructure props into variables
            string latitude = openWeatherServiceConfig.Latitude;
            string longitude = openWeatherServiceConfig.Longitude;
            string apiKey = openWeatherServiceConfig.ApiKey;

            // Validate config
            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude) || string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("Invalid configuration");
                return;
            }

            // HttpClient instance
            using (HttpClient client = new HttpClient()) 
            {
                try
                {
                    string requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric";

                    //Console.WriteLine($"Sending request to OpenWeather API:\n");
                    //Console.WriteLine(requestUrl);
                    //Console.WriteLine();

                    // GET
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    response.EnsureSuccessStatusCode(); // Throw if not a success code.

                    // response
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Format JSON
                    dynamic weatherData = JsonConvert.DeserializeObject(responseBody);
                    string formattedWeatherData = JsonConvert.SerializeObject(weatherData, Formatting.Indented);

                    // Extract and write data
                    string weatherDescription = weatherData.weather[0].description;
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    string titleCasedWeatherDescription = textInfo.ToTitleCase(weatherDescription.ToLower());

                    float celsius = (float)weatherData.main.temp;
                    float highCelsius = (float)weatherData.main.temp_max;
                    float lowCelsius = (float)weatherData.main.temp_min;
                    Func<float, float> convertToFahrenheit = c => (int)Math.Round(c * 9 / 5 + 32);
                    
                    DateTime sunriseMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunrise).DateTime.ToLocalTime();
                    DateTime sunsetMoment = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.sys.sunset).DateTime.ToLocalTime();

                    //Console.WriteLine("Formatted JSON Response:\n");
                    //Console.WriteLine(formattedWeatherData);
                    Console.WriteLine();
                    Console.WriteLine($"Hand-picked weather data for {weatherData.name}:\n");
                    Console.WriteLine($"\tWeather: {titleCasedWeatherDescription}");
                    Console.WriteLine();
                    Console.WriteLine($"\tTemperature: {convertToFahrenheit((float)celsius)}°F");
                    Console.WriteLine($"\tHigh: {convertToFahrenheit((float)highCelsius)}°F");
                    Console.WriteLine($"\tLow: {convertToFahrenheit((float)lowCelsius)}°F");
                    Console.WriteLine($"\tHumidity: {weatherData.main.humidity}%");
                    Console.WriteLine();
                    Console.WriteLine($"\tSunrise: {sunriseMoment.ToShortTimeString()}");
                    Console.WriteLine($"\tSunset: {sunsetMoment.ToShortTimeString()}");
                    Console.WriteLine();
                    
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }

            // Wait for user input before closing
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Console.WriteLine();
        }
    }
}