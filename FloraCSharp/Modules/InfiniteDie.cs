using Discord;
using Discord.Commands;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using FloraCSharp.Services.ExternalDB;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("InfiniteDie")]
    [Alias("ID")]
    public class InfiniteDie : ModuleBase
    {
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;
        private readonly Cooldowns _cooldowns;

        public InfiniteDie(FloraRandom random, FloraDebugLogger logger, Cooldowns cooldowns)
        {
            _random = random;
            _logger = logger;
            _cooldowns = cooldowns;

            _cooldowns.GetOrSetupCommandCooldowns("InfiniteDieCreate");
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
                Content = "-";

            //Try to resolve user
            IGuildUser user = await Context.Guild.GetUserAsync(UserID);
            string username = String.Empty;
            if (user == null)
                username = UserID.ToString();
            else
                username = user.Username;

            await Context.Channel.SendSuccessAsync("Infinite Die | Side: " + side.ToString(), Content, null, "Owner: " + username);
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

            if (Content == String.Empty)
                Content = "-";

            //Try to resolve user
            IGuildUser user = await Context.Guild.GetUserAsync(UserID);
            string username = String.Empty;
            if (user == null)
                username = UserID.ToString();
            else
                username = user.Username;

            await Context.Channel.SendSuccessAsync("Infinite Die | Side: " + DiceNumber.ToString(), Content, null, "Owner: " + username);
        }

        [Command("Claim"), Summary("Claim a side without adding to it")]
        public async Task Claim(ulong side)
        {
            DateTime curTime = DateTime.Now;
            DateTime lastMessage;

            lastMessage = _cooldowns.GetUserCooldownsForCommand("InfiniteDieCreate", Context.User.Id);

            if (lastMessage + new TimeSpan(3, 0, 0) > curTime)
            {
                await Context.Channel.SendErrorAsync("You may only use this command once every 3 hours!");
                return;
            }

            _cooldowns.AddUserCooldowns("InfiniteDieCreate", Context.User.Id, curTime);

            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong DiceID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceID FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(0);
                }
                _conn.Close();
            }

            if (DiceID != 0)
            {
                await Context.Channel.SendErrorAsync("You cannot claim a side owned by someone.");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "INSERT INTO dice(DiceNumber, DiceOwner) VALUES (@did, @dOwner)";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", side);
                cmd.Parameters.AddWithValue("@dOwner", Context.User.Id);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }
        }

        [Command("Create"), Summary("Adds a new side to the dice")]
        [Alias("Add")]
        public async Task Create(ulong side, [Remainder] string content)
        {
            DateTime curTime = DateTime.Now;
            DateTime lastMessage;

            lastMessage = _cooldowns.GetUserCooldownsForCommand("InfiniteDieCreate", Context.User.Id);

            if (lastMessage + new TimeSpan(3,0,0) > curTime)
            {
                await Context.Channel.SendErrorAsync("You may only use this command once every 3 hours!");
                return;
            }

            _cooldowns.AddUserCooldowns("InfiniteDieCreate", Context.User.Id, curTime);

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
                    while (await reader.ReadAsync())
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
            await Context.Channel.SendSuccessAsync("Infinite Die | Side: " + side.ToString(), content, null, "Owner: " + Context.User.Username);
        }

        [Command("Transfer"), Summary("Transfer your dice side to another person.")]
        public async Task Transfer(ulong side, IGuildUser userToTransfer)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong CurrentOwnerID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceOwner FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    CurrentOwnerID = (ulong)reader.GetInt64(0);
                }
                _conn.Close();
            }

            if (CurrentOwnerID != Context.User.Id)
            {
                await Context.Channel.SendErrorAsync("You cannot transfer a side you do not own.");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "UPDATE dice SET DiceOwner=@newOwner WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                cmd.Parameters.AddWithValue("@newOwner", userToTransfer.Id);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side {side} was successfully transferred to {userToTransfer.Username} from {Context.User.Username}.");
        }

        [Command("AdminTransfer"), Summary("Force transfer of the side.")]
        [OwnerOnly]
        public async Task AdminTransfer(ulong side, IGuildUser userToTransfer)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            if (_conn.IsConnected())
            {
                string query = "UPDATE dice SET DiceOwner=@newOwner WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                cmd.Parameters.AddWithValue("@newOwner", userToTransfer.Id);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side {side} was forcefully transferred to {userToTransfer.Username}. Ouch.");
        }

        [Command("Clear"), Summary("Clear a side")]
        public async Task Clear(ulong side)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong CurrentOwnerID = 0;
            ulong DiceID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceID,DiceOwner FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(0);
                    CurrentOwnerID = (ulong)reader.GetInt64(1);
                }
                _conn.Close();
            }

            if (CurrentOwnerID != Context.User.Id || DiceID == 0)
            {
                await Context.Channel.SendErrorAsync("You cannot clear a side you do not own.");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM content WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side ${side} successfully cleared.");
        }

        [Command("AdminClear"), Summary("Clear a side")]
        [OwnerOnly]
        public async Task AdminClear(ulong side)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong DiceID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceID FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(0);
                }
                _conn.Close();
            }

            if (DiceID == 0)
            {
                await Context.Channel.SendErrorAsync("That side doesn't exist, no need to clear it!");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM content WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side ${side} forcefully cleared.");
        }

        [Command("Unclaim"), Summary("Unclaim a side")]
        public async Task Unclaim(ulong side)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong CurrentOwnerID = 0;
            ulong DiceID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceID,DiceOwner FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(0);
                    CurrentOwnerID = (ulong)reader.GetInt64(1);
                }
                _conn.Close();
            }

            if (CurrentOwnerID != Context.User.Id || DiceID == 0)
            {
                await Context.Channel.SendErrorAsync("You cannot unclaim a side you do not own.");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM dice WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side ${side} successfully unclaimed.");
        }

        [Command("AdminUnclaim"), Summary("Unclaim a side")]
        [OwnerOnly]
        public async Task AdminUnclaim(ulong side)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong DiceID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceID FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(0);
                }
                _conn.Close();
            }

            if (DiceID == 0)
            {
                await Context.Channel.SendErrorAsync("That side is unclaimed, no need to clear it!");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM dice WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side ${side} forcefully unclaimed.");
        }

        [Command("Delete"), Summary("Unclaims and clears a side")]
        public async Task Delete(ulong side)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong CurrentOwnerID = 0;
            ulong DiceID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceID,DiceOwner FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(0);
                    CurrentOwnerID = (ulong)reader.GetInt64(1);
                }
                _conn.Close();
            }

            if (CurrentOwnerID != Context.User.Id || DiceID == 0)
            {
                await Context.Channel.SendErrorAsync("You cannot clear a side you do not own.");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM content WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM dice WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side ${side} successfully deleted.");
        }

        [Command("AdminDelete"), Summary("Unclaims and clears a side")]
        public async Task AdminDelete(ulong side)
        {
            DBconnection _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";

            ulong DiceID = 0;

            if (_conn.IsConnected())
            {
                string query = "SELECT DiceID FROM dice WHERE DiceNumber=@side";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@side", side);
                var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return;

                while (await reader.ReadAsync())
                {
                    DiceID = (ulong)reader.GetInt64(0);
                }
                _conn.Close();
            }

            if (DiceID == 0)
            {
                await Context.Channel.SendErrorAsync("This side does not exist, you do not need to delete it!");
                return;
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM content WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            if (_conn.IsConnected())
            {
                string query = "DELETE FROM dice WHERE DiceID=@did";
                var cmd = new MySqlCommand(query, _conn.Connection);
                cmd.Parameters.AddWithValue("@did", DiceID);
                await cmd.ExecuteNonQueryAsync();
                _conn.Close();
            }

            await Context.Channel.SendSuccessAsync($"Side ${side} forcefully deleted.");
        }
    }
}
