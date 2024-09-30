using OpenWeatherService.Interfaces;
using OpenWeatherService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWeatherService.Services
{
    public class CLIService
    {
        private readonly IWeatherService _weatherService;

        public CLIService(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task HandleArguments(string[] args)
        {
            string latitude = null;
            string longitude = null;

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
                        }
                        else
                        {
                            Console.WriteLine("Usage: --location <latitude> <longitude>");
                        }
                        break;

                    case "--random":
                        var randomLocation = _weatherService.GetRandomLocation();
                        latitude = randomLocation.latitude.ToString();
                        longitude = randomLocation.longitude.ToString();
                        Console.WriteLine($"Fetching random weather data...");
                        break;

                    case "--help":
                        Console.WriteLine("Usage: OpenWeatherService [--location <latitude> <longitude>] [--random] [--help]");
                        return;

                    default:
                        Console.WriteLine("Usage: OpenWeatherService [--location <latitude> <longitude>] [--random] [--help]");
                        return;
                }
            }

            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
            {
                var locationFromIP = await _weatherService.GetLocationFromIP();
                if (locationFromIP != null)
                {
                    latitude = locationFromIP.Value.latitude.ToString();
                    longitude = locationFromIP.Value.longitude.ToString();
                    Console.WriteLine($"Fetching ip-based weather data...");
                }
                else
                {
                    var randomLocation = _weatherService.GetRandomLocation();
                    latitude = randomLocation.latitude.ToString();
                    longitude = randomLocation.longitude.ToString();
                    Console.WriteLine($"Fetching random weather data...");
                }
            }

            WeatherInfo weatherInfo = await _weatherService.FetchWeatherData(latitude, longitude);
            _weatherService.PrintWeatherData(weatherInfo);
        }
    }

}
