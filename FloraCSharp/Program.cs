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
using FloraCSharp.Extensions;
using FloraCSharp.Modules.Games;
using System.Linq;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.Threading;

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

        CancellationTokenSource m_ctSource;

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
            _config.Shutdown = false;
            _random = new FloraRandom();

            _reactions = new Reactions(_random);
            await _reactions.LoadReactionsFromDatabase();

            _botGames = new BotGameHandler(_random, _client, _logger);
            await _botGames.LoadBotGamesFromDB();

            //Services
            await InitServices();

            var provider = _map.BuildServiceProvider();

            //Command Setup
            await InitCommands(provider);            

            _commands.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync(); 

            provider.GetRequiredService<CommandHandler>();
            //provider.GetRequiredService<StartupHandler>();
            provider.GetRequiredService<ReactionHandler>();
            provider.GetRequiredService<ImageRateLimitHandler>();
            provider.GetRequiredService<ReactionHandler>();

            _logger.Log("Updating User List", "Startup");

            foreach (SocketGuild guild in _client.Guilds)
            {
                await guild.DownloadUsersAsync();
            }

            if (_config.RotatingGames)
            {
                _logger.Log("Starting game rotation", "RotatingGames");
                WorkingTask();
            }    

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

        private async Task InitServices()
        {
            //Repeat for all service classes
            _map.AddSingleton(_client);
            _map.AddSingleton(_logger);
            _map.AddSingleton(_random);
            _map.AddSingleton(new Cooldowns());
            _map.AddSingleton(new WoodcuttingLocker());
            _map.AddSingleton(new HeartLocker());
            _map.AddSingleton(new NameLocker());
            _map.AddSingleton(_reactions);
            _map.AddSingleton(_botGames);

            _map.AddSingleton(_commands);
            _map.AddSingleton<CommandHandler>();
            //_map.AddSingleton<StartupHandler>();
            _map.AddSingleton<ReactionHandler>();
            _map.AddSingleton<ImageRateLimitHandler>();
            _map.AddSingleton<ReactionHandler>();
            _map.AddSingleton(_config);
        }

        private async Task InitCommands(ServiceProvider prov)
        {
            //For each module do the following
            await _commands.AddModuleAsync<NoLifes>(prov);
            await _commands.AddModuleAsync<Misc>(prov);
            await _commands.AddModuleAsync<Administration>(prov);
            await _commands.AddModuleAsync<InfiniteDie>(prov);
            await _commands.AddModuleAsync<CustomReactions>(prov);
            await _commands.AddModuleAsync<CustomRoles>(prov);
            await _commands.AddModuleAsync<Games>(prov);
            await _commands.AddModuleAsync<Money>(prov);
            await _commands.AddModuleAsync<DnD>(prov);
            await _commands.AddModuleAsync<Cyphers>(prov);
        }

        public void WorkingTask()
        {
            m_ctSource = new CancellationTokenSource();

            Task.Delay(1000).ContinueWith(async (x) =>
            {
                await _botGames.HandleGameChange(_config.RotationDelay);

                if (_client.ConnectionState == ConnectionState.Connected)
                    WorkingTask();
            }, m_ctSource.Token);
        }
    }
}
