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
using Newtonsoft.Json;
using FloraCSharp.Services.APIModels;
using IqdbApi;
using IqdbApi.Models;
using FloraCSharp.Services.ExternalDB;
using FloraCSharp.Services.ExternalDB.Models;
using MySql.Data.MySqlClient;
using CryptSharp;
using System.Text;
using System.Net;

namespace FloraCSharp.Modules
{
    [RequireContext(ContextType.Guild)]
    class Misc : ModuleBase
    {
        private HttpClient _client;
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;
        private readonly Configuration _config;
        private readonly NameLocker _nameLocker;
        private readonly CrypterOptions _cryptopts = new CrypterOptions()
        {
            { CrypterOption.Variant, BlowfishCrypterVariant.Corrected },
            { CrypterOption.Rounds, 10 }
        };

        //Steam API Stuff
        private readonly string SteamAPIUrl = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={key}&steamid={id}&include_appinfo=1";
        private readonly string VanityURL = "https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/?key={key}&vanityurl={url}";
        private readonly string VanityGameRun = "steam://run/{id}";

        //Name API
        private readonly TimeSpan timeToCache = new TimeSpan(6, 0, 0);
        private readonly string ApiURL = "https://uinames.com/api/?gender=male&amount=100&region=england";

        public Misc(FloraRandom random, FloraDebugLogger logger, Configuration config, NameLocker nameLocker)
        {
            _client = new HttpClient();
            _random = random;
            _logger = logger;
            _config = config;

            _nameLocker = nameLocker;
        }

        //Matt name command
        [Command("Matt"), Summary("Get a name")]
        public async Task Matt()
        {
            //Get a name
            string FetchedName = NameCaching();

            //Caching debug
            _logger.Log("Fetched name: " + FetchedName, "debug");

            //Check name isn't null
            if (String.IsNullOrEmpty(FetchedName)) return;

            //Now we do
            await Context.Channel.SendMessageAsync(FetchedName);
        }

