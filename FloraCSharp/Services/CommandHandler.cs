using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FloraCSharp.Extensions;
using FloraCSharp.Services;

namespace FloraCSharp
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly Configuration _config;
        private readonly FloraDebugLogger _logger;

        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            Configuration config,
            IServiceProvider provider,
            FloraDebugLogger logger)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _config = config;
            _logger = logger;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            if (msg.Author.Id == _discord.CurrentUser.Id) return;

            var context = new SocketCommandContext(_discord, msg);

            int argPos = 0;
            if (msg.HasStringPrefix(_config.Prefix, ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess && !(result.Error.ToString() == "UnknownCommand"))
                    await context.Channel.SendErrorAsync(result.ToString());
            }
        }
    }
}
