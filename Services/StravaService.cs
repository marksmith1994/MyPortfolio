using System.Net.Http.Json;
using System.Text.Json;

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
            var athleteId = _configuration["Strava:AthleteId"];

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"https://www.strava.com/api/v3/athletes/{athleteId}/activities?per_page=10");
            response.EnsureSuccessStatusCode();

            var activities = await response.Content.ReadFromJsonAsync<List<StravaActivity>>();
            return activities ?? new List<StravaActivity>();
        }
    }

    public class StravaActivity
    {
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public int MovingTime { get; set; }
        public double AverageSpeed { get; set; }
        public double AverageHeartrate { get; set; }
        public double AverageCadence { get; set; }
        public DateTime StartDate { get; set; }
        public string Type { get; set; } = string.Empty;
        public string MapPolyline { get; set; } = string.Empty;
    }
} 