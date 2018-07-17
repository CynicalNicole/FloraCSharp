using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    public class ReactionHandler
    {
        private readonly DiscordSocketClient _discord;

        public ReactionHandler(
            DiscordSocketClient discord)
        {
            _discord = discord;

            _discord.ReactionAdded += _discord_ReactionAdded;
        }

        private async Task _discord_ReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var msg = await arg1.GetOrDownloadAsync();

            if (!msg.Channel.IsNsfw) return;

            if (msg.Reactions[arg3.Emote].IsMe) return;

            await msg.AddReactionAsync(arg3.Emote);
        }
    }
}