        //Return a name and update cache if needed
        private string NameCaching()
        {
            //First check if we need to cache
            //If the current time is larger than the cache, lets get new names
            if (DateTime.Now > _nameLocker.GetLastCacheTime().Add(timeToCache))
            {
                //Caching debug
                _logger.Log("Caching names...", "debug");

                //Update cache
                //First get api sting
                using (WebClient wc = new WebClient())
                {
                    //Get string
                    string jsonString = wc.DownloadString(ApiURL);
                    //Error if null
                    if (String.IsNullOrEmpty(jsonString)) return null;

                    //String to json
                    List<RandomNameModel> namesJson = JsonConvert.DeserializeObject<List<RandomNameModel>>(jsonString);

                    //Temp name list
                    List<string> nameList = new List<string>();

                    //Loop through all strings and cache first name
                    namesJson.ForEach(x =>
                    {
                        nameList.Add(x.Name);
                    });

                    //Cache namelist
                    _nameLocker.SetCachedNames(nameList);

                    //Add the time cached
                    _nameLocker.SetLastCacheTime(DateTime.Now);
                }

                //Caching debug
                _logger.Log("Caching complete!", "debug");
            }

            //Now use the cache to get a name
            return _nameLocker.GetCachedName();
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
            await Context.Channel.SendSuccessAsync($"Random Number (Max: {max-1})", $"{_random.Next(max)}");
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
        [Alias("pur", "pickrole", "raffle")]
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
        [Alias("pur", "pickrole", "raffle")]
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
                    await channel.SendSuccessAsync("Chosen User", PossibleUsers.ElementAt(_random.Next(PossibleUsers.Count)).Username);
                else
                    await channel.SendSuccessAsync("Chosen User", PossibleUsers.First().Username);
            }
            else
            {
                await channel.SendSuccessAsync("Chosen User", GUsers.ElementAt(_random.Next(GUsers.Count)).Username);
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
            //Start flora typing
            var typing = Context.Channel.EnterTypingState();

            string url = "http://taskinoz.com/gdq/api/";
            _client.BaseAddress = new Uri(url);

            //Get response
            HttpResponseMessage resp = _client.GetAsync("").Result;

            if(resp.IsSuccessStatusCode)
            {
                //Parse
                var ResponseText = await resp.Content.ReadAsStringAsync();

                typing.Dispose();

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

        [Command("Quote"), Summary("Will quote a given post ID or the given user's last post in the current channel.")]
        public async Task Quote(ulong quoteID)
        {
            var Post = await Context.Channel.GetMessageAsync(quoteID);
            await QuotePost(Post, Context.Channel);
        }

        [Command("Quote"), Summary("Will quote a given post ID or the given user's last post in the current channel.")]
        public async Task Quote(ulong channelID, ulong quoteID)
        {
            var Channel = (IMessageChannel) await Context.Guild.GetChannelAsync(channelID);
            var Post = await Channel.GetMessageAsync(quoteID);
            await QuotePost(Post, Context.Channel);
        }

        [Command("Quote"), Summary("Will quote a given post ID or the given user's last post in the current channel.")]
        public async Task Quote(IGuildUser user)
        {
            var PostHistory = Context.Channel.GetMessagesAsync();
            var MessageList = PostHistory.First().GetAwaiter().GetResult();
            var Message = MessageList.First(x => x.Author.Id == user.Id);

            await QuotePost(Message, Context.Channel);
        }

        private async Task QuotePost(IMessage post, IMessageChannel channel)
        {
            var embed = new EmbedBuilder().WithQuoteColour().WithAuthor(x => x.WithIconUrl(post.Author.GetAvatarUrl()).WithName(post.Author.Username)).WithDescription(post.Content).WithFooter(x => x.WithText(post.Timestamp.ToString()));
            await channel.BlankEmbedAsync(embed);
        }

        [Command("Attention"), Summary("Give attention to a user.")]
        [Alias("Notice")]
        public async Task Attention(IGuildUser user, int amount = 1)
        {
            if (user.Id == Context.User.Id) return;
            if (amount < 1 || amount > 3) return;

            Attention UserAttention;
            using (var uow = DBHandler.UnitOfWork())
            {
                UserAttention = uow.Attention.GetOrCreateAttention(Context.User.Id);
            } 

            if (UserAttention.LastUsage + new TimeSpan(24, 0, 0) > DateTime.Now)
            {
                if (UserAttention.DailyRemaining <= 0)
                {
                    TimeSpan ts = (UserAttention.LastUsage + new TimeSpan(24, 0, 0)).Subtract(DateTime.Now);
                    await Context.Channel.SendErrorAsync($"You must wait {ts.ToString(@"hh\:mm\:ss")} before you can give someone attention.");
                    return;
                }

                if (UserAttention.DailyRemaining < amount)
                {
                    await Context.Channel.SendErrorAsync($"You do not have that much attention left to give today.");
                    return;
                }

                UserAttention.DailyRemaining -= amount;
            }
            else
            {
                UserAttention.DailyRemaining = 3;

                if (UserAttention.DailyRemaining < amount)
                {
                    await Context.Channel.SendErrorAsync($"You do not have that much attention left to give today.");
                    return;
                }

                UserAttention.LastUsage = DateTime.Now;
                UserAttention.DailyRemaining -= amount;
            }

            using (var uow = DBHandler.UnitOfWork())
            {
                uow.Attention.Update(UserAttention);
                uow.Attention.AwardAttention(user.Id, (ulong)amount);
                await uow.CompleteAsync();
            }

            string confirm = $"{Context.User.Username} ";

            switch (amount)
            {
                case 1:
                    confirm += $"has noticed {user.Username}.";
                    break;
                case 2:
                    confirm += $"has noticed {user.Username} twice.";
                    break;
                case 3:
                    confirm += $"is stalking {user.Username}. Creep.";
                    break;
            }

            await Context.Channel.SendSuccessAsync(confirm);
        }

        [Command("AttentionLB"), Summary("Check the attention leaderboard")]
        [Alias("MostLoved", "ALB", "NoticeLB", "NLB")]
        public async Task AttentionLB(int page = 0)
        {
            if (page != 0)
                page -= 1;

            List<Attention> TopAttention;
            using (var uow = DBHandler.UnitOfWork())
            {
                TopAttention = uow.Attention.GetTop(page);
            }

            if (!TopAttention.Any())
            {
                await Context.Channel.SendErrorAsync($"No users found for page {page + 1}");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder().WithQuoteColour().WithTitle("Attention Leaderboard").WithFooter(efb => efb.WithText($"Page: {page + 1}"));

            foreach (Attention c in TopAttention)
            {
                IGuildUser user = await Context.Guild.GetUserAsync(c.UserID);
                string userName = user?.Username ?? c.UserID.ToString();
                EmbedFieldBuilder efb = new EmbedFieldBuilder().WithName(userName).WithValue(c.AttentionPoints).WithIsInline(true);

                embed.AddField(efb);
            }

            await Context.Channel.BlankEmbedAsync(embed);
        }

        [Command("Notices"), Summary("Check your attention")]
        public async Task Notices(IUser user = null)
        {
            if (user == null)
                user = Context.User;

            Attention a;
            using (var uow = DBHandler.UnitOfWork())
            {
                a = uow.Attention.GetOrCreateAttention(user.Id);
            }

            await Context.Channel.SendSuccessAsync($"{user.Username} has been noticed {a.AttentionPoints} times!");
        }

        [Command("NoticeCD"), Summary("Check your CD")]
        [Alias("NCD")]
        public async Task NoticeCD()
        {
            Attention a;
            using (var uow = DBHandler.UnitOfWork())
            {
                a = uow.Attention.GetOrCreateAttention(Context.User.Id);
            }
                  
            if (a.LastUsage + new TimeSpan(24, 0, 0) > DateTime.Now)
            {
                if (a.DailyRemaining > 0)
                {
                    await Context.Channel.SendSuccessAsync($"{Context.User.Username} - You still have {a.DailyRemaining} notices left today!");
                }
                else
                {
                    TimeSpan ts = (a.LastUsage + new TimeSpan(24, 0, 0)).Subtract(DateTime.Now);
                    await Context.Channel.SendErrorAsync($"{Context.User.Username} - You still have to wait {ts.ToString(@"hh\:mm\:ss")} before you can notice someone!");
                }               
            }
            else 
            {
                await Context.Channel.SendSuccessAsync($"{Context.User.Username} - Your cooldown has reset! You have all 3 of your notices back.");
            }
        }

        [Command("PickRandomGame"), Summary("Picks a random game from the user's steam library.")]
        [Alias("SteamRandom")]
        public async Task PickRandomGame([Remainder] string options = null)
        {
            if (_config.SteamAPIKey == "" || _config.SteamAPIKey == null)
            {
                await Context.Channel.SendErrorAsync("Please set a valid Steam API Key!");
                return;
            }

            if (options == null) options = "";

            var userID = Context.User.Id;
            ulong SteamID = GetSteamUserID(userID);

            if (SteamID == 0)
            {
                await Context.Channel.SendErrorAsync("You haven't set your SteamID yet.");
                return;
            }

            var completeURL = SteamAPIUrl.Replace("{key}", _config.SteamAPIKey).Replace("{id}", SteamID.ToString());

            var response = await APIResponse(completeURL);
            var responseArray = JsonConvert.DeserializeObject<OwnedGamesResultContainer>(response);

            var gamesList = responseArray.Result.Games;

            switch (options.ToLower())
            {
                default:
                case null:
                    break;
                case "played":
                case "p":
                    gamesList = gamesList.Where(x => x.PlaytimeForever > 0).ToList();
                    break;
                case "not played":
                case "np":
                    gamesList = gamesList.Where(x => x.PlaytimeForever == 0).ToList();
                    break;
            }

            //Path 6
            //s L R Z y b

            var randomGame = gamesList.RandomItem();

            uint playtimeTwoWeeks = randomGame?.Playtime2Weeks ?? 0;

            Uri steamGame = new Uri(VanityGameRun.Replace("{id}", randomGame.AppID.ToString()));

            EmbedBuilder embed = new EmbedBuilder().WithOkColour().WithTitle("Random Game")
                //.WithUrl($"steam://run/{randomGame.AppID}")
                .AddField(new EmbedFieldBuilder().WithName("Game Title").WithValue(randomGame.Name).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Total Playtime").WithValue(PlaytimeStringGen(randomGame.PlaytimeForever)).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Playtime - Last 2 Weeks").WithValue(PlaytimeStringGen(playtimeTwoWeeks)).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Launch URI").WithValue(steamGame).WithIsInline(true));

            await Context.Channel.BlankEmbedAsync(embed);
        }

        [Command("SourceAnimeImage"), Alias("srca", "sourcea", "src", "source", "sauce", "heinz", "ketchup", "barbecue", "BBQ", "brown sauce")]
        public async Task SourceImage()
        {
            //Check and get first attachment
            if (Context.Message.Attachments.Count != 1) return;

            //Get url to first attachment
            var url = Context.Message.Attachments.First().Url;

            //results
            await HandleResults(url, Context.Channel);            
        }

        [Command("SourceAnimeImage"), Alias("srca", "sourcea", "src", "source", "sauce", "heinz", "ketchup", "barbecue", "BBQ", "brown sauce")]
        public async Task SourceImage([Remainder] string str)
        {
            //Ensure remainder is url
            if (!Uri.IsWellFormedUriString(str, UriKind.RelativeOrAbsolute)) return;

            //results 
            await HandleResults(str, Context.Channel);
        }

        private async Task HandleResults(string url, IMessageChannel channel)
        {
            IIqdbClient api = new IqdbClient();

            IqdbApi.Models.SearchResult res;
            
            try
            {
                res = await api.SearchUrl(url);
            }
            //Too lazy to implement separate exceptions this'll do
            catch (Exception)
            {
                await channel.SendErrorAsync("IQDB serarch error'd. Try a different image?");
                return;
            }

            //Lets get the results maybe?
            if (res.Matches.Where(x => x.MatchType == IqdbApi.Enums.MatchType.Best).Count() == 0)
            {
                await channel.SendErrorAsync("No source found for that image, sadly.");
                return;
            }

            //SWEET MOTHER OF GOD WE GOT SOMETHING
            EmbedBuilder e = new EmbedBuilder().WithOkColour().WithTitle("Potential Match Found").WithDescription("This is the \"best\" match according to IQDB.");
            EmbedFieldBuilder efb = new EmbedFieldBuilder().WithName("URL");

            //We only need the best
            Match bestmatch = res.Matches.Where(x => x.MatchType == IqdbApi.Enums.MatchType.Best).First();

            //There's gotta be a better way to do this but I'm okay with this as is
            string urlFix = bestmatch.Url;
            if (!urlFix.StartsWith("http:") && urlFix.StartsWith("//")) urlFix = "http:" + urlFix;

            //combine shit
            efb.WithValue(urlFix);
            e.AddField(efb);

            //Finally
            await channel.BlankEmbedAsync(e);
        }

        private string PlaytimeStringGen(uint playtime)
        {
            if (playtime == 0)
                return "Never Played";
            else if (playtime < 60)
                return $"{playtime} minutes";
            else
                return $"{Math.Floor((double)playtime / 60)} hours";
        }

        private ulong GetSteamUserID(ulong userID)
        {
            ulong SteamID = 0;
            using (var uow = DBHandler.UnitOfWork())
            {
                SteamID = uow.User.GetSteamID(userID);
            }

            return SteamID;
        }

        [Command("LinkSteam"), Summary("Links steam acc")]
        public async Task LinkSteam(ulong id) => await LinkSteam(Context, id);

        [Command("LinkSteam"), Summary("Links steam acc")]
        public async Task LinkSteam(string id)
        {
            var response = await APIResponse(VanityURL.Replace("{key}", _config.SteamAPIKey).Replace("{url}", id));
            var responseArray = JsonConvert.DeserializeObject<VanityURLContainer>(response);

            if (responseArray.Result.Success != 1)
            {
                await Context.Channel.SendErrorAsync("Invalid steam URL!");
                return;
            }

            await LinkSteam(Context, responseArray.Result.SteamID);

            List<string> l = new List<string>();
        }

        private async Task LinkSteam(ICommandContext Context, ulong id)
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                if (uow.User.GetSteamID(Context.User.Id) == 0)
                    uow.User.SetSteamID(Context.User.Id, id);
                else
                {
                    await Context.Channel.SendErrorAsync("You have already set your SteamID");
                    return;
                }
            }

            await Context.Channel.SendSuccessAsync($"Set steamd ID for {Context.User.Username} to {id}");
        }

        private async Task<string> APIResponse(string fullURL)
        {
            //delay for 1/2 a second to help with API rate limiting
            await Task.Delay(500);

            //Make the request
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetStringAsync(fullURL);
            }
        }

