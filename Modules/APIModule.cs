using System.ComponentModel.Design;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DXT_Resultmaker
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("data")] public T Data { get; set; }
    }
    public class ApiException : Exception
    {
        public HttpStatusCode? StatusCode { get; }
        public string RawContent { get; }

        public ApiException(string message, HttpStatusCode? statusCode = null, string rawContent = null)
            : base(message)
        {
            StatusCode = statusCode;
            RawContent = rawContent;
        }
    }
    public class Franchise
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("prefix")] public string Prefix { get; set; }
        [JsonPropertyName("logo")] public string Logo { get; set; }
        [JsonPropertyName("banner")] public string Banner { get; set; }
        [JsonPropertyName("emoji")] public string Emoji { get; set; }
        [JsonPropertyName("decal")] public string Decal { get; set; }
        [JsonPropertyName("discord")] public string Discord { get; set; }
        [JsonPropertyName("twitch")] public string Twitch { get; set; }
        [JsonPropertyName("twitter")] public string Twitter { get; set; }
        [JsonPropertyName("instagram")] public string Instagram { get; set; }
        [JsonPropertyName("tiktok")] public string Tiktok { get; set; }
        [JsonPropertyName("youtube")] public string Youtube { get; set; }
        [JsonPropertyName("active")] public bool? Active { get; set; }
        [JsonPropertyName("hex1")] public string Hex1 { get; set; }
        [JsonPropertyName("hex2")] public string Hex2 { get; set; }
        [JsonPropertyName("hex3")] public string Hex3 { get; set; }
        [JsonPropertyName("teams")] public List<Team> Teams { get; set; } = new();
    }
    public class Team
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("franchise_entry_id")] public int FranchiseEntryId { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("tier_id")] public int TierId { get; set; }
    }
    public class FranchiseTeam
    {
        public Team Team { get; set; }
        public List<SignedUpPlayer> Players { get; set; }
        public int TierId { get; set; }

    }
    public class SignedUpPlayer
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("user_id")] public int? UserId { get; set; }
        [JsonPropertyName("cmv")] public int? Cmv { get; set; }
        [JsonPropertyName("season_id")] public int? SeasonId { get; set; }
        [JsonPropertyName("team_id")] public int? TeamId { get; set; }
        [JsonPropertyName("bonus")] public int? Bonus { get; set; } 
        [JsonPropertyName("captain")] public bool? Captain { get; set; } 
        [JsonPropertyName("status")] public string Status { get; set; }
        [JsonPropertyName("tier_id")] public int? TierId { get; set; }
        [JsonPropertyName("free_agent")] public bool? FreeAgent { get; set; }
        [JsonPropertyName("inactive_reserve")] public bool? InactiveReserve { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }
    }
    public class Match
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("season_id")] public int SeasonId { get; set; }
        [JsonPropertyName("stage")] public string? Stage { get; set; }
        [JsonPropertyName("format")] public string? Format { get; set; }
        [JsonPropertyName("tierId")] public int? TierId { get; set; }
        [JsonPropertyName("week")] public int? Week { get; set; }
        [JsonPropertyName("team1_id")] public int? HomeTeamId { get; set; }
        [JsonPropertyName("team2_id")] public int? AwayTeamId { get; set; }
        [JsonPropertyName("scheduled_date")] public DateTime? ScheduledDate { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("external_id")] public string? ExternalId { get; set; }
    }
    public class User
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("discord_id")] public string DiscordId { get; set; }
        [JsonPropertyName("avatar_url")] public string AvatarUrl { get; set; }
        [JsonPropertyName("location_id")] public int? LocationId { get; set; }
        [JsonPropertyName("referral_id")] public int? ReferralId { get; set; }
        [JsonPropertyName("birthdate")] public DateTime? Birthdate { get; set; }
        [JsonPropertyName("bio")] public string Bio { get; set; }
        [JsonPropertyName("time_added")] public DateTime? TimeAdded { get; set; }
        [JsonPropertyName("last_namechange")] public DateTime? LastNameChange { get; set; }
        [JsonPropertyName("profile_style")] public string ProfileStyle { get; set; }
        [JsonPropertyName("donation_level")] public object DonationLevel { get; set; }
    }
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _baseUrl = baseUrl;
        }
        public void SetBearerToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
        public async Task<ApiResponse<T>> GetAsync<T>(string relativeUrl, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync(_baseUrl + relativeUrl, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new ApiException(response.ReasonPhrase, response.StatusCode, raw);
            try
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(raw, _jsonOptions);
                if (result == null)
                    throw new ApiException("Invalid API response", response.StatusCode, raw);

                return result;
            }
            catch (JsonException ex)
            {
                throw new ApiException($"Failed to deserialize response: {ex.Message}", response.StatusCode, raw);
            }
        }
        public async Task<Franchise> GetFranchiseByNameAsync(string name)
        {
            var allFranchises = await GetAllFranchisesAsync();
            var franchise = allFranchises.Where(x => x.Name == name).FirstOrDefault();
            return franchise;
        }
        public async Task<List<Franchise>> GetAllFranchisesAsync()
        {
            var resp = await GetAsync<List<Franchise>>($"/league-manager/EU/franchises");
            if (!resp.Success)
                throw new ApiException(resp.Message);
            return resp.Data;
        }
        public async Task<SignedUpPlayer> GetPlayerByIdAsync(int id)
        {
            var players = await GetPlayerListAsync();
            var player = players.Find(p => p.Id == id);
            if (player == null)
                throw new ApiException($"Player with ID {id} not found");
            return player;
        }
        public static string MakeTierIdToTiername(int tierId)
        {
            var tier = HelperFactory.Tiers.FirstOrDefault(x => x.Value == tierId);
            if (tier.Equals(default(KeyValuePair<string, int>)))
            {
                Console.WriteLine($"Unknown Tier ID: {tierId}");
                return "Unknown Tier";
            }
            return tier.Key;
        }
        public async Task<List<SignedUpPlayer>> GetPlayerListAsync()
        {
            var resp = await GetAsync<List<SignedUpPlayer>>("/league-manager/EU/players");
            if (!resp.Success)
                throw new ApiException(resp.Message);
            return resp.Data ?? new List<SignedUpPlayer>();
        }
        public async Task<FranchiseTeam> GetFranchiseTeamAsync(string franchiseName, int tierId)
        {
            Team team = new() { Id = -1};
            var playerList = await GetPlayerListAsync();
            var franchise = await GetFranchiseByNameAsync(franchiseName);

            foreach (var tierTeam in franchise.Teams)
            {
                if(tierTeam.TierId == tierId)
                {
                    team = tierTeam;
                    break;
                }
            }
            if(team.Id == -1) return null;
            var playersInTeam = playerList.Where(p => p.TeamId == team.Id).ToList();
            FranchiseTeam franchiseTeam = new()
            {
                Team = team,
                Players = playersInTeam,
                TierId = tierId
            };
            return franchiseTeam;
        }
        public async Task<List<Match>> GetMatchesAsync()
        {
            var resp = await GetAsync<List<Match>>("/series-manager/EU/series");
            if (!resp.Success)
                throw new ApiException(resp.Message);
            return resp.Data ?? new List<Match>();
        }
        public async Task<List<Match>> GetMatchesFromTierTeamAsync(int teamId, int week = -1)
        {
            var matches = await GetMatchesAsync();
            // Filter matches by team ID and optionally by week
            if (week != -1)
            {
                matches = matches.Where(m => m.Week == week).ToList();
            }
            var teamMatches = matches.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId).ToList();
            if (teamMatches.Count == 0)
                throw new ApiException($"No matches found for team ID {teamId}");
            return teamMatches;
        }
        public static Team GetTierteam(int teamId, List<Franchise> allFranchises)
        {
            Team team = new();
            foreach (var franchise in allFranchises)
            {
                if (franchise.Teams is null) continue;

                var teamList = franchise.Teams.Where(x => x.Id == teamId);
                if(teamList.Any())
                {
                    team = teamList.First();
                    break;
                }

            }
            return team;
        }
        public void Dispose() => _httpClient.Dispose();
    }
}
