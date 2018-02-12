using Discord.Commands;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    class Cyphers : ModuleBase
    { 
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;

        public Cyphers(FloraRandom random, FloraDebugLogger logger)
        {
            _random = random;
            _logger = logger;
        }

        [Command("Base64Encode"), Summary("Encode a string to base64")]
        [Alias("B64E")]
        public async Task Base64Encode([Remainder] string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            await Context.Channel.SendSuccessAsync(Convert.ToBase64String(bytes));
        }

        [Command("Base64Decode"), Summary("Decode a string from base64")]
        [Alias("B64D")]
        public async Task Base64Decode([Remainder] string str)
        {
            var bytes = Convert.FromBase64String(str);
            await Context.Channel.SendSuccessAsync(Encoding.UTF8.GetString(bytes));
        }
    }
}
