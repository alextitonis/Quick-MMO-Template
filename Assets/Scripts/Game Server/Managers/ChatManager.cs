using DarkRift.Server;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    public class ChatManager : MonoBehaviour
    {
        public static void Handle(_Message msg, IClient sender)
        {
            if (msg.Content.StartsWith("/"))
            {
                string[] data = msg.Content.Split(' ');
                if (Holder.player.ContainsKey(sender))
                {
                    Player player = Holder.player[sender];
                    if (player == null)
                        return;

                    if (msg.Content.Contains("setAccess"))
                    {
                        if (player.getSettings().access == Config.AdminAccess)
                        {
                            if (data.Length == 3)
                            {
                                string targetName = data[1];
                                int access = -2;
                                bool parsed = int.TryParse(data[2], out access);

                                Player _player = null;
                                foreach (var i in Holder.player.Values)
                                {
                                    if (i.getSettings().Name == targetName)
                                    {
                                        _player = i;
                                        break;
                                    }
                                }

                                if (parsed && access != -2)
                                    if (_player != null)
                                        _player.SetAccess(access);

                                if (_player == null)
                                    player.SendErrorMessage("Invalid target!");
                            }
                            else
                                player.SendErrorMessage("Invalid format (right format: /setAcces playerName access");
                        }
                    }
                    else if (msg.Content.Contains("spawn"))
                    {
                        if (player.getSettings().access == Config.GameMasterAccess)
                        {
                            if (data.Length == 3)
                            {
                                int npcID = -1;
                                bool parsedNpcId = int.TryParse(data[1], out npcID);
                                int respawnTime = -1;
                                bool parsedRespawnTime = int.TryParse(data[2], out respawnTime);

                                if (parsedNpcId && npcID != -1 && parsedRespawnTime && respawnTime != -1)
                                {
                                    NPC _data = NPCDatabase.getInstance.getNpc(npcID);
                                    if (_data == null)
                                    {
                                        player.SendErrorMessage("NPC with id: " + npcID + " not found!");
                                        return;
                                    }

                                    World.getInstance.SpawnNPC(-1, npcID, player.getPosition(), player.getRotation(), respawnTime, false, _data.vitals.MaxHealth, _data.vitals.MaxMana);
                                }
                            }
                            else
                                player.SendErrorMessage("Invalid format (right format: /spawn npcID");
                        }
                    }
                    else
                        player.SendErrorMessage("Command not found!");

                    //ban
                    //kick
                    //chat_ban
                    //un_ban
                    //un_chat_ban

                    //data[0] is the command it self (/kick)
                    //data[1] is the first argument (/kick player_name)
                    //data[2+] are more arguments
                }

                return;
            }

            string _sender = "Invalid Sender";
            if (Holder.player.ContainsKey(sender))
                _sender = Holder.player[sender].getSettings().Name;

            Player _Sender = null;
            if (Holder.player.ContainsKey(sender))
                _Sender = Holder.player[sender];

            msg.Sender = _sender;
            List<Player> players = new List<Player>();

            switch (msg.Type)
            {
                case _MessageType._Chat_General:
                    if (_Sender != null)
                        foreach (var i in Holder.player.Values)
                            players.Add(i);
                    else
                        return;
                    break;
                case _MessageType._Chat_Global:
                    foreach (var i in Holder.player)
                        players.Add(i.Value);
                    break;
                case _MessageType._Chat_Whisper:
                    if (!msg.Content.StartsWith("/w"))
                        return;

                    string m = msg.Content;
                    m.Remove(0, 3);

                    string to = m.Split(' ')[0];

                    if (m.Split(' ').Length == 1)
                        return;

                    m.Remove(0, to.Length + 1);

                    Player _to = null;
                    foreach (var i in Holder.player)
                    {
                        if (i.Value.getSettings().Name == to)
                        {
                            _to = i.Value;
                            break;
                        }
                    }

                    players.Add(_to);

                    msg.Content = m;
                    break;
                default:
                    return;
            }

            if (players.Count <= 0)
                return;

            Server.getInstance._SendMessage(msg, players);
        }
    }
}