using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyPortfolio.Models;

namespace MyPortfolio.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private const string GitHubApiBaseUrl = "https://api.github.com";
        private const string CacheKey = "github:repos";

        public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyPortfolio");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        public async Task<List<GitHubRepository>> GetRepositoriesAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<GitHubRepository>? cached) && cached != null)
            {
                _logger.LogInformation("Returning cached GitHub repositories");
                return cached;
            }

            try
            {
                _logger.LogInformation("Fetching fresh GitHub repositories from API");
                var username = _configuration["GitHub:Username"] ?? "marksmith1994";
                var response = await _httpClient.GetFromJsonAsync<List<GitHubRepository>>(
                    $"{GitHubApiBaseUrl}/users/{username}/repos?sort=updated&direction=desc&per_page=100");

                if (response == null)
                {
                    _logger.LogWarning("GitHub API returned null response");
                    return new List<GitHubRepository>();
                }

                var filtered = response
                    .Where(r => !r.Fork)
                    .OrderByDescending(r => r.StargazersCount)
                    .ToList();

                _cache.Set(CacheKey, filtered, TimeSpan.FromHours(24));
                _logger.LogInformation("Successfully cached {Count} GitHub repositories", filtered.Count);
                return filtered;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Error fetching GitHub repositories: {Message}", ex.Message);
                if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    _logger.LogWarning("GitHub API rate limit may have been exceeded");
                return new List<GitHubRepository>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GitHub repositories: {Message}", ex.Message);
                return new List<GitHubRepository>();
            }
        }

        public void ClearCache() => _cache.Remove(CacheKey);
    }
}
