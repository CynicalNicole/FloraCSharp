using Discord.Commands;
using FloraCSharp.Services;
using FloraCSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using System.Net.Http;
using FloraCSharp.Services.Database.Models;

namespace FloraCSharp.Modules
{
    [RequireContext(ContextType.Guild)]
    class Misc : ModuleBase
    {
        private HttpClient _client;
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;

        public Misc(FloraRandom random, FloraDebugLogger logger)
        {
            _client = new HttpClient();
            _random = random;
            _logger = logger;
        }

        [Command("Test"), Summary("Simple test command to see if the bot is running")]
        public async Task Test()
        {
            await Context.Channel.SendSuccessAsync("Hello!");
        }

        [Command("RNG"), Summary("Simple random number command")]
        public async Task RNG([Summary("The minimum, inclusive bound")] int min, [Summary("The maximum, exclusive bound")] int max)
        {
            await Context.Channel.SendSuccessAsync($"Random Number (Min : {min}, Max: {max})", $"{_random.Next(min, max)}");
        }

        [Command("RNG"), Summary("Simple random number command")]
        public async Task RNG([Summary("The maximum, exclusive bound")] int max)
        {
            await Context.Channel.SendSuccessAsync($"Random Number (Max: {max})", $"{_random.Next(max)}");
        }

        [Command("RNG"), Summary("Simple random number command")]
        public async Task RNG()
        {
            await Context.Channel.SendSuccessAsync("Random Number", $"{_random.Next()}");
        }

        [Command("Tastes"), Summary("Shitpost Generator (Tastes)")]
        public async Task Tastes([Remainder] [Summary("What is the kid's franchise?")] string taste)
        {
            string start = "I know you \"can't discuss taste\" (bollocks), but I'm now ready to admit that ";
            string end = " is a kids' franchise for those with low expectations who are happy as long as they get more.";

            await Context.Channel.SendSuccessAsync(start + taste + end);
        }

        [Command("ProfilePic"), Summary("Gets the profile pic of the user running it OR a specific user")]
        [Alias("pfp")]
        public async Task ProfilePic([Summary("The optional user who's pfp you're after")] IGuildUser user = null)
        {
            if (user == null)
                user = (IGuildUser)Context.User;

            await Context.Channel.SendPictureAsync("Profile Pic", user.Username, user.GetAvatarUrl());
        }

        [Command("PickUser"), Summary("Picks a random user from a role.")]
        [Alias("pur", "pickrole")]
        public async Task PickRole([Summary("The role you want to pick the user from. Defaults to everyone.")] string roleName = null)
        {
            IRole role = null;

            if (roleName == null)
                role = Context.Guild.EveryoneRole;
            else
            {
                foreach (IRole rl in Context.Guild.Roles)
                {
                    if (rl.Name.ToLower() == roleName.ToLower())
                    {
                        role = rl;
                        break;
                    }
                }
            }

            if (role != null)
                await PickUserFromRole(role, Context.Guild, Context.Channel);
        }

        [Command("PickUser"), Summary("Picks a random user from a role.")]
        [Alias("pur", "pickrole")]
        public async Task PickRole([Summary("The role you want to pick the user from. Defaults to everyone.")] IRole roleName) => await PickUserFromRole(roleName, Context.Guild, Context.Channel);

        private async Task PickUserFromRole(IRole role, IGuild guild, IMessageChannel channel)
        {
            var GUsers = await guild.GetUsersAsync();

            GUsers = new List<IGuildUser>(
                (from user in GUsers where !user.IsBot select user));

            if (role != guild.EveryoneRole)
            {
                List<IGuildUser> PossibleUsers = new List<IGuildUser>(
                    (from user in GUsers where user.RoleIds.Contains(role.Id) select user));

                if (PossibleUsers.Count > 1) 
                    await channel.SendSuccessAsync("Chosen User", PossibleUsers.ElementAt(_random.Next(PossibleUsers.Count)).Mention);
                else
                    await channel.SendSuccessAsync("Chosen User", PossibleUsers.First().Mention);
            }
            else
            {
                await channel.SendSuccessAsync("Chosen User", GUsers.ElementAt(_random.Next(GUsers.Count)).Mention);
            }
        }

        [Command("Choose"), Summary("Picks a random option from the given choices.")]
        public async Task Choose([Remainder] [Summary("Choices")] string options)
        {
            if (!options.Contains(";")) return;
            List <string> list = new List<string>(options.Split(';'));

            await Context.Channel.SendSuccessAsync(list.ElementAt(_random.Next(list.Count)));
        }

        [Command("GDQDonation"), Summary("Gets a randomly generated GDQ donation.")]
        [Alias("GDQD")]
        public async Task GDQDonation()
        {
            string url = "http://taskinoz.com/gdq/api/";
            _client.BaseAddress = new Uri(url);

            //Get response
            HttpResponseMessage resp = _client.GetAsync("").Result;

            if(resp.IsSuccessStatusCode)
            {
                //Parse
                var ResponseText = await resp.Content.ReadAsStringAsync();

                await Context.Channel.SendSuccessAsync(ResponseText);
            }
        }

        [Command("TestUpdate"), Summary("I added this cause I'm a little bitch")]
        [RequireUserPermission(GuildPermission.MentionEveryone)]
        public async Task TestUpdate()
        {
            await Context.Channel.SendSuccessAsync("This shit worked like a charm.");
        }

        [Command("RateUser"), Summary("Rates a given user")]
        [Alias("Rate")]
        public async Task RateUser([Summary("User to rate")] IGuildUser user)
        {
            UserRating rating = null;

            _logger.Log("Command started...", "RateUser");

            using (var uow = DBHandler.UnitOfWork())
            {
                _logger.Log("Getting rating...", "RateUser");
                rating = uow.UserRatings.GetUserRating(user.Id);
            }

            if (rating != null)
            {
                await Context.Channel.SendSuccessAsync("User Rating", $"The rating for {user.Mention} is {rating.Rating}/10");
            }
            else
            {
                string uIDsT = user.Id.ToString();
                char[] uIDcA = uIDsT.ToCharArray();

                int firstChar = Int32.Parse(uIDcA[0].ToString());
                int secondChar = Int32.Parse(uIDcA[(uIDcA.Count() - 1)].ToString());

                int finalHalf = Math.Abs(secondChar - firstChar);

                if (finalHalf <= 5)
                    finalHalf = finalHalf * 2;

                using (var uow = DBHandler.UnitOfWork())
                {
                    uow.UserRatings.CreateUserRating(user.Id, finalHalf);
                    await uow.CompleteAsync();
                }

                await Context.Channel.SendSuccessAsync("User Rating", $"The rating for {user.Mention} is {finalHalf}/10");
            }
        }
    }
}
