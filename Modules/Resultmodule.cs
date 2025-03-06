using Discord;
using Discord.Interactions;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading.Tasks;


namespace DXT_Resultmaker.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public class Resultmodule : InteractionModuleBase<SocketInteractionContext>
{

    /*
    [SlashCommand("results", "Returns a formatted result message of current week.")]
    public async Task Results([Choice("1", "1"), Choice("2", "2"), Choice("3", "3"), Choice("4", "4"), Choice("5", "5"), Choice("6", "6"), Choice("7", "7"), Choice("8", "8"), Choice("9", "9")] string week = "")
    {
        await DeferAsync();
        if (week == "")
        {
            DateTime startDate = HelperFactory.seasonStart;
            DateTime currentDate = DateTime.Now;
            TimeSpan timePassed = currentDate - startDate;
            if ((int)(timePassed.TotalDays / 7) > 9) week = "9";
            else
            {
                week = ((int)(timePassed.TotalDays / 7)).ToString();
            }

        }
        var manager = SheetHandler.manager;

        manager.UpdateSpreadsheetCell(SheetHandler.DXT_SHEET_URL, "Output Data!B2", week);

        string cell = manager.GetSpreadsheetData(SheetHandler.DXT_SHEET_URL, "Output Data!I1");

        List<string> IconAbbrevations = HelperFactory.SplitAndSortResultString(cell);

        // Takes all Abbreviations for Icons and replaces them with the Emote String to be displayed as Emote
        IconAbbrevations = IconAbbrevations.Distinct().ToList();
        foreach (var IconAbbr in IconAbbrevations)
        {
            cell = cell.Replace(":" + IconAbbr + ":", HelperFactory.MakeDiscordEmoteString(IconAbbr, 1093943074746531910));
        }
        // Format the requested franchise data
        string result;
        try
        {
            string standing = manager.GetSpreadsheetData(SheetHandler.ERS_SHEET_URL, "Rosters!L107");
            standing = standing.Replace("Rank #", "");
            result = "## Franchise Standing: " + standing;
            await FollowupAsync(cell + "\n" + result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
    */
    /*
    [SlashCommand("setstandings", "Returns a formatted result message of current week.")]
    public async Task SetFranchiseStanding([Choice("1", "1"), Choice("2", "2"), Choice("3", "3"), Choice("4", "4"), Choice("5", "5"), Choice("6", "6"), Choice("7", "7"), Choice("8", "8"), Choice("9", "9")] string week)
    {
        if(!AdminModule.IsAdmin(Context.User.Id))
        {
            await RespondAsync("You're not an admin!");
            return;
        }
        await DeferAsync();

        var manager = SheetHandler.manager;


        List<List<string>> values = manager.GetSpreadsheetDataListColumn(SheetHandler.ERS_SHEET_URL, "Franchises!B7:F46");
        DateTime seasonStart = new DateTime(2024, 2, 21);
        int currentWeek = (int)Math.Floor((double)(DateTime.Now - seasonStart).TotalDays / 7);

        string result = "";
        if (currentWeek.ToString() == week)
        {
            for (int i = 0; i < values[0].Count; i++)
            {
                if (values[3][i] == "Dexterity")
                {
                    result = values[0][i];
                    switch (values[0][i])
                    {
                        case "1":
                            result += "st";
                            break;
                        case "2":
                            result += "nd";
                            break;
                        case "3":
                            result += "rd";
                            break;
                        default:
                            result += "th";
                            break;
                    }
                    manager.UpdateSpreadsheetCell(SheetHandler.DXT_SHEET_URL, "Input Data!M19" + week, result);
                    break;
                }
            }
        }
        else
        {
            result = "Week does not match the current week. The future is bright.";
        }
        await FollowupAsync(result);
        
        // Format the requested franchise data
    }
    */
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
    [SlashCommand("roster", "Displays current roster of the franchise and tier you asked for.")]
    public async Task Roster([Autocomplete(typeof(AutoCompleteHandlerBase))] string franchise, [Choice("Master", 0), Choice("Elite", 1), Choice("Challenger", 2), Choice("Prospect", 3), Choice("Academy", 4)] int tier)
    {
        await RespondAsync("This command is currently disabled.");
        return;
        await DeferAsync();
        var manager = SheetHandler.manager;

        var data = manager.GetSpreadsheetDataListRow(SheetHandler.ERS_SHEET_URL, "Rosters!C3:AR100");

        var msg = "";
        var titel = "";
        if (data.Count == 0) await FollowupAsync("Seems like I cannot connect to the spreadsheet...");

        for (int j = 0; j < data.Count; j += 25)
        {
            for (int i = 0; i < data[j].Count; i += 9)
            {
                if (data[j][i] == franchise)
                {
                    titel += "*'" + data[j + tier * 4 + 6][i] + "'*\n\n";
                    for (int k = 3; k <= 6; k++)
                    {
                        if (data[j + tier * 4 + k][i + 3] == "C")
                        {
                            msg += "> `" + data[j + tier * 4 + k][i + 4] + "` " + data[j + tier * 4 + k][i + 5];
                            msg += " **(C.)**\n ";
                        }
                        else if (data[j + tier * 4 + k][i + 4] != "")
                        {
                            msg += "> `" + data[j + tier * 4 + k][i + 4] + "` " + data[j + tier * 4 + k][i + 5];
                            msg += "\n";
                        }

                    }
                }
            }
        }
        if (msg == "")
        {
            msg = "No signed played at the moment.";
        }
        var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault().Nickname;
        userName ??= Context.User.GlobalName;
        var emb = new EmbedBuilder()
            .WithColor(HelperFactory.GetTierColor(tier))
            .WithTitle(HelperFactory.MakeDiscordEmoteString(HelperFactory.tiers[tier], HelperFactory.emote_guild) + " " + franchise + "\n")
            .WithDescription(titel + msg)
            .WithCurrentTimestamp()
            .WithFooter("Requested By " + userName, Context.User.GetAvatarUrl())
            .Build();


        await FollowupAsync(embed: emb);
    }
    /*
    [SlashCommand("cupresults", "Returns a formatted result message of current week.")]
    public async Task CupResults([Choice("Rising Stars Cup", "Rising Stars Cup"), Choice("All-Stars Cup", "All-Stars Cup")] string tier)
    {
        await DeferAsync();
        var manager = SheetHandler.manager;


        List<List<string>> cupFixtureData = manager.GetSpreadsheetDataListColumn(SheetHandler.ERS_SHEET_URL, "CupFixtures!B2:L99");


        // Format the requested franchise data
        string result = "";
        for (int i = 0; i < cupFixtureData[0].Count; i++)
        {
            if ((HelperFactory.teamnames.Contains(cupFixtureData[4][i])|| HelperFactory.teamnames.Contains(cupFixtureData[5][i])) && cupFixtureData[1][i] == tier)
            {
                result += "** " + cupFixtureData[3][i] + "** - " + cupFixtureData[2][i] + "\n";
                result += "> " + cupFixtureData[4][i] + " "+ cupFixtureData[10][i] + " " + cupFixtureData[5][i] + "\n";
                DateTime TimeResult;
                if (DateTime.TryParseExact(cupFixtureData[7][i] + " " + cupFixtureData[8][i], "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeResult))
                {
                    DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    long unixTime = (long)(TimeResult.ToUniversalTime() - epochStart).TotalSeconds;
                    result += ">  <t:" + unixTime + ":f> \n\n";
                }
                else
                {
                    result += ">  Not scheduled.\n\n";
                }
            }
        }
        if (result == "") result = "> I did not find any results.";
        var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault().Nickname;
        userName ??= Context.User.GlobalName;
        var emb = new EmbedBuilder().WithColor(HelperFactory.dxtColor)
                .WithTitle("<:DXT:1213244949408129055> " + tier + " Results")
                .WithDescription(result)
                .WithFooter("Requested By " + userName, Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build();
        await FollowupAsync(embed: emb);
            
    }
    */
}
