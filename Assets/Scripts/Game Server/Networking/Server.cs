using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameServer
{
    public class Server : MonoBehaviour
    {
        public static Server getInstance;
        void Awake() { getInstance = this; }

        public XmlUnityServer server { get; private set; }

        void Start()
        {
            Application.targetFrameRate = 60;

            server = GetComponent<XmlUnityServer>();

            Database.Init();

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            Database.getInstance.loadWorldNPCs();

            server.Server.ClientManager.ClientConnected += ClientConnected;
            server.Server.ClientManager.ClientDisconnected += ClientDisconnected;
        }
        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (!Holder.clients.ContainsKey(e.Client.ID))
            {
                Holder.clients.Add(e.Client.ID, e.Client);
                e.Client.MessageReceived += MessageReceived;
                Welcome(e.Client);
            }
            else
            {
                Log("Client with id: " + e.Client.ID + " is already logged in and trying to log in again!", DarkRift.LogType.Warning);
            }
        }
        private void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            HandleData.HandleFromClient(sender, e);
        }
        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Disconnect(e.Client);
        }
        void OnProcessExit(object sender, EventArgs e)
        {
            Database.getInstance.closeConnection();
            Client.getInstance.Closing();
        }

        public void Log(string txt, DarkRift.LogType type, Exception exception = null)
        {
            Debug.Log("[" + type.ToString() + "] " + txt);
        }
        public void LogError(string txt, Exception exception = null)
        {
            Log(txt, DarkRift.LogType.Error, exception);
        }

        void SendTo(IClient client, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            writer = Cryptography.EncryptWriter(writer);

            using (Message msg = Message.Create(packetID, writer)) client.SendMessage(msg, sendMode);

            writer.Dispose();
        }
        void SendTo(List<IClient> clients, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            foreach (var i in clients)
                SendTo(i, writer, packetID, sendMode);
        }
        void SendToAll(DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            writer = Cryptography.EncryptWriter(writer);

            using (Message msg = Message.Create(packetID, writer))
            {
                foreach (var client in Holder.player.Keys)
                {
                    if (client != null)
                        client.SendMessage(msg, sendMode);
                }

                writer.Dispose();
            }
        }
        void SendToAllBut(IClient client, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            writer = Cryptography.EncryptWriter(writer);

            using (Message msg = Message.Create(packetID, writer))
            {
                foreach (var c in Holder.player.Keys)
                {
                    if (c != client && c != null)
                        c.SendMessage(msg, sendMode);
                }
            }

            writer.Dispose();
        }
        void SendToAllBut(List<IClient> clients, IClient exception, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            foreach (var i in clients)
                if (i != exception)
                    SendTo(i, writer, packetID, sendMode);
        }
        void SendToPlayersAround(List<WorldPlayer> p, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            foreach (var i in p)
                SendTo(i.player.getClient(), writer, packetID, sendMode);
        }
        void SendToPlayersAroundBut(List<WorldPlayer> p, IClient exception, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            foreach (var i in p)
                if (i.player.getClient() != exception)
                    SendTo(i.player.getClient(), writer, packetID, sendMode);
        }

        public void Welcome(IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(client.ID);
                writer.Write(Config.MaxCharacters);

                SendTo(client, writer, Packets.Welcome);
            }
        }
        public void Disconnect(IClient client)
        {
            if (Holder.clients.ContainsKey(client.ID))
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(client.ID);

                    SendToAll(writer, Packets.Disconnect);

                    Account acc = Holder.accounts[client];
                    Client.getInstance.SendDisconnectionToTheLoginServer(acc.email);

                    if (Holder.accounts.ContainsKey(client))
                        Holder.accounts.Remove(client);

                    if (Holder.player.ContainsKey(client))
                    {
                        if (World.getInstance.players.Find(x => x.player.getClient() == client))
                            World.getInstance.players.Remove(World.getInstance.players.Find(x => x.player.getClient() == client));

                        Holder.player[client].worldPlayer.currentZone.players.Remove(Holder.player[client].worldPlayer.currentZone.players.Find(x => x.player.getClient() == client));
                        Database.getInstance.SavePlayer(Holder.player[client]);
                        Holder.player.Remove(client);
                    }


                    Holder.clients.Remove(client.ID);
                }
            }
            else
            {
                Log("Client with id: " + client.ID + " tried to disconnect but can't be found in the server!", DarkRift.LogType.Warning);
            }
        }

        public void SendCharacters(List<CharacterSettings> chars, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(chars.Count);

                foreach (var c in chars)
                {
                    writer.Write(c.Name);
                    writer.Write((int)c.Race);
                    writer.Write((int)c.Class);
                    writer.Write((int)c.Gender);
                    writer.Write(c.ID);
                }

                SendTo(client, writer, Packets.RequestCharacters);
            }
        }
        public void CharacterDeleteResponse(bool done, string response, string deletedID, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(done);
                writer.Write(response);
                writer.Write(deletedID);

                SendTo(client, writer, Packets.DeleteCharacter);
            }
        }
        public void CharacterCreateResponse(bool done, string response, IClient client)
        {
            if (done) response = "Done";

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(done);
                writer.Write(response);

                SendTo(client, writer, Packets.CreateCharacter);
            }
        }
        public void EnterGame(Player player)
        {
            List<Player> players = player.worldPlayer.currentZone.getPlayers();
            List<IClient> clients = new List<IClient>();
            foreach (var i in players)
                clients.Add(i.getClient());

            foreach (var i in players)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(i.getClient().ID);
                    writer.Write(i.getSettings().Name);

                    writer.Write(i.getSettings().Position.x);
                    writer.Write(i.getSettings().Position.y);
                    writer.Write(i.getSettings().Position.z);

                    writer.Write(i.getSettings().Rotation.x);
                    writer.Write(i.getSettings().Rotation.y);
                    writer.Write(i.getSettings().Rotation.z);
                    writer.Write(i.getSettings().Rotation.w);

                    writer.Write((int)i.getSettings().Race);
                    writer.Write((int)i.getSettings().Class);
                    writer.Write((int)i.getSettings().Gender);

                    writer.Write(i.getSettings().vitals.MaxHealth);
                    writer.Write(i.getSettings().vitals.CurrentHealth);

                    SendTo(player.getClient(), writer, Packets.EnterGame);
                }
            }
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.getClient().ID);
                writer.Write(player.getSettings().Name);

                writer.Write(player.getSettings().Position.x);
                writer.Write(player.getSettings().Position.y);
                writer.Write(player.getSettings().Position.z);

                writer.Write(player.getSettings().Rotation.x);
                writer.Write(player.getSettings().Rotation.y);
                writer.Write(player.getSettings().Rotation.z);
                writer.Write(player.getSettings().Rotation.w);

                writer.Write((int)player.getSettings().Race);
                writer.Write((int)player.getSettings().Class);
                writer.Write((int)player.getSettings().Gender);

                writer.Write(player.getSettings().vitals.MaxHealth);
                writer.Write(player.getSettings().vitals.CurrentHealth);

                SendToAllBut(clients, player.getClient(), writer, Packets.EnterGame);
            }

            SendBaseStats(player);
            SendStats(player);
            SendLevel(player);

            SendInventoryData(player.getInventory(), player);

            foreach (var i in World.getInstance.npc)
                if (i.currentZone == player.worldPlayer.currentZone)
                    SpawnNPC(i, player.getClient());
        }
        public void ExitGame(Player player)
        {
            World.getInstance.DespawnPlayer(player.getClient());

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.getClient().ID);

                SendToAll(writer, Packets.ExitGame);
            }
        }

        public void TimeOutCheck(IClient client, bool isOnline = true)
        {
            TimeOutCheckerReturn(client);
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep((int)Utils.MinutesToMiliseconds(5));

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(isOnline);
                        Holder.accounts[client].timeOurSent = true;

                        SendTo(client, writer, Packets.TimeOut);
                    }
                }
            }).Start();
        }
        public void TimeOutCheckerReturn(IClient client)
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (Holder.accounts.ContainsKey(client))
                    {
                        Account acc = Holder.accounts[client];

                        if (acc.timeOurSent)
                        {
                            Thread.Sleep((int)Utils.MinutesToMiliseconds(1 / 3));
                            if (!acc.timeOurSent)
                                TimeOutCheck(client);
                            else
                                Disconnect(client);
                        }
                    }
                    else
                        return;
                }
            }).Start();
        }

        public void PlayerMovement(WorldPlayer player, Vector3 pos, Quaternion rot)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.player.getClient().ID);

                writer.Write(pos.x);
                writer.Write(pos.y);
                writer.Write(pos.z);

                writer.Write(rot.x);
                writer.Write(rot.y);
                writer.Write(rot.z);
                writer.Write(rot.w);

                SendToPlayersAround(World.getInstance.getWorldPlayersAround(player, World.MAX_PACKET_DISTANCE), writer, Packets.PlayerMovement, SendMode.Unreliable);
            }
        }
        public void PlayerAnimation(IClient animated, AnimationType type, string animationName, string value)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(animated.ID);
                writer.Write((int)type);
                writer.Write(animationName);
                writer.Write(value);

                SendToPlayersAround(World.getInstance.getWorldPlayersAround(World.getInstance.getPlayer(animated), World.MAX_PACKET_DISTANCE), writer, Packets.PlayerAnimation, SendMode.Unreliable);
            }
        }
        public void PlayAnimation(IClient client, string anim)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(client.ID);
                writer.Write(anim);

                SendToPlayersAround(World.getInstance.getWorldPlayersAround(World.getInstance.getPlayer(client), World.MAX_PACKET_DISTANCE), writer, Packets.PlayAnimation, SendMode.Unreliable);
            }
        }

        public void _SendMessage(_Message msg, List<Player> players)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(msg.Sender);
                writer.Write(msg.Content);
                writer.Write((int)msg.Type);

                foreach (var i in players)
                    SendTo(i.getClient(), writer, Packets._SendMessage);
            }
        }

        public void SendInventoryData(List<InventoryItem> inventory, Player p)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(inventory.Count);

                foreach (var i in inventory)
                {
                    writer.Write(i.item.ID);
                    writer.Write(i.Quantity);
                    writer.Write(i.slot);
                }

                SendTo(p.getClient(), writer, Packets.InventoryData);
            }
        }
        public void SendSlotData(InventoryItem item, Player p)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                if (item.item == null)
                    writer.Write(-1);
                else
                    writer.Write(item.item.ID);
                writer.Write(item.Quantity);
                writer.Write(item.slot);

                SendTo(p.getClient(), writer, Packets.SlotData);
            }
        }

        public void SendVitals(Vitals vitals, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(vitals.MaxHealth);
                writer.Write(vitals.CurrentHealth);
                writer.Write(vitals.MaxMana);
                writer.Write(vitals.CurrentMana);

                SendTo(client, writer, Packets.Vitals);
            }
        }
        public void SendMaxHealth(WorldPlayer player, float value)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.player.getClient().ID);
                writer.Write(value);

                foreach (var i in World.getInstance.getWorldPlayersAround(player.transform.position, player.currentZone, World.MAX_PACKET_DISTANCE))
                    SendTo(i.player.getClient(), writer, Packets.MaxHealth);
            }
        }
        public void SendCurrentHealth(WorldPlayer player, float value)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.player.getClient().ID);
                writer.Write(value);

                foreach (var i in World.getInstance.getWorldPlayersAround(player.transform.position, player.currentZone, World.MAX_PACKET_DISTANCE))
                    SendTo(i.player.getClient(), writer, Packets.CurrentHealth);
            }
        }
        public void SendMaxMana(float value, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(value);

                SendTo(client, writer, Packets.MaxMana);
            }
        }
        public void SendCurrentMana(float value, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(value);

                SendTo(client, writer, Packets.CurrentMana);
            }
        }

        public void SpawnNPC(WorldNPC npc, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(npc.npc.ID);
                writer.Write(npc.spawnID);

                writer.Write(npc.transform.position.x);
                writer.Write(npc.transform.position.y);
                writer.Write(npc.transform.position.z);

                writer.Write(npc.transform.rotation.x);
                writer.Write(npc.transform.rotation.y);
                writer.Write(npc.transform.rotation.z);
                writer.Write(npc.transform.rotation.w);

                writer.Write(npc.npc.vitals.MaxHealth);
                writer.Write(npc.npc.vitals.CurrentHealth);

                SendTo(client, writer, Packets.SpawnNPC);
            }
        }
        public void DespawnNPC(int spawnId, World.Zone zone)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(spawnId);

                foreach (var i in zone.players)
                    SendTo(i.player.getClient(), writer, Packets.DespawnNPC);
            }
        }
        public void MoveNPC(long spawnID, Vector3 newPos, Quaternion newRot, World.Zone zone)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(spawnID);

                writer.Write(newPos.x);
                writer.Write(newPos.y);
                writer.Write(newPos.z);

                writer.Write(newRot.x);
                writer.Write(newRot.y);
                writer.Write(newRot.z);
                writer.Write(newRot.w);

                foreach (var i in World.getInstance.getWorldPlayersAround(newPos, zone, World.MAX_PACKET_DISTANCE))
                    SendTo(i.player.getClient(), writer, Packets.MoveNPC);
            }
        }

        public void PlayerChangedZone(IClient client, string zoneName)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(zoneName);

                SendTo(client, writer, Packets.NewZone);
            }
        }
        public void PlayerLeftZone(IClient client, IClient to)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(client.ID);

                SendTo(to, writer, Packets.Disconnect);
            }
        }

        public void UpdateCurrentHealthForNPC(WorldNPC npc)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(npc.spawnID);
                writer.Write(npc.npc.vitals.CurrentHealth);

                foreach (var i in World.getInstance.getWorldPlayersAround(npc.transform.position, npc.currentZone, World.MAX_PACKET_DISTANCE))
                    SendTo(i.player.getClient(), writer, Packets.CurrentHealthNPC);
            }
        }

        public void SendBaseStats(Player player)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.getBaseStats().Strength);
                writer.Write(player.getBaseStats().Agility);
                writer.Write(player.getBaseStats().Stamina);
                writer.Write(player.getBaseStats().Intelligence);
                writer.Write(player.getBaseStats().Wit);
                writer.Write(player.getBaseStats().Mental);

                SendTo(player.getClient(), writer, Packets.SendBaseStats);
            }
        }
        public void SendStats(Player player)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.getStats().PhysicalAttack);
                writer.Write(player.getStats().MagicalAttack);
                writer.Write(player.getStats().AttackSpeed);
                writer.Write(player.getStats().CastingSpeed);
                writer.Write(player.getStats().PhysicalArmor);
                writer.Write(player.getStats().MagicalArmor);
                writer.Write(player.getStats().PhysicalCritical);
                writer.Write(player.getStats().MagicalCritical);
                writer.Write(player.getStats().Cooldown);
                writer.Write(player.getStats().MovementSpeed);
                writer.Write(player.getStats().HitChance);

                SendTo(player.getClient(), writer, Packets.SendStats);
            }
        }
        public void SendLevel(Player player)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.level);

                SendTo(player.getClient(), writer, Packets.SendLevel);
            }
        }

        public void EquipItem(Player player, Item item, bool equip)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.getClient().ID);

                writer.Write(equip);

                if (equip) writer.Write(item.ID);
                else writer.Write((int)item.EquipmentLocation);

                foreach (var i in World.getInstance.getWorldPlayersAround(player.worldPlayer.transform.position, player.worldPlayer.currentZone, World.MAX_PACKET_DISTANCE))
                    SendTo(i.player.getClient(), writer, Packets.EquipItem);
            }
        }
        public void UpdateSpell(Player player, Spell spell, SpellPacketUpdateType type, float currentCooldown = 0)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(spell.ID);
                writer.Write((int)type);
                if (type != SpellPacketUpdateType.Remove)
                    writer.Write(currentCooldown);

                SendTo(player.getClient(), writer, Packets.UpdatePlayerSpell);
            }
        }
        public void UseSpell(Player player, Spell spell, Vector3 target)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.getClient().ID);
                writer.Write(spell.ID);

                writer.Write(target.x);
                writer.Write(target.y);
                writer.Write(target.z);

                foreach (var i in World.getInstance.getWorldPlayersAround(player.worldPlayer.transform.position, player.worldPlayer.currentZone, World.MAX_PACKET_DISTANCE))
                    SendTo(i.player.getClient(), writer, Packets.UseSpell);
            }
        }
    }
}