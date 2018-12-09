using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.IO;

namespace FloraCSharp.Services.ExternalDB
{
    public class DBconnection
    {
        private DBconnection()
        {

        }

        private string dbName = string.Empty;
        public string DBName
        {
            get { return dbName; }
            set { dbName = value; }
        }

        public string Password { get; set; }
        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        private static DBconnection _instance = null;
        public static DBconnection Instance()
        {
            if (_instance == null)
                _instance = new DBconnection();
            return _instance;
        }

        public bool IsConnected()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(dbName))
                    return false;
                string[] details = File.ReadAllLines("data/extdb.txt");

                //Console.WriteLine($"{details[0]} | {details[1]} | {details[2]}");

                MySqlConnectionStringBuilder connstring = new MySqlConnectionStringBuilder
                {
                    Server = details[0],
                    UserID = details[1],
                    Password = details[2],
                    Database = dbName
                };

                connection = new MySqlConnection(connstring.ToString());
                connection.Open();
            }

            return true;
        }

        public void Close()
        {
            connection.Close();
            connection = null;
        }
    }
}
