using DarkRift;
using DarkRift.Client;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    public class HandleData
    {
        public static void HandleFromLogin(object sender, MessageReceivedEventArgs e)
        {
            using (Message msg = e.GetMessage())
            {
                using (DarkRiftReader reader = msg.GetReader())
                {
                    DarkRiftReader newReader = Cryptography.DecryptReader(reader);

                    if (msg.Tag == Packets.Welcome)
                    {
                        Client.localID = newReader.ReadUInt16();
                        Client.getInstance.JoinToTheGameServer();
                    }
                }
            }
        }
        public static void HandleFromClient(object sender, DarkRift.Server.MessageReceivedEventArgs e)
        {
            using (Message msg = e.GetMessage())
            {
                using (DarkRiftReader reader = msg.GetReader())
                {
                    DarkRiftReader newReader = Cryptography.DecryptReader(reader);

                    if (msg.Tag == Packets.Welcome)
                    {
                        string email = newReader.ReadString();
                        string password = newReader.ReadString();

                        Account acc = new Account();
                        acc.email = email;
                        acc.password = password;
                        acc.client = e.Client;

                        Holder.accounts.Add(e.Client, acc);
                        Server.getInstance.Log("new connection: " + email, DarkRift.LogType.Info);
                    }
                    else if (msg.Tag == Packets.CreateCharacter)
                    {
                        CharacterSettings c = new CharacterSettings();
                        c.Name = newReader.ReadString();
                        c.Race = (Races)reader.ReadInt32();
                        c.Class = (Classes)reader.ReadInt32();
                        c.Gender = (Genders)reader.ReadInt32();

                        Database.getInstance.CreateCharacter(c, e.Client);
                    }
                    else if (msg.Tag == Packets.DeleteCharacter)
                    {
                        string id = newReader.ReadString();

                        Database.getInstance.DeleteCharacter(id, e.Client);
                    }
                    else if (msg.Tag == Packets.RequestCharacters)
                    {
                        int temp = newReader.ReadInt32();

                        Server.getInstance.SendCharacters(Holder.accounts[e.Client].Characters, e.Client);
                    }
                    else if (msg.Tag == Packets.EnterGame)
                    {
                        string charID = newReader.ReadString();
                        Server.getInstance.Log("Char entered: " + charID, DarkRift.LogType.Info);

                        List<CharacterSettings> chars = Holder.accounts[e.Client].Characters;
                        foreach (var c in chars)
                        {
                            if (c.ID == charID)
                            {
                                Player player = new Player(e.Client, c);
                                Holder.player.Add(e.Client, player);

                                return;
                            }
                        }

                        Server.getInstance.LogError("Character with id: " + charID + " not found!");
                    }
                    else if (msg.Tag == Packets.Disconnect)
                    {
                        int temp = newReader.ReadInt32();

                        Server.getInstance.Disconnect(e.Client);
                    }
                    else if (msg.Tag == Packets.TimeOut)
                    {
                        bool isOnline = newReader.ReadBoolean();

                        if (isOnline)
                            Holder.accounts[e.Client].timeOurSent = false;
                        else
                            Server.getInstance.Disconnect(e.Client);
                    }
                    else if (msg.Tag == Packets.PlayerMovement)
                    {
                        float horizontal = reader.ReadSingle();
                        float vertical = reader.ReadSingle();

                        if (horizontal > 1 || horizontal < -1)
                        {
                            Server.getInstance.Log("Player with id: " + e.Client.ID + " sent invalid horizontal movement input data: " + horizontal, DarkRift.LogType.Warning);
                            return;
                        }
                        else if (vertical > 1 || vertical < -1)
                        {
                            Server.getInstance.Log("Player with id: " + e.Client.ID + " sent invalid vertical movement input data: " + vertical, DarkRift.LogType.Warning);
                            return;
                        }

                        if (Holder.player.ContainsKey(e.Client))
                            Holder.player[e.Client].Move(horizontal, vertical);
                    }
                    else if (msg.Tag == Packets.Jump)
                    {
                        byte temp = reader.ReadByte();

                        if (Holder.player.ContainsKey(e.Client))
                            Holder.player[e.Client].Jump();
                    }
                    else if (msg.Tag == Packets.PlayerAnimation)
                    {
                        AnimationType type = (AnimationType)reader.ReadInt32();
                        string animationName = newReader.ReadString();
                        string value = newReader.ReadString();

                        Server.getInstance.PlayerAnimation(e.Client, type, animationName, value);
                    }
                    else if (msg.Tag == Packets.PlayAnimation)
                    {
                        string anim = reader.ReadString();

                        Server.getInstance.PlayAnimation(e.Client, anim);
                    }
                    else if (msg.Tag == Packets._SendMessage)
                    {
                        _Message _msg = new _Message("", reader.ReadString(), (_MessageType)reader.ReadInt32());

                        ChatManager.Handle(_msg, e.Client);
                    }
                    else if (msg.Tag == Packets.RemoveItem)
                    {
                        int slot = reader.ReadInt32();

                        Holder.player[e.Client].RemoveItem(slot);
                    }
                    else if (msg.Tag == Packets.MoveWithMouse)
                    {
                        Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        WorldPlayer player = World.getInstance.getPlayer(e.Client);
                        if (player != null)
                            player.MoveWithmouse(pos);
                    }
                    else if (msg.Tag == Packets.SetTargetPlayer)
                    {
                        ushort id = reader.ReadUInt16();
                        Player player = Holder.getPlayer(id);
                        Debug.Log(id);
                        if (player != null)
                        {
                            if (Holder.player.ContainsKey(e.Client))
                            {
                                Holder.player[e.Client].SetTarget(player.worldPlayer, null);
                            }
                        }
                    }
                    else if (msg.Tag == Packets.SetTargetNPC)
                    {
                        long id = reader.ReadInt64();
                        Debug.Log(id);
                        if (id == -1)
                        {
                            if (Holder.player.ContainsKey(e.Client))
                            {
                                Holder.player[e.Client].SetTarget(null, null);
                            }
                        }

                        WorldNPC npc = World.getInstance.getNPC(id);
                        Debug.Log(npc == null);
                        if (npc != null)
                        {
                            if (Holder.player.ContainsKey(e.Client))
                            {
                                Holder.player[e.Client].SetTarget(null, npc);
                            }
                        }
                    }
                    else if (msg.Tag == Packets.ReportAProblem)
                    {
                        string data = reader.ReadString();

                        Player p = Holder.getPlayer(e.Client.ID);
                        bool hasPlayer = p != null;
                        if (!hasPlayer)
                            Debug.Log("Client with id: " + e.Client.ID + " has player online: " + hasPlayer + " had a problem: " + data);
                        else
                            Debug.Log("Client with id: " + e.Client.ID + " has player online: " + hasPlayer + " (Player name: " + p.getSettings().Name + " had a problem: " + data);
                    }
                    else if (msg.Tag == Packets.UseItem)
                    {
                        int itemId = reader.ReadInt32();

                        Player p = Holder.getPlayer(e.Client.ID);
                        if (p != null)
                            p.UseItem(ItemDatabase.getInstance.getItem(itemId));
                    }
                    else if (msg.Tag == Packets.EquipItem)
                    {
                        EquipmentLocation slot = (EquipmentLocation)reader.ReadInt32();

                        Player p = Holder.getPlayer(e.Client.ID);
                        if (p != null)
                            p.UnEquipItem(slot);
                    }
                    else if (msg.Tag == Packets.UseSpell)
                    {
                        int spellID = reader.ReadInt32();

                        Player player = Holder.getPlayer(e.Client.ID);
                        if (player != null)
                            player.UseSpell(SpellDatabase.getInstance.getSpell(spellID));
                    }
                }
            }
        }
    }
}