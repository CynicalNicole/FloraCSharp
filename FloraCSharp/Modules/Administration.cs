using Discord.Commands;
using Discord;
using FloraCSharp.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using FloraCSharp.Extensions;
using Newtonsoft.Json;

namespace FloraCSharp.Modules
{
    public class Administration : ModuleBase
    {
        private readonly FloraRandom _random;
        private FloraDebugLogger _logger;

        public Administration(FloraRandom random, FloraDebugLogger logger)
        {
            _random = random;
            _logger = logger;
        }

        [Command("save"), Summary("Saves a given user's role")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Save([Summary("Discord User to Save")] IGuildUser user, bool verbose = true)
        {
            //User and Server ID
            ulong uID = user.Id;
            ulong sID = Context.Guild.Id;

            //Server directory path
            string directory = @"data/roles/" + sID;

            //Create the directory for the server if it does not exist
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            //Path to storage
            string filePath = directory + @"/" + uID + ".json";

            //User's roles
            List<ulong> roles = new List<ulong>(user.RoleIds);

            //String roles
            List<string> rolesString = new List<string>(roles.Count);

            //If they've only got @everyone and newbies
            if (roles.Count == 2 && roles.Contains((ulong)229064523053531137))
            {
                await Context.Channel.SendErrorAsync("I don't think you need to do that.");
                return;
            }

            //So it can be ported over easily
            foreach (ulong rID in roles)
            {
                rolesString.Add($"{rID}");
            }

            //Serialize JSON
            string json = JsonConvert.SerializeObject(rolesString);

            //Write json to file (overwriting)
            File.WriteAllText(filePath, json);

            //Now tell the user we did it! Yay
            if (verbose)
                await Context.Channel.SendSuccessAsync("Saved roles for " + user.Mention);
        }

        [Command("Restore"), Summary("Restores a user's roles")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Restore([Summary("Discord User to Restore")] IGuildUser user)
        {
            var sID = Context.Guild.Id;
            var uID = user.Id;

            _logger.Log(uID.ToString(), "Restore");

            //Server directory path
            string directory = @"data/roles/" + sID;

            //Path to storage
            string filePath = directory + @"/" + uID + ".json";

            //Woah hold up there, they've not had roles saved
            if (!File.Exists(filePath)) return;

            _logger.Log(filePath, "Restore");

            //Get the roles
            List<string> SavedRoles = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(filePath));

            //Collection of roles
            List<IRole> roles = new List<IRole>();

            //Sort out the roles now
            foreach (string id in SavedRoles)
            {
                IRole role = Context.Guild.GetRole(ulong.Parse(id));
                if (!(role == Context.Guild.EveryoneRole))
                    roles.Add(role);
            }

            //Remove newbies role
            await user.RemoveRoleAsync(Context.Guild.GetRole(229064523053531137));

            //Add the roles they deserve
            await user.AddRolesAsync(roles);

            //Now tell the user we did it! Yay
            await Context.Channel.SendSuccessAsync("Restored roles for " + user.Mention);
        }

        [Command("SaveAll"), Summary("Saves all user's roles")]
        [RequireContext(ContextType.Guild)]
        [OwnerOnly]
        public async Task SaveAll()
        {
            List<IGuildUser> users = new List<IGuildUser>(await Context.Guild.GetUsersAsync());

            foreach (IGuildUser user in users)
            {
                await Save(user, false);
            }

            //Now tell the user we did it! Yay
            await Context.Channel.SendSuccessAsync("Saved all users");
        }

        [Command("Shutdown"), Summary("Kills the bot")]
        [Alias("die")]
        [RequireContext(ContextType.Guild)]
        [OwnerOnly]
        public async Task Shutdown()
        {
            //Now tell the user we did it! Yay
            await Context.Channel.SendSuccessAsync("Bye-bye!");

            //Safely stop the bot
            await Context.Client.StopAsync();

            //Close the client
            Environment.Exit(1);
        }

        [Command("RoleID"), Summary("Gets the ID of a role")]
        [Alias("rlid")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RoleID([Remainder] string RoleName)
        {
            IRole RoleFromName = null;
            foreach(IRole role in Context.Guild.Roles)
            {
                if (role.Name.ToLower() == RoleName.ToLower())
                {
                    RoleFromName = role;
                    break;
                }
            }

            if (RoleFromName == null)
                await Context.Channel.SendErrorAsync("That is not a valid role name.");
            else
                await Context.Channel.SendSuccessAsync($"RoleID ({RoleFromName.Name})", $"{RoleFromName.Id}");
        }

        
    }
}
