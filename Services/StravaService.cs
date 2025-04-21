using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyPortfolio.Services
{
    public class StravaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public StravaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<StravaActivity>> GetRecentActivitiesAsync()
        {
            var accessToken = _configuration["Strava:AccessToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("https://www.strava.com/api/v3/athlete/activities?per_page=50");
            response.EnsureSuccessStatusCode();

            var activities = await response.Content.ReadFromJsonAsync<List<StravaActivity>>();
            return activities ?? new List<StravaActivity>();
        }

        public async Task<List<StravaActivity>> GetAllActivitiesAsync()
        {
            var accessToken = _configuration["Strava:AccessToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Strava allows up to 200 per page
            var response = await _httpClient.GetAsync("https://www.strava.com/api/v3/athlete/activities?per_page=200");
            response.EnsureSuccessStatusCode();

            var activities = await response.Content.ReadFromJsonAsync<List<StravaActivity>>();
            return activities ?? new List<StravaActivity>();
        }
    }

    public class StravaActivity
    {
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        [JsonPropertyName("moving_time")]
        public int MovingTime { get; set; }
        [JsonPropertyName("elapsed_time")]
        public int ElapsedTime { get; set; }
        [JsonPropertyName("average_speed")]
        public double AverageSpeed { get; set; }
        [JsonPropertyName("max_speed")]
        public double MaxSpeed { get; set; }
        [JsonPropertyName("average_heartrate")]
        public double AverageHeartrate { get; set; }
        [JsonPropertyName("average_cadence")]
        public double AverageCadence { get; set; }
        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }
        [JsonPropertyName("start_date_local")]
        public DateTime StartDateLocal { get; set; }
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("sport_type")]
        public string SportType { get; set; } = string.Empty;
        [JsonPropertyName("total_elevation_gain")]
        public double TotalElevationGain { get; set; }
        public string Timezone { get; set; } = string.Empty;
        [JsonPropertyName("utc_offset")]
        public double UtcOffset { get; set; }
        [JsonPropertyName("map")]
        public StravaMap Map { get; set; } = new StravaMap();
    }

    public class StravaMap
    {
        public string Id { get; set; } = string.Empty;
        public string Polyline { get; set; } = string.Empty;
        [JsonPropertyName("resource_state")]
        public int ResourceState { get; set; }
    }
} 