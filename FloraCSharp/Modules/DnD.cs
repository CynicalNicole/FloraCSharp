using Discord.Commands;
using FloraCSharp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Discord;
using FloraCSharp.Extensions;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.IO;
using FloraCSharp.Services.APIModels;

namespace FloraCSharp.Modules
{
    [Group("DnD")]
    class DnD : ModuleBase
    {
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;
        private readonly DiscordSocketClient _client;

        private readonly Dictionary<string, string> SkillToStat = new Dictionary<string, string>()
        {
            { "strength", "strength" },
            { "athletics", "strength" },
            { "strengthsave", "strength" },
            { "strength save", "strength" },
            { "strength saving throw", "strength" },
            { "dexterity", "dexterity" },
            { "dexterity saving throw", "dexterity" },
            { "dex", "dexterity" },
            { "dexsave", "dexterity" },
            { "dexteritysave", "dexterity" },
            { "dexterity save", "dexterity" },
            { "acrobatics", "dexterity" },
            { "sleight of hand", "dexterity" },
            { "sleight", "dexterity" },
            { "soh", "dexterity" },
            { "stealth", "dexterity" },
            { "constitution", "constitution" },
            { "constitution saving throw", "constitution" },
            { "const", "constitution" },
            { "constsave", "constitution" },
            { "const save", "constitution" },
            { "constitution save", "constitution" },
            { "constitutionsave", "constitution" },
            { "intelligence", "intelligence" },
            { "intelligence saving throw", "intelligence" },
            { "intelligencesave", "intelligence" },
            { "int", "intelligence" },
            { "intsave", "intelligence" },
            { "int save", "intelligence" },
            { "intelligence save", "intelligence" },
            { "arcana", "intelligence" },
            { "history", "intelligence" },
            { "investigation", "intelligence" },
            { "nature", "intelligence" },
            { "religion", "intelligence" },
            { "wisdom", "wisdom" },
            { "wisdomsave", "wisdom" },
            { "wisdom save", "wisdom" },
            { "wisdom saving throw", "wisdom" },
            { "wis", "wisdom" },
            { "wis save", "wisdom" },
            { "wissave", "wisdom" },
            { "animal handling", "wisdom" },
            { "animalhandling", "wisdom" },
            { "insight", "wisdom" },
            { "medicine", "wisdom" },
            { "perception", "wisdom" },
            { "survival", "wisdom" },
            { "charisma", "charisma" },
            { "charismasave", "charisma" },
            { "charisma save", "charisma" },
            { "charisma saving throw", "charisma" },
            { "charsave", "charisma" },
            { "char save", "charisma" },
            { "deception", "charisma" },
            { "intimidation", "charisma" },
            { "performance", "charisma" },
            { "persuasion", "charisma" }
        };

