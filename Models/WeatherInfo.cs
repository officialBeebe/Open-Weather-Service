namespace OpenWeatherService.Models
{
    public class WeatherInfo
    {
        public string LocationName { get; set; }
        public string WeatherDescription { get; set; }
        public float Celsius { get; set; }
        public float Fahrenheit { get; set; }
        public DateTime SunriseMoment { get; set; }
        public DateTime SunsetMoment { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
