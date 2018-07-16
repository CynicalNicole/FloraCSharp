using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace FloraCSharp.Services
{
    public class RequireNoLife : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            IGuildUser user = (IGuildUser)context.User;

            if (user.RoleIds.Contains((ulong)217696584345976833))
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("You must be a certified weeb to run this command.");
        }
    }
}
