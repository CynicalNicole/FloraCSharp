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
    [Group("No-Life")]
    [RequireNoLife()]
    public class NoLifes : ModuleBase
    {
        private readonly FloraRandom _random;
        private EliteColours EC = new EliteColours();

        public NoLifes(FloraRandom random)
        {
            _random = random;
        }

        [Command("pink"), Summary("Gives a no-life the Pink-Life Role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Pink()
        {
            await EC.GiveEliteColour((IGuildUser) Context.User, Context.Channel, 1);
        }

        [Command("red"), Summary("Gives a no-life the Red-Life Role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Red()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 2);
        }

        [Command("blue"), Summary("Gives a no-life the Blue-Life Role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Blue()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 3);
        }

        [Command("orange"), Summary("Gives a no-life the Orange-Life Role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Orange()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 4);
        }

        [Command("purple"), Summary("Gives a no-life the Purple-Life Role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Purple()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 5);
        }

        [Command("yellow"), Summary("Gives a no-life the Yellow-Life Role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Yellow()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 6);
        }

        [Command("teal"), Summary("Gives a no-life the Teal-Life Role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Teal()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 7);
        }
    }
}
