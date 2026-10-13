using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MyPortfolio.Services;
using System.Net;
using System.Text;

namespace MyPortfolio.Tests.Services;

public class SpaceXServiceTests
{
    private const string SampleJson = """
    {
      "count": 2,
      "results": [
        {
          "name": "Falcon 9 Block 5 | Starlink Group 15-25",
          "net": "2026-10-13T10:33:44Z",
          "status": { "id": 3, "abbrev": "Success" },
          "rocket": { "configuration": { "name": "Falcon 9", "full_name": "Falcon 9 Block 5" } },
          "mission": { "description": "A batch of Starlink satellites." },
          "image": { "image_url": "https://example.com/full.jpg", "thumbnail_url": "https://example.com/thumb.jpg" },
          "mission_patches": [ { "image_url": "https://example.com/patch.png" } ],
          "vid_urls": [
            { "url": "https://youtube.com/second", "priority": 10 },
            { "url": "https://youtube.com/first", "priority": 1 }
          ]
        },
        {
          "name": "Starship | Flight 12",
          "net": null,
          "status": { "id": 1, "abbrev": "Go" },
          "rocket": { "configuration": { "name": "Starship" } },
          "mission": null,
          "image": { "image_url": "https://example.com/starship.jpg", "thumbnail_url": "https://example.com/starship-thumb.jpg" },
          "mission_patches": [],
          "vid_urls": []
        }
      ]
    }
    """;

    private static SpaceXService CreateService(HttpResponseMessage httpResponse, IMemoryCache? cache = null)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(httpResponse));
        return new SpaceXService(httpClient, NullLogger<SpaceXService>.Instance,
            cache ?? new MemoryCache(new MemoryCacheOptions()));
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task GetUpcomingLaunchesAsync_MapsLaunchLibraryResponse()
    {
        var result = await CreateService(JsonResponse(SampleJson)).GetUpcomingLaunchesAsync();

        Assert.Equal(2, result.Count);

        var starlink = result[0];
        Assert.Equal("Falcon 9 Block 5 | Starlink Group 15-25", starlink.Name);
        Assert.Equal(new DateTime(2026, 10, 13, 10, 33, 44, DateTimeKind.Utc), starlink.DateUtc);
        Assert.Equal("Falcon 9 Block 5", starlink.Rocket);
        Assert.True(starlink.Success);
        Assert.Equal("A batch of Starlink satellites.", starlink.Details);
        Assert.Equal("https://youtube.com/first", starlink.Links.Webcast);
        Assert.Equal("https://example.com/patch.png", starlink.Links.Patch?.Small);

        var starship = result[1];
        Assert.Equal(DateTime.MinValue, starship.DateUtc);
        Assert.Equal("Starship", starship.Rocket);
        Assert.Null(starship.Success);
        Assert.Null(starship.Details);
        Assert.Equal(string.Empty, starship.Links.Webcast);
        Assert.Equal("https://example.com/starship-thumb.jpg", starship.Links.Patch?.Small);
    }

    [Fact]
    public async Task GetPastLaunchesAsync_ReturnsCachedData_WhenCachePopulated()
    {
        var cached = new List<Launch> { new() { Name = "Cached" } };
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("spacex:past", cached);

        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.OK), cache).GetPastLaunchesAsync();

        Assert.Same(cached, result);
    }

    [Fact]
    public async Task GetUpcomingLaunchesAsync_ReturnsEmptyList_AndDoesNotCache_WhenApiFails()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());

        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError), cache)
            .GetUpcomingLaunchesAsync();

        Assert.Empty(result);
        Assert.False(cache.TryGetValue("spacex:upcoming", out _));
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(7, false)]
    [InlineData(2, null)]
    public async Task GetPastLaunchesAsync_MapsStatusToSuccess(int statusId, bool? expected)
    {
        var json = $$"""{ "results": [ { "name": "Test", "net": "2026-01-01T00:00:00Z", "status": { "id": {{statusId}} } } ] }""";

        var result = await CreateService(JsonResponse(json)).GetPastLaunchesAsync();

        Assert.Equal(expected, Assert.Single(result).Success);
    }
}
