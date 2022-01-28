using DarkRift.Server;
using System.Collections.Generic;

namespace GameServer
{
    public class Holder
    {
        public static Dictionary<ushort, IClient> clients = new Dictionary<ushort, IClient>();
        public static Dictionary<IClient, Account> accounts = new Dictionary<IClient, Account>();
        public static Dictionary<IClient, Player> player = new Dictionary<IClient, Player>();

        public static Player getPlayer(ushort id)
        {
            foreach (var i in player)
                if (i.Key.ID == id)
                    return i.Value;

            return null;
        }
    }
}