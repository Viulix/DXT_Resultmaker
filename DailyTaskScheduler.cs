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
        private readonly List<ulong> _channelIds; // The list of channel IDs where messages should be sent
        private readonly Dictionary<ulong, ulong> _messageIds; // Maps channelId -> messageId
        private Timer? _hourlyUpdateTimer; // Timer for hourly updates
        private Timer? _weeklyMessageTimer; // Timer for sending new weekly messages
        private readonly TimeZoneInfo _berlinTimeZone; // Berlin timezone
        private int _currentWeek; // Keeps track of the week index
        private readonly ulong _guildId; // The guild ID where the messages will be sent

        public DailyTaskScheduler(List<ulong> channelIds)
        {
            _channelIds = channelIds;
            _messageIds = new Dictionary<ulong, ulong>();
            _berlinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            _currentWeek = 0;
            _guildId = 1093941417061126174; // Default guild ID, can be changed if needed
        }

        /// <summary>
        /// Starts the scheduler. This sets up timers for weekly message creation and hourly updates.
        /// </summary>
        public async Task Start()
        {
            ScheduleWeeklyMessages();
            ScheduleHourlyUpdates();
        }

        /// <summary>
        /// Schedules the sending of new messages every Monday at 14:00 Berlin time.
        /// </summary>
        private void ScheduleWeeklyMessages()
        {
            DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _berlinTimeZone);
            DateTime nextMonday = now.Date.AddDays(((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7);
            DateTime nextMondayAt14 = nextMonday.AddHours(14);

            if (now > nextMondayAt14)
                nextMondayAt14 = nextMondayAt14.AddDays(7); // If already past, schedule for next week

            TimeSpan initialDelay = nextMondayAt14 - now;
            TimeSpan weeklyInterval = TimeSpan.FromDays(7);

            _weeklyMessageTimer = new Timer(async _ => await SendWeeklyMessagesAsync(),
                null, initialDelay, weeklyInterval);
        }

        /// <summary>
        /// Schedules hourly updates of all messages.
        /// </summary>
        private void ScheduleHourlyUpdates()
        {
            DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _berlinTimeZone);
            DateTime nextHour = now.AddHours(1).Date.AddHours(now.AddHours(1).Hour);
            TimeSpan initialDelay = nextHour - now;
            TimeSpan hourlyInterval = TimeSpan.FromHours(1);

            _hourlyUpdateTimer = new Timer(async _ => await UpdateFixtureMessagesAsync(),
                null, initialDelay, hourlyInterval);
        }

        /// <summary>
        /// Sends a new set of weekly messages (every Monday at 14:00).
        /// This will overwrite the message ID list with new messages.
        /// </summary>
        private async Task SendWeeklyMessagesAsync()
        {
            _currentWeek++;

            Console.WriteLine($"[Scheduler] Sending weekly messages for Week {_currentWeek}...");

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

            // Create base embed builder
            var embedBuilder = new Discord.EmbedBuilder()
                .WithColor(HelperFactory.dxtColor)
                .WithTitle($"Fixtures for Week {_currentWeek}");

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
                        embedToSend = embedBuilder.WithDescription(allMessages).Build();
                    }
                    else
                    {
                        // Other channels get only the corresponding single message
                        string singleMessage = index - 1 < messages.Length ? messages[index - 1] : "No data available";
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
        /// </summary>
        /// <summary>
        /// Updates all tracked messages once per hour.
        /// The first channel gets the full fixture list,
        /// all other channels get their respective single message.
        /// </summary>
        private async Task UpdateFixtureMessagesAsync()
        {
            Console.WriteLine("[Scheduler] Updating messages...");

            if (_messageIds.Count == 0)
            {
                Console.WriteLine("[Scheduler] No messages to update.");
                return;
            }

            // Generate all fixture messages again (latest data)
            string allMessages = await HelperFactory.MakeFixtureMessage(_currentWeek);
            var messages = allMessages.Split('ㅤ', StringSplitOptions.RemoveEmptyEntries);

            var client = new ApiClient(HelperFactory.defaultAPIUrl);
            var allMatches = await client.GetMatchesAsync();
            var allFranchises = await client.GetAllFranchisesAsync();
            var franchiseTeams = allFranchises.Where(x => x.Name == HelperFactory.defaultFranchise).FirstOrDefault();
            if (franchiseTeams == null || allMatches == null)
            {
                Console.WriteLine("[Scheduler] API error: Did not find any franchises or matches.");
                return;
            }

            // Filter matches to include only those belonging to the default franchise and within the next 2 hours (+/- 5 minutes)
            var now = DateTime.UtcNow;
            var timeWindowStart = now.AddMinutes(-5);
            var timeWindowEnd = now.AddHours(2).AddMinutes(5);

            var relevantMatches = allMatches.Where(match =>
            {
                // Check if the match belongs to the default franchise
                var isDefaultFranchise = franchiseTeams.Teams.Any(team => team.Id == match.HomeTeamId || team.Id == match.AwayTeamId);

                // Check if the match is within the time window
                var matchTime = match.ScheduledDate;
                var isWithinTimeWindow = matchTime >= timeWindowStart && matchTime <= timeWindowEnd;
                return isDefaultFranchise && isWithinTimeWindow;
            }).ToList();

            if (!relevantMatches.Any())
            {
                Console.WriteLine("[Scheduler] No relevant matches found within the time window. There will not be a reminder.");
                return;
            }

            var guild = CommandHandler._client.Guilds.FirstOrDefault(x => x.Id == _guildId);
            if (guild == null)
            {
                Console.WriteLine("[Scheduler] Guild not found!");
                return;
            }

            // Base embed
            var embedBuilder = new Discord.EmbedBuilder()
                .WithColor(HelperFactory.dxtColor)
                .WithTitle($"Fixtures for Week {_currentWeek}");

            int index = 0;
            foreach (var kvp in _messageIds)
            {
                ulong channelId = kvp.Key;
                ulong messageId = kvp.Value;

                var channel = guild.Channels.FirstOrDefault(x => x.Id == channelId);
                if (channel is Discord.ITextChannel textChannel)
                {
                    try
                    {
                        var message = await textChannel.GetMessageAsync(messageId) as Discord.IUserMessage;
                        if (message == null)
                        {
                            Console.WriteLine($"[Scheduler] Message {messageId} not found in channel {channelId}.");
                            continue;
                        }

                        Discord.Embed newEmbed;
                        if (index == 0)
                        {
                            // First channel = full message
                            newEmbed = embedBuilder.WithDescription(allMessages).Build();
                        }
                        else
                        {
                            // Other channels = single message
                            string singleMessage = index - 1 < messages.Length ? messages[index - 1] : "No data available";
                            newEmbed = embedBuilder.WithDescription(singleMessage).Build();
                        }

                        await message.ModifyAsync(msg => msg.Embed = newEmbed);
                        Console.WriteLine($"[Scheduler] Updated message {messageId} in channel {channelId}");

                        if (relevantMatches.Where(x => x.TierId == index + 35).Any())
                        {
                            var upcomingMatch = relevantMatches.FirstOrDefault(x => x.TierId == index + 35);
                            if (upcomingMatch != null)
                            {
                                var opponentTeam = allFranchises
                                    .SelectMany(f => f.Teams)
                                    .FirstOrDefault(t => t.Id == (upcomingMatch.HomeTeamId == franchiseTeams.Teams.FirstOrDefault(ft => ft.Id == upcomingMatch.HomeTeamId)?.Id
                                        ? upcomingMatch.AwayTeamId
                                        : upcomingMatch.HomeTeamId))?.Name;

                                await textChannel.SendMessageAsync(
                                    $"**Reminder 📢:** *Upcoming match against **{opponentTeam}**. Date:* {HelperFactory.ToDiscordTimestamp(upcomingMatch.ScheduledDate)}");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Scheduler] Failed to update message {messageId} in channel {channelId}: {ex.Message}");
                    }
                }

                index++;
            }
        }


        /// <summary>
        /// Dispose timers when shutting down the scheduler.
        /// </summary>
        public void Dispose()
        {
            _hourlyUpdateTimer?.Dispose();
            _weeklyMessageTimer?.Dispose();
        }
    }
}