        [Command("...")]
        private async Task Quote(int id)
        {
            var dbConVal = new ValDBConnection(_config.ValDB, _logger);

            //Got quote
            QuoteModel q = await dbConVal.GetQuoteByID(id);

            if (q != null) await Context.Channel.SendMessageAsync($"`\"{q.Keyword.ToLower()}\" #{q.ID}` :mega: {q.Quote}");
        }

        [Command("RoleInfo")]
        public async Task RoleInfo(IRole role)
        {
            var users = await Context.Guild.GetUsersAsync();

            int usersInRole = 0;

            users.ToList().ForEach(x =>
            {
                if (x.RoleIds.Contains(role.Id)) usersInRole++;
            });

            var embed = new EmbedBuilder().WithOkColour().WithTitle("📜 Role Information | " + role.Id)
                .AddField(efb => efb.WithName("Role Title").WithValue(role.Name).WithIsInline(true))
                .AddField(efb => efb.WithName("Role Colour").WithValue(role.Color.ToString()).WithIsInline(true))
                .AddField(efb => efb.WithName("User Count").WithValue(usersInRole).WithIsInline(true))
                .AddField(efb => efb.WithName("Role Relative Position").WithValue(role.Position).WithIsInline(true))
                .AddField(efb => efb.WithName("Hoisted?").WithValue(role.IsHoisted ? "✅" : "❌").WithIsInline(true))
                .AddField(efb => efb.WithName("Mentionable?").WithValue(role.IsMentionable ? "✅" : "❌").WithIsInline(true));

            await Context.Channel.BlankEmbedAsync(embed);
        }

