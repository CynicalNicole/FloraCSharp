using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services.ExternalDB
{
    class ValDBConnection
    {
        string connectionString;
        FloraDebugLogger _logger;

        public ValDBConnection(string dbLoc, FloraDebugLogger logger)
        {
            connectionString = dbLoc;
            _logger = logger;
        }

        public async Task<bool> AddCurrencyForUser(ulong userID, int amount)
        {
            if (connectionString == "") return false;

            using (var connection = new SqliteConnection($"Data Source={connectionString}"))
            {
                _logger.Log($"DB: {connectionString} | Amount: {amount} | UID: {userID}", "ValDB");

                try {
                    var command = connection.CreateCommand();
                    _logger.Log("Created command.", "ValDB");

                    command.CommandText =
                        "UPDATE DiscordUser SET CurrencyAmount = CurrencyAmount + $int1 WHERE UserId = $int2";
                    _logger.Log($"Command: {command.CommandText}", "ValDB");

                    command.Parameters.AddWithValue("$int1", amount);
                    command.Parameters.AddWithValue("$int2", userID);

                    _logger.Log($"Command: {command.CommandText}", "ValDB");

                    await connection.OpenAsync();

                    _logger.Log($"Connection open", "ValDB");

                    await command.ExecuteNonQueryAsync();

                    _logger.Log($"Command Executed", "ValDB");
                }
                catch (Exception ex)
                {
                    _logger.Log($"Exception: {ex.Message}", "ValDB");
                    return false;
                }
            }

            return true;
        }
    }
}
