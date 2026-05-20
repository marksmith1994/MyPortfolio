using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MyPortfolio.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MyPortfolio.Tests.Services;

public class F1ServiceTests
{
    private static F1Service CreateService(HttpResponseMessage httpResponse, IMemoryCache? cache = null)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(httpResponse));
        return new F1Service(httpClient, NullLogger<F1Service>.Instance,
            cache ?? new MemoryCache(new MemoryCacheOptions()));
    }

    [Fact]
    public async Task GetDriverStandingsAsync_ReturnsCachedData_WhenCachePopulated()
    {
        var cached = new List<F1DriverStanding> { new() { Position = "1" } };
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("f1:drivers:current", cached);

        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.OK), cache).GetDriverStandingsAsync();

        Assert.Same(cached, result);
    }

    [Fact]
    public async Task GetDriverStandingsAsync_ReturnsEmptyList_WhenApiFails()
    {
        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError)).GetDriverStandingsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetConstructorStandingsAsync_ReturnsCachedData_WhenCachePopulated()
    {
        var cached = new List<F1ConstructorStanding> { new() { Position = "1" } };
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("f1:constructors:current", cached);

        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.OK), cache).GetConstructorStandingsAsync();

        Assert.Same(cached, result);
    }

    [Fact]
    public async Task GetLastRaceAsync_ReturnsNull_WhenApiFails()
    {
        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError)).GetLastRaceAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetScheduleAsync_ReturnsCachedData_WhenCachePopulated()
    {
        var cached = new List<F1ScheduleRace> { new() { RaceName = "Monaco GP" } };
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("f1:schedule:current", cached);

        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.OK), cache).GetScheduleAsync();

        Assert.Same(cached, result);
    }

    [Fact]
    public async Task GetScheduleAsync_ReturnsEmptyList_WhenApiFails()
    {
        var result = await CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError)).GetScheduleAsync();

        Assert.Empty(result);
    }

    [Fact]
    public void F1ScheduleRace_IsPast_ReturnsFalse_ForFutureDate()
    {
        var race = new F1ScheduleRace { Date = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd") };

        Assert.False(race.IsPast);
    }

    [Fact]
    public void F1ScheduleRace_IsPast_ReturnsTrue_ForPastDate()
    {
        var race = new F1ScheduleRace { Date = "2020-03-15" };

        Assert.True(race.IsPast);
    }
}
