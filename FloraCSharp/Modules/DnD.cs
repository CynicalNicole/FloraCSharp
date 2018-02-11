using Discord.Commands;
using FloraCSharp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using FloraCSharp.Extensions;
using System.Text.RegularExpressions;

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

        [Command("QuickCS"), Summary("Gets all 6 charsheet stats at once. It'll speed it up.")]
        public async Task QuickCS(string alt = null)
        {
            int take = 0;
            int skip = 0;
            string rolltype = "";

            switch (alt?.ToLower())
            {
                case "3h":
                    take = 3;
                    skip = 0;
                    rolltype = "3h";
                    break;
                case "3l":
                    take = 3;
                    skip = 1;
                    rolltype = "3l";
                    break;
                case "2l":
                    take = 2;
                    skip = 2;
                    rolltype = "2l";
                    break;
                case "2h":
                    take = 2;
                    skip = 0;
                    rolltype = "2h";
                    break;
                default:
                    take = 3;
                    skip = 0;
                    rolltype = "3h";
                    break;
            }

            List<int> finalStats = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                int[] rolls = new int[4]
                {
                    _random.Next(6) + 1,
                    _random.Next(6) + 1,
                    _random.Next(6) + 1,
                    _random.Next(6) + 1
                };

                int stat = rolls.OrderByDescending(x => x).Skip(skip).Take(take).ToArray().Sum();
                finalStats.Add(stat);
            }
            finalStats = finalStats.OrderByDescending(x => x).ToList();
            EmbedBuilder embed = new EmbedBuilder().WithDnDColour().WithTitle($"Quick Char Sheet Rolls (4d6 {rolltype})").WithDescription(String.Join(", ", finalStats)).AddField(efb => efb.WithName("Total").WithValue(finalStats.Sum()));
            await Context.Channel.BlankEmbedAsync(embed);
        }

        [Command("RollCS"), Summary("Roll for your character sheet. It will take the top 3 of 4d6 rolls.")]
        public async Task RollCS(string alt = null)
        {
            int[] rolls = new int[4]
            {
                _random.Next(6) + 1,
                _random.Next(6) + 1,
                _random.Next(6) + 1,
                _random.Next(6) + 1
            };

            int take = 0;
            int skip = 0;
            string rolltype = "";

            switch (alt?.ToLower())
            {
                case "3h":
                    take = 3;
                    skip = 0;
                    rolltype = "3h";
                    break;
                case "3l":
                    take = 3;
                    skip = 1;
                    rolltype = "3l";
                    break;
                case "2l":
                    take = 2;
                    skip = 2;
                    rolltype = "2l";
                    break;
                case "2h":
                    take = 2;
                    skip = 0;
                    rolltype = "2h";
                    break;
                default:
                    take = 3;
                    skip = 0;
                    rolltype = "3h";
                    break;
            }

            int[] selected = rolls.OrderByDescending(x => x).Skip(skip).Take(take).ToArray();

            var embed = new EmbedBuilder().WithDnDColour()
                .WithTitle("Char Stat Roll")
                .AddField(efb => efb.WithName($"Rolls").WithValue($"`{rolls[0]}` `{rolls[1]}` `{rolls[2]}` `{rolls[3]}`"));

            string selectField = "";

            foreach (int sel in selected)
            {
                selectField += $" `{sel}` ";
            }

            selectField.Trim();

            embed.AddField(efb => efb.WithName($"Selected Rolls ({rolltype})").WithValue(selectField))
                .AddField(efb => efb.WithName("Final Value").WithValue($"{selected.Sum()}"));

            await Context.Channel.BlankEmbedAsync(embed);
        }

        [Command("Roll"), Summary("Rolls xdy")]
        public async Task Roll(string roll)
        {
            _logger.Log(roll, "DnD");
            roll = roll.Trim();
            if (!Regex.IsMatch(roll, @"\d[d]\d*"))
            {
                await Context.Channel.SendErrorAsync("Invalid dice string");
                return;
            }

            string[] sep = roll.Split('d');
            int count = Int32.Parse(sep[0]);
            int dice = Int32.Parse(sep[1]);

            if (count == 0) return;
            if (count > 50) return;

            List<int> rolls = new List<int>();

            for (int i = 0; i < count; i++)
            {
                rolls.Add(_random.Next(dice) + 1);
            }

            var embed = new EmbedBuilder().WithDnDColour().WithTitle($"Rolling {rolls.Count}d{dice}");
            string desc = "";

            foreach(int i in rolls)
            {
                desc += $"`{i}` ";
            }

            desc.TrimEnd();
            embed.AddField(efb => efb.WithName("Rolls").WithValue(desc)).AddField(efb => efb.WithName("Sum").WithValue(rolls.Sum()));

            await Context.Channel.BlankEmbedAsync(embed);
        }
    }
}
