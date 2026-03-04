using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MyPortfolio.Services
{
    public class SpaceXService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpaceXService> _logger;
        private readonly IMemoryCache _cache;
        private const string UpcomingKey = "spacex:upcoming";
        private const string PastKey = "spacex:past";

        public SpaceXService(HttpClient httpClient, ILogger<SpaceXService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<Launch>> GetUpcomingLaunchesAsync()
        {
            if (_cache.TryGetValue(UpcomingKey, out List<Launch>? cached) && cached != null)
            {
                _logger.LogInformation("Returning cached upcoming launches");
                return cached;
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Launch>>("https://api.spacexdata.com/v4/launches/upcoming");
                var result = response?.OrderBy(l => l.DateUtc).Take(12).ToList() ?? new List<Launch>();
                _cache.Set(UpcomingKey, result, TimeSpan.FromHours(1));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming SpaceX launches: {Message}", ex.Message);
                return new List<Launch>();
            }
        }

        public async Task<List<Launch>> GetPastLaunchesAsync()
        {
            if (_cache.TryGetValue(PastKey, out List<Launch>? cached) && cached != null)
            {
                _logger.LogInformation("Returning cached past launches");
                return cached;
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Launch>>("https://api.spacexdata.com/v4/launches/past");
                var result = response?.OrderByDescending(l => l.DateUtc).Take(12).ToList() ?? new List<Launch>();
                _cache.Set(PastKey, result, TimeSpan.FromHours(1));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching past SpaceX launches: {Message}", ex.Message);
                return new List<Launch>();
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
        public LaunchPatch? Patch { get; set; }
    }

    public class LaunchPatch
    {
        public string? Small { get; set; }
        public string? Large { get; set; }
    }
}
