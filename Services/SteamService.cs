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
        private const string CacheKey = "steam:summary";

        public SteamService(HttpClient httpClient, ILogger<SteamService> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<SteamSummary> GetSummaryAsync()
        {
            if (_cache.TryGetValue(CacheKey, out SteamSummary? cached) && cached != null)
                return cached;

            try
            {
                var apiKey = _configuration["Steam:ApiKey"];
                var steamId = _configuration["Steam:UserId"];

                var gamesResponse = await _httpClient.GetFromJsonAsync<SteamOwnedGamesResponse>(
                    $"{ApiBase}/IPlayerService/GetOwnedGames/v1/?key={apiKey}&steamid={steamId}&include_appinfo=true&include_played_free_games=true");

                var allGames = gamesResponse?.Response?.Games ?? [];
                var totalGames = gamesResponse?.Response?.GameCount ?? allGames.Count;
                var totalHours = (int)Math.Round(allGames.Sum(g => g.PlaytimeForever) / 60.0);

                var topGames = allGames
                    .Where(g => g.PlaytimeForever > 0)
                    .OrderByDescending(g => g.PlaytimeForever)
                    .Take(15)
                    .ToList();

                var recentlyPlayed = allGames
                    .Where(g => g.Playtime2Weeks > 0)
                    .OrderByDescending(g => g.Playtime2Weeks)
                    .ToList();

                SteamProfile? profile = null;
                try
                {
                    var summaryTask = _httpClient.GetFromJsonAsync<SteamPlayerSummariesResponse>(
                        $"{ApiBase}/ISteamUser/GetPlayerSummaries/v2/?key={apiKey}&steamids={steamId}");
                    var levelTask = _httpClient.GetFromJsonAsync<SteamLevelResponse>(
                        $"{ApiBase}/IPlayerService/GetSteamLevel/v1/?key={apiKey}&steamid={steamId}");

                    await Task.WhenAll(summaryTask, levelTask);

                    var player = summaryTask.Result?.Response?.Players?.FirstOrDefault();
                    var level = levelTask.Result?.Response?.PlayerLevel ?? 0;

                    if (player != null)
                        profile = new SteamProfile(player.PersonaName, player.AvatarFull, level);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not fetch Steam profile");
                }

                var summary = new SteamSummary(profile, topGames, recentlyPlayed, totalGames, totalHours);
                _cache.Set(CacheKey, summary, TimeSpan.FromDays(7));
                _logger.LogInformation("Cached Steam summary: {Games} games, {Hours} hrs", totalGames, totalHours);
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Steam data");
                return new SteamSummary(null, [], [], 0, 0);
            }
        }
    }

    public record SteamSummary(
        SteamProfile? Profile,
        List<SteamGame> TopGames,
        List<SteamGame> RecentlyPlayed,
        int TotalGames,
        int TotalHours);

    public record SteamProfile(string Name, string AvatarUrl, int Level);

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
        public double RecentHours => Math.Round(Playtime2Weeks / 60.0, 1);
        public string HeaderImageUrl => $"https://cdn.cloudflare.steamstatic.com/steam/apps/{AppId}/header.jpg";
    }

    public class SteamPlayerSummariesResponse
    {
        [JsonPropertyName("response")]
        public SteamPlayersContainer? Response { get; set; }
    }

    public class SteamPlayersContainer
    {
        [JsonPropertyName("players")]
        public List<SteamPlayer>? Players { get; set; }
    }

    public class SteamPlayer
    {
        [JsonPropertyName("personaname")]
        public string PersonaName { get; set; } = "";

        [JsonPropertyName("avatarfull")]
        public string AvatarFull { get; set; } = "";
    }

    public class SteamLevelResponse
    {
        [JsonPropertyName("response")]
        public SteamLevelContainer? Response { get; set; }
    }

    public class SteamLevelContainer
    {
        [JsonPropertyName("player_level")]
        public int PlayerLevel { get; set; }
    }
}
