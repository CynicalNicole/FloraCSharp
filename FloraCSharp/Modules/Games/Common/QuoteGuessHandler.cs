using Discord;
using Discord.WebSocket;
using FloraCSharp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FloraCSharp.Modules.Games.Common
{
    class QuoteGuessHandler
    {
        public event Func<IUserMessage, IGuildUser, Task> OnVoted;
        public QuoteGame Game { get; }
        private DiscordSocketClient _client;

        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);

        public QuoteGuessHandler(QuoteGame game, DiscordSocketClient client)
        {
            Game = game;
            _client = client;
            _client.MessageReceived += TryGuess;
        }

        public async Task<bool> TryGuess(SocketMessage msg)
        {
            QGuess G;
            await _locker.WaitAsync().ConfigureAwait(false);
            try
            {
                if (msg == null || msg.Author.IsBot || msg.Channel.Id != Game.Channel)
                    return false;

                string vote = msg.Content.ToUpper();

                var usr = msg.Author as IGuildUser;
                if (usr == null)
                    return false;

                G = new QGuess()
                {
                    UserID = msg.Author.Id,
                    QuoteGuess = vote,
                    Timestamp = DateTime.UtcNow.Ticks
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
