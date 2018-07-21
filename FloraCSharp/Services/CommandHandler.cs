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
        private readonly Reactions _reactions;
        private List<AsyncLazy<IDMChannel>> _ownerChannels;
        private readonly FloraRandom _random;

        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            Configuration config,
            IServiceProvider provider,
            FloraDebugLogger logger,
            Reactions reactions,
            FloraRandom random)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _config = config;
            _logger = logger;
            _reactions = reactions;
            _random = random;
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
            if (msg.HasStringPrefix(_config.Prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess && !(result.Error.ToString() == "UnknownCommand"))
                    await context.Channel.SendErrorAsync(result.ToString());

                return;
            }

            if (context.Channel is IPrivateChannel && !_config.Owners.Contains(context.User.Id))
            {
                await DMHandling(context);
                return;
            }

            string reaction = _reactions.GetReactionOrNull(context.Message.Content.ToLower());
            if (reaction != null)
            {
                await context.Channel.SendMessageAsync(reaction);
                return;
            }

            if (context.Guild.Id == 199658366421827584 && context.Channel.Id != 199658366421827584)
            {
                int rng = _random.Next(0, 100);
                if (rng > 95)
                {
                    await context.Message.AddReactionAsync(context.Guild.Emotes.RandomItem());
                }
            }
        }

        private async Task DMHandling(SocketCommandContext context)
        {
            foreach (var OwnerChannel in _ownerChannels)
            {
                IDMChannel ownerChannel = await OwnerChannel;
                await ownerChannel.SendSuccessAsync($"DM from [{context.User.Username}] | {context.User.Id}", context.Message.Content);
            }
        }
    }
}
