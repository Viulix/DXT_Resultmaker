using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DXT_Resultmaker
{
    /// <summary>
    /// Manages scheduled Discord tasks, such as sending and updating weekly messages.
    /// </summary>
    public class DailyTaskScheduler : IDisposable
    {
        private static List<ulong> _channelIds = []; // The list of channel IDs where messages should be sent
        private readonly Dictionary<ulong, ulong> _messageIds = []; // Maps channelId -> messageId
        private Timer? _updateTimer;
        private Timer? _weeklyMessageTimer;
        private readonly TimeZoneInfo _berlinTimeZone; // Berlin timezone
        private int _currentWeek; // Keeps track of the week index
        private static ulong _guildId; // The guild ID where the messages will be sent

        private TimeSpan _updateInterval = TimeSpan.FromMinutes(1);
        private DayOfWeek _weeklyDay = DayOfWeek.Saturday;
        private TimeSpan _weeklyTime = new(23, 30, 0);

        public async Task Start()
        {
            _currentWeek = GetCurrentWeek(HelperFactory.SeasonStart);
            ScheduleWeeklyMessages();
            ScheduleUpdates();
        }

        public void SetUpdateInterval(TimeSpan interval)
        {
            _updateInterval = interval;
            ScheduleUpdates(); // Neu starten
        }

        public void SetWeeklyTime(DayOfWeek day, TimeSpan time)
        {
            Console.WriteLine($"[Scheduler{DateTime.Now}] Setting weekly message time to {day} at {time}");
            _weeklyDay = day;
            _weeklyTime = time;
            ScheduleWeeklyMessages(); // Neu starten
        }

        private void ScheduleWeeklyMessages()
        {
            _weeklyMessageTimer?.Dispose();
            DateTime now = DateTime.Now;

            int daysUntilTarget = ((_weeklyDay - now.DayOfWeek + 7) % 7);
            DateTime nextTarget = now.Date.AddDays(daysUntilTarget).Add(_weeklyTime);

            if (now > nextTarget)
                nextTarget = nextTarget.AddDays(7);

            TimeSpan initialDelay = nextTarget - now;
            TimeSpan weeklyInterval = TimeSpan.FromDays(7);

            _weeklyMessageTimer = new Timer(async _ => await SendWeeklyMessagesAsync(),
                null, initialDelay, weeklyInterval);
        }

        private void ScheduleUpdates()
        {
            _updateTimer?.Dispose();

            DateTime now = DateTime.Now;
            DateTime nextRun = now.Add(_updateInterval);
            TimeSpan initialDelay = nextRun - now;

            _updateTimer = new Timer(async _ => await UpdateFixtureMessagesAsync(),
                null, initialDelay, _updateInterval);
        }

        // Manual triggers
        public async Task RunWeeklyNow() => await SendWeeklyMessagesAsync();
        public async Task RunUpdateNow() => await UpdateFixtureMessagesAsync();

        /// <summary>
        /// Sends a new set of weekly messages (every Monday at 14:00).
        /// This will overwrite the message ID list with new messages.
        /// </summary>
        private async Task SendWeeklyMessagesAsync()
        {
            _channelIds = HelperFactory.SaveData.ChannelIds;
            _guildId = GetGuildId();

            _currentWeek++;

            Console.WriteLine($"[Scheduler {DateTime.Now}] Sending weekly messages for Week {_currentWeek}...");

            if(_channelIds.Count == 0)
            {
                Console.WriteLine("[Scheduler] No channels configured for weekly messages.");
                return;
            }

            if(_messageIds.Count > 0)
                _messageIds.Clear();

            // Generate all fixture messages
            string allMessages = await HelperFactory.MakeFixtureMessage(_currentWeek);
            var messages = allMessages.Split('ㅤ', StringSplitOptions.RemoveEmptyEntries);

            var guild = CommandHandler._client.Guilds.FirstOrDefault(x => x.Id == _guildId);
            if (guild == null)
            {
                Console.WriteLine("[Scheduler] Guild not found!");
                return;
            }
            // Get banner and logo from default franchise
            var franchise = HelperFactory.SaveData.Franchises
                .FirstOrDefault(x => x.Name == HelperFactory.SaveData.DefaultFranchise);
            var bannerUrl = franchise?.Banner ?? " "; 
            var logoUrl = franchise?.Logo ?? " "; 


            // Create base embed builder
            var embedBuilder = new Discord.EmbedBuilder()
                .WithColor(new Discord.Color(HelperFactory.SaveData.MainColor))
                .WithTitle($"Fixtures for Week {_currentWeek}")
                .WithImageUrl(bannerUrl)
                .WithThumbnailUrl(logoUrl);
            int index = 0;
            foreach (ulong channelId in _channelIds)
            {
                var channel = guild.Channels.FirstOrDefault(x => x.Id == channelId);
                if (channel is Discord.ITextChannel textChannel)
                {
                    Discord.Embed embedToSend;

                    if (index == 0)
                    {
                        // First channel gets the full message as description
                        embedToSend = embedBuilder.WithDescription(HelperFactory.RemoveCaptainLine(allMessages)).Build();
                    }
                    else
                    {
                        // Other channels get only the corresponding single message
                        string singleMessage = index - 1 < messages.Length ? messages[index - 1] : "No data available";
                        var tierColor = HelperFactory.GetTierColor(index + 35);
                        embedBuilder.Color = tierColor;
                        embedToSend = embedBuilder.WithDescription(singleMessage).Build();
                    }

                    // Send the message to Discord
                    var sentMessage = await textChannel.SendMessageAsync(embed: embedToSend);

                    // Store the messageId for later updates
                    _messageIds[channelId] = sentMessage.Id;

                    Console.WriteLine($"[Scheduler] Sent message to channel {channelId}, messageId={sentMessage.Id}");
                }
                else
                {
                    Console.WriteLine($"[Scheduler] Channel {channelId} not found or not a text channel.");
                }
                Thread.Sleep(50);
                index++;
            }
        }

        /// <summary>
        /// Updates all tracked messages once per hour.
        /// The first channel gets the full fixture list,
        /// all other channels get their respective single message.
        /// </summary>
        private async Task UpdateFixtureMessagesAsync()
        {
            Console.WriteLine($"[Scheduler: {DateTime.Now}] Updating messages...");
            _channelIds = HelperFactory.SaveData.ChannelIds;
            _guildId = GetGuildId();

            if (_messageIds.Count == 0)
            {
                Console.WriteLine("[Scheduler] No messages to update.");
                return;
            }

            // Generate all fixture messages again (latest data)
            string allMessages = await HelperFactory.MakeFixtureMessage(_currentWeek);
            var messages = allMessages.Split('ㅤ', StringSplitOptions.RemoveEmptyEntries);

            var client = new ApiClient(HelperFactory.DefaultAPIUrl);
            var allMatches = await client.GetMatchesAsync();

            var franchise = HelperFactory.SaveData.Franchises
                .FirstOrDefault(x => x.Name == HelperFactory.SaveData.DefaultFranchise);
            var bannerUrl = franchise?.Banner ?? " ";
            var logoUrl = franchise?.Logo ?? " ";
            // Filter matches to include only those belonging to the default franchise and within the next 2 hours (+/- 5 minutes)
            var now = DateTime.Now;
            var timeWindowStart = now.AddMinutes(-5);
            var timeWindowEnd = now.AddHours(2).AddMinutes(5);

            var relevantMatches = allMatches.Where(match =>
            {
                // Check if the match belongs to the default franchise
                var isDefaultFranchise = franchise.Teams.Any(team => team.Id == match.HomeTeamId || team.Id == match.AwayTeamId);
                // Check if the match is within the time window
                var matchTime = match.ScheduledDate;
                var isWithinTimeWindow = matchTime >= timeWindowStart && matchTime <= timeWindowEnd;
                return isDefaultFranchise && isWithinTimeWindow;
            }).ToList();

            var guild = CommandHandler._client.Guilds.FirstOrDefault(x => x.Id == _guildId);
            if (guild == null)
            {
                Console.WriteLine("[Scheduler] Guild not found!");
                return;
            }

            // Base embed
            var embedBuilder = new Discord.EmbedBuilder()
                .WithColor(HelperFactory.SaveData.MainColor)
                .WithTitle($"Fixtures for Week {_currentWeek}")
                .WithImageUrl(bannerUrl)
                .WithThumbnailUrl(logoUrl);

            foreach (var kvp in _messageIds)
            {
                ulong channelId = kvp.Key;
                ulong messageId = kvp.Value;

                var channel = guild.Channels.FirstOrDefault(x => x.Id == channelId);
                if (channel is Discord.ITextChannel textChannel)
                {
                    try
                    {
                        if (await textChannel.GetMessageAsync(messageId) is not Discord.IUserMessage message)
                        {
                            Console.WriteLine($"[Scheduler] Message {messageId} not found in channel {channelId}.");
                            continue;
                        }

                        // Ermittle den Kanalindex anhand der ChannelId aus Savedata
                        int channelIndex = _channelIds.IndexOf(channelId);
                        if (channelIndex < 0)
                            channelIndex = 0;

                        Discord.Embed newEmbed;
                        if (channelIndex == 0)
                        {
                            // Erstkanal: Vollständige Nachricht
                            newEmbed = embedBuilder.WithDescription(allMessages).Build();
                        }
                        else
                        {
                            // Andere Kanäle: Einzelne Nachricht basierend auf dem ChannelIndex
                            string singleMessage = (channelIndex - 1 < messages.Length) ? messages[channelIndex - 1] : "No data available";
                            var tierColor = HelperFactory.GetTierColor(channelIndex + 35);
                            embedBuilder.Color = tierColor;
                            newEmbed = embedBuilder.WithDescription(singleMessage).Build();
                        }

                        await message.ModifyAsync(msg => msg.Embed = newEmbed);
                        Console.WriteLine($"[Scheduler] Updated message {messageId} in channel {channelId}");

                        var teamIds = HelperFactory.SaveData.Franchises
                            .FirstOrDefault(x => x.Name == HelperFactory.SaveData.DefaultFranchise)?
                            .Teams.Select(x => x.Id)
                            .ToList() ?? new List<int>();

                        if (relevantMatches.Any(x => x.TierId == channelIndex + 35))
                        {
                            var upcomingMatch = relevantMatches.FirstOrDefault(x => x.TierId == channelIndex + 35);
                            if (upcomingMatch != null)
                            {
                                var allTeams = HelperFactory.SaveData.Franchises.SelectMany(x => x.Teams).ToList();
                                var allIds = allTeams.Select(y => y.Id).ToList();
                                var opponentTeamName = "N/V";
                                var opponentTeam = new Team();
                                if (allIds.Contains(upcomingMatch.HomeTeamId ?? 0) && !teamIds.Contains(upcomingMatch.HomeTeamId ?? 0))
                                {
                                    opponentTeam = allTeams.FirstOrDefault(x => x.Id == upcomingMatch.HomeTeamId);
                                    opponentTeamName = opponentTeam?.Name ?? "N/V";
                                }
                                else if (allIds.Contains(upcomingMatch.AwayTeamId ?? 0) && !teamIds.Contains(upcomingMatch.AwayTeamId ?? 0))
                                {
                                    opponentTeam = allTeams.FirstOrDefault(x => x.Id == upcomingMatch.AwayTeamId);
                                    opponentTeamName = opponentTeam?.Name ?? "N/V";
                                }

                                string roleMention = "";
                                if (channelIndex > 0 && HelperFactory.SaveData.RoleIds.Count >= channelIndex)
                                {
                                    roleMention = HelperFactory.SaveData.RoleIds[channelIndex - 1] != 0 ? $"<@&{HelperFactory.SaveData.RoleIds[channelIndex - 1]}>" : "";
                                }
                                await textChannel.SendMessageAsync(
                                    $"**Reminder 📢:** *Upcoming match against **{opponentTeamName}**. Date:* {HelperFactory.ToDiscordTimestamp(upcomingMatch.ScheduledDate)} {roleMention}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Scheduler] Failed to update message {messageId} in channel {channelId}: {ex.Message}");
                    }
                }
            }
        }
        public void SetCurrentWeek(int week)
        {
            _currentWeek = week;
        }
        public static int GetCurrentWeek(DateTime seasonStart)
        {
            // Heute in Berlin
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"));

            // Differenz in Tagen
            var diffDays = (now.Date - seasonStart.Date).Days;

            // Ganze Wochen seit SeasonStart + 1 (erste Woche = 1, nicht 0)
            return (diffDays / 7) + 1;
        }



        /// <summary>
        /// Dispose timers when shutting down the scheduler.
        /// </summary>
        public void Dispose()
        {
            _updateTimer?.Dispose();
            _weeklyMessageTimer?.Dispose();
        }
        public static ulong GetGuildId()
        {
            foreach (var guild in CommandHandler._client.Guilds)
            {
                foreach (var channel in guild.Channels)
                {
                    if (_channelIds.Contains(channel.Id)) return guild.Id;
                }
            }
            return 0;
        }
    }
}
