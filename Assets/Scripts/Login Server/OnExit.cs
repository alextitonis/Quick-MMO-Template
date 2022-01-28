using DarkRift.Server;

namespace LoginServer
{
    public class OnExit
    {
        public static void Handle(IClient client)
        {
            if (Holder.accounts.ContainsKey(client))
                Database.getInstance.SaveAccount(client);
        }
    }
}