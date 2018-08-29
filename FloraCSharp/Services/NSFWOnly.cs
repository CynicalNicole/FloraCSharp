using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    class NSFWOnly : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(context.Channel.IsNsfw ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("This must be ran in the NSFW channel."));
        }
    }
}
