using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.Net;
using UnityEngine;

namespace GameServer
{
    public class Client : MonoBehaviour
    {
        public static Client getInstance;
        void Awake() { getInstance = this; }

        public UnityClient client { get; private set; }
        public static ushort localID;
        public static int maxCharacters = 0;

        [SerializeField] string loginServerIP = "127.0.0.1";
        [SerializeField] int loginServerPort = 4296;
        [SerializeField] IPVersion loginServerIpVersion = IPVersion.IPv4;
        void Start()
        {
            client = GetComponent<UnityClient>();
            client.isServer = true;
            client.Connect(IPAddress.Parse(loginServerIP), loginServerPort, loginServerIpVersion);
            SetUp();
        }

        public void SetUp()
        {
            client.MessageReceived += MessageReceived;
            client.Disconnected += ServerDisconnected;
        }

        public void CloseConnection()
        {
            client.Disconnect();
        }

        private void ServerDisconnected(object sender, DisconnectedEventArgs e)
        {
            Server.getInstance.Log("Login Server Disconnected!", DarkRift.LogType.Info);
        }
        private void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            HandleData.HandleFromLogin(sender, e);
        }

        void Send(DarkRiftWriter writer, ushort packetID, SendMode sendMode = SendMode.Reliable)
        {
            writer = Cryptography.EncryptWriter(writer);

            using (Message msg = Message.Create(packetID, writer)) client.SendMessage(msg, sendMode);

            writer.Dispose();
        }

        public void JoinToTheGameServer()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(Config.Game_Server_Name);
                writer.Write(Config.Game_Server_Ip);
                writer.Write(Config.Game_Server_Port);
                writer.Write(Config.Game_Server_Ip_Version_IpV4);

                Send(writer, Packets.GameServer);
            }
        }
        public void SendDisconnectionToTheLoginServer(string email)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(email);

                Send(writer, Packets.ClientDisconnected);
            }
        }
        public void Closing()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(Holder.accounts.Count);

                foreach (var acc in Holder.accounts)
                    writer.Write(acc.Value.email);

                Send(writer, Packets.GameServerDisconnected);
            }

            CloseConnection();
        }
    }
}