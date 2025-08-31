using Discord;
using Discord.Interactions;
using System.Globalization;

namespace DXT_Resultmaker.Modules
{

    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("fixtures", "Returns all games that have not been scheduled yet!")]
        public async Task Fixtures(
            [Choice("1", 1), Choice("2", 2), Choice("3", 3), Choice("4", 4), Choice("5", 5), Choice("6", 6), Choice("7", 7), Choice("8", 8), Choice("9", 9)] int week = -1,
            [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchise = HelperFactory.DefaultFranchiseConst)
        {
            await DeferAsync();
            try
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
                var client = new ApiClient(HelperFactory.DefaultAPIUrl);
                var matchesData = await client.GetMatchesAsync();
                var allFranchiseData = await client.GetAllFranchisesAsync();
                var allTeams = allFranchiseData.Select(y => y.Teams);
                var allPlayers = await client.GetPlayerListAsync();
                var franchiseData = allFranchiseData.Where(x => x.Name == franchise).FirstOrDefault();
                if (franchiseData == null || matchesData == null)
                {
                    await FollowupAsync("Looks like an API error... Did not find any franchises or matches.");
                    return;
                }
                string allTierMatches = "";
                foreach (Team teamTier in franchiseData.Teams)
                {
                    string tierMatches = "";
                    var teamMatches = matchesData.Where(x => x.Week == week && (x.HomeTeamId == teamTier.Id || x.AwayTeamId == teamTier.Id)).ToList();
                    if (teamMatches.Count > 0)
                    {
                        tierMatches += $"{HelperFactory.MakeDiscordEmoteString(HelperFactory.Tiers.Where(x => x.Value == teamTier.TierId).FirstOrDefault().Key, HelperFactory.SaveData.EmoteGuild, true)} **{teamTier.Name}** - *{ApiClient.MakeTierIdToTiername(teamTier.TierId)}*\n";
                        foreach (var match in teamMatches)
                        {
                            if (match is null || match.Format != "League Play" || match.HomeTeamId is null || match.AwayTeamId is null) continue;
                            var awayTeam = ApiClient.GetTierteam((int)match.AwayTeamId, allFranchiseData);
                            var homeTeam = ApiClient.GetTierteam((int)match.HomeTeamId, allFranchiseData);
                            if (homeTeam is null || awayTeam is null)
                            {
                                continue; // Skip if teams are not found
                            }
                            // Ermittle das gegnerische Team, das nicht zu franchiseData gehört
                            Team otherTeam = teamTier.Id == homeTeam.Id ? awayTeam : homeTeam;
                            string captainDisplay = "";
                            // Angenommen, dass jedes Team eine Eigenschaft 'Captain' enthält
                            if (allTeams.Any())
                            {
                                var captain = allPlayers.Where(y => y.TeamId == otherTeam.Id && y.Captain == true).FirstOrDefault();
                                captainDisplay = $" - Captain: {captain?.User.Name ?? "N/V"}";
                            }
                            string currentMatchDate = "> " + HelperFactory.ToDiscordTimestamp(match.ScheduledDate);
                            var discordEmoteStringHome = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == homeTeam.FranchiseEntryId).First().Prefix, HelperFactory.SaveData.EmoteGuild);
                            var discordEmoteStringAway = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == awayTeam.FranchiseEntryId).First().Prefix, HelperFactory.SaveData.EmoteGuild);
                            tierMatches += $"> {discordEmoteStringHome} {homeTeam.Name} vs {awayTeam.Name} {discordEmoteStringAway}\n> `{match.ExternalId}` \n {currentMatchDate}";
                            Console.WriteLine(match.ScheduledDate);
                        }
                        allTierMatches += tierMatches + "\n";
                    }
                }

                var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault()?.Nickname;
                userName ??= Context.User.GlobalName;

                var emoteUrl = HelperFactory.MakeDiscordEmoteString(franchiseData.Prefix, HelperFactory.SaveData.EmoteGuild);
                var bannerUrl = franchiseData.Banner ?? "";
                var logoUrl = franchiseData.Logo ?? "";
                int hexValue = int.Parse(franchiseData.Hex1.TrimStart('#'), NumberStyles.HexNumber);

                Discord.Color color = new((uint)hexValue);
                var emb = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"{emoteUrl} Fixtures of week " + week)
                .WithFooter("Requested By " + userName, Context.User.GetAvatarUrl())
                .WithDescription(allTierMatches)
                .WithCurrentTimestamp()
                .WithImageUrl(bannerUrl)
                .WithThumbnailUrl(logoUrl);

                await FollowupAsync(embed: emb.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("Something went wrong.");
            }
        }
    }
}
