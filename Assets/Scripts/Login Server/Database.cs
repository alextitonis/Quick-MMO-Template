using DarkRift;
using DarkRift.Server;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading;

namespace LoginServer
{
    public class Database
    {
        public static Database getInstance;
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
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on trying to login from client with id: " + client.ID + " with error: " + ex.Message, LogType.Error);
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
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on trying to register from account with id: " + client.ID + " with error: " + ex.Message, LogType.Error);
                return;
            }

            Server.getInstance.RegisterResponse(true, "", client);
        }
        public void SaveAccount(IClient client)
        {
            if (!Holder.accounts.ContainsKey(client)) return;

            try
            {
                if (openConnection())
                {
                    string query = "UPDATE accounts SET password='" + Holder.accounts[client].password + "' WHERE email='" + Holder.accounts[client].email + "'";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    closeConnection();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on saving account from client with id: " + client.ID + " with error: " + ex.Message, LogType.Error);
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
        bool isLoggedIn(IClient client)
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
            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM accounts WHERE email='" + email + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool exists = false;

                    if (reader.Read())
                        exists = true;

                    reader.Close();
                    closeConnection();

                    return exists;
                }
                else return false;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on trying to seach for an account with email: " + email + " with error: " + ex.Message, LogType.Error);
                return true;
            }
        }
        void SaveAccountsThread()
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
            }
            catch (MySqlException ex) { Server.getInstance.Log("Error on searching for a password for email: " + email + " with error: " + ex.Message, LogType.Error); }
            return password;
        }

        MySqlConnection connection;
        void Connect()
        {
            string connectionString = "Server=" + Config.DbServer + ";Database=" + Config.DbName + ";Uid=" + Config.DbUsername + ";Pwd=" + Config.DbPassword + ";SslMode=" + Config.DBSSslMode + ";";
            connection = new MySqlConnection(connectionString);
        }
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
        bool closeConnection()
        {
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
        }
    }
}