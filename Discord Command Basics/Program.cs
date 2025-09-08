using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DXT_Resultmaker.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DXT_Resultmaker
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        public static DailyTaskScheduler DailyTaskScheduler { get; private set; } = new DailyTaskScheduler();

        public async Task MainAsync()
        {
            // Read Save Data
            string token = File.ReadAllText("./token.txt");

            // Load the save data in HelperFactory
            HelperFactory.LoadSaveData();
            var franchiseNames = HelperFactory.SaveData.Franchises.Select(x => x.Name).ToList();
            if (franchiseNames.Count > 0)
            {
                HelperFactory.Franchises = franchiseNames;
            }

            // Set the static properties outside helper factory
            AdminModule.Admins = HelperFactory.SaveData.Admins;
            AdminModule.TierDiscordRoleId = HelperFactory.SaveData.RoleIds;
            DailyTaskScheduler.SetUpdateInterval(TimeSpan.FromMinutes(HelperFactory.SaveData.UpdateInterval));
            DailyTaskScheduler.SetWeeklyTime(HelperFactory.SaveData.StartDate.DayOfWeek, HelperFactory.SaveData.StartDate.TimeOfDay);
            DailyTaskScheduler.SetReminderMinutes(HelperFactory.SaveData.ReminderMinutes);
            // Start with configuration and booting
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
                LogGatewayIntentWarnings = false

            };
            // Services
            var services = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>(new DiscordSocketClient(config))
                .AddSingleton<InteractionService>(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>();

            var serviceProvider = services.BuildServiceProvider();
            var _commandService = serviceProvider.GetRequiredService<InteractionService>();
            var _client = serviceProvider.GetRequiredService<DiscordSocketClient>();

            //SheetHandler.manager = new SheetManager("./key.json", scopes);


            // Commands
            var commands = serviceProvider.GetRequiredService<InteractionService>();
            await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetGameAsync("with you!", null);
            await DailyTaskScheduler.Start();
            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
