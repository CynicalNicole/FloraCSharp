using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace FloraCSharp.Services
{
    public class ReactionHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly Configuration _config;

        public ReactionHandler(
            DiscordSocketClient discord,
            Configuration config)
        {
            _discord = discord;
            _config = config;

            _discord.ReactionAdded += _discord_ReactionAdded;
        }

        private async Task _discord_ReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var msg = await arg1.GetOrDownloadAsync();

            if (msg.Reactions[arg3.Emote].IsMe) return;

            if (!msg.Channel.IsNsfw)
            {
                await msg.AddReactionAsync(arg3.Emote);
                return;
            }
            
            if (_config.Owners.Contains(msg.Author.Id))
            {
                await msg.AddReactionAsync(arg3.Emote);
                return;
            }
        }
    }
}
