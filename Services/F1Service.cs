using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace MyPortfolio.Services
{
    public class F1Service
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<F1Service> _logger;

        private static List<F1DriverStanding>? _cachedDriverStandings;
        private static List<F1ConstructorStanding>? _cachedConstructorStandings;
        private static F1RaceResult? _cachedLastRace;
        private static List<F1ScheduleRace>? _cachedSchedule;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public F1Service(HttpClient httpClient, ILogger<F1Service> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private bool CacheIsValid() =>
            _lastCacheTime != DateTime.MinValue && DateTime.UtcNow - _lastCacheTime < CacheDuration;

        public async Task<List<F1DriverStanding>> GetDriverStandingsAsync()
        {
            if (CacheIsValid() && _cachedDriverStandings != null) return _cachedDriverStandings;
            try
            {
                var root = await _httpClient.GetFromJsonAsync<F1DriverStandingsRoot>(
                    "https://api.jolpi.ca/ergast/f1/current/driverStandings.json");
                _cachedDriverStandings = root?.MRData?.StandingsTable?.StandingsLists?
                    .FirstOrDefault()?.DriverStandings ?? new();
                _lastCacheTime = DateTime.UtcNow;
                return _cachedDriverStandings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching F1 driver standings");
                return _cachedDriverStandings ?? new();
            }
        }

        public async Task<List<F1ConstructorStanding>> GetConstructorStandingsAsync()
        {
            if (CacheIsValid() && _cachedConstructorStandings != null) return _cachedConstructorStandings;
            try
            {
                var root = await _httpClient.GetFromJsonAsync<F1ConstructorStandingsRoot>(
                    "https://api.jolpi.ca/ergast/f1/current/constructorStandings.json");
                _cachedConstructorStandings = root?.MRData?.StandingsTable?.StandingsLists?
                    .FirstOrDefault()?.ConstructorStandings ?? new();
                _lastCacheTime = DateTime.UtcNow;
                return _cachedConstructorStandings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching F1 constructor standings");
                return _cachedConstructorStandings ?? new();
            }
        }

        public async Task<F1RaceResult?> GetLastRaceAsync()
        {
            if (CacheIsValid() && _cachedLastRace != null) return _cachedLastRace;
            try
            {
                var root = await _httpClient.GetFromJsonAsync<F1RaceResultRoot>(
                    "https://api.jolpi.ca/ergast/f1/current/last/results.json");
                _cachedLastRace = root?.MRData?.RaceTable?.Races?.FirstOrDefault();
                _lastCacheTime = DateTime.UtcNow;
                return _cachedLastRace;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching F1 last race results");
                return _cachedLastRace;
            }
        }

        public async Task<List<F1ScheduleRace>> GetScheduleAsync()
        {
            if (CacheIsValid() && _cachedSchedule != null) return _cachedSchedule;
            try
            {
                var root = await _httpClient.GetFromJsonAsync<F1ScheduleRoot>(
                    "https://api.jolpi.ca/ergast/f1/current.json");
                _cachedSchedule = root?.MRData?.RaceTable?.Races ?? new();
                _lastCacheTime = DateTime.UtcNow;
                return _cachedSchedule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching F1 schedule");
                return _cachedSchedule ?? new();
            }
        }
    }

    // ── Driver Standings ─────────────────────────────────────────────────────

    public class F1DriverStandingsRoot
    {
        [JsonPropertyName("MRData")]
        public F1DriverStandingsMRData? MRData { get; set; }
    }

    public class F1DriverStandingsMRData
    {
        [JsonPropertyName("StandingsTable")]
        public F1DriverStandingsTable? StandingsTable { get; set; }
    }

    public class F1DriverStandingsTable
    {
        [JsonPropertyName("StandingsLists")]
        public List<F1DriverStandingsList>? StandingsLists { get; set; }
    }

    public class F1DriverStandingsList
    {
        [JsonPropertyName("season")]
        public string Season { get; set; } = "";

        [JsonPropertyName("round")]
        public string Round { get; set; } = "";

        [JsonPropertyName("DriverStandings")]
        public List<F1DriverStanding> DriverStandings { get; set; } = new();
    }

    public class F1DriverStanding
    {
        [JsonPropertyName("position")]
        public string Position { get; set; } = "";

        [JsonPropertyName("points")]
        public string Points { get; set; } = "";

        [JsonPropertyName("wins")]
        public string Wins { get; set; } = "";

        [JsonPropertyName("Driver")]
        public F1Driver? Driver { get; set; }

        [JsonPropertyName("Constructors")]
        public List<F1Constructor>? Constructors { get; set; }

        public string TeamName => Constructors?.FirstOrDefault()?.Name ?? "";
    }

    public class F1Driver
    {
        [JsonPropertyName("driverId")]
        public string DriverId { get; set; } = "";

        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("givenName")]
        public string GivenName { get; set; } = "";

        [JsonPropertyName("familyName")]
        public string FamilyName { get; set; } = "";

        [JsonPropertyName("nationality")]
        public string Nationality { get; set; } = "";

        public string FullName => $"{GivenName} {FamilyName}";
    }

    // ── Constructor Standings ────────────────────────────────────────────────

    public class F1ConstructorStandingsRoot
    {
        [JsonPropertyName("MRData")]
        public F1ConstructorStandingsMRData? MRData { get; set; }
    }

    public class F1ConstructorStandingsMRData
    {
        [JsonPropertyName("StandingsTable")]
        public F1ConstructorStandingsTable? StandingsTable { get; set; }
    }

    public class F1ConstructorStandingsTable
    {
        [JsonPropertyName("StandingsLists")]
        public List<F1ConstructorStandingsList>? StandingsLists { get; set; }
    }

    public class F1ConstructorStandingsList
    {
        [JsonPropertyName("season")]
        public string Season { get; set; } = "";

        [JsonPropertyName("ConstructorStandings")]
        public List<F1ConstructorStanding> ConstructorStandings { get; set; } = new();
    }

    public class F1ConstructorStanding
    {
        [JsonPropertyName("position")]
        public string Position { get; set; } = "";

        [JsonPropertyName("points")]
        public string Points { get; set; } = "";

        [JsonPropertyName("wins")]
        public string Wins { get; set; } = "";

        [JsonPropertyName("Constructor")]
        public F1Constructor? Constructor { get; set; }
    }

    public class F1Constructor
    {
        [JsonPropertyName("constructorId")]
        public string ConstructorId { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("nationality")]
        public string Nationality { get; set; } = "";
    }

    // ── Race Result ──────────────────────────────────────────────────────────

    public class F1RaceResultRoot
    {
        [JsonPropertyName("MRData")]
        public F1RaceResultMRData? MRData { get; set; }
    }

    public class F1RaceResultMRData
    {
        [JsonPropertyName("RaceTable")]
        public F1RaceResultTable? RaceTable { get; set; }
    }

    public class F1RaceResultTable
    {
        [JsonPropertyName("Races")]
        public List<F1RaceResult>? Races { get; set; }
    }

    public class F1RaceResult
    {
        [JsonPropertyName("raceName")]
        public string RaceName { get; set; } = "";

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("Circuit")]
        public F1Circuit? Circuit { get; set; }

        [JsonPropertyName("Results")]
        public List<F1DriverResult> Results { get; set; } = new();
    }

    public class F1DriverResult
    {
        [JsonPropertyName("position")]
        public string Position { get; set; } = "";

        [JsonPropertyName("points")]
        public string Points { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("Driver")]
        public F1Driver? Driver { get; set; }

        [JsonPropertyName("Constructor")]
        public F1Constructor? Constructor { get; set; }

        [JsonPropertyName("Time")]
        public F1LapTime? Time { get; set; }

        [JsonPropertyName("FastestLap")]
        public F1FastestLap? FastestLap { get; set; }

        public bool HasFastestLap => FastestLap?.Rank == "1";
    }

    public class F1LapTime
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = "";
    }

    public class F1FastestLap
    {
        [JsonPropertyName("rank")]
        public string Rank { get; set; } = "";
    }

    public class F1Circuit
    {
        [JsonPropertyName("circuitName")]
        public string CircuitName { get; set; } = "";

        [JsonPropertyName("Location")]
        public F1Location? Location { get; set; }
    }

    public class F1Location
    {
        [JsonPropertyName("locality")]
        public string Locality { get; set; } = "";

        [JsonPropertyName("country")]
        public string Country { get; set; } = "";
    }

    // ── Season Schedule ──────────────────────────────────────────────────────

    public class F1ScheduleRoot
    {
        [JsonPropertyName("MRData")]
        public F1ScheduleMRData? MRData { get; set; }
    }

    public class F1ScheduleMRData
    {
        [JsonPropertyName("RaceTable")]
        public F1ScheduleTable? RaceTable { get; set; }
    }

    public class F1ScheduleTable
    {
        [JsonPropertyName("season")]
        public string Season { get; set; } = "";

        [JsonPropertyName("Races")]
        public List<F1ScheduleRace>? Races { get; set; }
    }

    public class F1ScheduleRace
    {
        [JsonPropertyName("round")]
        public string Round { get; set; } = "";

        [JsonPropertyName("raceName")]
        public string RaceName { get; set; } = "";

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("time")]
        public string Time { get; set; } = "";

        [JsonPropertyName("Circuit")]
        public F1Circuit? Circuit { get; set; }

        public DateTime RaceDateTime
        {
            get
            {
                if (!string.IsNullOrEmpty(Time) && DateTime.TryParse($"{Date}T{Time}", null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var dtWithTime))
                    return dtWithTime.ToLocalTime();
                return DateTime.TryParse(Date, out var d) ? d : DateTime.MinValue;
            }
        }

        public bool IsPast => RaceDateTime != DateTime.MinValue && RaceDateTime < DateTime.Now;
    }
}
