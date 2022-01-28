using DarkRift;
using DarkRift.Server;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameServer
{
    public class Database
    {
        public static Database getInstance;
        public static void Init()
        {
            getInstance = new Database();
            getInstance.Connect();
            getInstance.SavePlayersThread();
        }

        public void CreateCharacter(CharacterSettings settings, IClient client)
        {
            try
            {
                if (CharacterNameExists(settings.Name))
                {
                    Server.getInstance.CharacterCreateResponse(false, "Name already exists!", client);
                    return;
                }

#pragma warning disable CS0436 // Type conflicts with imported type
                string id = Generator.RandomString(6);
#pragma warning restore CS0436 // Type conflicts with imported type
                while (CharacterExists(id))
#pragma warning disable CS0436 // Type conflicts with imported type
                    id = Generator.RandomString(6);
#pragma warning restore CS0436 // Type conflicts with imported type

                settings.experience = 0;

                if (openConnection())
                {
                    StatsManager.Stats_Vitals _stats = StatsManager.getStartingStats(settings.Race, settings.Class);
                    string query = "INSERT INTO players(email, ID, Name, Race, Class, Gender, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, RotationW, CurrentHealth, CurrentMana, Access, STR, AGI, STA, _INT, WIT, MEN, Experience) VALUES('" + Holder.accounts[client].email + "', '"
                        + id + "', '" + settings.Name + "', '" + (int)settings.Race + "', '" + (int)settings.Class + "', '" + (int)settings.Gender + "'"
                        + ", '" + Config.StartingPosition.x + "', '" + Config.StartingPosition.y + "', '" + Config.StartingPosition.z + "'"
                        + ", '" + Config.StartingRotation.x + "', '" + Config.StartingRotation.y + "', '" + Config.StartingRotation.z + "', '" + Config.StartingRotation.w + "'"
                        + ", '" + _stats.vitals.CurrentHealth + "', '" + _stats.vitals.CurrentMana + "', '" + Config.PlayerAccess + "'"
                        + ", '" + _stats.baseStats.Strength + "', '" + _stats.baseStats.Agility + "', '" + _stats.baseStats.Stamina + "', '" + _stats.baseStats.Intelligence + "', '" + _stats.baseStats.Wit + "', '" + _stats.baseStats.Mental + "'"
                        + ", '" + settings.experience + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.CharacterCreateResponse(false, "Error on creating the character: " + ex.Message, client);
                Server.getInstance.Log(ex.Message + " | " + ex.StackTrace, DarkRift.LogType.Error);
            }


            Holder.accounts[client].Characters = GetCharacters(client);
            Server.getInstance.CharacterCreateResponse(true, "", client);
        }
        public List<CharacterSettings> GetCharacters(IClient client)
        {
            List<CharacterSettings> chars = new List<CharacterSettings>();

            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM players WHERE email='" + Holder.accounts[client].email + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CharacterSettings c = new CharacterSettings();
                        c.ID = reader.GetString(reader.GetOrdinal("ID"));
                        c.Name = reader.GetString(reader.GetOrdinal("Name"));
                        c.Race = (Races)reader.GetInt32(reader.GetOrdinal("Race"));
                        c.Class = (Classes)reader.GetInt32(reader.GetOrdinal("Class"));
                        c.Gender = (Genders)reader.GetInt32(reader.GetOrdinal("Gender"));
                        c.Position = Vector3.zero;
                        c.Rotation = new Quaternion(0f, 0f, 0f, 0f);
                        c.vitals = new Vitals();
                        c.vitals.CurrentHealth = reader.GetFloat(reader.GetOrdinal("CurrentHealth"));
                        c.vitals.CurrentMana = reader.GetFloat(reader.GetOrdinal("CurrentMana"));
                        c.access = reader.GetInt32(reader.GetOrdinal("Access"));
                        c.baseStats.Strength = reader.GetUInt16(reader.GetOrdinal("STR"));
                        c.baseStats.Agility = reader.GetUInt16(reader.GetOrdinal("AGI"));
                        c.baseStats.Stamina = reader.GetUInt16(reader.GetOrdinal("STA"));
                        c.baseStats.Intelligence = reader.GetUInt16(reader.GetOrdinal("_INT"));
                        c.baseStats.Wit = reader.GetUInt16(reader.GetOrdinal("WIT"));
                        c.baseStats.Mental = reader.GetUInt16(reader.GetOrdinal("MEN"));
                        c.experience = reader.GetInt64(reader.GetOrdinal("Experience"));
                        // c.Position = new Vector3(reader.GetFloat(reader.GetOrdinal("PositionX")), reader.GetFloat(reader.GetOrdinal("PositionY")), reader.GetFloat(reader.GetOrdinal("PositionZ")));
                        // c.Rotation = new Quaternion(reader.GetFloat(reader.GetOrdinal("RotationX")), reader.GetFloat(reader.GetOrdinal("RotationY")), reader.GetFloat(reader.GetOrdinal("RotationZ")), reader.GetFloat(reader.GetOrdinal("RotationW")));

                        StatsManager.Stats_Vitals _stats = StatsManager.calculateVitals(c);
                        if (c.vitals.CurrentHealth > _stats.vitals.MaxHealth)
                            c.vitals.CurrentHealth = _stats.vitals.MaxHealth;
                        if (c.vitals.CurrentMana > _stats.vitals.MaxMana)
                            c.vitals.CurrentMana = _stats.vitals.MaxMana;
                        c.vitals.MaxHealth = _stats.vitals.MaxHealth;
                        c.vitals.MaxMana = _stats.vitals.MaxMana;
                        c.stats = _stats.stats;

                        chars.Add(c);
                    }

                    reader.Close();
                }
            }
            catch (MySqlException ex) { Server.getInstance.Log("Error on getting characters for client with id: " + client.ID + " with error: " + ex.Message, DarkRift.LogType.Error); }

            return chars;
        }
        public void DeleteCharacter(string ID, IClient client)
        {
            if (!CharacterExists(ID))
            {
                Server.getInstance.CharacterDeleteResponse(false, "Character ID not found!", ID, client);
                return;
            }

            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM players WHERE ID='" + ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.CharacterDeleteResponse(false, "Error on delete character: " + ex.Message, ID, client);
            }

            Server.getInstance.CharacterDeleteResponse(true, "", ID, client);
        }
        bool CharacterExists(string ID)
        {
            bool exists = true;

            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM players WHERE ID='" + ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read()) exists = true;
                    else exists = false;

                    reader.Close();
                }
            }
            catch (MySqlException ex) { Server.getInstance.Log("Error on checking if a character exists: " + ex.Message, DarkRift.LogType.Error); }

            return exists;
        }
        bool CharacterNameExists(string Name)
        {
            bool exists = true;

            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM players WHERE Name='" + Name + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read()) exists = true;
                    else exists = false;

                    reader.Close();
                }
            }
            catch (MySqlException ex) { Server.getInstance.Log("Error on checking if a character exists: " + ex.Message, DarkRift.LogType.Error); }

            return exists;
        }
        public void SavePlayer(Player player)
        {
            if (player == null) return;
            if (!Holder.player.ContainsKey(player.getClient())) return;
            IClient client = player.getClient();

            try
            {
                if (openConnection())
                {
                    string query = "UPDATE players SET Name='" + player.getSettings().Name
                        + "', Race='" + player.getSettings().Race + "', Class='" + player.getSettings().Class + "', Gender='" + player.getSettings().Gender
                        + "', PositionX='" + (int)player.getPosition().x + "', PositionY='" + (int)player.getPosition().y + "', PositionZ='" + (int)player.getPosition().z
                        + "', RotationX='" + player.getRotation().x + "', RotationY='" + player.getRotation().y + "', RotationZ='" + player.getRotation().z + "', RotationW='" + player.getRotation().w
                        + "', CurrentHealth='" + player.getSettings().vitals.CurrentHealth + "', CurrentMana='" + player.getSettings().vitals.CurrentMana + "', Access='" + player.getSettings().access
                        + "', STR='" + player.getBaseStats().Strength + ", AGI='" + player.getBaseStats().Agility + "', STA='" + player.getBaseStats().Stamina + "', _INT='" + player.getBaseStats().Intelligence + "', WIT='" + player.getBaseStats().Wit + "', MEN='" + player.getBaseStats().Mental
                        + "', Experience='" + player.getSettings().experience
                        + "' WHERE ID='" + player.getSettings().ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    foreach (var i in player.getSpells())
                    {
                        query = "UPDATE player_spells SET CurrentCooldown='" + i.currentCooldown + "' WHERE player='" + player.getSettings().ID + "' AND ID='" + i.spell.ID + "'";

                        cmd = new MySqlCommand(query, connection);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on saving player from client with id: " + client.ID + " with error: " + ex.Message, DarkRift.LogType.Error);
            }
        }
        void SavePlayersThread()
        {
            new Thread(() =>
            {
                foreach (var i in Holder.player.Values)
                    SavePlayer(i);

                Thread.Sleep((int)Utils.MinutesToMiliseconds(10));
            }).Start();
        }

        public Dictionary<EquipmentLocation, Item> loadEquipment(Player p)
        {
            Dictionary<EquipmentLocation, Item> equipment = new Dictionary<EquipmentLocation, Item>();

            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM player_equipment WHERE PlayerID='" + p.getSettings().ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        EquipmentLocation slot = (EquipmentLocation)reader.GetInt32(reader.GetOrdinal("EquipmentLocation"));
                        Item item = ItemDatabase.getInstance.getItem(reader.GetInt32(reader.GetOrdinal("ItemID")));

                        if (!equipment.ContainsKey(slot))
                            equipment.Add(slot, item);
                    }

                    reader.Close();
                }
            }
            catch (MySqlException ex)
            { Server.getInstance.LogError("Error on loading equipment for player: " + p.getSettings().Name + " with error: " + ex.Message); }

            return equipment;
        }
        public bool EquipItem(Player p, Item item)
        {
            try
            {
                if (openConnection())
                {
                    string query = "INSERT INTO player_equipment(PlayerID, EquipmentLocation, ItemID) VALUES('" + p.getSettings().ID + "', '" + (int)item.EquipmentLocation + "', '" + item.ID + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    return true;
                }
                else return false;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on equiping item (" + item.Name + ") for player: " + p.getSettings().Name + " with error: " + ex.Message);
                return false;
            }
        }
        public bool UnEquipItem(Player p, EquipmentLocation slot)
        {
            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM player_equipment WHERE PlayerID='" + p.getSettings().ID + "' AND EquipmentLocation='" + (int)slot + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    return true;
                }
                else return false;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on unequiping slot (" + slot + ") for player: " + p.getSettings().Name + " with error: " + ex.Message);
                return false;
            }
        }

        public List<InventoryItem> loadInventory(Player p)
        {
            List<InventoryItem> inventory = new List<InventoryItem>();

            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM player_inventory WHERE owner='" + p.getSettings().ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        InventoryItem item = new InventoryItem();
                        item.item = ItemDatabase.getInstance.getItem(reader.GetInt32(reader.GetOrdinal("item")));
                        item.Quantity = reader.GetInt32(reader.GetOrdinal("quantity"));
                        item.slot = reader.GetInt32(reader.GetOrdinal("slot"));

                        inventory.Add(item);
                    }

                    reader.Close();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on loading inventory for player with ID: " + p.getSettings().ID + " with error: " + ex.Message); }

            return inventory;
        }   
        public bool addInventoryItem(Player p, InventoryItem item)
        {
            if (item == null)
                return false;

            bool done = false;

            try
            {
                if (openConnection())
                {
                    string query = "INSERT INTO player_inventory(owner, item, quantity, slot) VALUES('" + p.getSettings().ID + "', '" + item.item.ID + "', '" + item.Quantity + "', '" + item.slot + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on adding item to player with ID: " + p.getSettings().ID + " with error: " + ex.Message); }

            return done;
        }
        public bool removeInventoryItem(Player p, int slot, int quantity)
        {
            if (slot < 0)
                return false;

            bool done = false;

            if (quantity < 0)
                return done;
            if (quantity == 0)
                return removeInventoryItem(p, slot);

            try
            {
                if (openConnection())
                {
                    string query = "UPDATE player_inventory SET quantity='" + quantity + "' WHERE owner='" + p.getSettings().ID + "' AND slot='" + slot + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on removing item from player with ID: " + p.getSettings().ID + " with error: " + ex.Message); }

            return done;
        }
        public bool removeInventoryItem(Player p, int slot)
        {
            if (slot < 0)
                return false;

            bool done = false;

            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM player_inventory WHERE owner='" + p.getSettings().ID + "' AND slot='" + slot + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on removing item from player with ID: " + p.getSettings().ID + " with error: " + ex.Message); }

            return done;
        }

        public List<PlayerSpell> loadPlayerSpells(Player player)
        {
            List<PlayerSpell> spells = new List<PlayerSpell>();

            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM player_spells WHERE player='" + player.getSettings().ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        PlayerSpell spell = new PlayerSpell();
                        spell.spell = SpellDatabase.getInstance.getSpell(reader.GetInt32(reader.GetOrdinal("ID")));
                        spell.currentCooldown = reader.GetFloat(reader.GetOrdinal("CurrentCooldown"));

                        spells.Add(spell);
                    }

                    reader.Close();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on load spells for player: " + player.getSettings().Name + " with error: " + ex.Message); }

            return spells;
        }
        public bool addPlayerSpell(Player player, Spell spell)
        {
            try
            {
                if (openConnection())
                {
                    string query = "INSERT INTO player_spells(player, ID) VALUES('" + player.getSettings().ID + "', '" + spell.ID + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    return true;
                }
                else return false;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on adding spell for player: " + player.getSettings().Name + " with error: " + ex.Message);
                return false;
            }
        }
        public bool removePlayerSpell(Player player, Spell spell)
        {
            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM player_spells WHERE player='" + player.getSettings().ID + "' AND ID='" + spell.ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    return true;
                }
                else return false;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on removing spell from player: " + player.getSettings().Name + " with error: " + ex.Message);
                return false;
            }
        }

        public void loadWorldNPCs()
        {
            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM worldNPCs";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        long spawnID = reader.GetInt64(reader.GetOrdinal("SpawnID"));
                        int ID = reader.GetInt32(reader.GetOrdinal("ID"));
                        float RespawnTime = reader.GetFloat(reader.GetOrdinal("RespawnTime"));
                        Vector3 Position = new Vector3(reader.GetFloat(reader.GetOrdinal("PositionX")), reader.GetFloat(reader.GetOrdinal("PositionY")), reader.GetFloat(reader.GetOrdinal("PositionZ")));
                        Quaternion Rotation = new Quaternion(reader.GetFloat(reader.GetOrdinal("RotationX")), reader.GetFloat(reader.GetOrdinal("RotationY")), reader.GetFloat(reader.GetOrdinal("RotationZ")), reader.GetFloat(reader.GetOrdinal("RotationW")));
                        float currentHealth = reader.GetFloat(reader.GetOrdinal("CurrentHealth"));
                        float currentMana = reader.GetFloat(reader.GetOrdinal("CurrentMana"));

                        World.getInstance.SpawnNPC(spawnID, ID, Position, Rotation, RespawnTime, true, currentHealth, currentMana);
                    }

                    reader.Close();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on loading world npcs: " + ex.Message); }
        }
        public void SpawnNPC(long spawnID, int ID, float respawnTime, Vector3 spawnPosition, Quaternion rotation, float currentHealth, float currentMana)
        {
            try
            {
                if (openConnection())
                {
                    string query = "INSERT INTO worldNPCs(SpawnID, ID, RespawnTime, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, RotationW, CurrentHealth, CurrentMana, Level) VALUES('" + spawnID + "', '" + ID + "', '" + respawnTime + "'"
                        + ", '" + spawnPosition.x + "', '" + spawnPosition.y + "', '" + spawnPosition.z + "'"
                        + ", '" + rotation.x + "', '" + rotation.y + "', '" + rotation.z + "', '" + rotation.w
                        + ", '" + currentHealth + "', '" + currentMana + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on spawning npc with id: " + ID + " and spawn id: " + spawnID + " with error: " + ex.Message); }
        }
        public void DespawnNPC(long spawnID)
        {
            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM worldNPCs WHERE SpawnID='" + spawnID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex) { Server.getInstance.LogError("Error on despawning npc with id: " + spawnID + " with error: " + ex.Message); }
        }

        MySqlConnection connection;
        void Connect()
        {
            string connectionString = "Server=" + Config.DbServer + ";Database=" + Config.DbName + ";Uid=" + Config.DbUsername + ";Pwd=" + Config.DbPassword + ";SslMode=" + Config.DBSSslMode + ";";
            connection = new MySqlConnection(connectionString);

            int tries = 0;
            int maxTries = 10;
            while (!openConnection())
            {
                tries++;
                if (tries > maxTries)
                {
                    Server.getInstance.LogError("Could not connect to database!");
                    return;
                }
            }
        }
        public bool openConnection()
        {
            if (connection == null)
                Connect();

            if (connection.State == System.Data.ConnectionState.Open)
                return true;

            try
            {
                connection.OpenAsync();
                return true;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on opening a database connection! " + ex.Message, DarkRift.LogType.Error);
                return false;
            }
        }
        public bool closeConnection()
        {
            if (connection == null)
            {
                Server.getInstance.Log("Trying to close a null database connection!", DarkRift.LogType.Error);
                return false;
            }

            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on closing a database connection: " + ex.Message, DarkRift.LogType.Error);
                return false;
            }
        }
    }
}