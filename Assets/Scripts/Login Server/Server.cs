using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using UnityEngine;

namespace LoginServer
{
    public class Server : MonoBehaviour
    {
        public static Server getInstance;
        void Awake() { getInstance = this; }

        public XmlUnityServer server { get; private set; }
        public float version = 1.0f;

        void Start()
        {
            Application.targetFrameRate = -1;
            server = GetComponent<XmlUnityServer>();

            Database.Init();

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

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(version);

                    SendTo(e.Client, writer, Packets.Version);
                }
            }
            else
            {
                Log("Client with id: " + e.Client.ID + " is already logged in and trying to log in again!", DarkRift.LogType.Warning);
            }
        }
        private void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            HandleData.Handle(sender, e);
        }
        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            if (Holder.getGameServer(e.Client) != null)
                Holder.gameServers.Remove(Holder.getGameServer(e.Client));
            else
            {
                if (Holder.accounts.ContainsKey(e.Client))
                {
                    if (Holder.accounts[e.Client].gs == null)
                    {
                        Holder.accounts.Remove(e.Client);

                        if (Holder.clients.ContainsKey(e.Client.ID))
                            Holder.clients.Remove(e.Client.ID);
                    }
                }
            }
        }

        public void Log(string txt, DarkRift.LogType type, Exception exception = null)
        {
            Debug.Log("[" + type.ToString() + "] " + txt);
        }

        void SendTo(IClient client, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            writer = Cryptography.EncryptWriter(writer);

            using (Message msg = Message.Create(packetID, writer)) client.SendMessage(msg, sendMode);

            writer.Dispose();
        }
        void SendToAll(DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            writer = Cryptography.EncryptWriter(writer);

            using (Message msg = Message.Create(packetID, writer))
            {
                foreach (var client in Holder.clients.Values)
                    client.SendMessage(msg, sendMode);
            }

            writer.Dispose();
        }
        void SendToGameServer(ushort ID, DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            writer = Cryptography.EncryptWriter(writer);

            using (Message msg = Message.Create(packetID, writer))
            {
                foreach (var gs in Holder.gameServers)
                {
                    if (gs.ID == ID)
                    {
                        gs.client.SendMessage(msg, sendMode);
                        return;
                    }
                }
            }

            writer.Dispose();
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
            if (client == null)
            {
                Log("null client", DarkRift.LogType.Info);
                return;
            }

            if (!Holder.clients.ContainsKey(client.ID))
                Log("Client with id: " + client.ID + " not found!", DarkRift.LogType.Error);

            if (Holder.clients.ContainsKey(client.ID))
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(client.ID);


                    SendToAll(writer, Packets.Disconnect);

                    OnExit.Handle(client);

                    if (Holder.accounts.ContainsKey(client))
                        Holder.accounts.Remove(client);

                    Holder.clients.Remove(client.ID);
                }
            }
            else
            {
                Log("Client with id: " + client.ID + " tried to disconnect but can't be found in the server!", DarkRift.LogType.Warning);
            }
        }
        public void LoginResponse(bool done, string response, string email, string password, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(done);

                if (done) response = "Done";
                Log("Login Response: " + response, DarkRift.LogType.Info);

                writer.Write(response);

                writer.Write(email);
                writer.Write(password);


                SendTo(client, writer, Packets.Login);
            }
        }
        public void RegisterResponse(bool done, string response, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(done);

                if (done) response = "Done";

                writer.Write(response);


                SendTo(client, writer, Packets.Register);
            }
        }
        public void ForgotPasswordResponse(string response, IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(response);


                SendTo(client, writer, Packets.ForgotPassowrd);
            }
        }

        public void SendGameServers(IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(Holder.gameServers.Count);
                Log("Gameservers available: " + Holder.gameServers.Count, DarkRift.LogType.Info);

                foreach (var gs in Holder.gameServers)
                {
                    writer.Write(gs.ID);
                    writer.Write(gs.Name);
                    writer.Write(gs.IP);
                    writer.Write(gs.Port);
                    bool isIpV4 = false;
                    if (gs.ipVersion == IPVersion.IPv4)
                        isIpV4 = true;
                    writer.Write(isIpV4);
                }


                SendTo(client, writer, Packets.RequestGameServers);
            }
        }
    }
}