        [Command("ResetWeebPass")]
        public async Task ResetWeebPass()
        {
            //Get the user
            IUser user = Context.User;

            //Open DB
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            //Fetched user
            string fetchedUserName = await GetExistingUser(_conn, user);

            //If user exists do nothing
            if (fetchedUserName == null)
            {
                await Context.Channel.SendErrorAsync("There is no account under your Discord ID.");
                return;
            }

            //Ok so now we know they exist, we need to do a few things.
            if (_conn.IsConnected())
            {
                //Set new user to 1 so they reset their PW
                string q2 = "UPDATE users SET NewUser=@nu WHERE DiscordID=@di";
                var cmd2 = new MySqlCommand(q2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@nu", 1);
                cmd2.Parameters.AddWithValue("@di", user.Id);
                await cmd2.ExecuteNonQueryAsync();
                _conn.Close();
            }

            //Now we can generate them a password
            string PlainTextPass = CreatePassword(16);
            string CryptedPass = Crypter.Blowfish.Crypt(PlainTextPass, _cryptopts);

            //Now to update the pass in the DB
            if (_conn.IsConnected())
            {
                //Set new user to 1 so they reset their PW
                string q2 = "UPDATE users SET Password=@nu WHERE DiscordID=@di";
                var cmd2 = new MySqlCommand(q2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@nu", CryptedPass);
                cmd2.Parameters.AddWithValue("@di", user.Id);
                await cmd2.ExecuteNonQueryAsync();
                _conn.Close();
            }

            //DM the user
            await user.SendMessageAsync("Your username: " + fetchedUserName + "\nYour password: " + PlainTextPass + "\nYou'll be asked to change your password on login.\nhttps://cynicalpopcorn.me/weebnation/index.php");
        }

        [Command("Register")]
        public async Task Register(string username = null)
        {
            //Get the user
            IUser user = Context.User;

            //Open DB
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            //If user exists do nothing
            if (await GetExistingUser(_conn, user) != null)
            {
                await Context.Channel.SendErrorAsync("There is already an account under your DiscordID.");
                return;
            }

            //If username is null, we'll use their discord name
            if (username == null) username = user.Username;
            string fetchedUserName = null;

            //Check if username already exists
            if (_conn.IsConnected())
            {
                string q2 = "SELECT * FROM users WHERE Username=@un";
                var cmd2 = new MySqlCommand(q2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@un", username);

                var reader2 = await cmd2.ExecuteReaderAsync();

                //_logger.Log("Second set up", "InfiniteDie");
                while (await reader2.ReadAsync())
                {
                    fetchedUserName = reader2.GetString(1);
                }
                _conn.Close();
            }

            if (fetchedUserName != null)
            {
                await Context.Channel.SendErrorAsync("That username is taken.");
                return;
            }

            //Now we can generate them a password
            string PlainTextPass = CreatePassword(16);
            string CryptedPass = Crypter.Blowfish.Crypt(PlainTextPass, _cryptopts);

            //Now to update the pass in the DB
            if (_conn.IsConnected())
            {
                //Set new user to 1 so they reset their PW
                string q2 = "INSERT INTO users(Username, Password, UserType, NewUser, DiscordID) VALUES (@u, @p, @ut, @nu, @di)";
                var cmd2 = new MySqlCommand(q2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@u", username);
                cmd2.Parameters.AddWithValue("@p", CryptedPass);
                cmd2.Parameters.AddWithValue("@ut", 0);
                cmd2.Parameters.AddWithValue("@nu", 1);
                cmd2.Parameters.AddWithValue("@di", user.Id);
                await cmd2.ExecuteNonQueryAsync();
                _conn.Close();
            }

            //DM the user
            await user.SendMessageAsync("Your username: " + username + "\nYour password: " + PlainTextPass + "\nYou'll be asked to change your password on login.\nhttps://cynicalpopcorn.me/weebnation/index.php");
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            while (0 < length--)
            {
                res.Append(valid[_random.Next(valid.Length)]);
            }
            return res.ToString();
        }

        private async Task<string> GetExistingUser(DBconnection _conn, IUser user)
        {
            string fetchedUserName = null;
            //Check
            if (_conn.IsConnected())
            {
                string q2 = "SELECT * FROM users WHERE DiscordID=@di";
                var cmd2 = new MySqlCommand(q2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@di", user.Id);

                var reader2 = await cmd2.ExecuteReaderAsync();

                //_logger.Log("Second set up", "InfiniteDie");
                while (await reader2.ReadAsync())
                {
                    fetchedUserName = reader2.GetString(1);
                }
                _conn.Close();
            }

            //If user exists do nothing
            return fetchedUserName;
        }
    }
}
