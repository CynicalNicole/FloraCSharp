using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using FloraCSharp.Services;
using FloraCSharp.Modules;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Concurrent;

namespace FloraCSharp
{
    public class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly CommandService _commands = new CommandService(new CommandServiceConfig {
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose
        });
        private Configuration _config;
        private FloraDebugLogger _logger = new FloraDebugLogger();
        private FloraRandom _random;
        private Reactions _reactions;
        private BotGameHandler _botGames;

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 1000
            });

            _client.Log += Log;
        }

        public static void Main(string[] args)
            => new Program().AsyncMain().GetAwaiter().GetResult();

        public async Task AsyncMain()
        {
            _config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(@"data/config.json"));
            _random = new FloraRandom();

            _reactions = new Reactions(_random);
            await _reactions.LoadReactionsFromDatabase();

            _botGames = new BotGameHandler(_random, _client, _logger);
            await _botGames.LoadBotGamesFromDB();

            //Command Setup
            await InitCommands();

            var provider = _map.BuildServiceProvider();

            _commands.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            provider.GetRequiredService<CommandHandler>();

            if (_config.RotatingGames)
                await _botGames.HandleGameChange();

            //Block task until program is closed
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }
            

            Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message} {msg.Exception}");
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        private async Task InitCommands()
        {
            //Repeat for all service classes
            _map.AddSingleton(_client);
            _map.AddSingleton(_logger);
            _map.AddSingleton(_random);
            _map.AddSingleton(new Cooldowns());
            _map.AddSingleton(_reactions);
            _map.AddSingleton(_botGames);

            //For each module do the following
            await _commands.AddModuleAsync<NoLifes>();
            await _commands.AddModuleAsync<Misc>();
            await _commands.AddModuleAsync<Administration>();
            await _commands.AddModuleAsync<InfiniteDie>();
            await _commands.AddModuleAsync<CustomReactions>();
            await _commands.AddModuleAsync<CustomRoles>();

            _map.AddSingleton(_commands);
            _map.AddSingleton<CommandHandler>();
            _map.AddSingleton(_config);
        }
    }
}
