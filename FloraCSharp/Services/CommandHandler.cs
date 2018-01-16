using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using Discord;
using Nito.AsyncEx;
using System.Collections.Immutable;
using System.Linq;

namespace FloraCSharp
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly Configuration _config;
        private readonly FloraDebugLogger _logger;
        private List<AsyncLazy<IDMChannel>> _ownerChannels;

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
            _ownerChannels = new List<AsyncLazy<IDMChannel>>();

            //Set up DM channels for owners
            foreach (ulong ownerID in _config.Owners)
            {
                _ownerChannels.Add(new AsyncLazy<IDMChannel>(async () => await _discord.GetUser(ownerID).GetOrCreateDMChannelAsync()));
            }

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

            if (context.Channel is IPrivateChannel && !_config.Owners.Contains(context.User.Id))
            {
                await DMHandling(context);
            }
        }

        private async Task DMHandling(SocketCommandContext context)
        {
            foreach (var OwnerChannel in _ownerChannels)
            {
                IDMChannel ownerChannel = await OwnerChannel;
                await ownerChannel.SendSuccessAsync($"DM from [{context.User.Username}{context.User.Discriminator}]({context.User.Id})", context.Message.Content);
            }
        }
    }
}
