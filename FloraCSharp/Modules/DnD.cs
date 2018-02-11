using Discord.Commands;
using FloraCSharp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using FloraCSharp.Extensions;

namespace FloraCSharp.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("DnD")]
    class DnD : ModuleBase
    {
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;

        public DnD(FloraRandom random, FloraDebugLogger logger)
        {
            _random = random;
            _logger = logger;
        }

        [Command("RollCS"), Summary("Roll for your character sheet. It will take the top 3 of 4d6 rolls.")]
        public async Task RollCS()
        {
            int[] rolls = new int[4]
            {
                _random.Next(6) + 1,
                _random.Next(6) + 1,
                _random.Next(6) + 1,
                _random.Next(6) + 1
            };

            int[] highestThree = rolls.OrderByDescending(x => x).Take(3).ToArray();

            var embed = new EmbedBuilder().WithQuoteColour()
                .WithTitle("Char Stat Roll")
                .AddField(efb => efb.WithName("Rolls").WithValue($"`{rolls[0]}` | `{rolls[1]}` | `{rolls[2]}` | `{rolls[3]}`"))
                .AddField(efb => efb.WithName("Highest Rolls").WithValue($"`{highestThree[0]}` | `{highestThree[1]}` | `{highestThree[2]}`"))
                .AddField(efb => efb.WithName("Final Value").WithValue($"{highestThree[0]} + {highestThree[1]} + {highestThree[2]} = {highestThree.Sum()}"));

            await Context.Channel.BlankEmbedAsync(embed);
        }
    }
}
