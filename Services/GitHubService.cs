using System.Net.Http.Json;
using MyPortfolio.Models;

namespace MyPortfolio.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private const string GitHubApiBaseUrl = "https://api.github.com";
        private const string Username = "marksmith1994"; 

        public GitHubService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyPortfolio");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        public async Task<List<GitHubRepository>> GetRepositoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<GitHubRepository>>(
                    $"{GitHubApiBaseUrl}/users/{Username}/repos?sort=updated&direction=desc");

                return response ?? new List<GitHubRepository>();
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                Console.WriteLine($"Error fetching GitHub repositories: {ex.Message}");
                return new List<GitHubRepository>();
            }
        }
    }
} 