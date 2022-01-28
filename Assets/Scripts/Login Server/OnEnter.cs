using DarkRift.Server;
using System.Threading;

namespace LoginServer
{
    public class OnEnter
    {
        public static void HandleClient(IClient client)
        {
            new Thread(() =>
            {

            }).Start();
        }
        public static void HandleLogin(IClient client)
        {
        }
    }
}