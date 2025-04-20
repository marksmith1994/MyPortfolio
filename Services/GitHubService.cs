using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using MyPortfolio.Models;

namespace MyPortfolio.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubService> _logger;
        private const string GitHubApiBaseUrl = "https://api.github.com";
        private const string Username = "marksmith1994";

        public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyPortfolio");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        public async Task<List<GitHubRepository>> GetRepositoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<GitHubRepository>>(
                    $"{GitHubApiBaseUrl}/users/{Username}/repos?sort=updated&direction=desc&per_page=100");

                if (response == null)
                {
                    return new List<GitHubRepository>();
                }

                // Filter out forked repositories and sort by stars
                return response
                    .Where(r => !r.Fork) // Remove this line if you want to include forks
                    .OrderByDescending(r => r.StargazersCount)
                    .ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP Error fetching GitHub repositories: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("GitHub API rate limit may have been exceeded");
                }
                return new List<GitHubRepository>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching GitHub repositories: {ex.Message}");
                return new List<GitHubRepository>();
            }
        }
    }
} 