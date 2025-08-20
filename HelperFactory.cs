using Discord;
using System.Globalization;
using System.Linq;

namespace DXT_Resultmaker
{
    public static class HelperFactory
    {
        public static string[] tiers = { "Master", "Elite", "Rival", "Challenger", "Prospect", "Academy" };
        public static ulong emote_guild = 1093943074746531910;
        public const string defaultFranchise = "Dexterity";
        public const string defaultAPIUrl = "https://api.rsc-community.com/v2";
        public static Discord.Color dxtColor = new(0x9ee345);
        public static string userDataPath = "./userData.json";
        public static DateTime seasonStart = new(2025, 8, 18);
        public static int seasonCalenderWeek = 33;
        public static List<Franchise> Franchises = new();
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
            Discord.Color color = new Discord.Color(000000);

            switch ((Tiers)tier)
            {
                case Tiers.Master:
                    color = new Discord.Color(0x6ed1ff);
                    break;
                case Tiers.Elite:
                    color = new Discord.Color(0x2ccc6f);
                    break;
                case Tiers.Rival:
                    color = new Discord.Color(0xf1c40d);
                    break;
                case Tiers.Challenger:
                    color = new Discord.Color(0xe67c20);
                    break;
                case Tiers.Prospect:
                    color = new Discord.Color(0xe06564);
                    break;
                case Tiers.Academy:
                    color = new Discord.Color(0xb270bb);
                    break;
            }
            return color;
        }
        public static Discord.EmbedBuilder DefaultEmbed(Discord.IUser author)
        {
            var embed = new EmbedBuilder() { }
                .WithCurrentTimestamp()
                .WithColor(dxtColor);
            return embed;
                
        }
        public static string ToDiscordTimestamp(DateTime? date, string format = "f", string fallback = "*Not Scheduled*")
        {
            if (date == null)
                return $"> {fallback}\n";

            // Zeitzone Berlin (beachtet Sommer-/Winterzeit automatisch)
            var berlinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            var berlinTime = TimeZoneInfo.ConvertTime(date.Value, berlinTimeZone);

            long unixSeconds = ((DateTimeOffset)berlinTime).ToUnixTimeSeconds();

            return $"<t:{unixSeconds}:{format}>\n";
        }
        public static async Task<string> MakeFixtureMessage(int week, string franchise = defaultFranchise)
        {
            if (week == -1)
            {
                week = (ISOWeek.GetWeekOfYear(TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))) - HelperFactory.seasonCalenderWeek);

                if (week > 9)
                {
                    week = 9;
                }
            }
            // Make API call to get the matches and the franchise
            var client = new ApiClient(HelperFactory.defaultAPIUrl);
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
                    tierMatches += $"{HelperFactory.MakeDiscordEmoteString(HelperFactory.tiers[teamTier.TierId - 36], HelperFactory.emote_guild, true)} **{teamTier.Name}** - *{ApiClient.MakeTierIdToTiername(teamTier.TierId)}*\n";
                    foreach (var match in teamMatches)
                    {
                        if (match is null || match.Format != "League Play") continue;
                        var awayTeam = ApiClient.GetTierteam((int)match.AwayTeamId, allFranchiseData);
                        var homeTeam = ApiClient.GetTierteam((int)match.HomeTeamId, allFranchiseData);
                        if (homeTeam is null || awayTeam is null)
                        {
                            continue; // Skip if teams are not found
                        }
                        string currentMatchDate = "> " + HelperFactory.ToDiscordTimestamp(match.ScheduledDate);
                        var discordEmoteStringHome = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == homeTeam.FranchiseEntryId).First().Prefix, HelperFactory.emote_guild);
                        var discordEmoteStringAway = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == awayTeam.FranchiseEntryId).First().Prefix, HelperFactory.emote_guild);
                        tierMatches += $"> {discordEmoteStringHome} {homeTeam.Name} vs {awayTeam.Name} {discordEmoteStringAway}\n> `{match.ExternalId}` \n {currentMatchDate}";
                    }
                    allTierMatches += tierMatches + "ㅤ\n";
                }
            }
            return allTierMatches;
        }


    }
}
