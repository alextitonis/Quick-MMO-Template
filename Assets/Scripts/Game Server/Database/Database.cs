#define _SQLITE
using DarkRift;
using DarkRift.Server;
#if _SQLITE
using SQLite;
using System.IO;
#else
using MySql.Data.MySqlClient;
#endif
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace GameServer
{
    public class Database
    {
        private static Database _getInstance;
        public static Database getInstance => _getInstance;
#if _SQLITE
        // use this for testing
        // file name
        private string databaseFile = "Database_World.sqlite";
        private SQLiteConnection connection;
#else
        private MySqlConnection connection;
#endif
        public static void Init()
        {
            if (_getInstance == null)
            {
                _getInstance = new Database();
            }

            getInstance.Connect();
            getInstance.SavePlayersThread();
        }

        public void CreateCharacter(CharacterSettings settings, IClient client)
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
#if _SQLITE
                StatsManager.Stats_Vitals _stats = StatsManager.getStartingStats(settings.Race, settings.Class);
                connection.Insert(new players()
                {
                    email = Holder.accounts[client].email,
                    ID = id,
                    Name = settings.Name,
                    Race = (int)settings.Race,
                    Class = (int)settings.Class,
                    Gender = (int)settings.Gender,
                    PositionX = (int)Config.StartingPosition.x,
                    PositionY = (int)Config.StartingPosition.y,
                    PositionZ = (int)Config.StartingPosition.z,
                    RotationX = (int)Config.StartingRotation.x,
                    RotationY = (int)Config.StartingRotation.y,
                    RotationZ = (int)Config.StartingRotation.z,
                    RotationW = (int)Config.StartingRotation.w,
                    CurrentHealth = (int)_stats.vitals.CurrentHealth,
                    CurrentMana = (int)_stats.vitals.CurrentMana,
                    Access = Config.PlayerAccess,
                    STR = _stats.baseStats.Strength,
                    AGI = _stats.baseStats.Agility,
                    _INT = _stats.baseStats.Intelligence,
                    STA = _stats.baseStats.Stamina,
                    WIT = _stats.baseStats.Wit,
                    MEN = _stats.baseStats.Mental,
                    Experience = settings.experience
                });
#else
            try
            {
                if (openConnection())
                {
                    StatsManager.Stats_Vitals _stats = StatsManager.getStartingStats(settings.Race, settings.Class);
                    string query = "INSERT INTO players(email, ID, Name, Race, Class, Gender, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, RotationW, CurrentHealth, CurrentMana, Access, STR, AGI, STA, _INT, WIT, MEN, Experience) VALUES('" + 
                                   Holder.accounts[client].email + "', '" + 
                                   id + "', '" + 
                                   settings.Name + "', '" + 
                                   (int)settings.Race + "', '" + 
                                   (int)settings.Class + "', '" + 
                                   (int)settings.Gender + "'" + ", '" + 
                                   Config.StartingPosition.x + "', '" + 
                                   Config.StartingPosition.y + "', '" + 
                                   Config.StartingPosition.z + "'" + ", '" + 
                                   Config.StartingRotation.x + "', '" + 
                                   Config.StartingRotation.y + "', '" + 
                                   Config.StartingRotation.z + "', '" + 
                                   Config.StartingRotation.w + "'" + ", '" + 
                                   _stats.vitals.CurrentHealth + "', '" + 
                                   _stats.vitals.CurrentMana + "', '" + 
                                   Config.PlayerAccess + "'" + ", '" + 
                                   _stats.baseStats.Strength + "', '" + 
                                   _stats.baseStats.Agility + "', '" + 
                                   _stats.baseStats.Stamina + "', '" + 
                                   _stats.baseStats.Intelligence + "', '" + 
                                   _stats.baseStats.Wit + "', '" + 
                                   _stats.baseStats.Mental + "'" + ", '" + 
                                   settings.experience + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.CharacterCreateResponse(false, "Error on creating the character: " + ex.Message, client);
                Server.getInstance.Log(ex.Message + " | " + ex.StackTrace, DarkRift.LogType.Error);
            }
#endif

            Holder.accounts[client].Characters = GetCharacters(client);
            Server.getInstance.CharacterCreateResponse(true, "", client);
        }
            
        public List<CharacterSettings> GetCharacters(IClient client) // updated sqlite
        {
            List<CharacterSettings> chars = new List<CharacterSettings>();
#if _SQLITE
            foreach (players player in connection.Query<players>("SELECT * FROM players WHERE email=?",
                         Holder.accounts[client].email))
            {
                CharacterSettings c = new CharacterSettings();
                c.ID = player.ID;
                c.Name = player.Name;
                c.Race = (Races)player.Race;
                c.Class = (Classes)player.Class;
                c.Gender = (Genders)player.Gender;
                c.Position = Vector3.zero;
                c.Rotation = new Quaternion(0f, 0f, 0f, 0f);
                c.vitals = new Vitals();
                c.vitals.CurrentHealth = player.CurrentHealth;
                c.vitals.CurrentMana = player.CurrentMana;
                c.access = player.Access;
                c.baseStats.Strength = (ushort)player.STR;
                c.baseStats.Agility = (ushort)player.AGI;
                c.baseStats.Stamina = (ushort)player.STA;
                c.baseStats.Intelligence = (ushort)player._INT;
                c.baseStats.Wit = (ushort)player.WIT;
                c.baseStats.Mental = (ushort)player.MEN;
                c.experience = player.Experience;
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
#else
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
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on getting characters for client with id: " + client.ID + " with error: " + ex.Message, DarkRift.LogType.Error);
            }
#endif
            return chars;
        }
        public void DeleteCharacter(string ID, IClient client) // updated sqlite
        {
            if (!CharacterExists(ID))
            {
                Server.getInstance.CharacterDeleteResponse(false, "Character ID not found!", ID, client);
                return;
            }
#if _SQLITE
            //connection.Execute("UPDATE players SET deleted=1 WHERE ID=?", ID);
            connection.Execute("DELETE FROM players WHERE ID=?", ID);
#else
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
#endif
            Server.getInstance.CharacterDeleteResponse(true, "", ID, client);
        }
        private bool CharacterExists(string ID) // updated sqlite
        {
#if _SQLITE
            return connection.FindWithQuery<players>("SELECT * FROM players WHERE ID=?", ID) != null;
#else
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
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on checking if a character exists: " + ex.Message, DarkRift.LogType.Error);
            }

            return exists;
#endif
        }
        
        private bool CharacterNameExists(string Name) // updated sqlite
        {
#if _SQLITE
            return connection.FindWithQuery<players>("SELECT * FROM players WHERE name=?", Name) != null;
#else
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
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on checking if a character exists: " + ex.Message, DarkRift.LogType.Error);
            }

            return exists;
#endif
        }
        
        public void SavePlayer(Player player, bool useTransaction = true) // updated sqlite
        {
            if (player == null) return;
            if (!Holder.player.ContainsKey(player.getClient())) return;
            IClient client = player.getClient();
#if _SQLITE
            // only use a transaction if not called within SaveMany transaction
            if (useTransaction) connection.BeginTransaction();

            connection.InsertOrReplace(new players
            {
                Name = player.getSettings().Name,
                Race = (int)player.getSettings().Race,
                Class = (int)player.getSettings().Class,
                Gender = (int)player.getSettings().Gender,
                PositionX = (int)player.getPosition().x,
                PositionY = (int)player.getPosition().y,
                PositionZ = (int)player.getPosition().z,
                RotationX = (int)player.getRotation().x,
                RotationY = (int)player.getRotation().y,
                RotationZ = (int)player.getRotation().z,
                RotationW = (int)player.getRotation().w,
                CurrentHealth = (int)player.getSettings().vitals.CurrentHealth,
                CurrentMana = (int)player.getSettings().vitals.CurrentMana,
                Access = player.getSettings().access,
                STR = player.getBaseStats().Strength,
                AGI = player.getBaseStats().Agility,
                STA = player.getBaseStats().Stamina,
                _INT = player.getBaseStats().Intelligence,
                WIT = player.getBaseStats().Wit,
                MEN = player.getBaseStats().Mental,
                Experience = player.getSettings().experience,
                ID = player.getSettings().ID
            });

            if (useTransaction) connection.Commit();
#else
            try
            {
                if (openConnection())
                {
                    string query = "UPDATE players SET Name='" + 
                                   player.getSettings().Name + "', Race='" + 
                                   player.getSettings().Race + "', Class='" + 
                                   player.getSettings().Class + "', Gender='" + 
                                   player.getSettings().Gender + "', PositionX='" + 
                                   (int)player.getPosition().x + "', PositionY='" + 
                                   (int)player.getPosition().y + "', PositionZ='" + 
                                   (int)player.getPosition().z + "', RotationX='" + 
                                   player.getRotation().x + "', RotationY='" + 
                                   player.getRotation().y + "', RotationZ='" + 
                                   player.getRotation().z + "', RotationW='" + 
                                   player.getRotation().w + "', CurrentHealth='" + 
                                   player.getSettings().vitals.CurrentHealth + "', CurrentMana='" + 
                                   player.getSettings().vitals.CurrentMana + "', Access='" + 
                                   player.getSettings().access + "', STR='" + 
                                   player.getBaseStats().Strength + ", AGI='" + 
                                   player.getBaseStats().Agility + "', STA='" + 
                                   player.getBaseStats().Stamina + "', _INT='" + 
                                   player.getBaseStats().Intelligence + "', WIT='" + 
                                   player.getBaseStats().Wit + "', MEN='" + 
                                   player.getBaseStats().Mental + "', Experience='" + 
                                   player.getSettings().experience + "' WHERE ID='" + 
                                   player.getSettings().ID + "'";

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
#endif
        }
        
        private void SavePlayersThread()
        {
            new Thread(() =>
            {
                foreach (var i in Holder.player.Values)
                {
                    SavePlayer(i);
                }
                Thread.Sleep((int)Utils.MinutesToMiliseconds(10));
            }).Start();
        }

        public Dictionary<EquipmentLocation, Item> loadEquipment(Player p)
        {
            Dictionary<EquipmentLocation, Item> equipment = new Dictionary<EquipmentLocation, Item>();
#if _SQLITE
            foreach (player_equipment playerEquipment in connection.Query<player_equipment>("SELECT * FROM player_equipment WHERE PlayerID=?", p.getSettings().ID))
            {
                EquipmentLocation slot = (EquipmentLocation)playerEquipment.EquipmentLocation;
                Item item = global::ItemDatabase.getInstance.getItem(playerEquipment.ItemID);

                if (!equipment.ContainsKey(slot))
                {
                    equipment.Add(slot, item);
                }
            }
#else
            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM player_equipment WHERE PlayerID='" + p.getSettings().ID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        EquipmentLocation slot =
                            (EquipmentLocation)reader.GetInt32(reader.GetOrdinal("EquipmentLocation"));
                        Item item = ItemDatabase.getInstance.getItem(reader.GetInt32(reader.GetOrdinal("ItemID")));

                        if (!equipment.ContainsKey(slot))
                            equipment.Add(slot, item);
                    }

                    reader.Close();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on loading equipment for player: " + p.getSettings().Name + " with error: " + ex.Message);
            }
#endif
            return equipment;
        }
        
        public bool EquipItem(Player p, Item item)
        {
#if _SQLITE
            connection.Insert(new player_equipment
            {
                PlayerID = p.getSettings().ID,
                EquipmentLocation = (int)item.EquipmentLocation,
                ItemID = item.ID
            });

            return true;
#else
            try
            {
                if (openConnection())
                {
                    string query =
 "INSERT INTO player_equipment(PlayerID, EquipmentLocation, ItemID) VALUES('" + p.getSettings().ID + "', '" + (int)item.EquipmentLocation + "', '" + item.ID + "')";

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
#endif
        }
        
        public bool UnEquipItem(Player p, EquipmentLocation slot)
        {
#if _SQLITE
            connection.Execute("DELETE FROM player_equipment WHERE PlayerID=? AND EquipmentLocation=?", p.getSettings().ID, (int)slot);
            return true;
#else
            try
            {
                if (openConnection())
                {
                    string query =
 "DELETE FROM player_equipment WHERE PlayerID='" + p.getSettings().ID + "' AND EquipmentLocation='" + (int)slot + "'";

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
#endif
        }

        public List<InventoryItem> loadInventory(Player p)
        {
            List<InventoryItem> inventory = new List<InventoryItem>();
#if _SQLITE
            foreach (player_inventory inventoryItem in connection.Query<player_inventory>("SELECT * FROM player_inventory WHERE owner=?", p.getSettings().ID))
            {
                InventoryItem item = new InventoryItem();
                item.item = global::ItemDatabase.getInstance.getItem(inventoryItem.item);
                item.Quantity = inventoryItem.quantity;
                item.slot = inventoryItem.slot;
                
                inventory.Add(item);
            }
#else
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
#endif
            return inventory;
        }
        
        public bool addInventoryItem(Player p, InventoryItem item)
        {
            if (item == null)
                return false;

            bool done = false;
#if _SQLITE
            connection.Insert(new player_inventory
            {
                owner = p.getSettings().ID,
                item = item.item.ID,
                quantity = item.Quantity,
                slot = item.slot
            });
#else
            try
            {
                if (openConnection())
                {
                    string query = "INSERT INTO player_inventory(owner, item, quantity, slot) VALUES('" + p.getSettings().ID + "', '" + item.item.ID + "', '" + item.Quantity + "', '" + item.slot + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex) 
            {
                Server.getInstance.LogError("Error on adding item to player with ID: " + p.getSettings().ID + " with error: " + ex.Message); 
            }
#endif
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
#if _SQLITE
            
#else
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
#endif
            return done;
        }
        
        public bool removeInventoryItem(Player p, int slot)
        {
            if (slot < 0)
                return false;

            bool done = false;
#if _SQLITE
            connection.Execute("DELETE FROM player_inventory WHERE owner=? AND slot=?", p.getSettings().ID, slot);
#else            
            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM player_inventory WHERE owner='" + p.getSettings().ID + "' AND slot='" + slot + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex) 
            {
                Server.getInstance.LogError("Error on removing item from player with ID: " + p.getSettings().ID + " with error: " + ex.Message); 
            }
#endif
            return done;
        }

        public List<PlayerSpell> loadPlayerSpells(Player player)
        {
            List<PlayerSpell> spells = new List<PlayerSpell>();
#if _SQLITE
            foreach (player_spells playerSpell in connection.Query<player_spells>("SELECT * FROM player_spells WHERE player=?", player.getSettings().ID))
            {
                PlayerSpell spell = new PlayerSpell();
                spell.spell = global::SpellDatabase.getInstance.getSpell(playerSpell.ID);
                spell.currentCooldown = playerSpell.currentCooldown;

                spells.Add(spell);
            }
#else
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
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on load spells for player: " + player.getSettings().Name + " with error: " + ex.Message);
            }
#endif
            return spells;
        }
        
        public bool addPlayerSpell(Player player, Spell spell)
        {
#if _SQLITE
            connection.Insert(new player_spells
            {
                player = player.getSettings().ID,
                spell = spell.ID
            });
#else
            try
            {
                if (openConnection())
                {
                    string query =
 "INSERT INTO player_spells(player, ID) VALUES('" + player.getSettings().ID + "', '" + spell.ID + "')";

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
#endif
            return true;
        }
        
        public bool removePlayerSpell(Player player, Spell spell)
        {
#if _SQLITE
            connection.Execute("DELETE FROM player_spells WHERE player=? AND ID=?", player.getSettings().ID, spell.ID);
#else
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
#endif
            return true;
        }

        public void loadWorldNPCs()
        {
#if _SQLITE
            foreach (worldNPCs worldNPC in connection.Query<worldNPCs>("SELECT * FROM worldNPCs"))
            {
                long spawnID = worldNPC.spawnID;
                int ID = worldNPC.ID;
                float RespawnTime = worldNPC.RespawnTime;
                Vector3 Position = new Vector3(
                    worldNPC.SpawnPositionX,
                    worldNPC.SpawnPositionY,
                    worldNPC.SpawnPositionZ
                );
                Quaternion Rotation = new Quaternion(
                    worldNPC.SpawnRotationX,
                    worldNPC.SpawnRotationY,
                    worldNPC.SpawnRotationZ,
                    worldNPC.SpawnRotationW
                );
                float currentHealth = worldNPC.CurrentHealth;
                float currentMana = worldNPC.CurrentMana;
                
                World.getInstance.SpawnNPC(spawnID, ID, Position, Rotation, RespawnTime, true, currentHealth, currentMana);
            }
#else
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
                        Vector3 Position = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("PositionX")),
                            reader.GetFloat(reader.GetOrdinal("PositionY")),
                            reader.GetFloat(reader.GetOrdinal("PositionZ"))
                            );
                        Quaternion Rotation = new Quaternion(
                            reader.GetFloat(reader.GetOrdinal("RotationX")),
                            reader.GetFloat(reader.GetOrdinal("RotationY")),
                            reader.GetFloat(reader.GetOrdinal("RotationZ")),
                            reader.GetFloat(reader.GetOrdinal("RotationW"))
                            );
                        float currentHealth = reader.GetFloat(reader.GetOrdinal("CurrentHealth"));
                        float currentMana = reader.GetFloat(reader.GetOrdinal("CurrentMana"));

                        World.getInstance.SpawnNPC(spawnID, ID, Position, Rotation, RespawnTime, true, currentHealth,
                            currentMana);
                    }

                    reader.Close();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on loading world npcs: " + ex.Message);
            }
#endif
        }

        public void SpawnNPC(long spawnID, int ID, float respawnTime, Vector3 spawnPosition, Quaternion rotation,
            float currentHealth, float currentMana)
        {
#if _SQLITE
            connection.Insert(new worldNPCs
            {
                spawnID = spawnID,
                ID = ID,
                RespawnTime = respawnTime,
                SpawnPositionX = spawnPosition.x,
                SpawnPositionY = spawnPosition.y,
                SpawnPositionZ = spawnPosition.z,
                SpawnRotationX = rotation.x,
                SpawnRotationY = rotation.y,
                SpawnRotationZ = rotation.z,
                SpawnRotationW = rotation.w,
                CurrentHealth = currentHealth,
                CurrentMana = currentMana
            });
#else
            try
            {
                if (openConnection())
                {
                    string query =
                        "INSERT INTO worldNPCs(SpawnID, ID, RespawnTime, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, RotationW, CurrentHealth, CurrentMana, Level) VALUES('" +
                        spawnID + "', '" + 
                        ID + "', '" + 
                        respawnTime + "'" + ", '" + 
                        spawnPosition.x + "', '" + 
                        spawnPosition.y + "', '" + 
                        spawnPosition.z + "'" + ", '" + 
                        rotation.x + "', '" + 
                        rotation.y + "', '" + 
                        rotation.z + "', '" + 
                        rotation.w + ", '" + 
                        currentHealth + "', '" + 
                        currentMana + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on spawning npc with id: " + ID + " and spawn id: " + spawnID + " with error: " + ex.Message);
            }
#endif
        }

        public void DespawnNPC(long spawnID) // updated sqlite
        {
#if _SQLITE
            connection.Execute("DELETE FROM worldNPCs WHERE SpawnID=?", spawnID);
#else
            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM worldNPCs WHERE SpawnID='" + spawnID + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.LogError("Error on despawning npc with id: " + spawnID + " with error: " + ex.Message);
            }
#endif
        }
        
#if _SQLITE
private void Connect()
        {
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

            connection.CreateTable<players>();
            connection.CreateTable<player_equipment>();
            connection.CreateTable<player_spells>();
            connection.CreateTable<worldNPCs>();
            connection.CreateTable<player_inventory>();
        }
#else
        private void Connect()
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
#endif
        
#if !_SQLITE        
        private bool openConnection()
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
#endif

        public bool closeConnection()
        {
#if _SQLITE
            connection.Close();
            return true;
#else
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
#endif
        }

#if _SQLITE
        // classes for SQLite
        private class players
        {
            //email, ID, Name, Race, Class, Gender, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ,
            //RotationW, CurrentHealth, CurrentMana, Access, STR, AGI, STA, _INT, WIT, MEN, Experience
            [PrimaryKey]
            [Collation("NOCASE")]
            public string ID { get; set; }
            public string email { get; set; }
            public string Name { get; set; }
            public int Race { get; set; }
            public int Class { get; set; }
            public int Gender { get; set; }
            public int PositionX { get; set; }
            public int PositionY { get; set; }
            public int PositionZ { get; set; }
            public int RotationX { get; set; }
            public int RotationY { get; set; }
            public int RotationZ { get; set; }
            public int RotationW { get; set; }
            public int CurrentHealth { get; set; }
            public int CurrentMana { get; set; }
            public int Access { get; set; }
            public int STR { get; set; }
            public int AGI { get; set; }
            public int STA { get; set; }
            public int _INT { get; set; }
            public int WIT { get; set; }
            public int MEN { get; set; }
            public long Experience { get; set; }
        }

        private class player_inventory
        {
            public string owner { get; set; } 
            public int item { get; set; } 
            public int quantity { get; set; } 
            public int slot { get; set; }
        }

        private class player_equipment
        {
            public string PlayerID { get; set; }
            public int EquipmentLocation { get; set; }
            public int ItemID { get; set; }
        }

        private class player_spells
        {
            public string player { get; set; }
            public int spell { get; set; }
            public int currentCooldown { get; set; }
            public int ID { get; set; }
        }

        private class worldNPCs
        {
            public int ID { get; set; }
            public long spawnID { get; set; }
            public float RespawnTime { get; set; }
            public float SpawnPositionX { get; set; }
            public float SpawnPositionY { get; set; }
            public float SpawnPositionZ { get; set; }
            public float SpawnRotationX { get; set; }
            public float SpawnRotationY { get; set; }
            public float SpawnRotationZ { get; set; }
            public float SpawnRotationW { get; set; }
            public float CurrentHealth { get; set; }
            public float CurrentMana { get; set; }
        }
#endif
    }
}