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
    public class NoLifes : ModuleBase
    {
        private readonly FloraRandom _random;
        private EliteColours EC = new EliteColours();

        public NoLifes(FloraRandom random)
        {
            _random = random;
        }

        [Command("pink"), Summary("Gives a no-life the Pink Role.")]
        [RequireContext(ContextType.Guild)]
        [RequireNoLife]
        public async Task Pink()
        {
            await EC.GiveEliteColour((IGuildUser) Context.User, Context.Channel, 1);
        }

        [Command("red"), Summary("Gives a user the Red Role.")]
        [RequireContext(ContextType.Guild)]
        [RequireUser]
        public async Task Red()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 2);
        }

        [Command("blue"), Summary("Gives a user the Blue Role.")]
        [RequireContext(ContextType.Guild)]
        [RequireUser]
        public async Task Blue()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 3);
        }

        [Command("orange"), Summary("Gives a user the Orange Role.")]
        [RequireContext(ContextType.Guild)]
        [RequireUser]
        public async Task Orange()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 4);
        }

        [Command("purple"), Summary("Gives a user the Purple Role.")]
        [RequireContext(ContextType.Guild)]
        [RequireUser]
        public async Task Purple()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 5);
        }

        [Command("yellow"), Summary("Gives a no-life the Yellow Role.")]
        [RequireContext(ContextType.Guild)]
        [RequireNoLife]
        public async Task Yellow()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 6);
        }

        [Command("teal"), Summary("Gives a no-life the Teal Role.")]
        [RequireContext(ContextType.Guild)]
        [RequireNoLife]
        public async Task Teal()
        {
            await EC.GiveEliteColour((IGuildUser)Context.User, Context.Channel, 7);
        }
    }
}
