using Discord;
using Discord.Commands;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using FloraCSharp.Services.ExternalDB;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("InfiniteDie")]
    public class InfiniteDie : ModuleBase
    {
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;
        private static ConcurrentDictionary<ulong, DateTime> _cooldowns;

        public InfiniteDie(FloraRandom random, FloraDebugLogger logger)
        {
            _random = random;
            _logger = logger;
            _cooldowns = new ConcurrentDictionary<ulong, DateTime>();
        }

        [Command]
        public async Task Default(ulong side) => await Get(side);

        [Command("Get")]
        public async Task Get(ulong side)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";
            ulong DiceID = 0;
            string Content = String.Empty;
            ulong UserID = 0;

            if (_conn.IsConnected())
            {
                string q2 = "SELECT * FROM dice WHERE DiceNumber=@dn";
                var cmd2 = new MySqlCommand(q2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@dn", side);

                var reader2 = await cmd2.ExecuteReaderAsync();

                //_logger.Log("Second set up", "InfiniteDie");
                while (await reader2.ReadAsync())
                {
                    UserID = (ulong)reader2.GetInt64(2);
                    DiceID = (ulong)reader2.GetInt64(0);
                }
                _conn.Close();
            }

            if (DiceID == 0)
            {
                await Context.Channel.SendErrorAsync($"Side {side} is unavailable.");
                return;
            }

            if (_conn.IsConnected())
            {
                //_logger.Log("DB Connection: " + _conn.IsConnected().ToString(), "InfiniteDie");
                string query = "SELECT Content FROM content WHERE DiceID=@did ORDER BY RAND() LIMIT 1";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                var reader = await cmd.ExecuteReaderAsync();

                //_logger.Log("First command setup.", "InfiniteDie");
                while (await reader.ReadAsync())
                {
                    Content = reader.GetString(0);
                }
                _conn.Close();
            }

            if (Content == String.Empty)
            {
                await Context.Channel.SendErrorAsync($"Side {side} is broken. @ Nicole immediately.");
                return;
            }

            //Try to resolve user
            IGuildUser user = await Context.Guild.GetUserAsync(UserID);
            string username = String.Empty;
            if (user == null)
                username = UserID.ToString();
            else
                username = user.Username;

            await Context.Channel.SendSuccessAsync("Infinite Die | Side: " + side.ToString(), Content, null, "Dice Owner: " + username);
        }

        [Command("Get"), Summary("Returns a random side")]
        public async Task Get()
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";
            ulong DiceID = 0;
            string Content = String.Empty;
            ulong UserID = 0;
            ulong DiceNumber = 0;

            if (_conn.IsConnected())
            {
                //_logger.Log("DB Connection: " + _conn.IsConnected().ToString(), "InfiniteDie");
                string query = "SELECT * FROM content ORDER BY RAND() LIMIT 1";
                var cmd = new MySqlCommand(query, _conn.Connection);
                var reader = await cmd.ExecuteReaderAsync();

                //_logger.Log("First command setup.", "InfiniteDie");
                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(1);
                    Content = reader.GetString(2);
                }
                _conn.Close();
            }

            if (_conn.IsConnected())
            {
                string q2 = "SELECT * FROM dice WHERE DiceID=@did";
                var cmd2 = new MySqlCommand(q2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@did", DiceID);

                var reader2 = await cmd2.ExecuteReaderAsync();

                //_logger.Log("Second set up", "InfiniteDie");
                while (await reader2.ReadAsync())
                {
                    UserID = (ulong)reader2.GetInt64(2);
                    DiceNumber = (ulong)reader2.GetInt64(1);
                }
                _conn.Close();
            }

            //Try to resolve user
            IGuildUser user = await Context.Guild.GetUserAsync(UserID);
            string username = String.Empty;
            if (user == null)
                username = UserID.ToString();
            else
                username = user.Username;

            await Context.Channel.SendSuccessAsync("Infinite Die | Side: " + DiceNumber.ToString(), Content, null, "Dice Owner: " + username);
        }

        [Command("Create"), Summary("Adds a new side to the dice")]
        [Alias("Add")]
        public async Task Create(ulong side, [Remainder] string content)
        {
            DateTime curTime = DateTime.Now;
            DateTime lastMessage;

            if(_cooldowns.TryGetValue(Context.User.Id, out lastMessage) || (lastMessage + new TimeSpan(3,0,0) > curTime))
            {
                await Context.Channel.SendErrorAsync($"Sorry, you may only add a side every 3 hours.");
                return;
            }

            _cooldowns.AddOrUpdate(Context.User.Id, curTime, (i, d) => curTime);

            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";
            bool isAvailable = false;
            ulong diceOwnerID = 0;

            _logger.Log(side.ToString(), "IDie");

            if (_conn.IsConnected())
            {
                string query = "SELECT * FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!(reader.HasRows))
                    isAvailable = true;
                else
                {
                    while(await reader.ReadAsync())
                    {
                        diceOwnerID = (ulong)reader.GetInt64(2);
                    }
                }
                _conn.Close();
            }

            if (!isAvailable && diceOwnerID != Context.User.Id)
            {
                await Context.Channel.SendErrorAsync($"Side {side} is taken.");
                return;
            }

            //They can have it, lets do it.
            if (diceOwnerID == 0)
            {
                if (_conn.IsConnected())
                {
                    string query = "INSERT INTO dice(DiceNumber, DiceOwner) VALUES (@side, @uid)";
                    var cmd = new MySqlCommand(query, _conn.Connection);
                    cmd.Parameters.AddWithValue("@side", side);
                    cmd.Parameters.AddWithValue("@uid", Context.User.Id);
                    await cmd.ExecuteNonQueryAsync();

                    _conn.Close();
                }
            }

            long DiceID = 0;

            if (_conn.IsConnected())
            {        
                string query = "SELECT DiceID FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    DiceID = reader.GetInt64(0);
                }

                if (DiceID == 0)
                    return;

                _conn.Close();
            }

            if (_conn.IsConnected())
            {
                string query2 = "INSERT INTO content(DiceID, Content) VALUES (@did, @cont)";
                var cmd2 = new MySqlCommand(query2, _conn.Connection);
                cmd2.Parameters.AddWithValue("@did", DiceID);
                cmd2.Parameters.AddWithValue("@cont", content);
                await cmd2.ExecuteNonQueryAsync();

                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync("Side Added!");
            await Context.Channel.SendSuccessAsync("Infinite Die | Side: " + side.ToString(), content, null, "Dice Owner: " + Context.User.Username);
        }
    }
}
