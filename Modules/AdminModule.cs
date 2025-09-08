using Discord;
using Discord.Interactions;
using System.Globalization;
namespace DXT_Resultmaker.Modules
{
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Ändere die Sichtbarkeit der statischen Felder von public zu internal, um CA2211 zu beheben.
        internal static List<ulong> Admins = [];
        internal static List<ulong> TierDiscordRoleId = [];
        
        private const ulong CreatorId = 300570127869411329; // nur der Bot-Creator darf Admins verwalten

        // Hilfsfunktion: Admin-Check
        public static bool IsAdmin(ulong userId)
        {
            return HelperFactory.SaveData.Admins.Contains(userId);
        }

        [SlashCommand("makeadmin", "Only for creator of the bot.")]
        public async Task MakeAdmin(IUser user)
        {
            if (Context.User.Id != CreatorId)
            {
                await RespondAsync("You are not goat enough to pull this.", ephemeral: true);
                return;
            }

            if (IsAdmin(user.Id))
            {
                await RespondAsync("This user is already an admin.", ephemeral: true);
                return;
            }

            HelperFactory.AddAdmin(user.Id);
            await RespondAsync($"User {user.Username} is now an admin ✅", ephemeral: true);
        }

        [SlashCommand("removeadmin", "Only for the creator of the bot.")]
        public async Task RemoveAdmin(IUser user)
        {
            if (Context.User.Id != CreatorId)
            {
                await RespondAsync("You are not goat enough to pull this.", ephemeral: true);
                return;
            }

            if (!IsAdmin(user.Id))
            {
                await RespondAsync("No user with that ID found.", ephemeral: true);
                return;
            }

            HelperFactory.RemoveAdmin(user.Id);
            await RespondAsync($"User {user.Username} was removed from admins ❌", ephemeral: true);
        }

