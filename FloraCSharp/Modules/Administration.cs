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
using Discord.WebSocket;
using System.Globalization;

namespace FloraCSharp.Modules
{
    public class Administration : ModuleBase
    {
        private readonly FloraRandom _random;
        private FloraDebugLogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly BotGameHandler _botGames;

        public Administration(FloraRandom random, FloraDebugLogger logger, DiscordSocketClient client, BotGameHandler botGames)
        {
            _random = random;
            _logger = logger;
            _client = client;
            _botGames = botGames;
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

        [Command("SetGame"), Summary("Sets the game the bot is currently playing")]
        [Alias("sgm")]
        [OwnerOnly]
        public async Task SetGame([Remainder] string gameName)
        {
            await _client.SetGameAsync(gameName);
        }

        [Command("SetStream"), Summary("Sets the stream the bot is currently streaming..?")]
        [Alias("sst")]
        [OwnerOnly]
        public async Task SetStream(string stream, [Remainder] string gameName)
        {
            await _client.SetGameAsync(gameName, stream, StreamType.Twitch);
        }

        [Command("AddRotatingGame"), Summary("Adds a game to the list of the rotating games")]
        [Alias("argm")]
        [OwnerOnly]
        public async Task AddRotatingGame([Remainder] string gameName)
        {
            int botGameID = await _botGames.AddGame(gameName);
            await Context.Channel.SendSuccessAsync($"Added Rotating Game #{botGameID}", gameName);
        }

        [Command("DeleteRotatingGame"), Summary("Removes a game from the list of the rotating games")]
        [Alias("drgm")]
        [OwnerOnly]
        public async Task DeleteRotatingGame([Remainder] string gameName)
        {
            await _botGames.RemoveBotGame(gameName);
            await Context.Channel.SendSuccessAsync($"Removed {gameName}.");
        }

        [Command("DeleteRotatingGame"), Summary("Removes a game from the list of the rotating games")]
        [Alias("drgm")]
        [OwnerOnly]
        public async Task DeleteRotatingGame(int id)
        {
            await _botGames.RemoveBotGameByID(id);
            await Context.Channel.SendSuccessAsync($"Removed rotating game #{id}.");
        }

        [Command("AddUserBirthday"), Summary("Adds a user's birthday.")]
        [Alias("AddBday", "Birthday", "AddDOB")]
        [OwnerOnly]
        public async Task AddUserBirthday(IGuildUser user, string birthday, int age)
        {
            DateTime dt;
            if (!DateTime.TryParseExact(birthday, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                await Context.Channel.SendErrorAsync("That is not a valid date.");
                return;
            }

            using (var uow = DBHandler.UnitOfWork())
            {
                uow.Birthdays.Add(new Services.Database.Models.Birthday
                {
                    UserID = user.Id,
                    Date = dt,
                    Age = age
                });
                await uow.CompleteAsync();
            }

            await Context.Channel.SendSuccessAsync($"Added Birthday for {user.Username}.");
        }
    }
}
