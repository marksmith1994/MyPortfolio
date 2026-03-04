using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MyPortfolio.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private static readonly List<string> DefaultCities = ["London", "Auckland", "New York"];

        public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<List<WeatherResult>> GetWeatherForDefaultCitiesAsync()
        {
            var results = new List<WeatherResult>();
            foreach (var city in DefaultCities)
            {
                var weather = await GetWeatherForCityAsync(city);
                if (weather != null) results.Add(weather);
            }
            return results;
        }

        public async Task<WeatherResult?> GetWeatherForCityAsync(string city)
        {
            string cacheKey = $"weather:{city.ToLowerInvariant()}";

            if (_cache.TryGetValue(cacheKey, out WeatherResult? cached))
            {
                _logger.LogInformation("Returning cached weather for {City}", city);
                return cached;
            }

            try
            {
                var apiKey = _configuration["Weather:ApiKey"] ?? "DEMO_KEY";
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
                var response = await _httpClient.GetFromJsonAsync<WeatherResult>(url);

                if (response != null)
                {
                    _cache.Set(cacheKey, response, TimeSpan.FromHours(1));
                    _logger.LogInformation("Cached weather data for {City}", city);
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching weather for {City}: {Message}", city, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather for {City}: {Message}", city, ex.Message);
                return null;
            }
        }
    }

    public class WeatherResult
    {
        public string Name { get; set; } = string.Empty;
        public SysInfo Sys { get; set; } = new();
        public MainInfo Main { get; set; } = new();
        public List<WeatherInfo> Weather { get; set; } = new();
        public WindInfo Wind { get; set; } = new();
        public int Visibility { get; set; }
        public int Timezone { get; set; }
        public int Dt { get; set; }
        public CloudsInfo Clouds { get; set; } = new();
        public int Cod { get; set; }
    }

    public class SysInfo { public string Country { get; set; } = string.Empty; }
    public class MainInfo { public double Temp { get; set; } public int Humidity { get; set; } }
    public class WeatherInfo { public string Main { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public string Icon { get; set; } = string.Empty; }
    public class WindInfo { public double Speed { get; set; } }
    public class CloudsInfo { public int All { get; set; } }
}
