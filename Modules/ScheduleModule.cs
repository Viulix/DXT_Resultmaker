using Discord;
using Discord.Interactions;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DXT_Resultmaker.Modules
{

    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        // public static Timer _timer = new Timer(TimerCallback);
        [SlashCommand("fixtures", "Returns all games that have not been scheduled yet!")]
        public async Task Fixtures(
            [Choice("1", "1"), Choice("2", "2"), Choice("3", "3"), Choice("4", "4"), Choice("5", "5"), Choice("6", "6"), Choice("7", "7"), Choice("8", "8"), Choice("9", "9")] string week = "",
            [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchise = "Team Dexterity")
        {
            await DeferAsync();
            try
            {
                if (week == "")
                {
                    week = (ISOWeek.GetWeekOfYear(DailyTaskScheduler.GetBerlinTime()) - HelperFactory.seasonCalenderWeek).ToString();
                }
                if (franchise == "")
                {
                    franchise = DailyTaskScheduler.franchiseName;
                }

                List<string> teamnames = HelperFactory.Franchises.Where(x => x.name == franchise).First().subteams.Select(x => x.name).ToList();
                var manager = SheetHandler.manager;
                var data = await manager.ReadSpreadsheet(SheetHandler.ERS_SHEET_URL, "Fixtures!A2:L901");
                string matches = "";
                int count = 0;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    if (teamnames.Contains(data.GetValue(4, i)) || teamnames.Contains(data.GetValue(5, i)))
                    {
                        if (data.GetValue(2, i) == "Week " + week)
                        {
                            if (count % 2 == 0)
                            {
                                if (i != 0) matches += "\n";
                                matches += HelperFactory.MakeDiscordEmoteString("RSC_" + data.GetValue(1, i), 1093943074746531910) + " **" + data.GetValue(1, i) + "**\n";
                                count = 0;
                            }
                            if (data.GetValue(4, i) != "")
                            {
                                if (data.GetValue(9, i) != "0-0")
                                {
                                    matches += "> **"
                                        + HelperFactory.MakeDiscordEmoteString(HelperFactory.Franchises.Where(x => x.subteams.Any(y => y.name == data.GetValue(4, i))).First().abbreviation, HelperFactory.emote_guild)
                                        + " " + data.GetValue(4, i) + " " + data.GetValue(9, i) + " " + data.GetValue(5, i) + " "
                                        + HelperFactory.MakeDiscordEmoteString(HelperFactory.Franchises.Where(x => x.subteams.Any(y => y.name == data.GetValue(5, i))).First().abbreviation, HelperFactory.emote_guild) + "** \n";
                                }
                                else
                                {
                                    matches += "> **" +
                                        HelperFactory.MakeDiscordEmoteString(HelperFactory.Franchises.Where(x => x.subteams.Any(y => y.name == data.GetValue(4, i))).First().abbreviation, HelperFactory.emote_guild) +
                                        " " + data.GetValue(4, i) + " vs. " + data.GetValue(5, i) + " " +
                                        HelperFactory.MakeDiscordEmoteString(HelperFactory.Franchises.Where(x => x.subteams.Any(y => y.name == data.GetValue(5, i))).First().abbreviation, HelperFactory.emote_guild) + "** \n";
                                }
                            }
                            if (data.GetValue(10, i) == "Match is pending schedule date")
                            {
                                matches = matches + "> Not scheduled\n";
                            }
                            else
                            {
                                DateTime result;
                                if (DateTime.TryParseExact(
                                        data.GetValue(7, i) + " " + data.GetValue(8, i),
                                        "dd/MM/yyyy HH:mm",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out result))
                                {
                                    TimeZoneInfo berlinZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

                                    DateTimeOffset berlinTime = new DateTimeOffset(result, berlinZone.GetUtcOffset(result));

                                    DateTimeOffset utcTime = berlinTime.ToUniversalTime();

                                    long unixTime = utcTime.ToUnixTimeSeconds();

                                    matches = matches + ">  <t:" + unixTime + ":f>" + " |  <t:" + unixTime + ":R> \n";
                                }
                                else
                                {
                                    Console.WriteLine("Die Eingabe entspricht nicht dem angegebenen Format.");
                                    matches = matches + ">" + data.GetValue(7, i) + "  " + data.GetValue(8, i) + "\n";
                                }
                            }
                            if (data.GetValue(11, i) != "N/A")
                            {
                                matches += "> 🧷 [Link](" + data.GetValue(11, i) + ") \n";
                            }
                            count++;
                        }
                    }
                }
                var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault()?.Nickname;
                userName ??= Context.User.GlobalName;
                var franchiseData = HelperFactory.Franchises.FirstOrDefault(x => x.name == franchise);
                if (franchiseData == null)
                {
                    await FollowupAsync("Franchise not found.");
                    return;
                }

                var emoteUrl = franchiseData.emoteUrl ?? "";
                var bannerUrl = franchiseData.bannerurl ?? "";
                var logoUrl = franchiseData.logo_url ?? "";
                int hexValue = int.Parse(franchiseData.main_color, NumberStyles.HexNumber);

                Discord.Color color = new Discord.Color((uint)hexValue);
                var emb = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"{emoteUrl} Fixtures of week " + week)
                .WithDescription(matches)
                .WithFooter("Requested By " + userName, Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                //.WithImageUrl(bannerUrl)
                .WithThumbnailUrl(logoUrl)
                .Build();


                await FollowupAsync(embed: emb);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("Something went wrong.");
            }
        }
        /*
        public static void TimerCallback(object state)
        {

            DateTime startDate = HelperFactory.seasonStart;
            DateTime currentDate = DateTime.Now;
            Console.WriteLine("Timer called at: " + currentDate.TimeOfDay);
            TimeSpan timePassed = currentDate - startDate;
            int week = (int)(timePassed.TotalDays / 7) + 1;
            if ((currentDate.DayOfWeek == DayOfWeek.Friday || currentDate.DayOfWeek == DayOfWeek.Sunday) && (week <= 9) && (currentDate.TimeOfDay >= new TimeSpan(18,0,0)) && (currentDate.TimeOfDay <= new TimeSpan(18, 5, 0))) { 
                Console.WriteLine("Reminding captains at: " + currentDate.TimeOfDay);
                RemindCaptains(week);
            }
            UpdateFranchiseStanding(week - 2);
            if (currentDate.DayOfWeek == DayOfWeek.Monday && currentDate.TimeOfDay >= TimeSpan.FromHours(0) && currentDate.TimeOfDay <= TimeSpan.FromHours(12))
            {
                UpdateFranchiseStanding(week-1);
                StatsFactory.WeeklyStatsUpdate(week, StatsModule.users.Values.ToArray());  
            }
            if ((week <= 9) && (currentDate.TimeOfDay >= new TimeSpan(18, 0, 0)) && (currentDate.TimeOfDay <= new TimeSpan(18, 5, 0)))
            {
                UpdateCaptain();
            }
            var a = SheetHandler.manager.GetAllSpreadsheetValuesColumn(SheetHandler.ERS_SHEET_URL, "Import").Result;
            string[][] stringArray = a
                    .Select(innerList => innerList.Select(item => item.ToString()).ToArray()).ToArray();
            SheetHandler.manager.WriteToSpreadsheetColumn(SheetHandler.DXT_SHEET_URL, "IMPORTS!A1", stringArray);
           
            UpdateStats();
        }
        */

        private static void RemindCaptains(int week)
        {
            List<string> teamnames = new List<string>() { "Pure Violet", "Absolute Red", "Utmost Yellow", "Untainted Blue", "Pristine Green" };
            var manager = SheetHandler.manager;
            var data = manager.GetSpreadsheetDataListColumn(SheetHandler.ERS_SHEET_URL, "Fixtures!A2:J901");

            List<List<string>> matches_missed = new List<List<string>>()
            {
                new List<string>(){},
                new List<string>(){},
                new List<string>(){},
                new List<string>(){},
                new List<string>(){}
            };
            ;
            for (int i = 0; i < data[0].Count; i++)
            {
                if (teamnames.Contains(data[4][i]) || teamnames.Contains(data[5][i]))
                {
                    if (data[2][i] == "Week " + week)
                    {
                        if (data[8][i] == "")
                        {
                            if (teamnames.Contains(data[4][i]))
                            {
                                int ind = teamnames.IndexOf(data[4][i]);

                                matches_missed[ind].Add("> `" + data[0][i] + "` | " + data[4][i] + " vs. " + data[5][i] + "\n");
                            }
                            else
                            {
                                int ind = teamnames.IndexOf(data[5][i]);
                                matches_missed[ind].Add("> `" + data[0][i] + "` | " + data[5][i] + " vs. " + data[4][i] + "\n");
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < HelperFactory.captains.Count; i++)
            {
                string msg = "# " + HelperFactory.MakeDiscordEmoteString(HelperFactory.tiers[i], HelperFactory.emote_guild) + " Missing games for week #" + week + " \n";
                for (int j = 0; j < matches_missed[i].Count; j++)
                {
                    msg += matches_missed[i][j];
                }
                if (matches_missed[i].Count > 0)
                {
                    var user = CommandHandler._client.GetUser(HelperFactory.captains[i]);
                    msg += "\n*Take this as a little reminder. It is **important** to schedule in time. If you struggle to schedule due to opponents not responding or other issues, feel free to message the Management.* \n *This message was sent automatically.*";
                    user.SendMessageAsync(msg);
                }
            }
        }
        public static bool UpdateFranchiseStanding(int week)
        {
            var manager = SheetHandler.manager;


            List<List<string>> values = manager.GetSpreadsheetDataListColumn(SheetHandler.ERS_SHEET_URL, "Franchises!B7:F46");
            string result = "";

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
                    manager.UpdateSpreadsheetCell(SheetHandler.DXT_SHEET_URL, "Input Data!AI1" + week, result);
                    break;
                }
            }
            return true;
        }
        /*
        private static void UpdateCaptain()
        {
            SaveData current_Data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("./bot.json"));

            var manager = SheetHandler.manager;
            var shorts = manager.GetSpreadsheetDataListColumn(SheetHandler.ERS_SHEET_URL, "Import!B2:AP121");
            List<Franchise> franchises = new List<Franchise>();


            for (int i = 0; i < shorts[0].Count - 5; i += 6)
            {
                if (shorts[0][i] != "")
                {
                    Franchise current = new();
                    List<string> onlineData = new();
                    onlineData.Add(shorts[1][i + 1]);
                    onlineData.Add(shorts[1][i + 2]);
                    onlineData.Add(shorts[1][i + 3]);
                    onlineData.Add(shorts[1][i + 4]);
                    onlineData.Add(shorts[1][i + 5]);
                    current.subteams = onlineData;
                    current.name = shorts[0][i];
                    current.abbreviation = shorts[4][i];
                    franchises.Add(current);
                }
            }
            var allFranchises = AdminModule.GetCaptainList();
            
            
            
            foreach (var franchise in franchises)
            {
                Console.WriteLine(franchise.name);
                for (int i = 0; i < allFranchises.Count; i++)
                {
                    if (allFranchises[i][0] == franchise.name)
                    {
                        franchise.captains = allFranchises[i].Skip(1).Where((value, index) => index % 1 == 0).ToList();
                        Console.WriteLine(franchise.captains.Count);
                    }
                }
            }

            current_Data.franchises = franchises;


            HelperFactory.Franchises = franchises;
            string json = JsonConvert.SerializeObject(current_Data, Formatting.Indented);
            try
            {
                File.WriteAllText("./bot.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Schreiben der Datei: " + ex.Message);
            }

            List<string> master_captains = new();
            List<string> elite_captains = new();
            List<string> chally_captains = new();
            List<string> prospect_captains = new();
            List<string> academy_captains = new();


            List<string> master= new();
            List<string> elite = new();
            List<string> chally= new();
            List<string> prospects = new();
            List<string> academy = new();

            List<List<string>> franchiName = new();
            List<string> one = new();
            List<string> two = new();
            List<string> three = new();
            List<string> four = new();
            List<string> five = new();

            franchiName.Add(one);
            franchiName.Add(two);
            franchiName.Add(three);
            franchiName.Add(four);
            franchiName.Add(five);
            foreach (var franchise in franchises)
            {
                master_captains.Add(franchise.captains[0]);
                elite_captains.Add(franchise.captains[1]);
                chally_captains.Add(franchise.captains[2]);
                prospect_captains.Add(franchise.captains[3]);
                academy_captains.Add(franchise.captains[4]);

                for(int i = 0; i < 5; i++)
                {
                    franchiName[i].Add(franchise.name);
                }
                
                master.Add(franchise.subteams[0]);
                elite.Add(franchise.subteams[1]);
                chally.Add(franchise.subteams[2]);
                prospects.Add(franchise.subteams[3]);
                academy.Add(franchise.subteams[4]);
            }
            master_captains.AddRange(elite_captains);
            master_captains.AddRange(chally_captains);
            master_captains.AddRange(prospect_captains);
            master_captains.AddRange(academy_captains);

            master.AddRange(elite);
            master.AddRange(chally);
            master.AddRange(prospects);
            master.AddRange(academy);

            List<List<string>> all = new();
            all.Add(master_captains);
            all.Add(master);
            all.Add(franchiName.SelectMany(x => x).ToList());
            string[][] stringArray = all
                    .Select(innerList => innerList.Select(item => item.ToString()).ToArray())
                    .ToArray();
            manager.WriteToSpreadsheetColumn(SheetHandler.DXT_SHEET_URL, "Captains!A1", stringArray);


            Console.WriteLine("Updated captains.");
        }
        private static void UpdateStats()
        {
            List<List<string>> allUserData = new List<List<string>>();

            foreach (var groupId in HelperFactory.ballchasingGroupIds)
            {
                if (groupId != "")
                {
                    var userData = StatsFactory.ProcessReplayGroup(groupId, StatsModule.users.Values.ToArray());

                    foreach (var user in userData)
                    {
                        var existingUser = allUserData.FirstOrDefault(u => u.Any() && u[0] == user[0]);
                        int index = allUserData.IndexOf(existingUser);
                        if (existingUser != null)
                        {
                            if( index > -1)
                            {
                                allUserData[index] = StatsFactory.CombineData(existingUser, user);
                            }
                        }
                        else
                        {
                            allUserData.Add(user); 
                        }
                    }
                }
            }
            List<string> liste = Enumerable.Repeat("", 49).ToList();

            while (allUserData.Count < 20)
            {
                allUserData.Add(liste);
            }
            string[][] stringArray = allUserData
                .Select(innerList => innerList.Select(item => item.ToString()).ToArray()).ToArray();
            SheetHandler.manager.WriteToSpreadsheetColumn(SheetHandler.DXT_SHEET_URL, StatsFactory.stats_range + "B3", stringArray);

        }
        */
    }
}
