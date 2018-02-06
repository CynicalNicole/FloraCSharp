using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    class FirstOwner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var configuration = (Configuration)services.GetService(typeof(Configuration));
            var firstOwner = configuration.Owners.First();

            return Task.FromResult((firstOwner == context.User.Id || context.Client.CurrentUser.Id == context.User.Id ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("You must be the first listed bot owner.")));
        }
    }
}
