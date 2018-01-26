using Discord;
using Discord.Commands;
using FloraCSharp.Modules.Games.Common;
using FloraCSharp.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FloraCSharp.Extensions;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;

namespace FloraCSharp.Modules.Games
{
    public class Games : ModuleBase
    {
        private readonly FloraRandom _random;
        private FloraDebugLogger _logger;
        private readonly BotGameHandler _botGames;
        private ConcurrentDictionary<ulong, RNGHandler> ActiveRNG { get; } = new ConcurrentDictionary<ulong, RNGHandler>();

        public Games(FloraRandom random, FloraDebugLogger logger, BotGameHandler botGames)
        {
            _random = random;
            _logger = logger;
            _botGames = botGames;
        }

        [Command("RNGGame"), Summary("Starts an RNG Game between given bounds. Optionally specifying a timeout in seconds")]
        [RequireContext(ContextType.Guild)]
        public async Task RNGGame([Summary("The minimum, inclusive bound")] int min, [Summary("The maximum, exclusive bound")] int max, int timeout = 30)
        {
            if (ActiveRNG.ContainsKey(Context.Channel.Id))
            {
                await Context.Channel.SendErrorAsync("There is already a game running.");
                return;
            }

            if (timeout > 0)
            {
                timeout = timeout * 1000;
            }
            else
            {
                timeout = 30000;
            }

            RNGGame Game = new RNGGame
            {
                Channel = Context.Channel.Id,
                MinGuess = min,
                MaxGuess = max,
                Guesses = new HashSet<Guess>()
            };

            if (StartRNGG(Game))
            {
                await Context.Channel.SendSuccessAsync($"RNG Game (Min: {min}, Max: {max})", $"Game started! Type your guesses now! You have {timeout/1000} seconds.");
                await Task.Delay(timeout);
                await EndGameInChannel(Context.Guild, Context.Channel);
            }
        }

        [Command("RNGGameEnd"), Summary("Ends the RNG Game in this channel.")]
        [RequireContext(ContextType.Guild)]
        public async Task RNGGameEnd() => await EndGameInChannel(Context.Guild, Context.Channel);

        public async Task EndGameInChannel(IGuild guild, IMessageChannel ChannelID)
        {
            if (!ActiveRNG.ContainsKey(ChannelID.Id))
            {
                await Context.Channel.SendErrorAsync("There is no running game in this channel.");
                return;
            }

            RNGGame game = StopRNGG(ChannelID.Id);

            if (game != null)
            {
                int roll = _random.Next(game.MinGuess, game.MaxGuess + 1);
                if (game.Guesses.Select(x => x.GuessIndex == roll).FirstOrDefault())
                {
                    var winnerID = game.Guesses.First(x => x.GuessIndex == roll);
                    IGuildUser user = await guild.GetUserAsync(winnerID.UserID);

                    await ChannelID.BlankEmbedAsync(new EmbedBuilder().WithOkColour()
                        .AddField(new EmbedFieldBuilder().WithName("🎲 Roll").WithValue(roll))
                        .AddField(new EmbedFieldBuilder().WithName("🎉 Winner").WithValue(user.Username)));
                }
                else
                {
                    await ChannelID.BlankEmbedAsync(new EmbedBuilder().WithErrorColour()
                        .AddField(new EmbedFieldBuilder().WithName("🎲 Roll").WithValue(roll))
                        .AddField(new EmbedFieldBuilder().WithName("🎉 Winner").WithValue("Nobody. That's sad.")));
                }
            }
        }

        public RNGGame StopRNGG(ulong channelID)
        {
            if (ActiveRNG.TryRemove(channelID, out var rngg))
            {
                rngg.OnVoted -= Rh_onvote;
                rngg.End();
                return rngg.Game;
            }
            return null;
        }

        public bool StartRNGG(RNGGame game)
        {
            var rh = new RNGHandler(game, (DiscordSocketClient)Context.Client);
            if (ActiveRNG.TryAdd(game.Channel, rh))
            {
                rh.OnVoted += Rh_onvote;
                return true;
            }
            return false;
        }

        private async Task Rh_onvote(IUserMessage msg, IGuildUser user)
        {
            var toDelete = await msg.Channel.SendSuccessAsync($"{user.Username}: Guess counted");
            toDelete.DeleteAfter(5);
            try { await msg.DeleteAsync(); } catch { }
        }
    }
}
