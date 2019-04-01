using Discord;
using Discord.WebSocket;
using FloraCSharp.Extensions;
using FloraCSharp.Modules.Games.Common;
using FloraCSharp.Services;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace FloraCSharp.Modules.Games.Services
{
    public class RNGService
    {
        private ConcurrentDictionary<ulong, RNGHandler> ActiveRNG { get; } = new ConcurrentDictionary<ulong, RNGHandler>();

        public async Task EndGameInChannel(IGuild guild, IMessageChannel ChannelID, FloraRandom _random)
        {
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
                        .AddField(new EmbedFieldBuilder().WithName("🎉 Winner").WithValue(user.Username)).Build());
                }
                else
                {
                    await ChannelID.BlankEmbedAsync(new EmbedBuilder().WithErrorColour()
                        .AddField(new EmbedFieldBuilder().WithName("🎲 Roll").WithValue(roll))
                        .AddField(new EmbedFieldBuilder().WithName("🎉 Winner").WithValue("Nobody. That's sad.")).Build());
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

        public bool StartRNGG(RNGGame game, DiscordSocketClient client)
        {
            var rh = new RNGHandler(game, client);
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
