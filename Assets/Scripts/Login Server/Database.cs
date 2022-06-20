#define _SQLITE
using DarkRift;
using DarkRift.Server;
using System.Collections.Generic;
using System.Threading;
#if _SQLITE
using SQLite;
using System.IO;
using UnityEngine;
using LogType = DarkRift.LogType;
#else
using MySql.Data.MySqlClient;
#endif
namespace LoginServer
{
    public class Database
    {
        public static Database getInstance;

#if _SQLITE
        // use this for testing
        // file name
        private string databaseFile = "Database_Login.sqlite";
        private SQLiteConnection connection;
#else
        MySqlConnection connection;
#endif
        public static void Init()
        {
            getInstance = new Database();
            getInstance.Connect();
            getInstance.SaveAccountsThread();
        }

        public void Login(IClient client, string email, string password)
        {
            password = Cryptography.Encrypt_Custom(password);

            try
            {
                if (isLoggedIn(client))
                {
                    Server.getInstance.LoginResponse(false, "Client already logged in!", email, password, client);
                    return;
                }
                else if (isLoggedIn(email))
                {
                    Server.getInstance.LoginResponse(false, "Account already logged in!", email, password, client);
                    return;
                }
                else if (!accountExists(email))
                {
                    Server.getInstance.LoginResponse(false, "Account not found!", email, password, client);
                    return;
                }

#if _SQLITE
                accounts accounts =
                    connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE email=? AND password=?", email,
                        password);

                bool done = false;
                if (accounts != null)
                {

                    Account account = new Account();
                    account.email = email;
                    account.client = client;
                    account.password = password;

                    Holder.accounts.Add(client, account);

                    done = true;

                    OnEnter.HandleLogin(client);
                }

                if (done)
                {
                    Server.getInstance.LoginResponse(true, "", email, password, client);
                }
                else
                {
                    Server.getInstance.LoginResponse(false, "Wrong password!", email, password, client);
                }
#else
                if (openConnection())
                {
                    
                    string query = "SELECT * FROM accounts WHERE email='" + email + "' AND password='" + password + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool done = false;

                    if (reader.Read())
                    {
                        Account account = new Account();
                        account.client = client;
                        account.email = email;
                        account.password = password;

                        Holder.accounts.Add(client, account);

                        done = true;

                        OnEnter.HandleLogin(client);
                    }

                    reader.Close();
                    closeConnection();

                    if (done) Server.getInstance.LoginResponse(true, "", email, password, client);
                    else Server.getInstance.LoginResponse(false, "Wrong password!", email, password, client);
                }
                else
                {
                    Server.getInstance.LoginResponse(false, "Server error!", email, password, client);
                }
#endif
            }
#if _SQLITE
            catch (SQLiteException ex)
#else
            catch (MySqlException ex)
#endif
            {
                Server.getInstance.Log(
                    "Error on trying to login from client with id: " + client.ID + " with error: " + ex.Message,
                    LogType.Error);
            }
        }

        public void Register(IClient client, string email, string password)
        {
            password = Cryptography.Encrypt_Custom(password);

            try
            {
                if (!email.Contains("@") || !email.Contains(".") || email.Length < 2)
                {
                    Server.getInstance.RegisterResponse(false, "Wrong email form!", client);
                    return;
                }
                else if (password.Length <= 4)
                {
                    Server.getInstance.RegisterResponse(false, "Password's length sould be greater than 4!", client);
                    return;
                }
                else if (accountExists(email))
                {
                    Server.getInstance.RegisterResponse(false, "Account already exists!", client);
                    return;
                }
#if _SQLITE
                if (connection.InsertOrReplace(new accounts { email = email, password = password }) < 1)
                {
                    Server.getInstance.RegisterResponse(false, "Server error!", client);
                }
#else
                if (openConnection())
                {
                    string query = "INSERT INTO accounts(email, password) VALUES ('" + email + "', '" + password + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    closeConnection();
                }
                else
                {
                    Server.getInstance.RegisterResponse(false, "Server error!", client);
                    return;
                }
#endif
            }
#if _SQLITE
            catch (SQLiteException ex)
#else
            catch (MySqlException ex)
#endif
            {
                Server.getInstance.Log(
                    "Error on trying to register from account with id: " + client.ID + " with error: " + ex.Message,
                    LogType.Error);
                return;
            }

            Server.getInstance.RegisterResponse(true, "", client);
        }

