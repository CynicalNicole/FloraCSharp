using Discord;
using Discord.Commands;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    public class CustomRoles : ModuleBase
    {
        private readonly FloraRandom _random;
        private FloraDebugLogger _logger;

        private string[] changeResponsesName = new string[7]
        {
            "Ooh, nice name!",
            "I like that name.",
            "Interesting choice.",
            "I don't know what it means, but I like it.",
            "Sounds good to me.",
            "As you wish.",
            "Consider it done!"
        };

        private string[] changeResponsesColour = new string[6]
        {
            "That colour looks great on you, excellent choice!",
            "What an interesting colour choice.",
            "Is that your favourite colour?",
            "Ooh, pretty!",
            "As you wish.",
            "Consider it done!"
        };

        public CustomRoles(FloraRandom random, FloraDebugLogger logger)
        {
            _random = random;
            _logger = logger;
        }

        [Command("AddCustomRole"), Summary("Adds a custom role for a user")]
        [Alias("ACsR")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddCustomRole(IGuildUser user, [Remainder] string RoleName)
        {
            IRole RoleFromName = null;
            foreach (IRole role in Context.Guild.Roles)
            {
                if (role.Name.ToLower() == RoleName.ToLower())
                {
                    RoleFromName = role;
                    break;
                }
            }

            if (RoleFromName == null)
            {
                await Context.Channel.SendErrorAsync("Role not found.");
                return;
            }

            using (var uow = DBHandler.UnitOfWork())
            {
                uow.CustomRole.CreateCustomRole(user.Id, RoleFromName.Id);
                await uow.CompleteAsync();
            }

            await Context.Channel.SendSuccessAsync($"Role {RoleFromName.Name} bound to user {user.Username}.");
        }

        [Command("DeleteCustomRole"), Summary("Deletes a custom role for a user")]
        [Alias("DCsR")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task DeleteCustomRole([Remainder] string RoleName)
        {
            IRole RoleFromName = null;
            foreach (IRole role in Context.Guild.Roles)
            {
                if (role.Name.ToLower() == RoleName.ToLower())
                {
                    RoleFromName = role;
                    break;
                }
            }

            if (RoleFromName == null)
            {
                await Context.Channel.SendErrorAsync("Role not found.");
                return;
            }

            using (var uow = DBHandler.UnitOfWork())
            {
                uow.CustomRole.DeleteCustomRole(RoleFromName.Id);
                await uow.CompleteAsync();
            }

            await Context.Channel.SendSuccessAsync($"Role {RoleFromName.Name} unbound from user.");
        }

        [Command("RenameRole"), Summary("Renames your custom role")]
        [Alias("RR", "NR")]
        public async Task RenameRole([Remainder] string roleName)
        {
            IRole role = null;
            using (var uow = DBHandler.UnitOfWork())
            {
                var CR = uow.CustomRole.GetCustomRole(Context.User.Id);
                if (CR == null)
                {
                    await Context.Channel.SendErrorAsync("You do not have a custom role.");
                    return;
                }

                role = Context.Guild.GetRole(CR.RoleID);
            }

            if (roleName.Length > 24)
            {
                await Context.Channel.SendErrorAsync("I'm sorry, that role name is too long.");
                return;
            }

            if (roleName.ToLower().Contains("best") || roleName.ToLower().Contains("girl"))
            {
                await Context.Channel.SendErrorAsync("I'm sorry, I can't let you lie to yourself.");
                return;
            }

            await role.ModifyAsync(x => x.Name = roleName);
            await Context.Channel.SendSuccessAsync(changeResponsesName[_random.Next(changeResponsesName.Length)]);
        }

        [Command("RecolourRole"), Summary("Re-colours your custom role")]
        [Alias("RCR", "CR", "RecolorRole")]
        public async Task RecolourRole([Remainder] string roleColour)
        {
            IRole role = null;
            using (var uow = DBHandler.UnitOfWork())
            {
                var CR = uow.CustomRole.GetCustomRole(Context.User.Id);
                if (CR == null)
                {
                    await Context.Channel.SendErrorAsync("You do not have a custom role.");
                    return;
                }

                role = Context.Guild.GetRole(CR.RoleID);
            }

            if (roleColour.Length > 7 || roleColour.Length < 6)
            {
                await Context.Channel.SendErrorAsync("That isn't a valid hex code you silly goose!");
                return;
            }

            if (roleColour.StartsWith("#") && roleColour.Length == 7)
                roleColour = roleColour.Substring(1);

            Regex rgx = new Regex(@"[0-9A-F]{6}$");
            if (!rgx.IsMatch(roleColour.ToUpper()))
            {
                await Context.Channel.SendErrorAsync("That isn't a valid hex code you silly goose!");
                return;
            }

            await role.ModifyAsync(x => x.Color = GetColour(roleColour));
            await Context.Channel.SendSuccessAsync(changeResponsesColour[_random.Next(changeResponsesColour.Length)]);
        }

        private Color GetColour(string hex)
        {
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return new Color(r, g, b);
        }
    }
}
