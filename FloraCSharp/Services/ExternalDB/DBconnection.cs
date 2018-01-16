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

                MySqlConnectionStringBuilder connstring = new MySqlConnectionStringBuilder();
                connstring.Server = details[0];
                connstring.UserID = details[1];
                connstring.Password = details[2];
                connstring.Database = dbName;

                connection = new MySqlConnection(connstring.ToString());
                connection.Open();
            }

            return true;
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
