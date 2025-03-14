using Discord;
using Discord.Interactions;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace DXT_Resultmaker.Modules
{
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        public static List<ulong> admins;
        private static List<ulong> TierDiscordRoleId = new()
        {
            796892694261399613,
            1200155287533932656,
            796892696408358962,
            796892697570050059,
            796895790433697793
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
        [SlashCommand("setsheet", "Sets sheet. Admin only!")]
        public async Task SetSheet([Choice("ERS-Database", 1), Choice("DXT-Sheet", 2)] int sheet, string id)
        {
            if (IsAdmin(Context.User.Id))
            {
                SaveData current_Data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("./bot.json"));
                switch (sheet)
                {

                    case 1:
                        current_Data.database_sheet = id;
                        SheetHandler.ERS_SHEET_URL = id;
                        break;
                    case 2:
                        current_Data.dxt_sheet = id;
                        SheetHandler.DXT_SHEET_URL = id;
                        break;
                    default:
                        await RespondAsync("Unable to find the sheet you selected. This is probably a bug.", ephemeral: true);
                        break;
                }
                string json = JsonConvert.SerializeObject(current_Data, Formatting.Indented);
                File.WriteAllText("./bot.json", json);
                await RespondAsync("Worked.", ephemeral: true);
            }
            else
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
            }
        }
        [SlashCommand("testthis", "Developer Command. Admin only!")]
        public async Task TestThis()
        {
            if (IsAdmin(Context.User.Id))
            {
                await DeferAsync(ephemeral: true);
                DailyTaskScheduler.SendFixtureMessage(true);
                await FollowupAsync("Worked.");
            }
            else
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
            }
        }

        [SlashCommand("refreshfranchises", "Developer-Command.")]
        public async Task RefreshFranchises()
        {
            if (IsAdmin(Context.User.Id))
            {
                await DeferAsync(true);
                var data = SheetHandler.manager.ReadSpreadsheet(SheetHandler.ERS_SHEET_URL, "Rosters!A2:AI128").Result;
                List<Franchise> franchises = new List<Franchise>();

                int rowIndex = 4;
                int coloumn_gap = 9;
                while (rowIndex < 107)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Franchise franchise = new Franchise();
                        franchise.name = data.GetValue(coloumn_gap * i + 2, rowIndex);
                        franchise.subteams = new List<Subteam>();
                        franchise.rank = data.GetValue(coloumn_gap * i + 2, rowIndex + 1).Split("Rank ").Last();
                        franchise.manager = data.GetValue(coloumn_gap * i + 7, rowIndex - 1);
                        franchise.assistentManagers = new List<string>();

                        for (int j = 0; j < 2; j++)
                        {
                            string assistentManager = data.GetValue(coloumn_gap * i + 7, rowIndex + j);
                            if (assistentManager != "")
                            {
                                franchise.assistentManagers.Add(assistentManager);
                            }
                        }
                        // Subteams
                        for (int j = 0; j < 5; j++)
                        {
                            Subteam subteam = new Subteam();
                            subteam.name = data.GetValue(coloumn_gap * i + 2, rowIndex + 5 + j * 4);
                            List<string> players = new List<string>();
                            // Add the players
                            for (int k = 0; k < 4; k++)
                            {
                                string player = data.GetValue(coloumn_gap * i + 7, rowIndex + j * 4 + 2 + k);
                                if (player != "")
                                {
                                    players.Add(player);
                                }
                                if (data.GetValue(coloumn_gap * i + 5, rowIndex + j * 4 + 2 + k) == "C")
                                {
                                    subteam.captain = player;
                                }
                            }
                            subteam.players = players;
                            franchise.subteams.Add(subteam);
                        }
                        franchises.Add(franchise);
                    }
                    rowIndex += 25;
                }
                // Update the abbreviations
                var franchiseAbrrData = SheetHandler.manager.ReadSpreadsheet(SheetHandler.ERS_SHEET_URL, "Import!A2:I121").Result;
                for (int i = 0; i < franchiseAbrrData.Rows.Count; i++)
                {
                    string currentFranchiseName = franchiseAbrrData.GetValue(2, i * 6);
                    var franchise = franchises.FirstOrDefault(f => f.name == currentFranchiseName);
                    if (franchise != null)
                    {
                        franchise.abbreviation = franchiseAbrrData.GetValue(5, i * 6);
                        franchise.logo_url = franchiseAbrrData.GetValue(6, i * 6);
                        franchise.bannerurl = franchiseAbrrData.GetValue(7, i * 6);
                        franchise.emoteUrl = HelperFactory.MakeDiscordEmoteString(franchise.abbreviation, 1093943074746531910);
                        franchise.main_color = franchiseAbrrData.GetValue(8, i * 6).Remove(0, 1);
                    }
                }

                HelperFactory.Franchises = franchises;
                SaveData current_Data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("./bot.json"));
                current_Data.franchises = franchises;
                string json = JsonConvert.SerializeObject(current_Data, Formatting.Indented);
                File.WriteAllText("./bot.json", json);
                await FollowupAsync("Updated the franchises.");
            }
            else
            {
                await RespondAsync("You are not an admin.", ephemeral: true);
            }
        }
        [SlashCommand("matches", "Returns your matches for the upcoming week or any other week.")]
        public async Task FindMatches([Choice("Master", 0), Choice("Elite", 1), Choice("Rival", 2), Choice("Challenger", 3), Choice("Prospect", 4)]
            int tier = -1,

            [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchise = "Team Dexterity",

            [Choice("1", 1), Choice("2", 2), Choice("3", 3), Choice("4", 4), Choice("5", 5),
             Choice("6", 6), Choice("7", 7), Choice("8", 8), Choice("9", 9)]
            int week = 0)
        {
            {
                // Get the current calendar week
                int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
                // Get the tier based on the user's roles
                if (tier < 0)
                {
                    var user = Context.Guild.GetUser(Context.User.Id);
                    var roles = user.Roles;
                    foreach (var role in roles)
                    {
                        if (TierDiscordRoleId.Contains(role.Id))
                        {
                            tier = TierDiscordRoleId.IndexOf(role.Id);
                        }
                    }
                }
                if (tier < 0) await RespondAsync("You do not have any tier-roles. Either choose a tier manually or make sure you have your designated tier-role.", ephemeral: true);

                if (week == 0)
                {
                    week = currentWeek - HelperFactory.seasonCalenderWeek + 1;
                }
                await DeferAsync(ephemeral: false);

                string subTeamName = string.Empty;
                SaveData current_Data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("./bot.json"));
                var selectedFranchise = current_Data.franchises.FirstOrDefault(f => f.name.Equals(franchise, StringComparison.OrdinalIgnoreCase));
                if (selectedFranchise != null)
                {
                    subTeamName = selectedFranchise.subteams[tier].name;
                }
                else
                {
                    Console.WriteLine("Team was not found.");
                }

                var fixturesData = await SheetHandler.manager.ReadSpreadsheet(SheetHandler.ERS_SHEET_URL, "Fixtures!A1:L901");

                string message = "";
                try
                {
                    for (int i = 0; i < fixturesData.Rows.Count; i++)
                    {
                        string tempWeek = fixturesData.GetValue(2, i);
                        string shouldWeek = $"Week {week}";
                        if (tempWeek != shouldWeek) continue;

                        if (fixturesData.GetValue(4, i) == subTeamName || fixturesData.GetValue(5, i) == subTeamName)
                        {
                            string opponentTeamname = fixturesData.GetValue(4, i);
                            if (opponentTeamname == subTeamName) opponentTeamname = fixturesData.GetValue(5, i);
                            var opponentFranchise = current_Data.franchises.FirstOrDefault(f => f.subteams.Any(st => st.name.Equals(opponentTeamname, StringComparison.OrdinalIgnoreCase)));
                            if (opponentFranchise == null) Console.WriteLine("Error on finding the corresponding franchise.");
                            string captain = opponentFranchise.subteams.FirstOrDefault(st => st.name.Equals(opponentTeamname, StringComparison.OrdinalIgnoreCase)).captain ?? "No one (?)";
                            if (message == "")
                            {
                                message += "**Match 1**\n";
                                message += $"> {selectedFranchise.emoteUrl} {subTeamName} vs {opponentTeamname} {opponentFranchise.emoteUrl}\n";
                                message += "> `" + fixturesData.GetValue(0, i) + "`\n";
                                message += "> " + fixturesData.GetValue(7, i) + " " + fixturesData.GetValue(8, i) + "\n";
                                message += $"> ➡️ {captain} is the captain of {opponentTeamname} \n";
                            }
                            else
                            {
                                message += "\n";
                                message += "**Match 2**\n";
                                message += $"> {selectedFranchise.emoteUrl} {subTeamName} vs {opponentTeamname} {opponentFranchise.emoteUrl}\n";
                                message += "> `" + fixturesData.GetValue(0, i) + "`\n";
                                message += "> " + fixturesData.GetValue(7, i) + " " + fixturesData.GetValue(8, i) + "\n";
                                message += $"> ➡️ {captain} is the captain of {opponentTeamname} \n";
                            }
                        }
                    }
                    string tiername;
                    switch (tier)
                    {
                        case 0:
                            tiername = "Master";
                            break;
                        case 1:
                            tiername = "Elite";
                            break;
                        case 2:
                            tiername = "Rival";
                            break;
                        case 3:
                            tiername = "Challenger";
                            break;
                        case 4:
                            tiername = "Prospect";
                            break;
                        default:
                            tiername = "N/V";
                            break;
                    }

                    var emb = new EmbedBuilder()
                        .WithTitle(HelperFactory.MakeDiscordEmoteString("RSC_" + tiername, 690948036540760147) + " Matches for " + subTeamName + " in Week " + week)
                        .WithDescription(message)
                        .WithThumbnailUrl(HelperFactory.MakeDiscordEmoteUrl("RSC_" + tiername, 690948036540760147))
                        .WithCurrentTimestamp()
                        .WithColor(HelperFactory.GetTierColor(tier))
                        .WithFooter("Requested By " + Context.User.GlobalName, Context.User.GetAvatarUrl())
                        .WithImageUrl(selectedFranchise.bannerurl ?? "")
                        .Build();

                    await FollowupAsync(embed: emb, ephemeral: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }

        }
    }
}