        public DnD(FloraRandom random, FloraDebugLogger logger, DiscordSocketClient client)
        {
            _random = random;
            _logger = logger;
            _client = client;
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
        public async Task Roll(string roll, int modifier = 0)
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

            var embed = new EmbedBuilder().WithDnDColour().WithTitle($"Rolling {rolls.Count}d{dice} {modifier.ToString("+0;-#")}");
            string desc = "";

            foreach(int i in rolls)
            {
                desc += $"`{i}` ";
            }

            desc.TrimEnd();
            embed.AddField(efb => efb.WithName("Rolls").WithValue(desc).WithIsInline(true)).AddField(efb => efb.WithName("Modifier").WithValue(modifier.ToString("+0;-#")).WithIsInline(true)).AddField(efb => efb.WithName("Total").WithValue(rolls.Sum() + modifier));

            await Context.Channel.BlankEmbedAsync(embed);
        }

        [Command("StatRoll"), Summary("Roll a stat for a character")]
        public async Task StatRoll(string CharName, [Remainder] string StatName)
        {
            //Check if file exists
            CharName = CharName.ToLower();

            //Server directory path
            string directory = @"data/dnd";

            //Path to storage
            string filePath = directory + @"/" + CharName + ".json";

            //Woah hold up there, they've not had roles saved
            if (!File.Exists(filePath))
            {
                await Context.Channel.SendErrorAsync("Character does not exist.");
                return;
            }

            string rawJson = File.ReadAllText(filePath);

            //Neat shit. Go
            DndCharModel sheet = JsonConvert.DeserializeObject<DndCharModel>(rawJson);

            //Okay now convert their statname into a base name
            if (!SkillToStat.ContainsKey(StatName.ToLower()))
            {
                await Context.Channel.SendErrorAsync("Skill does not exist.");
                return;
            }

            string statHereWeGo = SkillToStat[StatName.ToLower()];
            bool profQM = false;
            bool doubleProfQM = false;

            //Do they have proficiency ? ? ?  ?? 
            if (sheet.DoubleProficiencies.Contains(StatName.ToLower().Replace(" ", string.Empty))) doubleProfQM = true;
            else if (sheet.ProficientSkills.Contains(StatName.ToLower().Replace(" ", string.Empty))) profQM = true;

            //Go?
            int modifier = 0;

            switch (statHereWeGo)
            {
                case "strength":
                    modifier = sheet.Strength;
                    break;
                case "dexterity":
                    modifier = sheet.Dexterity;
                    break;
                case "constitution":
                    modifier = sheet.Constitution;
                    break;
                case "intelligence":
                    modifier = sheet.Intelligence;
                    break;
                case "wisdom":
                    modifier = sheet.Wisdom;
                    break;
                case "charisma":
                    modifier = sheet.Charisma;
                    break;
                default:
                    await Context.Channel.SendErrorAsync("Panic. Please.");
                    return;
            };

            //haha
            modifier = ((int)Math.Floor((double)modifier / 2)) - 5;

            // O K
            if (doubleProfQM) modifier += (2 * sheet.ProficiencyBonus);
            else if (profQM) modifier += sheet.ProficiencyBonus;

            //O  K K K K 
            int roll = _random.Next(20) + 1;

            //O  K K K K K KK K K 
            int finalMeme = roll + modifier;

            //GET IT OUT
            var embed = new EmbedBuilder().WithDnDColour().WithTitle($"Rolling {StatName} for {sheet.Name}").AddField(efb => efb.WithName("Roll").WithValue(roll).WithIsInline(true)).AddField(efb => efb.WithName("Modifier").WithValue(modifier).WithIsInline(true)).AddField(efb => efb.WithName("Total").WithValue(finalMeme));

            await Context.Channel.BlankEmbedAsync(embed);
        }

        [Command("ConvertToGold"), Alias("ConvGold")]
        public async Task ConvertToGold([Remainder] string GoldString)
        {
            //Counts
            int ethCount = 0;
            int ppCount = 0;
            int gpCount = 0;
            int spCount = 0;
            int cpCount = 0;

            //Get count from string
            Match m = new Regex(@"\d*eth", RegexOptions.IgnoreCase).Match(GoldString);

            if (m.Success)
            {
                _logger.Log(m.Value, "[DND ConvGold] ETH ");

                if (!Int32.TryParse(m.Value.Substring(0, m.Value.Length - 3), out ethCount))
                {
                    await Context.Channel.SendErrorAsync("Invalid eth.");
                    return;
                }
            }

            m = new Regex(@"\d*pp", RegexOptions.IgnoreCase).Match(GoldString);

            if (m.Success)
            {
                _logger.Log(m.Value, "[DND ConvGold] PP ");

                if (!Int32.TryParse(m.Value.Substring(0, m.Value.Length - 2), out ppCount))
                {
                    await Context.Channel.SendErrorAsync("Invalid pp.");
                    return;
                }
            }

            m = new Regex(@"\d*gp", RegexOptions.IgnoreCase).Match(GoldString);

            if (m.Success)
            {
                _logger.Log(m.Value, "[DND ConvGold] GP ");

                if (!Int32.TryParse(m.Value.Substring(0, m.Value.Length - 2), out gpCount))
                {
                    await Context.Channel.SendErrorAsync("Invalid gp.");
                    return;
                }
            }

            m = new Regex(@"\d*sp", RegexOptions.IgnoreCase).Match(GoldString);

            if (m.Success)
            {
                _logger.Log(m.Value, "[DND ConvGold] SP ");

                if (!Int32.TryParse(m.Value.Substring(0, m.Value.Length - 2), out spCount))
                {
                    await Context.Channel.SendErrorAsync("Invalid sp.");
                    return;
                }
            }

            m = new Regex(@"\d*cp", RegexOptions.IgnoreCase).Match(GoldString);

            if (m.Success)
            {
                _logger.Log(m.Value, "[DND ConvGold] CP ");

                if (!Int32.TryParse(m.Value.Substring(0, m.Value.Length - 2), out cpCount))
                {
                    await Context.Channel.SendErrorAsync("Invalid cp.");
                    return;
                }
            }

            //Multiply/Divide values
            float finalGoldTotal = (ethCount * 500) + (ppCount * 10) + gpCount + (spCount / 10) + (cpCount / 100);

            await Context.Channel.SendSuccessAsync("Gold Value", finalGoldTotal.ToString());
        }

        [RequireContext(ContextType.DM)]
        [Command("PrivateRoll"), Alias("PR")]
        public async Task PrivateRoll(string username, string roll, [Remainder] string reason = "") => await PrivateRoll(username, roll, 0, reason);

        [RequireContext(ContextType.DM)]
        [Command("PrivateRoll"), Alias("PR")]
        public async Task PrivateRoll(string username, string roll, int modifier = 0, [Remainder] string reason = "")
        {
            //Is username a username
            username = username.Trim();

            if (username.StartsWith('@')) username = username.Substring(1);

            if (!Regex.IsMatch(username, @".*#\d{4}$"))
            {
                await Context.Channel.SendErrorAsync("Invalid target user.");
                return;
            }

            int index = username.LastIndexOf('#');

            string un = username.Substring(0, index);
            string disc = username.Substring(index + 1);

            //Get DM
            IUser targetUser = _client.GetUser(un, disc);
            IDMChannel dmchannel = await targetUser.GetOrCreateDMChannelAsync();

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

            var embed = new EmbedBuilder().WithDnDColour().WithTitle($"Rolling {rolls.Count}d{dice} {modifier.ToString("+0;-#")} | Rolled by: {Context.User.Username} / Sent to: {targetUser.Username}");
            string desc = "";

            foreach (int i in rolls)
            {
                desc += $"`{i}` ";
            }

            desc.TrimEnd();
            embed.AddField(efb => efb.WithName("Rolls").WithValue(desc).WithIsInline(true)).AddField(efb => efb.WithName("Modifier").WithValue(modifier.ToString("+0;-#")).WithIsInline(true)).AddField(efb => efb.WithName("Total").WithValue(rolls.Sum() + modifier));

            if (reason != "")
            {
                embed.AddField(efb => efb.WithName("Reason").WithValue(reason));
            }
            
            await dmchannel.BlankEmbedAsync(embed);
            await Context.Channel.BlankEmbedAsync(embed);
        }
    }
}
