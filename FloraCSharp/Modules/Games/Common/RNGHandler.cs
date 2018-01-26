using Discord;
using FloraCSharp.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;

namespace FloraCSharp.Modules.Games.Common
{
    class RNGHandler
    {
        public event Func<IUserMessage, IGuildUser, Task> OnVoted;
        public RNGGame Game { get; }
        private DiscordSocketClient _client;

        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);

        public RNGHandler(RNGGame game, DiscordSocketClient client)
        {
            Game = game;
            _client = client;
            _client.MessageReceived += TryGuess;
        }

        public async Task<bool> TryGuess(SocketMessage msg)
        {
            Guess G;
            await _locker.WaitAsync().ConfigureAwait(false);
            try
            {
                if (msg == null || msg.Author.IsBot || msg.Channel.Id != Game.Channel)
                    return false;

                if (!int.TryParse(msg.Content, out int vote))
                    return false;

                if (vote < Game.MinGuess || vote > Game.MaxGuess)
                    return false;

                var usr = msg.Author as IGuildUser;
                if (usr == null)
                    return false;

                G = new Guess()
                {
                    UserID = msg.Author.Id,
                    GuessIndex = vote
                };

                if (Game.Guesses.Any(x => x.UserID == msg.Author.Id))
                    return false;

                if (!Game.Guesses.Add(G))
                    return false;

                var _ = OnVoted?.Invoke(msg as IUserMessage, usr);
            }
            finally { _locker.Release(); }
            return true;
        }

        public void End()
        {
            _client.MessageReceived -= TryGuess;
            OnVoted = null;
        }
    }
}
