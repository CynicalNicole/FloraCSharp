using Discord;
using Discord.Commands;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using FloraCSharp.Services.ExternalDB;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("InfiniteDie")]
    public class InfiniteDie : ModuleBase
    {
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;
        private DBconnection _conn;

        public InfiniteDie(FloraRandom random, FloraDebugLogger logger)
        {
            _random = random;
            _logger = logger;
            _conn = DBconnection.Instance();
            _conn.DBName = "cynicalp_weebnation";
        }

        [Command("Get"), Summary("Returns a random side (or optionally a given side")]
        public async Task Get()
        {
            ulong DiceID = 0;
            string Content = String.Empty;
            ulong UserID = 0;
            ulong DiceNumber = 0;

            if (_conn.IsConnected())
            {
                _logger.Log("DB Connection: " + _conn.IsConnected().ToString(), "InfiniteDie");
                string query = "SELECT * FROM content ORDER BY RAND() LIMIT 1";
                var cmd = new MySqlCommand(query, _conn.Connection);
                var reader = await cmd.ExecuteReaderAsync();

                _logger.Log("First command setup.", "InfiniteDie");
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

                _logger.Log("Second set up", "InfiniteDie");
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
    }
}
