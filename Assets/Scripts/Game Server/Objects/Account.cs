using DarkRift.Server;
using System.Collections.Generic;

namespace GameServer
{
    public class Account
    {
        public IClient client;
        public string email;
        public string password;
        public bool timeOurSent;
        List<CharacterSettings> _Characters;
        public List<CharacterSettings> Characters
        {
            get
            {
                if (_Characters == null) _Characters = Database.getInstance.GetCharacters(client);
                return _Characters;
            }
            set
            {
                _Characters = value;
            }
        }
    }
}