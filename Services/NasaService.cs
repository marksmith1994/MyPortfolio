using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MyPortfolio.Services
{
    public class NasaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NasaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public NasaService(HttpClient httpClient, ILogger<NasaService> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<ApodResult?> GetApodAsync(string? date = null)
        {
            var apiKey = _configuration["Nasa:ApiKey"] ?? "DEMO_KEY";
            bool isToday = string.IsNullOrEmpty(date) || date == DateTime.UtcNow.ToString("yyyy-MM-dd");
            string cacheKey = isToday ? "apod:today" : $"apod:{date}";

            if (_cache.TryGetValue(cacheKey, out ApodResult? cached))
                return cached;

            try
            {
                var url = isToday
                    ? $"https://api.nasa.gov/planetary/apod?api_key={apiKey}"
                    : $"https://api.nasa.gov/planetary/apod?date={date}&api_key={apiKey}";

                var response = await _httpClient.GetFromJsonAsync<ApodResult>(url);

                if (response != null)
                {
                    // Historical APODs never change; cache them much longer than today's
                    var expiry = isToday ? TimeSpan.FromHours(24) : TimeSpan.FromDays(30);
                    _cache.Set(cacheKey, response, expiry);
                    _logger.LogInformation("Cached APOD for {Date}", response.Date);
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching NASA APOD: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching NASA APOD: {Message}", ex.Message);
                return null;
            }
        }
    }

    public class ApodResult
    {
        public string Title { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Copyright { get; set; } = string.Empty;
    }
}
