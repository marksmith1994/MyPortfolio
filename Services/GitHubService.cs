using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using MyPortfolio.Models;

namespace MyPortfolio.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubService> _logger;
        private readonly IConfiguration _configuration;
        private const string GitHubApiBaseUrl = "https://api.github.com";
        
        // Cache for repositories
        private List<GitHubRepository>? _cachedRepositories;
        private DateTime _lastCacheTime = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24); // Cache for 24 hours

        public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyPortfolio");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        public async Task<List<GitHubRepository>> GetRepositoriesAsync()
        {
            // Check if we have valid cached data
            if (_cachedRepositories != null && DateTime.Now - _lastCacheTime < _cacheDuration)
            {
                _logger.LogInformation("Returning cached GitHub repositories (cached at {CacheTime})", _lastCacheTime);
                return _cachedRepositories;
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
                    return _cachedRepositories ?? new List<GitHubRepository>();
                }

                // Filter out forked repositories and sort by stars
                var filteredRepositories = response
                    .Where(r => !r.Fork) // Remove this line if you want to include forks
                    .OrderByDescending(r => r.StargazersCount)
                    .ToList();

                // Update cache
                _cachedRepositories = filteredRepositories;
                _lastCacheTime = DateTime.Now;
                
                _logger.LogInformation("Successfully cached {Count} GitHub repositories", filteredRepositories.Count);
                return filteredRepositories;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP Error fetching GitHub repositories: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("GitHub API rate limit may have been exceeded");
                }
                
                // Return cached data if available, otherwise empty list
                return _cachedRepositories ?? new List<GitHubRepository>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching GitHub repositories: {ex.Message}");
                
                // Return cached data if available, otherwise empty list
                return _cachedRepositories ?? new List<GitHubRepository>();
            }
        }

        // Method to clear cache (useful for testing or manual refresh)
        public void ClearCache()
        {
            _cachedRepositories = null;
            _lastCacheTime = DateTime.MinValue;
            _logger.LogInformation("GitHub repository cache cleared");
        }
    }
} 