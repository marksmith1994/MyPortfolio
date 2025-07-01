using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyPortfolio.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherService> _logger;
        private readonly IConfiguration _configuration;
        private static readonly Dictionary<string, CachedWeatherData> _weatherCache = new();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
        private static readonly List<string> DefaultCities = new() { "London", "Auckland", "New York" };

        public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<WeatherResult>> GetWeatherForDefaultCitiesAsync()
        {
            var results = new List<WeatherResult>();
            
            foreach (var city in DefaultCities)
            {
                var weather = await GetWeatherForCityAsync(city);
                if (weather != null)
                {
                    results.Add(weather);
                }
            }
            
            return results;
        }

        public async Task<WeatherResult?> GetWeatherForCityAsync(string city)
        {
            try
            {
                var cacheKey = city.ToLowerInvariant();
                
                // Check if we have a valid cached result
                if (_weatherCache.TryGetValue(cacheKey, out var cachedData) && 
                    DateTime.UtcNow - cachedData.CachedAt < CacheDuration)
                {
                    _logger.LogInformation("Returning cached weather data for {City} from {CacheTime}", city, cachedData.CachedAt);
                    return cachedData.WeatherData;
                }

                // Fetch new data from API
                var apiKey = _configuration["Weather:ApiKey"] ?? "DEMO_KEY";
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
                var response = await _httpClient.GetFromJsonAsync<WeatherResult>(url);

                if (response != null)
                {
                    // Cache the result
                    _weatherCache[cacheKey] = new CachedWeatherData
                    {
                        WeatherData = response,
                        CachedAt = DateTime.UtcNow
                    };
                    _logger.LogInformation("Cached new weather data for {City}", city);
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Error fetching weather for {City}: {Message}", city, ex.Message);
                return _weatherCache.TryGetValue(city.ToLowerInvariant(), out var cachedData) ? cachedData.WeatherData : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather for {City}: {Message}", city, ex.Message);
                return _weatherCache.TryGetValue(city.ToLowerInvariant(), out var cachedData) ? cachedData.WeatherData : null;
            }
        }
    }

    public class CachedWeatherData
    {
        public WeatherResult WeatherData { get; set; } = new();
        public DateTime CachedAt { get; set; }
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

    public class SysInfo 
    { 
        public string Country { get; set; } = string.Empty; 
    }
    
    public class MainInfo 
    { 
        public double Temp { get; set; } 
        public int Humidity { get; set; } 
    }
    
    public class WeatherInfo 
    { 
        public string Main { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public string Icon { get; set; } = string.Empty; 
    }
    
    public class WindInfo 
    { 
        public double Speed { get; set; } 
    }
    
    public class CloudsInfo 
    { 
        public int All { get; set; } 
    }
} 