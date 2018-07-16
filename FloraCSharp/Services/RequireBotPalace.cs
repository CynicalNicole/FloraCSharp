using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    class RequireBotPalace : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = (Configuration)services.GetService(typeof(Configuration));
            
            if (config.Owners.Contains(context.User.Id))
            {
                return PreconditionResult.FromSuccess();
            }

            return context.Channel.Id == 205878045859381257 ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("This must be ran in #bot-palace.");
        }
    }
}
