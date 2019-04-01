using Discord;
using Discord.WebSocket;
using FloraCSharp.Extensions;
using FloraCSharp.Modules.Games.Common;
using FloraCSharp.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Modules.Games.Services
{
    class QuoteGuessService
    {
        private ConcurrentDictionary<ulong, QuoteGuessHandler> ActiveQG { get; } = new ConcurrentDictionary<ulong, QuoteGuessHandler>();

        public async Task EndGameInChannel(IGuild guild, IMessageChannel ChannelID)
        {
            QuoteGame game = StopQG(ChannelID.Id);

            if (game != null)
            {
                if (game.Guesses.Select(x => x.QuoteGuess == game.Answer).FirstOrDefault())
                {
                    var winnerID = game.Guesses.OrderBy(x => x.Timestamp).First(x => x.QuoteGuess == game.Answer);
                    IGuildUser user = await guild.GetUserAsync(winnerID.UserID);

                    await ChannelID.BlankEmbedAsync(new EmbedBuilder().WithOkColour()
                        .AddField(new EmbedFieldBuilder().WithName("📣 Keyword").WithValue(game.Answer))
                        .AddField(new EmbedFieldBuilder().WithName("🎉 Winner").WithValue(user.Username)).Build());
                }
                else
                {
                    await ChannelID.BlankEmbedAsync(new EmbedBuilder().WithErrorColour()
                        .AddField(new EmbedFieldBuilder().WithName("📣 Keyword").WithValue(game.Answer))
                        .AddField(new EmbedFieldBuilder().WithName("🎉 Winner").WithValue("Nobody. That's sad.")).Build());
                }
            }
        }

        public QuoteGame StopQG(ulong channelID)
        {
            if (ActiveQG.TryRemove(channelID, out var rngg))
            {
                rngg.OnVoted -= Rh_onvote;
                rngg.End();
                return rngg.Game;
            }
            return null;
        }

        public bool StartQG(QuoteGame game, DiscordSocketClient client)
        {
            var rh = new QuoteGuessHandler(game, client);
            if (ActiveQG.TryAdd(game.Channel, rh))
            {
                rh.OnVoted += Rh_onvote;
                return true;
            }
            return false;
        }

        private async Task Rh_onvote(IUserMessage msg, IGuildUser user)
        {
            var toDelete = await msg.Channel.SendSuccessAsync($"{user.Username}: Guess saved.");
            toDelete.DeleteAfter(5);
            try { await msg.DeleteAsync(); } catch { }
        }
    }
}
