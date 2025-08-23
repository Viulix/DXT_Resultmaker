using Discord;
using DXT_Resultmaker.Modules;
using System.Globalization;

namespace DXT_Resultmaker
{
    public static class HelperFactory
    {
        // Konstante Defaults
        public static readonly Dictionary<string, int> Tiers = new Dictionary<string, int>
        {
            { "Master", 36 },
            { "Elite", 37 },
            { "Rival", 38 },
            { "Challenger", 39 },
            { "Prospect", 40 },
            { "Academy", 41 }
        };
        internal static Dictionary<int, uint> TierColors = new()
        {
                { 36, 7262719 }, // Master
                { 37, 0x2ccc6f }, // Elite
                { 38, 0xf1c40d }, // Rival
                { 39, 0xe67c20 }, // Challenger
                { 40, 0xe06564 }, // Prospect
                { 41, 0xb270bb }  // Academy
            };
        internal static List<string> Franchises = [];
        public static readonly DateTime SeasonStart = new(2025, 8, 18);

        // Constants
        public const string DefaultAPIUrl = "https://api.rsc-community.com/v2";
        public const string DefaultFranchiseConst = "Dexterity";
        public const int SeasonCalenderWeek = 33;

        // Path
        public static readonly string SaveDataPath = "./savedata.json";

        public static SaveData SaveData { get; private set; } = new();
        public static void LoadSaveData()
        {
            if (!File.Exists(SaveDataPath))
            {
                // Datei anlegen mit Defaults
                SaveData = new SaveData();
                Save();
            }
            else
            {
                string json = File.ReadAllText(SaveDataPath);
                SaveData? data = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(json);

                SaveData = data ?? new SaveData();
            }
        }
        public static void Save()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(SaveData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(SaveDataPath, json);
        }

        public static void AddAdmin(ulong userId)
        {
            if (!SaveData.Admins.Contains(userId))
                SaveData.Admins.Add(userId);
            AdminModule.Admins = SaveData.Admins;
            Save();
        }

        public static void RemoveAdmin(ulong userId)
        {
            if (SaveData.Admins.Contains(userId))
                SaveData.Admins.Remove(userId);
            AdminModule.Admins = SaveData.Admins;
            Save();
        }

        public static void ReplaceRole(int tierIndex, ulong roleId)
        {
            while (SaveData.RoleIds.Count <= tierIndex)
                SaveData.RoleIds.Add(0); 

            SaveData.RoleIds[tierIndex] = roleId;
            AdminModule.TierDiscordRoleId = SaveData.RoleIds;
            Save();
        }

        public static ulong? GetRole(int tierIndex)
        {
            if (tierIndex < 0 || tierIndex >= SaveData.RoleIds.Count)
                return null;

            return SaveData.RoleIds[tierIndex];
        }

        public static void RemoveRole(ulong roleId)
        {
            if (SaveData.RoleIds.Contains(roleId))
                SaveData.RoleIds.Remove(roleId);
            AdminModule.TierDiscordRoleId = SaveData.RoleIds;
            Save();
        }

        public static void SetDefaultFranchise(string franchise)
        {
            SaveData.DefaultFranchise = franchise;
            Save();
        }

        public static void SetFranchises(List<Franchise> franchises)
        {
            SaveData.Franchises = franchises;
            Save();
        }

        public static void SetDefaultAPIUrl(string url)
        {
            SaveData.DefaultAPIUrl = url;
            Save();
        }

        public static void SetEmoteGuild(ulong emoteGuild)
        {
            SaveData.EmoteGuild = emoteGuild;
            Save();
        }
        public static void AddGuild(ulong guildId)
        {
            SaveData.GuildIds.Add(guildId);
            Save();
        }
        public static void RemoveGuild(ulong guildId)
        {
            SaveData.GuildIds.Remove(guildId);
            Save();
        }

        public static void SetSeasonStart(DateTime start)
        {
            SaveData.SeasonStart = start;
            Save();
        }

        public static void SetSeasonCalenderWeek(int week)
        {
            SaveData.SeasonCalenderWeek = week;
            Save();
        }

        public static void SetMainColor(Discord.Color color)
        {
            SaveData.MainColor = color;
            Save();
        }

        public static void SetTierColor(int tierIndex, Discord.Color color)
        {
            if (SaveData.TierColors.ContainsKey(tierIndex))
                SaveData.TierColors[tierIndex] = color;
            else
                SaveData.TierColors.Add(tierIndex, color);
            Save();
        }

