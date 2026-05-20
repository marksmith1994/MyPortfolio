using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyPortfolio.Models;
using MyPortfolio.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MyPortfolio.Tests.Services;

public class GitHubServiceTests
{
    private static GitHubService CreateService(HttpResponseMessage httpResponse, IMemoryCache? cache = null, string? token = null)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(httpResponse));
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["GitHub:Username"]).Returns("testuser");
        config.Setup(c => c["GitHub:Token"]).Returns(token);
        return new GitHubService(httpClient, NullLogger<GitHubService>.Instance, config.Object,
            cache ?? new MemoryCache(new MemoryCacheOptions()));
    }

    [Fact]
    public async Task GetRepositoriesAsync_ReturnsCachedData_WhenCachePopulated()
    {
        var cached = new List<GitHubRepository> { new() { Name = "cached-repo" } };
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("github:repos", cached);

        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.OK), cache).GetRepositoriesAsync();

        Assert.Same(cached, result);
    }

    [Fact]
    public async Task GetRepositoriesAsync_ReturnsEmptyList_WhenApiFails()
    {
        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError)).GetRepositoriesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRepositoriesAsync_FiltersForks_FromApiResponse()
    {
        var repos = new[]
        {
            new GitHubRepository { Name = "my-repo", Fork = false, StargazersCount = 1 },
            new GitHubRepository { Name = "forked-repo", Fork = true, StargazersCount = 10 }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(repos), Encoding.UTF8, "application/json")
        };

        var result = await CreateService(response).GetRepositoriesAsync();

        Assert.Single(result);
        Assert.Equal("my-repo", result[0].Name);
    }

    [Fact]
    public async Task GetRepositoriesAsync_OrdersByStars_Descending()
    {
        var repos = new[]
        {
            new GitHubRepository { Name = "low-stars", Fork = false, StargazersCount = 1 },
            new GitHubRepository { Name = "high-stars", Fork = false, StargazersCount = 50 },
            new GitHubRepository { Name = "mid-stars", Fork = false, StargazersCount = 10 }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(repos), Encoding.UTF8, "application/json")
        };

        var result = await CreateService(response).GetRepositoriesAsync();

        Assert.Equal("high-stars", result[0].Name);
        Assert.Equal("low-stars", result[^1].Name);
    }

    [Fact]
    public async Task ClearCache_CausesNextCallToHitApi()
    {
        var cached = new List<GitHubRepository> { new() { Name = "stale-repo" } };
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("github:repos", cached);

        var service = CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError), cache);
        service.ClearCache();
        var result = await service.GetRepositoriesAsync();

        Assert.Empty(result);
    }
}
