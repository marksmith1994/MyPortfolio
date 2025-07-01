using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyPortfolio.Services
{
    public class SpaceXService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpaceXService> _logger;
        private static List<Launch>? _cachedUpcomingLaunches;
        private static List<Launch>? _cachedPastLaunches;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1); // Cache for 1 hour

        public SpaceXService(HttpClient httpClient, ILogger<SpaceXService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Launch>> GetUpcomingLaunchesAsync()
        {
            try
            {
                // Check if we have valid cached data
                if (_cachedUpcomingLaunches != null && DateTime.UtcNow - _lastCacheTime < CacheDuration)
                {
                    _logger.LogInformation("Returning cached upcoming launches from {CacheTime}", _lastCacheTime);
                    return _cachedUpcomingLaunches;
                }

                // Fetch new data from API
                var response = await _httpClient.GetFromJsonAsync<List<Launch>>("https://api.spacexdata.com/v4/launches/upcoming");
                
                if (response != null)
                {
                    // Cache the result
                    _cachedUpcomingLaunches = response.OrderBy(l => l.DateUtc).Take(12).ToList();
                    _lastCacheTime = DateTime.UtcNow;
                    _logger.LogInformation("Cached new upcoming launches data");
                }

                return _cachedUpcomingLaunches ?? new List<Launch>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Error fetching upcoming SpaceX launches: {Message}", ex.Message);
                return _cachedUpcomingLaunches ?? new List<Launch>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming SpaceX launches: {Message}", ex.Message);
                return _cachedUpcomingLaunches ?? new List<Launch>();
            }
        }

        public async Task<List<Launch>> GetPastLaunchesAsync()
        {
            try
            {
                // Check if we have valid cached data
                if (_cachedPastLaunches != null && DateTime.UtcNow - _lastCacheTime < CacheDuration)
                {
                    _logger.LogInformation("Returning cached past launches from {CacheTime}", _lastCacheTime);
                    return _cachedPastLaunches;
                }

                // Fetch new data from API
                var response = await _httpClient.GetFromJsonAsync<List<Launch>>("https://api.spacexdata.com/v4/launches/past");
                
                if (response != null)
                {
                    // Cache the result
                    _cachedPastLaunches = response.OrderByDescending(l => l.DateUtc).Take(12).ToList();
                    _lastCacheTime = DateTime.UtcNow;
                    _logger.LogInformation("Cached new past launches data");
                }

                return _cachedPastLaunches ?? new List<Launch>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Error fetching past SpaceX launches: {Message}", ex.Message);
                return _cachedPastLaunches ?? new List<Launch>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching past SpaceX launches: {Message}", ex.Message);
                return _cachedPastLaunches ?? new List<Launch>();
            }
        }
    }

    public class Launch
    {
        public string Name { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; }
        public string Rocket { get; set; } = string.Empty;
        public bool? Success { get; set; }
        public string? Details { get; set; }
        public LaunchLinks Links { get; set; } = new();
    }

    public class LaunchLinks 
    { 
        public string Webcast { get; set; } = string.Empty; 
    }
} 