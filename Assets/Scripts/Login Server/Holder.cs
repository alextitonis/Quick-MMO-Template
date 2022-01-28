using DarkRift.Server;
using System.Collections.Generic;

namespace LoginServer
{
    public class Holder
    {
        public static Dictionary<ushort, IClient> clients = new Dictionary<ushort, IClient>();
        public static Dictionary<IClient, Account> accounts = new Dictionary<IClient, Account>();
        public static List<_GameServer> gameServers = new List<_GameServer>();

        public static _GameServer getGameServer(ushort ID)
        {
            foreach (var gs in gameServers)
            {
                if (gs.ID == ID)
                    return gs;
            }

            return null;
        }
        public static _GameServer getGameServer(IClient client)
        {
            foreach (var gs in gameServers)
            {
                if (gs.client == client)
                    return gs;
            }

            return null;
        }
        public static void removeGameServer(IClient client)
        {
            foreach (var gs in gameServers)
            {
                if (gs.client == client)
                {
                    gameServers.Remove(gs);
                    return;
                }
            }
        }
    }
}