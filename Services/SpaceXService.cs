using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MyPortfolio.Services
{
    /// <summary>
    /// SpaceX launch data from The Space Devs' Launch Library 2 API.
    /// Replaces the community r/SpaceX API (api.spacexdata.com), which is no longer maintained.
    /// LL2 needs no API key; anonymous use is rate-limited (~15 requests/hour), which the 1-hour cache keeps us well under.
    /// </summary>
    public class SpaceXService
    {
        private const string BaseUrl = "https://ll.thespacedevs.com/2.3.0/launches";
        private const string Query = "?lsp__name=SpaceX&mode=detailed&limit=12&format=json";

        // Launch Library 2 status ids
        private const int StatusSuccess = 3;
        private const int StatusFailure = 4;
        private const int StatusPartialFailure = 7;

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

        public Task<List<Launch>> GetUpcomingLaunchesAsync() =>
            GetLaunchesAsync(UpcomingKey, $"{BaseUrl}/upcoming/{Query}&ordering=net", "upcoming");

        public Task<List<Launch>> GetPastLaunchesAsync() =>
            GetLaunchesAsync(PastKey, $"{BaseUrl}/previous/{Query}&ordering=-net", "past");

        private async Task<List<Launch>> GetLaunchesAsync(string cacheKey, string url, string label)
        {
            if (_cache.TryGetValue(cacheKey, out List<Launch>? cached) && cached != null)
            {
                _logger.LogInformation("Returning cached {Label} launches", label);
                return cached;
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<LlLaunchList>(url);
                var result = response?.Results.Select(MapLaunch).ToList() ?? new List<Launch>();

                // Don't cache an empty result, so a transient failure isn't pinned for an hour.
                if (result.Count > 0)
                {
                    _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching {Label} SpaceX launches: {Message}", label, ex.Message);
                return new List<Launch>();
            }
        }

        internal static Launch MapLaunch(LlLaunch l) => new()
        {
            Name = l.Name,
            DateUtc = l.Net?.UtcDateTime ?? DateTime.MinValue,
            Rocket = l.Rocket?.Configuration?.FullName ?? l.Rocket?.Configuration?.Name ?? string.Empty,
            Success = l.Status?.Id switch
            {
                StatusSuccess => true,
                StatusFailure or StatusPartialFailure => false,
                _ => null
            },
            Details = l.Mission?.Description,
            Links = new LaunchLinks
            {
                Webcast = l.VidUrls
                    .OrderBy(v => v.Priority ?? int.MaxValue)
                    .Select(v => v.Url)
                    .FirstOrDefault(u => !string.IsNullOrEmpty(u)) ?? string.Empty,
                Patch = new LaunchPatch
                {
                    Small = l.MissionPatches.Select(p => p.ImageUrl).FirstOrDefault(u => !string.IsNullOrEmpty(u))
                            ?? l.Image?.ThumbnailUrl
                            ?? l.Image?.ImageUrl,
                    Large = l.MissionPatches.Select(p => p.ImageUrl).FirstOrDefault(u => !string.IsNullOrEmpty(u))
                            ?? l.Image?.ImageUrl
                }
            }
        };
    }

    // ----- View model used by the SpaceX page (shape unchanged) -----

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

    // ----- Launch Library 2 response models -----

    internal class LlLaunchList
    {
        [JsonPropertyName("results")]
        public List<LlLaunch> Results { get; set; } = new();
    }

    internal class LlLaunch
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("net")]
        public DateTimeOffset? Net { get; set; }

        [JsonPropertyName("status")]
        public LlStatus? Status { get; set; }

        [JsonPropertyName("rocket")]
        public LlRocket? Rocket { get; set; }

        [JsonPropertyName("mission")]
        public LlMission? Mission { get; set; }

        [JsonPropertyName("image")]
        public LlImage? Image { get; set; }

        [JsonPropertyName("mission_patches")]
        public List<LlPatch> MissionPatches { get; set; } = new();

        [JsonPropertyName("vid_urls")]
        public List<LlVidUrl> VidUrls { get; set; } = new();
    }

    internal class LlStatus
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("abbrev")]
        public string? Abbrev { get; set; }
    }

    internal class LlRocket
    {
        [JsonPropertyName("configuration")]
        public LlRocketConfiguration? Configuration { get; set; }
    }

    internal class LlRocketConfiguration
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }
    }

    internal class LlMission
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    internal class LlImage
    {
        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }
    }

    internal class LlPatch
    {
        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }
    }

    internal class LlVidUrl
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("priority")]
        public int? Priority { get; set; }
    }
}