        public static string MakeDiscordEmoteString(string abbrevation, ulong guildId, bool tierIcon = false)
        {
            try
            {
                if (tierIcon) abbrevation = "RSC_" + abbrevation;

                List<Discord.GuildEmote> emotes = CommandHandler._client.Guilds
                    .Where(x => x.Id == guildId || x.Id == 690948036540760147) // Alle passenden Guilds filtern
                    .SelectMany(g => g.Emotes) // Alle Emotes aus den gefundenen Guilds zusammenfügen
                    .ToList(); // In eine Liste umwandeln


                string msg = "N/V";
            foreach (var emote in emotes)
            {
                if (emote.Name == abbrevation)
                {
                    msg = "<:" + abbrevation + ":" + emote.Id + ">";
                    break;
                }
            }
            return msg;
            }
            catch  (Exception ex)
            {
                Console.WriteLine(ex);
                return "N/V";
            }
        }
        public static string MakeDiscordEmoteUrl(string abbrevation, ulong guildId)
        {
            try
            {

                List<Discord.GuildEmote> emotes = CommandHandler._client.Guilds
                    .Where(x => x.Id == guildId || x.Id == 690948036540760147) // Alle passenden Guilds filtern
                    .SelectMany(g => g.Emotes) // Alle Emotes aus den gefundenen Guilds zusammenfügen
                    .ToList(); // In eine Liste umwandeln


                string msg = "N/V";
                foreach (var emote in emotes)
                {
                    if (emote.Name == abbrevation)
                    {
                        msg = emote.Url;
                        break;
                    }
                }
                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "N/V";
            }
        }
        public static List<string> SplitAndSortResultString(string raw_result) { 
            var result = new List<string>();
            var splitted_raw_Result = raw_result.Split(':').ToList();
            splitted_raw_Result = splitted_raw_Result.Where((item, index) => index % 2 != 0).ToList();
            return splitted_raw_Result;
        }
        public static Discord.Color GetTierColor(int tier)
        {
            return TierColors.TryGetValue(tier, out var color) ? new Discord.Color(color) : new Discord.Color(0x000000);
        }
        public static Discord.EmbedBuilder DefaultEmbed(Discord.IUser author)
        {
            var embed = new EmbedBuilder() { }
                .WithCurrentTimestamp()
                .WithColor(SaveData.MainColor);
            return embed;
                
        }
        public static string ToDiscordTimestamp(DateTime? date, string format = "f", string fallback = "*Not Scheduled*")
        {
            if (date == null)
                return $"{fallback}\n";

            // Zeitzone Berlin (beachtet Sommer-/Winterzeit automatisch)
            var berlinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            var berlinTime = TimeZoneInfo.ConvertTime(date.Value, berlinTimeZone);

            long unixSeconds = ((DateTimeOffset)berlinTime).ToUnixTimeSeconds();

            return $"<t:{unixSeconds}:{format}>\n";
        }
        public static async Task<string> MakeFixtureMessage(int week, string franchise = DefaultFranchiseConst)
        {
            if (week == -1)
            {
                week = (ISOWeek.GetWeekOfYear(TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))) - HelperFactory.SeasonCalenderWeek);

                if (week > 9)
                {
                    week = 9;
                }
            }
            // Make API call to get the matches and the franchise
            var client = new ApiClient(DefaultAPIUrl);
            var matchesData = await client.GetMatchesAsync();
            var allFranchiseData = await client.GetAllFranchisesAsync();
            var franchiseData = allFranchiseData.Where(x => x.Name == franchise).FirstOrDefault();
            if (franchiseData == null || matchesData == null)
            {
                return null;
            }
            string allTierMatches = "";
            foreach (Team teamTier in franchiseData.Teams)
            {
                string tierMatches = "";
                var teamMatches = matchesData.Where(x => x.Week == week && x.Format == "League Play" && (x.HomeTeamId == teamTier.Id || x.AwayTeamId == teamTier.Id)).ToList();
                if (teamMatches.Count > 0)
                {
                    tierMatches += $"{MakeDiscordEmoteString(Tiers.Where(x =>x.Value == teamTier.TierId).FirstOrDefault().Key, SaveData.EmoteGuild, true)} **{teamTier.Name}** - *{ApiClient.MakeTierIdToTiername(teamTier.TierId)}*\n";
                    foreach (var match in teamMatches)
                    {
                        if (match is null || match.Format != "League Play") continue;
                        var awayTeam = ApiClient.GetTierteam((int)match.AwayTeamId, allFranchiseData);
                        var homeTeam = ApiClient.GetTierteam((int)match.HomeTeamId, allFranchiseData);
                        if (homeTeam is null || awayTeam is null)
                        {
                            continue; // Skip if teams are not found
                        }
                        string currentMatchDate = "> " + ToDiscordTimestamp(match.ScheduledDate);
                        var discordEmoteStringHome = MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == homeTeam.FranchiseEntryId).First().Prefix, SaveData.EmoteGuild);
                        var discordEmoteStringAway = MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == awayTeam.FranchiseEntryId).First().Prefix, SaveData.EmoteGuild);
                        tierMatches += $"> {discordEmoteStringHome} {homeTeam.Name} vs {awayTeam.Name} {discordEmoteStringAway}\n> `{match.ExternalId}` \n {currentMatchDate}";
                    }
                    allTierMatches += tierMatches + "ㅤ\n";
                }
            }
            return allTierMatches;
        }
    }
}