        public void SaveAccount(IClient client)
        {
            if (!Holder.accounts.ContainsKey(client)) return;

            try
            {
#if _SQLITE
                connection.Execute("UPDATE accounts SET password=? WHERE email=?",
                    Holder.accounts[client].password, Holder.accounts[client].email);
#else
                if (openConnection())
                {
                    string query = "UPDATE accounts SET password='" + Holder.accounts[client].password +
                                   "' WHERE email='" + Holder.accounts[client].email + "'";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    closeConnection();
                }
#endif
            }
#if _SQLITE
            catch (SQLiteException ex)
#else
            catch (MySqlException ex)
#endif
            {
                Server.getInstance.Log(
                    "Error on saving account from client with id: " + client.ID + " with error: " + ex.Message,
                    LogType.Error);
            }
        }

        public bool isLoggedIn(string email)
        {
            foreach (var account in Holder.accounts.Values)
            {
                if (account.email == email)
                    return true;
            }

            return false;
        }

        private bool isLoggedIn(IClient client)
        {
            foreach (var account in Holder.accounts.Keys)
            {
                if (account == client)
                    return true;
            }

            return false;
        }

        public bool accountExists(string email)
        {
#if _SQLITE
            return connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE email=?", email) != null;
#else
            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM accounts WHERE email='" + email + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool exists = false;

                    if (reader.Read())
                    {
                        exists = true;
                    }

                    reader.Close();
                    closeConnection();

                    return exists;
                }
                else return false;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log(
                    "Error on trying to seach for an account with email: " + email + " with error: " + ex.Message,
                    LogType.Error);
                return true;
            }
#endif
        }

        private void SaveAccountsThread()
        {
            new Thread(() =>
            {
                while (true)
                {
                    foreach (var account in Holder.accounts.Keys)
                        SaveAccount(account);

                    Thread.Sleep((int)Utils.MinutesToMiliseconds(10));
                }
            }).Start();
        }

        public string getPassword(string email)
        {
            string password = string.Empty;

            try
            {
                #if _SQLITE
                accounts row = connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE email=?", email);
                password = row.email;
#else
                if (openConnection())
                {
                    string query = "SELECT * FROM accounts WHERE email='" + email + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                        password = Cryptography.Encrypt_Custom(reader.GetString(reader.GetOrdinal("password")));

                    reader.Close();
                    closeConnection();
                }
#endif
            }
            #if _SQLITE
            catch(SQLiteException ex)
#else
            catch (MySqlException ex)
#endif
            {
                Server.getInstance.Log(
                    "Error on searching for a password for email: " + email + " with error: " + ex.Message,
                    LogType.Error);
            }

            return password;
        }


        private void Connect()
        {
#if _SQLITE
#if UNITY_EDITOR
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, databaseFile);
#elif UNITY_ANDROID
            string path = Path.Combine(Application.persistentDataPath, databaseFile);
#elif UNITY_IOS
            string path = Path.Combine(Application.persistentDataPath, databaseFile);
#else
            string path = Path.Combine(Application.dataPath, databaseFile);
#endif
            // open connection
            // note: automatically creates database file if not created yet
            connection = new SQLiteConnection(path);

            connection.CreateTable<accounts>();
#else
            string connectionString = "Server=" + Config.DbServer + ";Database=" + Config.DbName + ";Uid=" +
                                      Config.DbUsername + ";Pwd=" + Config.DbPassword + ";SslMode=" +
                                      Config.DBSSslMode + ";";
            connection = new MySqlConnection(connectionString);
#endif
        }
#if !_SQLITE
        bool openConnection()
        {
            if (connection == null)
                Connect();

            if (connection.State == System.Data.ConnectionState.Broken)
                Connect();

            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    closeConnection();

                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on opening a database connection! " + ex.Message, LogType.Error);
                return false;
            }
        }
#endif

        private bool closeConnection()
        {
#if _SQLITE
            connection.Close();
            return true;
#else
            if (connection == null)
            {
                Server.getInstance.Log("Trying to close a null database connection!", LogType.Error);
                return false;
            }

            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on closing a database connection: " + ex.Message, LogType.Error);
                return false;
            }
#endif
        }


        private class accounts
        {
            [PrimaryKey] public string email { get; set; }
            public string password { get; set; }
        }
    }
}