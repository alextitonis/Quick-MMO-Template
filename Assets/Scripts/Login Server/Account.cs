using DarkRift.Server;

namespace LoginServer
{
    public class Account
    {
        public IClient client;
        public string email;
        public string password;
        public _GameServer gs;
    }
}