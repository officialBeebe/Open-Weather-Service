using System;
using System.Configuration;
using System.Threading.Tasks;
using OpenWeatherService.Interfaces;
using OpenWeatherService.Services;

namespace OpenWeatherService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Load API keys from App.config
            string openWeatherApiKey = ConfigurationManager.AppSettings["OpenWeather:ApiKey"];
            string iPInfoApiKey = ConfigurationManager.AppSettings["IPInfo:ApiKey"];

            // Validate API keys
            if (string.IsNullOrEmpty(openWeatherApiKey) || string.IsNullOrEmpty(iPInfoApiKey))
            {
                Console.WriteLine("Error: API keys not detected in App.config.");
                return;
            }

            // Instantiate the core weather service with the API keys
            IWeatherService weatherService = new WeatherService(openWeatherApiKey, iPInfoApiKey);

            // Instantiate the CLI service and pass the weather service
            CLIService cliService = new CLIService(weatherService);

            // Handle command-line arguments and trigger the core logic
            await cliService.HandleArguments(args);
        }
    }
}
