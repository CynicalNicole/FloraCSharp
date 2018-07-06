using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    public class RequireSenpai : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            IGuildUser user = (IGuildUser)context.User;

            if (user.RoleIds.Contains((ulong)374900017418338304))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("You must be a senpai to run this command."));
        }
    }
}
