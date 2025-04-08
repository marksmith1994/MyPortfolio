namespace MyPortfolio.Models
{
    public class GitHubRepository
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string HtmlUrl { get; set; }
        public string Language { get; set; }
        public int StargazersCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Topics { get; set; }

        public GitHubRepository()
        {
            Name = string.Empty;
            Description = string.Empty;
            HtmlUrl = string.Empty;
            Language = string.Empty;
            Topics = new List<string>();
        }
    }
} 