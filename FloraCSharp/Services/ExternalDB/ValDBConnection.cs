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

        public ValDBConnection(string dbLoc)
        {
            connectionString = dbLoc;
        }

        public async Task<bool> AddCurrencyForUser(ulong userID, int amount)
        {
            if (connectionString == "") return false;

            using (var connection = new SqliteConnection($"Data Source={connectionString}"))
            {
                try {
                    var command = connection.CreateCommand();
                    command.CommandText =
                        "UPDATE DiscordUser SET CurrencyAmount = CurrencyAmount + $int1 WHERE UserId = $int2";
                    command.Parameters.AddWithValue("$int1", amount);
                    command.Parameters.AddWithValue("$int2", userID);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
