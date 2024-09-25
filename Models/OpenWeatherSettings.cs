namespace OpenWeatherService.Models
{
    public class OpenWeatherSettings
    {
        public required string Latitude { get; set; }
        public required string Longitude { get; set; }
        public required string ApiKey { get; set; }
    }
}