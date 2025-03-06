using BallchasingSharp;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DXT_Resultmaker.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DXT_Resultmaker
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();


        public async Task MainAsync()
        {
            // Read Save Data
            string json = File.ReadAllText("./bot.json");
            SaveData item = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(json);
            AdminModule.admins = item.admins;
            SheetHandler.ERS_SHEET_URL = item.database_sheet;
            SheetHandler.DXT_SHEET_URL = item.dxt_sheet;

            HelperFactory.Franchises = item.franchises;


            // Start with configuration and booting
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
                LogGatewayIntentWarnings = false

            };
            // Konfigurationsdatei für Google Sheets
            var scopes = new[] { Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets };

            // Services
            var services = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>(new DiscordSocketClient(config))
                .AddSingleton<InteractionService>()
                .AddSingleton<CommandHandler>();

            var serviceProvider = services.BuildServiceProvider();
            var _commandService = serviceProvider.GetRequiredService<InteractionService>();
            var _client = serviceProvider.GetRequiredService<DiscordSocketClient>();

            SheetHandler.manager = new SheetManager("./key.json", scopes);


            // Commands
            var commands = serviceProvider.GetRequiredService<InteractionService>();
            await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, item.token);
            await _client.StartAsync();
            await _client.SetGameAsync("with you!", null);


            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }
    }
}
