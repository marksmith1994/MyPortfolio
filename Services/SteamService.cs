using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace MyPortfolio.Services
{
    public class SteamService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SteamService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        private const string ApiBase = "https://api.steampowered.com";
        private const string CacheKey = "steam:games";

        public SteamService(HttpClient httpClient, ILogger<SteamService> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<List<SteamGame>> GetTopGamesAsync(int count = 15)
        {
            if (_cache.TryGetValue(CacheKey, out List<SteamGame>? cached) && cached != null)
                return cached;

            try
            {
                var apiKey = _configuration["Steam:ApiKey"];
                var steamId = _configuration["Steam:UserId"];

                var response = await _httpClient.GetFromJsonAsync<SteamOwnedGamesResponse>(
                    $"{ApiBase}/IPlayerService/GetOwnedGames/v1/?key={apiKey}&steamid={steamId}&include_appinfo=true&include_played_free_games=true");

                var games = response?.Response?.Games ?? [];

                var top = games
                    .Where(g => g.PlaytimeForever > 0)
                    .OrderByDescending(g => g.PlaytimeForever)
                    .Take(count)
                    .ToList();

                _cache.Set(CacheKey, top, TimeSpan.FromHours(24));
                _logger.LogInformation("Cached {Count} Steam games", top.Count);
                return top;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Steam games");
                return [];
            }
        }
    }

    public class SteamOwnedGamesResponse
    {
        [JsonPropertyName("response")]
        public SteamGamesContainer? Response { get; set; }
    }

    public class SteamGamesContainer
    {
        [JsonPropertyName("game_count")]
        public int GameCount { get; set; }

        [JsonPropertyName("games")]
        public List<SteamGame>? Games { get; set; }
    }

    public class SteamGame
    {
        [JsonPropertyName("appid")]
        public int AppId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("playtime_forever")]
        public int PlaytimeForever { get; set; }

        [JsonPropertyName("playtime_2weeks")]
        public int Playtime2Weeks { get; set; }

        public double HoursPlayed => Math.Round(PlaytimeForever / 60.0, 1);
        public bool RecentlyPlayed => Playtime2Weeks > 0;
        public string HeaderImageUrl => $"https://cdn.cloudflare.steamstatic.com/steam/apps/{AppId}/header.jpg";
    }
}
