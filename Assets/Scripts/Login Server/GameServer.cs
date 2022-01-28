using DarkRift;
using DarkRift.Server;

namespace LoginServer
{
    public class _GameServer
    {
        public IClient client;
        public ushort ID;
        public string Name;
        public string IP;
        public int Port;
        public IPVersion ipVersion;
    }
}