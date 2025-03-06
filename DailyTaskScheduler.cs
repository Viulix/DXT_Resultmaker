using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXT_Resultmaker
{
    using Discord;
    using Newtonsoft.Json;
    using System.Globalization;
    using System.Timers;

    public static class DailyTaskScheduler
    {
        private static Timer dailyTimer;
        private static Timer weeklyTimer;
        public readonly static string franchiseName = "Team Dexterity";
        private static readonly ulong fixturesChannelId = 1226613828838621337;

        // Initialisiert beide Timer beim Programmstart
        public static void InitializeTasks()
        {
            InitializeDailyTask();
            InitializeWeeklyTask();
        }

        // Täglicher Timer für Mitternacht (Berliner Zeit)
        public static void InitializeDailyTask()
        {
            TimeSpan timeToMidnight = GetTimeToNextMidnight().Add(-TimeSpan.FromMinutes(2));
            dailyTimer = new Timer(timeToMidnight.TotalMilliseconds)
            {
                AutoReset = false
            };
            dailyTimer.Elapsed += (sender, e) =>
            {
                ExecuteDailyTask();
                dailyTimer.Interval = TimeSpan.FromDays(1).TotalMilliseconds;
                dailyTimer.Start();
            };
            Console.WriteLine($"{GetBerlinTime():HH:mm:ss} Initialized daily timer. Next call in {timeToMidnight.Hours}h {timeToMidnight.Minutes}min.");

            dailyTimer.Start();
        }

        // Wöchentlicher Timer für Montag 12:00 Uhr (Berliner Zeit)
        public static void InitializeWeeklyTask()
        {
            TimeSpan timeToNextMondayNoon = GetTimeToNextMondayNoon();
            weeklyTimer = new Timer(timeToNextMondayNoon.TotalMilliseconds)
            {
                AutoReset = false
            };
            weeklyTimer.Elapsed += (sender, e) =>
            {
                ExecuteWeeklyTask();
                weeklyTimer.Interval = TimeSpan.FromDays(7).TotalMilliseconds;
                weeklyTimer.Start();
            };
            Console.WriteLine($"{GetBerlinTime():HH:mm:ss} Initialized weekly timer. Next call in {timeToNextMondayNoon.Days} days and {timeToNextMondayNoon.Hours} hours.");

            weeklyTimer.Start();
        }

        // Liefert die aktuelle Zeit in der Zeitzone Berlin
        public static DateTime GetBerlinTime()
        {
            TimeZoneInfo berlinZone;
            try
            {
                // Gilt für Windows
                berlinZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Alternative für Linux
                berlinZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            }
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, berlinZone);
        }

        // Berechnet die verbleibende Zeit bis zur nächsten Mitternacht (Berlin)
        private static TimeSpan GetTimeToNextMidnight()
        {
            DateTime now = GetBerlinTime();
            DateTime nextMidnight = now.Date.AddDays(1);
            return nextMidnight - now;
        }
        // Berechnet die Zeit bis zum nächsten Montag um 12:00 Uhr (Berlin)
        private static TimeSpan GetTimeToNextMondayNoon()
        {
            DateTime now = GetBerlinTime();
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            DateTime nextMondayNoon = now.Date.AddDays(daysUntilMonday).AddHours(12);
            if (daysUntilMonday == 0 && now.TimeOfDay >= TimeSpan.FromHours(12))
            {
                nextMondayNoon = nextMondayNoon.AddDays(7); // Falls heute Montag ist und die Zeit schon nach 12:00 Uhr
            }
            return nextMondayNoon - now;
        }

        // Hier wird der täglich auszuführende Code implementiert
        private static void ExecuteDailyTask()
        {
            try
            {
                Console.WriteLine($"{GetBerlinTime():HH:mm:ss} Executing Daily Schedule.");
                RefreshFranchises();
                SendFixtureMessage(true);
                Console.WriteLine($"{GetBerlinTime():HH:mm:ss} Executed Daily Schedule!.");

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        // Die wöchentliche Aufgabe
        private static void ExecuteWeeklyTask()
        {
            try
            {
                Console.WriteLine($"{GetBerlinTime():HH:mm:ss} - Executing Weekly Schedule.");
                SendFixtureMessage();
                RefreshFranchiseStanding();
                Console.WriteLine($"{GetBerlinTime():HH:mm:ss} - Executed Weekly Schedule!.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void RefreshFranchises()
        {
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
            SaveData current_Data = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("./bot.json"));
            current_Data.franchises = franchises;
            string json = JsonConvert.SerializeObject(current_Data, Formatting.Indented);
            File.WriteAllText("./bot.json", json);
            Console.WriteLine("Franchises refreshed");
        }

        public static async void RefreshFranchiseStanding()
        {
            var currentRank = HelperFactory.Franchises.FirstOrDefault(x => x.name == franchiseName).rank;
            int currentWeek = ISOWeek.GetWeekOfYear(GetBerlinTime()) - HelperFactory.seasonCalenderWeek;
            if (currentWeek < 1)
            {
                Console.WriteLine("Date mismatch. Current week cannot be negative!");
                return;
            }
            try
            {
                var data = SheetHandler.manager.ReadSpreadsheet(SheetHandler.DXT_SHEET_URL, "WEEKSTANDING!B1:B10").Result;
                data.SetValue(currentWeek - 1, 0, $"{currentRank}");
                await SheetHandler.manager.WriteSpreadsheet(SheetHandler.DXT_SHEET_URL, "WEEKSTANDING!B1:B10", data);
                Console.WriteLine("Franchise standing refreshed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async void SendFixtureMessage(bool edit=false)
        {
            var embed = await Fixtures();
            if (embed != null)
            {
                var channel = CommandHandler._client.GetChannel(fixturesChannelId) as IMessageChannel;
                if (edit)
                {
                    var messages = await channel.GetMessagesAsync(1).FlattenAsync();
                    var message = messages.FirstOrDefault();
                    if(message.Author.Id == CommandHandler._client.CurrentUser.Id)
                        await (message as IUserMessage).ModifyAsync(x => x.Embed = embed);
                    else Console.WriteLine("Message is not from the bot");
                }

                else
                {
                    await channel.SendMessageAsync(embed: embed);
                }
            }
        }
        public static async Task<Discord.Embed> Fixtures()
        {
            try
            {
                string week = (ISOWeek.GetWeekOfYear(GetBerlinTime()) - HelperFactory.seasonCalenderWeek).ToString();
                string franchise = DailyTaskScheduler.franchiseName;

                List<string> teamnames = HelperFactory.Franchises.Where(x => x.name == franchise).First().subteams.Select(x => x.name).ToList();
                var manager = SheetHandler.manager;
                var data = await manager.ReadSpreadsheet(SheetHandler.ERS_SHEET_URL, "Fixtures!A2:L901");
                string matches = "";
                int count = 0;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    if (teamnames.Contains(data.GetValue(4, i)) || teamnames.Contains(data.GetValue(5, i)))
                    {
                        if (data.GetValue(2, i) == ("Week " + week))
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
                                if (DateTime.TryParseExact(data.GetValue(7, i) + " " + data.GetValue(8, i),
                                                           "dd/MM/yyyy HH:mm",
                                                           CultureInfo.InvariantCulture,
                                                           DateTimeStyles.None,
                                                           out result))
                                {
                                    // Hier wird angenommen, dass das Datum in Berliner Zeit vorliegt
                                    TimeZoneInfo berlinZone;
                                    try
                                    {
                                        berlinZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                                    }
                                    catch (TimeZoneNotFoundException)
                                    {
                                        berlinZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
                                    }
                                    DateTime resultBerlin = DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
                                    DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(resultBerlin, berlinZone);
                                    DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    long unixTime = (long)(utcTime - epochStart).TotalSeconds;
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
                var franchiseData = HelperFactory.Franchises.FirstOrDefault(x => x.name == franchise);
                var emoteUrl = franchiseData.emoteUrl ?? "";
                var bannerUrl = franchiseData.bannerurl ?? "";
                var logoUrl = franchiseData.logo_url ?? "";
                int hexValue = int.Parse(franchiseData.main_color, System.Globalization.NumberStyles.HexNumber);

                Discord.Color color = new Discord.Color((uint)hexValue);
                var emb = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"{emoteUrl} Fixtures of week " + week)
                .WithDescription(matches)
                .WithCurrentTimestamp()
                .WithFooter(franchiseData.name, iconUrl: logoUrl)
                .Build();

                return emb;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
