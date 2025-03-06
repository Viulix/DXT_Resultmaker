using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DXT_Resultmaker
{
    public class CommandHandler
    {
        public static DiscordSocketClient _client;
        public static InteractionService _interactions;
        public static IServiceProvider _provider;



        public CommandHandler(DiscordSocketClient client, IServiceProvider provider, InteractionService interactions)

        {

            _client = client;
            _client.MessageReceived += OnMessageReceived;
            _client.Ready += Refresh;
            _provider = provider;
            _interactions = interactions;

        }

        public async Task InitializeAsync()
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _interactions.SlashCommandExecuted += SlashCommandExecuted;
            _interactions.ContextCommandExecuted += ContextCommandExecuted;
            _interactions.ComponentCommandExecuted += ComponentCommandExecuted;
            _interactions.AutocompleteHandlerExecuted += AutocompleteLogger;
        }
        private async Task OnMessageReceived(SocketMessage arg)
        {

        }
        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, arg);
                await _interactions.ExecuteCommandAsync(ctx, _provider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
        private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.BadArgs:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.Exception:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    default:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                }
            }

            return Task.CompletedTask;
        }
        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.Exception:
                        Console.WriteLine(arg3.ErrorReason);
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }
        private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }
        private Task AutocompleteLogger(IAutocompleteHandler arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.BadArgs:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.Exception:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        Console.WriteLine(arg3.ErrorReason);
                        break;
                    default:
                        break;
                }
            }
            return Task.CompletedTask;
        }
        private async Task Refresh()
        {
            // await _interactions.RegisterCommandsToGuildAsync(1093943074746531910, true); # test guild
            await _interactions.RegisterCommandsToGuildAsync(690948036540760147, true);
            DailyTaskScheduler.InitializeDailyTask();
            DailyTaskScheduler.InitializeWeeklyTask();
        }
    }
}
