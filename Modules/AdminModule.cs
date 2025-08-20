using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using System.Globalization;
namespace DXT_Resultmaker.Modules
{
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        public static List<ulong> admins;
        private static List<ulong> TierDiscordRoleId = new() // These are the IDs of the roles that are used to determine the tier of a user.
        {
            796892694261399613,
            1200155287533932656,
            796892696408358962,
            796892697570050059,
            796895790433697793,
            1395460550690996365
        };


        [SlashCommand("makeadmin", "Only for creator of the bot.")]
        public async Task MakeAdmin(IUser user)
        {
            if (Context.User.Id == 300570127869411329)
            {
                Console.WriteLine(admins.Count);
                var current_admins = admins;
                SaveData current_Data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("./bot.json"));

                if (current_admins.Contains(user.Id))
                {
                    await RespondAsync("This user is already an admin.", ephemeral: true);
                    return;
                }
                var newAdmins = current_admins.Append(user.Id);
                current_Data.admins = newAdmins.ToList();
                admins = newAdmins.ToList();
                string json = JsonConvert.SerializeObject(current_Data, Formatting.Indented);

                File.WriteAllText("./bot.json", json);
                await RespondAsync("Worked.", ephemeral: true);
            }
            else
            {
                await RespondAsync("You are not goat enough to pull this.", ephemeral: true);
            }
        }
        [SlashCommand("removeadmin", "Only for the creator of the bot.")]
        public async Task RemoveAdmin(IUser user)
        {
            if (Context.User.Id == 300570127869411329)
            {
                var current_admins = admins;
                SaveData current_Data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("./bot.json"));
                if (!current_admins.Contains(user.Id))
                {
                    await RespondAsync("No user with that ID found.", ephemeral: true);
                    return;
                }

                var newAdmins = current_admins.Where(x => x != user.Id);
                Console.WriteLine(current_admins.Count);
                current_Data.admins = newAdmins.ToList();
                admins = newAdmins.ToList();
                string json = JsonConvert.SerializeObject(current_Data, Formatting.Indented);

                File.WriteAllText("./bot.json", json);
                await RespondAsync("Worked.", ephemeral: true);
            }
            else
            {
                await RespondAsync("You are not goat enough to pull this.", ephemeral: true);
            }
        }
        public static bool IsAdmin(ulong userid)
        {
            if (admins.Contains(userid)) return true;
            else return false;
        }

        [SlashCommand("testthis", "Developer Command. Admin only!")]
        public async Task TestThis()
        {
            if (IsAdmin(Context.User.Id))
            {
                await DeferAsync(ephemeral: true);
                var client = new ApiClient("https://api.dev.rsc-community.com/v2");

                try
                {
                    var franchis = await client.GetAllFranchisesAsync();
                    if (franchis != null)
                    {

                    }
                    else
                    {
                        Console.WriteLine("No team found.");
                    }
                }
                catch (ApiException ex)
                {
                    Console.WriteLine($"API error: {ex.Message} (Status: {ex.StatusCode})");
                }
                client.Dispose();
                await FollowupAsync("Worked.");
            }
            else
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
            }
        }

        [SlashCommand("matches", "Returns your matches for the upcoming week or any other week.")]
        public async Task FindMatches([Choice("Master", (int)Tiers.Master), Choice("Elite", (int)Tiers.Elite), Choice("Rival", (int)Tiers.Rival), 
            Choice("Challenger", (int)Tiers.Challenger), Choice("Prospect", (int)Tiers.Prospect), Choice("Academy", (int)Tiers.Academy)]
            int tier = -1,

            [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchise = HelperFactory.defaultFranchise,

            [Choice("1", 1), Choice("2", 2), Choice("3", 3), Choice("4", 4), Choice("5", 5),
             Choice("6", 6), Choice("7", 7), Choice("8", 8), Choice("9", 9)]
            int week = -1)
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
                if (tier < 0)
                {
                    var user = Context.Guild.GetUser(Context.User.Id);
                    var roles = user.Roles;
                    foreach (var role in roles)
                    {
                        if (TierDiscordRoleId.Contains(role.Id))
                        {
                            tier = TierDiscordRoleId.IndexOf(role.Id) + 36;
                        }
                    }
                }
                if (tier < 0) 
                {
                    await FollowupAsync("You do not have any tier-roles. Either choose a tier manually or make sure you have your designated tier-role.", ephemeral: true); 
                    return;
                }
                
                string matches = "";
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
                    if (teamTier.TierId != tier)
                    {
                        continue; // Skip if the tier does not match
                    }
                    string tierMatches = "";
                    var teamMatches = matchesData.Where(x => x.Week == week && (x.HomeTeamId == teamTier.Id || x.AwayTeamId == teamTier.Id)).ToList();
                    if (teamMatches.Count > 0)
                    {
                        tierMatches += $"{HelperFactory.MakeDiscordEmoteString(HelperFactory.tiers[teamTier.TierId - 36], HelperFactory.emote_guild, true)} **{teamTier.Name}** - *{ApiClient.MakeTierIdToTiername(teamTier.TierId)}*\n";
                        foreach (var match in teamMatches)
                        {
                            if (match is null) continue;
                            var awayTeam = ApiClient.GetTierteam((int)match.AwayTeamId, allFranchiseData);
                            var homeTeam = ApiClient.GetTierteam((int)match.HomeTeamId, allFranchiseData);
                            if (homeTeam is null || awayTeam is null)
                            {
                                continue; // Skip if teams are not found
                            }
                            string currentMatchDate = match.ScheduledDate != null
                                ? $"> {match.ScheduledDate:dd.MM.yyyy HH:mm}\n"
                                : "> *Not Scheduled*\n";
                            var discordEmoteStringHome = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == homeTeam.FranchiseEntryId).First().Prefix, HelperFactory.emote_guild);
                            var discordEmoteStringAway = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == awayTeam.FranchiseEntryId).First().Prefix, HelperFactory.emote_guild);
                            tierMatches += $"> {discordEmoteStringHome} {homeTeam.Name} vs {awayTeam.Name} {discordEmoteStringAway}\n> `{match.ExternalId}` \n {currentMatchDate}";
                            //tierMatches += $"> `{match.ExternalId}`\n> {discordEmoteStringHome} {homeTeam.Name} vs {awayTeam.Name} {discordEmoteStringAway} \n{currentMatchDate}";
                        }
                        allTierMatches += tierMatches + "\n";
                    }
                }
                var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault()?.Nickname;
                userName ??= Context.User.GlobalName;

                var emoteUrl = HelperFactory.MakeDiscordEmoteString(franchiseData.Prefix, HelperFactory.emote_guild);
                var logoUrl = franchiseData.Logo ?? "";
                var color = HelperFactory.GetTierColor(tier);
                var emb = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"{emoteUrl} Matches of week " + week)
                .WithFooter("Requested By " + userName, Context.User.GetAvatarUrl())
                .WithDescription(allTierMatches)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(logoUrl);


                await FollowupAsync(embed: emb.Build(), ephemeral: false);

            }
            catch (ApiException ex)
            {
                Console.WriteLine($"API error: {ex.Message} (Status: {ex.StatusCode})");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("An error occured. It is likely that either the discord API or the RSC API resulted in an error.", ephemeral: true);
                return;
            }

        }

        [SlashCommand("refresh", "Refreshed the command pattern. This is an admin command.")]
        public async Task RefreshCommands()
        {
            if (Context.User.Id == 300570127869411329)
            {
                await DeferAsync();
                // await CommandHandler._interactions.RestClient.BulkOverwriteGlobalCommands(Array.Empty<ApplicationCommandProperties>());
                await CommandHandler._interactions.RegisterCommandsToGuildAsync(1093943074746531910, true);
                await CommandHandler._interactions.RegisterCommandsToGuildAsync(690948036540760147, true);
                await CommandHandler._interactions.RegisterCommandsToGuildAsync(1093941417061126174, true);
                await FollowupAsync("All set!");
            }
            else
            {
                await RespondAsync("This is a developer-only command!");
            }
        }
    }

}

