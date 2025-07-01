using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyPortfolio.Services
{
    public class NasaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NasaService> _logger;
        private readonly IConfiguration _configuration;
        private static ApodResult? _cachedApod;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        public NasaService(HttpClient httpClient, ILogger<NasaService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ApodResult?> GetApodAsync()
        {
            try
            {
                // Check if we have a valid cached result
                if (_cachedApod != null && DateTime.UtcNow - _lastCacheTime < CacheDuration)
                {
                    _logger.LogInformation("Returning cached APOD data from {CacheTime}", _lastCacheTime);
                    return _cachedApod;
                }

                // Fetch new data from API
                var apiKey = _configuration["Nasa:ApiKey"] ?? "DEMO_KEY";
                var response = await _httpClient.GetFromJsonAsync<ApodResult>($"https://api.nasa.gov/planetary/apod?api_key={apiKey}");

                if (response != null)
                {
                    // Cache the result
                    _cachedApod = response;
                    _lastCacheTime = DateTime.UtcNow;
                    _logger.LogInformation("Cached new APOD data for {Date}", response.Date);
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Error fetching NASA APOD: {Message}", ex.Message);
                return _cachedApod; // Return cached data if available, even if expired
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching NASA APOD: {Message}", ex.Message);
                return _cachedApod; // Return cached data if available, even if expired
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