using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    public class BotGameHandler
    {
        private List<string> _botGames = new List<string>();
        private readonly FloraRandom _random;
        private readonly DiscordSocketClient _client;
        private readonly FloraDebugLogger _logger;

        public BotGameHandler(FloraRandom random, DiscordSocketClient client, FloraDebugLogger logger)
        {
            _random = random;
            _client = client;
            _logger = logger;
        }

        public async Task LoadBotGamesFromDB()
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var gamesList = await uow.BotGames.LoadBotGames();
                foreach (var botGame in gamesList)
                {
                    _botGames.Add(botGame.GameName);
                }
            }
        }

        public async Task HardReload()
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var gamesList = await uow.BotGames.LoadBotGames();
                List<string> temp = new List<string>();
                foreach (var botGame in gamesList)
                {
                    temp.Add(botGame.GameName);
                }

                _botGames = new List<string>(temp);
            }
        }

        public async Task<int> AddGame(string gameName)
        {
            int reactID = -1;
            using (var uow = DBHandler.UnitOfWork())
            {
                var BG = new Database.Models.BotGames()
                {
                    GameName = gameName
                };
                uow.BotGames.Add(BG);
                await uow.CompleteAsync();

                reactID = BG.ID;
            }

            _botGames.Add(gameName);

            return reactID;
        }

        public async Task RemoveBotGame(string gameName)
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var allBotGames = await uow.BotGames.LoadBotGames();
                var gamesToDelete = allBotGames.Where(x => x.GameName.ToLower() == gameName.ToLower()).ToArray();

                uow.BotGames.RemoveRange(gamesToDelete);
                await uow.CompleteAsync();
            }

            await HardReload();
        }

        public async Task RemoveBotGameByID(int id)
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                uow.BotGames.Remove(id);
                await uow.CompleteAsync();
            }

            await HardReload();
        }

        public async Task HandleGameChange()
        {
            _logger.Log("Starting game rotation", "RotatingGames");
            while (_client.LoginState == Discord.LoginState.LoggedIn)
            {
                if (_botGames.Count == 0)
                    return;
                _logger.Log("Setting game", "RotatingGames");
                await _client.SetGameAsync(_botGames[_random.Next(_botGames.Count)]);
                await Task.Delay(300000);
            }
        }
    }
}