        [SlashCommand("set_role", "Set a Discord role for a specific tier.")]
        public async Task SetRole(IRole role, [Choice("Master", (int)TierId.Master), Choice("Elite", (int)TierId.Elite), Choice("Rival", (int)TierId.Rival),
            Choice("Challenger", (int)TierId.Challenger), Choice("Prospect", (int)TierId.Prospect), Choice("Academy", (int)TierId.Academy)]
            int tier)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            int tierIndex = tier - 36; // Convert TierId to index (Master = 0, Elite = 1, etc.)
            HelperFactory.ReplaceRole(tierIndex, role.Id);
            await RespondAsync($"Role for tier {tierIndex} set to {role.Mention} ✅", ephemeral: true);
        }


        [SlashCommand("set_franchises", "Set the default franchise.")]
        public async Task SetFranchises()
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            ApiClient apiClient = new(HelperFactory.SaveData.DefaultAPIUrl);
            var allFranchises = await apiClient.GetAllFranchisesAsync();
            HelperFactory.SetFranchises(allFranchises);
            await RespondAsync($"Refreshed all franchise names ✅", ephemeral: true);
        }
        [SlashCommand("set_default_franchise", "Set the default franchise.")]
        public async Task SetDefaultFranchise(string franchise)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            if (string.IsNullOrWhiteSpace(franchise))
            {
                await RespondAsync("Franchise name cannot be empty.", ephemeral: true);
                return;
            }
            HelperFactory.SetDefaultFranchise(franchise);
            await RespondAsync($"Default franchise set to **{franchise}** ✅", ephemeral: true);
        }

        [SlashCommand("set_apiurl", "Set the default API url.")]
        public async Task SetApiUrl(string url)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            HelperFactory.SetDefaultAPIUrl(url);
            await RespondAsync($"API URL set to {url} ✅", ephemeral: true);
        }

        [SlashCommand("set_emoteguild", "Set the emote guild ID.")]
        public async Task SetEmoteGuild(string guildId)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            if (!ulong.TryParse(guildId, out ulong id))
            {
                await RespondAsync("Ungültige ID!");
                return;
            }
            HelperFactory.SetEmoteGuild(id);
            await RespondAsync($"Emote guild set to `{guildId}` ✅", ephemeral: true);
        }
        [SlashCommand("add_guild", "Adds an guild id to the bot's guilds.")]
        public async Task AddGuild(string guildId)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            if (!ulong.TryParse(guildId, out ulong id))
            {
                await RespondAsync("Invalid Id!");
                return;
            }

            if (HelperFactory.SaveData.GuildIds.Contains(id))
            {
                await RespondAsync("This guild id is already added.", ephemeral: true);
                return;
            }
            HelperFactory.AddGuild(id);
            await RespondAsync($"Added this id: `{guildId}` ✅", ephemeral: true);
        }
        [SlashCommand("remove_guild", "Removes an guild id from the bot's guilds.")]
        public async Task RemoveGuild(string guildId)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            if (!ulong.TryParse(guildId, out ulong id))
            {
                await RespondAsync("Invalid Id!");
                return;
            }
            if (!HelperFactory.SaveData.GuildIds.Contains(id))
            {
                await RespondAsync("This guild id was not found in the stored guilds.", ephemeral: true);
                return;
            }
            HelperFactory.RemoveGuild(id);
            await RespondAsync($"Removed this id: `{guildId}` ✅", ephemeral: true);
        }

        [SlashCommand("set_channel", "Sets the channel for a specific tier to place fixtures.")]
        public async Task SetFixtureChannel(string channelId, [Choice("Fixture", 0), Choice("Master", (int)TierId.Master), Choice("Elite", (int)TierId.Elite), Choice("Rival", (int)TierId.Rival),
            Choice("Challenger", (int)TierId.Challenger), Choice("Prospect", (int)TierId.Prospect), Choice("Academy", (int)TierId.Academy)] int tierId)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            if (!ulong.TryParse(channelId, out ulong id))
            {
                await RespondAsync("Invalid Id!");
                return;
            }
            int offset = 0;
            if (tierId != 0) offset = 35;
            if (!HelperFactory.SaveData.ChannelIds.Contains(id))
            {
                HelperFactory.ReplaceChannel(tierId - offset,id);
                await RespondAsync($"Set the channel for `{HelperFactory.Tiers.Where(x => x.Value == tierId).FirstOrDefault().Key ?? "Fixture"}` to: <#{channelId}> ✅", ephemeral: true); return;
            }
            else
                await RespondAsync("This channel is already in the channellist. Override it first.", ephemeral: true);
        }

        [SlashCommand("set_seasonstart", "Set the season start date.")]
        public async Task SetSeasonStart(DateTime startDate)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            HelperFactory.SetSeasonStart(startDate);
            await RespondAsync($"Season start set to {startDate:yyyy-MM-dd} ✅", ephemeral: true);
        }

        [SlashCommand("set_seasonweek", "Set the season calendar week.")]
        public async Task SetSeasonWeek(int week)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            HelperFactory.SetSeasonCalenderWeek(week);
            await RespondAsync($"Season calendar week set to {week} ✅", ephemeral: true);
        }

        [SlashCommand("set_color", "Set the main color.")]
        public async Task SetColor(string hexValue)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            // '#' am Anfang optional entfernen
            hexValue = hexValue.TrimStart('#');

            if (!uint.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, null, out var hex))
            {
                await RespondAsync("Invalid hex value. Please use format like `#9EE345`.", ephemeral: true);
                return;
            }

            var color = new Discord.Color(hex);
            HelperFactory.SetMainColor(color);

            await RespondAsync($"Main color set to `#{hexValue.ToUpper()}` ✅", ephemeral: true);
        }
        [SlashCommand("set_schedule_week", "Set the current week counter manually.")]
        public async Task SetScheduleWeek(int week)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            Program.DailyTaskScheduler.SetCurrentWeek(week);
            await RespondAsync($"Current week manually set to {week} ✅", ephemeral: true);
        }

        [SlashCommand("set_tiercolor", "Set the color for a specific tier.")]
        public async Task SetTierColor([Choice("Master", (int)TierId.Master), Choice("Elite", (int)TierId.Elite), Choice("Rival", (int)TierId.Rival),
            Choice("Challenger", (int)TierId.Challenger), Choice("Prospect", (int)TierId.Prospect), Choice("Academy", (int)TierId.Academy)]
            int tier, uint hexValue)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            int tierIndex = tier - 36;
            var color = new Discord.Color(hexValue);
            HelperFactory.SetTierColor(tierIndex, color);
            await RespondAsync($"Tier {tierIndex} color set to `#{hexValue:X}` ✅", ephemeral: true);
        }
        [SlashCommand("set_weekly_time", "Set the weekly message time (day + HH:mm)")]
        public async Task SetWeeklyTime(DayOfWeek day, string time)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            if (!TimeSpan.TryParse(time, out var parsedTime))
            {
                await RespondAsync("Invalid time format. Use HH:mm (e.g. 14:00).", ephemeral: true);
                return;
            }

            // Update the DailyTaskScheduler
            var today = DateTime.Today;
            var daysUntilNext = ((int)day - (int)today.DayOfWeek + 7) % 7;
            var nextOccurrence = today.AddDays(daysUntilNext).Date + parsedTime;

            Program.DailyTaskScheduler.SetWeeklyTime(day, parsedTime);

            // Save the updated values to SaveData
            HelperFactory.SaveData.StartDate = nextOccurrence;
            Program.DailyTaskScheduler.SetWeeklyTime(day, parsedTime);

            // Save the updated values to SaveData
            HelperFactory.SaveData.StartDate = DateTime.Today.AddDays((int)day - (int)DateTime.Today.DayOfWeek).Date + parsedTime;
            HelperFactory.Save();

            await RespondAsync($"Weekly messages set to **{day} {time}** ✅", ephemeral: true);
        }

        [SlashCommand("set_update_interval", "Set the update interval in minutes")]
        public async Task SetUpdateInterval(int minutes)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            if (minutes < 1)
            {
                await RespondAsync("Interval must be >= 1 minute.", ephemeral: true);
                return;
            }

            Program.DailyTaskScheduler.SetUpdateInterval(TimeSpan.FromMinutes(minutes));
            HelperFactory.SaveData.UpdateInterval = minutes;
            HelperFactory.Save();

            await RespondAsync($"Update interval set to **{minutes} minutes** ✅", ephemeral: true);
        }

        [SlashCommand("run-weekly", "Manually run the weekly task")]
        public async Task RunWeekly()
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            await DeferAsync();
            try
            {
                await Program.DailyTaskScheduler.RunWeeklyNow();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running weekly task: {ex.Message}");
                await FollowupAsync("An error occurred while executing the weekly task.", ephemeral: true);
                return;
            }
            await FollowupAsync("Weekly task executed manually ✅", ephemeral: true);
        }

        [SlashCommand("run-update", "Manually run the update task")]
        public async Task RunUpdate()
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }
            await DeferAsync();
            await Program.DailyTaskScheduler.RunUpdateNow();
            await FollowupAsync("Update task executed manually ✅", ephemeral: true);
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
        public async Task FindMatches([Choice("Master", (int)TierId.Master), Choice("Elite", (int)TierId.Elite), Choice("Rival", (int)TierId.Rival), 
            Choice("Challenger", (int)TierId.Challenger), Choice("Prospect", (int)TierId.Prospect), Choice("Academy", (int)TierId.Academy)]
            int tier = -1,

            [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchise = HelperFactory.DefaultFranchiseConst,

            [Choice("1", 1), Choice("2", 2), Choice("3", 3), Choice("4", 4), Choice("5", 5),
             Choice("6", 6), Choice("7", 7), Choice("8", 8), Choice("9", 9)]
            int week = -1)
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
                var client = new ApiClient(HelperFactory.DefaultAPIUrl);
                var matchesData = await client.GetMatchesAsync();
                var allFranchiseData = await client.GetAllFranchisesAsync();
                var allPlayers = await client.GetPlayerListAsync();
                var allTeams = allFranchiseData.Select(y => y.Teams);
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
                    var teamMatches = matchesData.Where(x => x.Week == week && (x.HomeTeamId == teamTier.Id || x.AwayTeamId == teamTier.Id) && x.Format == "League Play").ToList();
                    if (teamMatches.Count > 0)
                    {
                        tierMatches += $"{HelperFactory.MakeDiscordEmoteString(HelperFactory.Tiers.Where(x => x.Value == teamTier.TierId).FirstOrDefault().Key, HelperFactory.SaveData.EmoteGuild, true)} **{teamTier.Name}** - *{ApiClient.MakeTierIdToTiername(teamTier.TierId)}*\n";
                        foreach (var match in teamMatches)
                        {
                            if (match is null) continue;
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
                                captainDisplay = $"> *Captain is: {captain?.User.Name ?? "N/V"}*\n\n";
                            }
                            string currentMatchDate = HelperFactory.ToDiscordTimestamp(match.ScheduledDate);
                            var discordEmoteStringHome = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == homeTeam.FranchiseEntryId).First().Prefix, HelperFactory.SaveData.EmoteGuild);
                            var discordEmoteStringAway = HelperFactory.MakeDiscordEmoteString(allFranchiseData.Where(x => x.Id == awayTeam.FranchiseEntryId).First().Prefix, HelperFactory.SaveData.EmoteGuild);
                            tierMatches += $"> {discordEmoteStringHome} {homeTeam.Name} vs {awayTeam.Name} {discordEmoteStringAway}\n> `{match.ExternalId}` \n> {currentMatchDate}{captainDisplay}";
                            //tierMatches += $"> `{match.ExternalId}`\n> {discordEmoteStringHome} {homeTeam.Name} vs {awayTeam.Name} {discordEmoteStringAway} \n{currentMatchDate}";
                        }
                        allTierMatches += tierMatches + "\n";
                    }
                }
                var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault()?.Nickname;
                userName ??= Context.User.GlobalName;

                var emoteUrl = HelperFactory.MakeDiscordEmoteString(franchiseData.Prefix, HelperFactory.SaveData.EmoteGuild);
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
        [SlashCommand("config_view", "Zeigt die Konfiguration aus SaveData an (ohne Franchises).")]
        public async Task ConfigView()
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            var saveData = HelperFactory.SaveData;

            // Erstelle eine Liste der Tiernamen
            string tierRolesInfo = "";
            string channelIdsInfo = "";
            var channelIds = saveData.ChannelIds;
            if (channelIds == null || channelIds.Count == 0)
            {
                channelIdsInfo += "None";
            }
            else if (channelIds.Count > 0)
            {
                channelIdsInfo += $"Fixture: <#{channelIds[0]}>\n";
            }

            var tierNames = HelperFactory.Tiers
                .OrderBy(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();
            for (int i = 0; i < HelperFactory.Tiers.Keys.Count; i++)
            {
                // Prüfen, ob ein Rollen-ID für das Tier vorhanden ist
                if (TierDiscordRoleId != null && TierDiscordRoleId.Count > i && TierDiscordRoleId[i] != 0)
                {
                    tierRolesInfo += $"{tierNames[i]}: <@&{TierDiscordRoleId[i]}>\n";
                }
                else
                {
                    tierRolesInfo += $"{tierNames[i]}: None\n";
                }
                if (channelIds is not null)
                {
                    if (channelIds.Count > i + 1 && channelIds[i + 1] != 0)
                        channelIdsInfo += $"{tierNames[i]}: <#{channelIds[i + 1]}>\n";
                }
            }

            var embed = new EmbedBuilder()
                .WithTitle("⚙️ Configs")
                .WithColor(new Discord.Color(saveData.MainColor))
                .WithCurrentTimestamp()
                .AddField("Default API URL", string.IsNullOrWhiteSpace(saveData.DefaultAPIUrl) ? "N/V" : saveData.DefaultAPIUrl, true)
                .AddField("Default Franchise", string.IsNullOrWhiteSpace(saveData.DefaultFranchise) ? "N/V" : saveData.DefaultFranchise, true)
                .AddField("Emote Guild ID", saveData.EmoteGuild.ToString(), true)
                .AddField("Season Start", saveData.SeasonStart.ToString("yyyy-MM-dd"), true)
                .AddField("Season Week", saveData.SeasonCalenderWeek.ToString(), true)
                .AddField("Admins Count", saveData.Admins?.Count.ToString() ?? "0", true)
                .AddField("Guild IDs",
                    (saveData.GuildIds != null && saveData.GuildIds.Any())
                        ? string.Join(", ", saveData.GuildIds)
                        : "None", false)
                .AddField("Tier Rollen", tierRolesInfo, false)
                .AddField("Fixture Channels", channelIdsInfo, false);

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }
        [SlashCommand("set_reminder_minutes", "Set the reminder minutes for scheduled tasks.")]
        public async Task SetReminderMinutes(int minutes)
        {
            if (!AdminModule.IsAdmin(Context.User.Id))
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
                return;
            }

            if (minutes < 0)
            {
                await RespondAsync("Reminder minutes must be >= 0.", ephemeral: true);
                return;
            }

            Program.DailyTaskScheduler.SetReminderMinutes(minutes);
            HelperFactory.SaveData.ReminderMinutes = minutes;
            HelperFactory.Save();

            await RespondAsync($"Reminder minutes set to **{minutes} minutes** ✅", ephemeral: true);
        }
    }

}

