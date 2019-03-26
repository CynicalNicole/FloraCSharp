using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using FloraCSharp.Extensions;
using FloraCSharp.Services.Database.Models;

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

            _discord.MessageDeleted += DeletedAsync;
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

        private async Task DeletedAsync(Cacheable<IMessage, ulong> CacheableMessage, ISocketMessageChannel origChannel)
        {
            var CachedMessage = await CacheableMessage.GetOrDownloadAsync();
            var MessageChannel = (ITextChannel)origChannel;

            if (CachedMessage.Source != MessageSource.User) return;
            if (MessageChannel.Guild == null) return;
            if (CachedMessage == null) return;

            Guild G = null;
            List<BlockedLogs> BLs = new List<BlockedLogs>();
            using (var uow = DBHandler.UnitOfWork())
            {
                if (!uow.Guild.IsDeleteLoggingEnabled(MessageChannel.Guild.Id)) return;
                G = uow.Guild.GetOrCreateGuild(MessageChannel.Guild.Id);
                if (G.GuildID == 0) return;
                BLs = uow.BlockedLogs.GetServerBlockedLogs(MessageChannel.Guild.Id);
            }

            if (BLs != null && BLs.Count > 0)
            {
                if (BLs.Any(x => CachedMessage.Content.StartsWith(x.BlockedString))) return;
            }

            if (G == null) return;

            var ChannelToSend = (IMessageChannel)_discord.GetChannel(G.DeleteLogChannel);

            string content = CachedMessage.Content;
            if (content == "") content = "*original message was blank*";

            EmbedBuilder embed = new EmbedBuilder().WithAuthor(eab => eab.WithIconUrl(CachedMessage.Author.GetAvatarUrl()).WithName(CachedMessage.Author.Username)).WithOkColour()
                                                    .AddField(efb => efb.WithName("Channel").WithValue("#" + origChannel.Name).WithIsInline(true))
                                                    .AddField(efb => efb.WithName("MessageID").WithValue(CachedMessage.Id).WithIsInline(true))
                                                    .AddField(efb => efb.WithName("UserID").WithValue(CachedMessage.Author.Id).WithIsInline(true))
                                                    .AddField(efb => efb.WithName("Message").WithValue(content));

            string footerText = "Created: " + CachedMessage.CreatedAt.ToString();

            if (CachedMessage.EditedTimestamp != null) footerText += $" | Edited: " + CachedMessage.EditedTimestamp.ToString();

            EmbedFooterBuilder footer = new EmbedFooterBuilder().WithText(footerText);

            await ChannelToSend.BlankEmbedAsync(embed.WithFooter(footer));

            if (CachedMessage.Attachments.Count == 0) return;

            foreach (var attatchment in CachedMessage.Attachments)
            {
                await ChannelToSend.SendMessageAsync($"Message ID: {CachedMessage.Id} has attachment: {attatchment.Url}");
            }
        }
    }
}
