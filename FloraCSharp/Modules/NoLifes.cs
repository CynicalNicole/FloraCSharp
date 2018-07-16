using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FloraCSharp.Services;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    [RequireContext(ContextType.Guild)]
    public class NoLifes : ModuleBase
    {
        private readonly FloraRandom _random;
        private EliteColours EC = new EliteColours();

        public NoLifes(FloraRandom random)
        {
            _random = random;
        }

        //Removes your colour

        [Command("none"), Summary("Removes colour.")]
        [RequireNoLife]
        public async Task None()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 0);
        }

        //Colours for true weebs

        [Command("pink"), Summary("Gives a True Weeb the Pink Role.")]
        [RequireNoLife]
        public async Task Pink()
        {
            await EC.GiveEliteColour((IGuildUser) Context.User, Context.Channel, 1);
        }

        [Command("red"), Summary("Gives a True Weeb the Red Role.")]
        [RequireUser]
        public async Task Red()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 2);
        }

        [Command("blue"), Summary("Gives a True Weeb the Blue Role.")]
        [RequireUser]
        public async Task Blue()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 3);
        }

        [Command("orange")]
        [RequireNoLife]
        public async Task Orange()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 4);
        }

        [Command("purple"), Summary("Gives a True Weeb the Purple Role.")]
        [RequireNoLife]
        public async Task Purple()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 5);
        }

        [Command("yellow"), Summary("Gives a True Weeb the Yellow Role.")]
        [RequireUser]
        public async Task Yellow()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 6);
        }

        [Command("teal"), Summary("Gives a True Weeb the Teal Role.")]
        [RequireNoLife]
        public async Task Teal()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 7);
        }

        //Colours for Senpais

        [Command("Pastel Green"), Alias("pgreen", "senpaigreen", "pastelgreen")]
        [RequireSenpai]
        public async Task PastelGreen()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 8);
        }

        [Command("Pastel Purple"), Alias("ppurple", "senpaipurple", "pastelpurple")]
        [RequireSenpai]
        public async Task PastelPurple()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 9);
        }

        [Command("Pastel Blue"), Alias("pblue", "senpaiblue", "pastelblue")]
        [RequireSenpai]
        public async Task PastelBlue()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 10);
        }

        //Colours for Traps

        [Command("white"), Alias("discordlight", "dclight", "light", "discord light")]
        [RequireSuperSenpai]
        public async Task White()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 11);
        }

        [Command("dark"), Alias("discorddark", "dcdark", "discord dark")]
        [RequireSuperSenpai]
        public async Task Dark()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 12);
        }

        [Command("gold"), Alias("minecraft:gold_ingot")]
        [RequireSuperSenpai]
        public async Task Gold()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 13);
        }

        [Command("emerald"), Alias("minecraft:emerald")]
        [RequireSuperSenpai]
        public async Task Emerald()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 14);
        }

        [Command("royal blue"), Alias("rblue", "royalblue")]
        [RequireSuperSenpai]
        public async Task RoyalBlue()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 15);
        }
    }
}
