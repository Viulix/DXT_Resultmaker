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
            [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchise = HelperFactory.defaultFranchise)
        {
            await DeferAsync();
            try
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
                        allTierMatches += tierMatches + "\n";
                    }
                }

                var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault()?.Nickname;
                userName ??= Context.User.GlobalName;

                var emoteUrl = HelperFactory.MakeDiscordEmoteString(franchiseData.Prefix, HelperFactory.emote_guild);
                var bannerUrl = franchiseData.Banner ?? "";
                var logoUrl = franchiseData.Logo ?? "";
                int hexValue = int.Parse(franchiseData.Hex1.TrimStart('#'), NumberStyles.HexNumber);

                Discord.Color color = new Discord.Color((uint)hexValue);
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
