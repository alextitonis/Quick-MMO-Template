using DarkRift;
using DarkRift.Server;
using System.Collections.Generic;

namespace LoginServer
{
    public class HandleData
    {
        public static void Handle(object sender, MessageReceivedEventArgs e)
        {
            using (Message msg = e.GetMessage())
            {
                using (DarkRiftReader reader = msg.GetReader())
                {
                    DarkRiftReader newReader = Cryptography.DecryptReader(reader);
                    if (msg.Tag == Packets.GameServer)
                    {
                        _GameServer gs = new _GameServer();
                        gs.client = e.Client;
                        gs.ID = gs.client.ID;
                        gs.Name = newReader.ReadString();
                        gs.IP = newReader.ReadString();
                        gs.Port = newReader.ReadInt32();
                        bool isIpV4 = newReader.ReadBoolean();

                        if (isIpV4)
                            gs.ipVersion = IPVersion.IPv4;
                        else
                            gs.ipVersion = IPVersion.IPv6;

                        Server.getInstance.Log("A gameserver connected with ip: " + gs.IP, LogType.Info);

                        if (Holder.clients.ContainsKey(gs.ID))
                            Holder.clients.Remove(gs.ID);

                        if (!Holder.gameServers.Contains(gs))
                            Holder.gameServers.Add(gs);
                        else
                            Server.getInstance.Log("Gameserver with ip: " + gs.IP + " already exists in the holder!", LogType.Error);
                    }
                    if (msg.Tag == Packets.Disconnect)
                    {
                        int temp = newReader.ReadInt32();
                        Server.getInstance.Disconnect(e.Client);
                    }
                    else if (msg.Tag == Packets.Login)
                    {
                        string email = newReader.ReadString();
                        string password = newReader.ReadString();

                        Database.getInstance.Login(e.Client, email, password);
                    }
                    else if (msg.Tag == Packets.Register)
                    {
                        string email = newReader.ReadString();
                        string password = newReader.ReadString();

                        Database.getInstance.Register(e.Client, email, password);
                    }
                    else if (msg.Tag == Packets.RequestGameServers)
                    {
                        int temp = newReader.ReadInt32();

                        Server.getInstance.SendGameServers(e.Client);
                    }
                    else if (msg.Tag == Packets.ServerSelected)
                    {
                        ushort id = newReader.ReadUInt16();
                        foreach (var gs in Holder.gameServers)
                        {
                            if (gs.ID == id)
                            {
                                Holder.accounts[e.Client].gs = gs;
                                return;
                            }
                        }
                    }
                    else if (msg.Tag == Packets.ForgotPassowrd)
                    {
                        string email = newReader.ReadString();

                        if (Database.getInstance.accountExists(email))
                        {
                            string password = Database.getInstance.getPassword(email);
                            EmailManager.SendEmail("Forgot Password", "Password: " + password, email);
                            Server.getInstance.ForgotPasswordResponse("An email has been sent with the password!", e.Client);
                        }
                        else
                        {
                            Server.getInstance.ForgotPasswordResponse("Account not found!", e.Client);
                        }
                    }
                    else if (msg.Tag == Packets.ClientDisconnected)
                    {
                        string email = newReader.ReadString();
                        Account acc = null;
                        foreach (var a in Holder.accounts.Values)
                        {
                            if (a.email == email)
                            {
                                acc = a;
                                break;
                            }
                        }

                        if (acc != null)
                            Server.getInstance.Disconnect(acc.client);
                        else
                            Server.getInstance.Log("Account with email: " + email + " not found online!", LogType.Error);

                    }
                    else if (msg.Tag == Packets.GameServerDisconnected)
                    {
                        int length = newReader.ReadInt32();

                        List<string> emails = new List<string>();
                        for (int i = 0; i < length; i++)
                            emails.Add(reader.ReadString());

                        foreach (var email in emails)
                        {
                            foreach (var acc in Holder.accounts)
                            {
                                if (acc.Value.email == email)
                                {
                                    Holder.accounts.Remove(acc.Key);
                                    break;
                                }
                            }
                        }

                        Holder.removeGameServer(e.Client);
                    }
                }
            }
        }
    }
}