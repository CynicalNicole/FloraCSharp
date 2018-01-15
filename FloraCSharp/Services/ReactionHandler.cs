using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    class ReactionHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;
        private readonly Configuration _config;
        private readonly Reactions _reactions;

        public ReactionHandler(
            DiscordSocketClient discord,
            Configuration config,
            Reactions reactions,
            IServiceProvider provider)
        {
            _discord = discord;
            _provider = provider;
            _config = config;
            _reactions = reactions;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            if (msg.Author.Id == _discord.CurrentUser.Id) return;

            var context = new SocketCommandContext(_discord, msg);
            
            if (_reactions.Reacts.ContainsKey(msg.ToString()))
            {
                await context.Channel.SendMessageAsync(_reactions.Reacts[msg.ToString()]);
            }
        }
    }
}
