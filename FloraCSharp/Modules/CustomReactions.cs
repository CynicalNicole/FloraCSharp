using Discord.Commands;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    public class CustomReactions : ModuleBase
    {
        private readonly FloraRandom _random;
        private FloraDebugLogger _logger;
        private readonly Reactions _reactions;

        public CustomReactions(FloraRandom random, FloraDebugLogger logger, Reactions reactions)
        {
            _random = random;
            _logger = logger;
            _reactions = reactions;
        }

        [Command("AddReaction"), Summary("Add a reaction to the bot")]
        [Alias("AR")]
        [OwnerOnly]
        public async Task AddReaction(string prompt, [Remainder] string reactionString)
        {
            int reactionID = await _reactions.AddReaction(prompt, reactionString);
            await Context.Channel.SendSuccessAsync($"Custom Reaction #{reactionID} | {prompt}", reactionString);
        }

        [Command("DeleteAllForPrompt"), Summary("Delete a reaction from the bot by prompt.")]
        [Alias("DAFP")]
        [OwnerOnly]
        public async Task DeleteAllForPrompt(string prompt)
        {
            await _reactions.RemoveReaction(prompt);
            await Context.Channel.SendSuccessAsync($"Custom Reactions for {prompt} removed.");
        }

        [Command("DeleteReaction"), Summary("Delete a reaction from the bot by reaction ID")]
        [Alias("DR")]
        [OwnerOnly]
        public async Task DeleteReaction(int id)
        {
            await _reactions.RemoveReactionByID(id);
            await Context.Channel.SendSuccessAsync($"Custom Reactions #{id} removed.");
        }
    }
}
