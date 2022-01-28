using DarkRift.Server;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    public class World : MonoBehaviour
    {
        [System.Serializable]
        public class Zone
        {
            public int ID;
            public string Name;
            public Box box;
            public ZoneType type;

            [HideInInspector] public List<WorldPlayer> players = new List<WorldPlayer>();

            public List<Player> getPlayers()
            {
                List<Player> players = new List<Player>();

                foreach (var i in this.players)
                    players.Add(i.player);

                return players;
            }

            public void OnEnter(Player player)
            {
                switch (type)
                {
                    case ZoneType.Normal:
                        break;
                    default:
                        break;
                }
            }
        }
        public enum ZoneType
        {
            Normal = 0,
        }

        public static World getInstance;
        void Awake() { getInstance = this; }

        public const float MAX_PACKET_DISTANCE = 100f;

        [SerializeField] GameObject mapParent;
        [SerializeField] GameObject playerPrefab;

        [SerializeField] List<Zone> zones = new List<Zone>();
        
        public List<WorldPlayer> players = new List<WorldPlayer>();
        public List<WorldNPC> npc = new List<WorldNPC>();

        System.Random rnd = new System.Random();
        
        public void SpawnPlayer(Player player)
        {
            if (player == null)
            {
                print("player " + player == null);
                Debug.Log("Found invalid client id: " + player.getClient().ID);
                return;
            }

            GameObject p = Instantiate(playerPrefab, player.getPosition(), player.getRotation());
            WorldPlayer _p = p.GetComponent<WorldPlayer>();
            _p.SetUp(player);
            players.Add(_p);
            player.worldPlayer = _p;
            _p.setup = true;
            UpdateZoneForPlayer(_p);
        }
        public void DespawnPlayer(IClient client)
        {
            Destroy(getPlayer(client).gameObject);
            players.Remove(getPlayer(client));
        }
        public void DespawnPlayer(ushort id)
        {
            Destroy(getPlayer(id).gameObject);
            players.Remove(getPlayer(id));
        }

        public WorldPlayer getPlayer(ushort id) { return players.Find(x => x.player.getClient().ID == id); }
        public WorldPlayer getPlayer(IClient client) { return players.Find(x => x.player.getClient() == client); }

        public bool IsInsideZone(Vector3 pos, Zone zone)
        {
            return Utils_Maths.PointIsInsideBox(pos, zone.box);
        }
        public void UpdateZoneForPlayer(WorldPlayer p)
        {
            foreach (var zone in zones)
            {
                if (Utils_Maths.PointIsInsideBox(p.transform.position, zone.box))
                {
                    if (p.currentZone != zone)
                        EnterNewZone(p, zone);
                    return;
                }
            }

            p.currentZone = null;
            throw new System.NotImplementedException("Zone for player " + p.player.getSettings().Name + " not found!");
        }
        public Zone getZoneForPosition(Vector3 pos)
        {
            foreach (var i in zones)
                if (Utils_Maths.PointIsInsideBox(pos, i.box))
                    return i;

            return null;
        }
        public void EnterNewZone(WorldPlayer p, Zone newZone)
        {
            print("Player: " + p.player.getSettings().Name + " changed zone to: " + newZone.Name);
            Zone previousZone = p.currentZone;
            if (previousZone != null)
            {
                foreach (var i in previousZone.players)
                {
                    if (i == p)
                    {
                        previousZone.players.Remove(p);
                        break;
                    }
                }
            }
            Server.getInstance.PlayerChangedZone(p.player.getClient(), newZone.Name);

            p.currentZone = newZone;
            newZone.players.Add(p);

            Server.getInstance.EnterGame(p.player);
            foreach (var i in Holder.player)
                if (i.Value.worldPlayer.currentZone != newZone && i.Value != p.player && i.Key != p.player.getClient())
                    Server.getInstance.PlayerLeftZone(p.player.getClient(), i.Key);

            p.currentZone.OnEnter(p.player);
        }

        public List<WorldPlayer> getWorldPlayersAround(WorldPlayer player, float distance)
        {
            List<WorldPlayer> players = new List<WorldPlayer>();
            if (player.currentZone == null)
                return players;

            Sphere sphere = new Sphere();
            sphere.center = player.transform.position;
            sphere.radius = distance;
            
            foreach (var p in player.currentZone.players)
                if (Utils_Maths.PointIsInsideSphere(p.transform.position, sphere))
                    players.Add(p);

            return players;
        }
        public List<WorldPlayer> getWorldPlayersAround(Vector3 pos, Zone zone, float distance)
        {
            List<WorldPlayer> players = new List<WorldPlayer>();
            Sphere sphere = new Sphere();
            sphere.center = pos;
            sphere.radius = distance;

            foreach (var p in zone.players)
                if (Utils_Maths.PointIsInsideSphere(p.transform.position, sphere))
                    players.Add(p);

            return players;
        }

        public void SpawnNPC(long spawnId, int id, Vector3 pos, Quaternion rot, float respawnTime, bool onLoad, float currentHealth, float currentMana)
        {
            if (!onLoad)
            {
                spawnId = 0;
                long currentTry = 0;
                long maxTries = long.MaxValue;
                while (npcSpawnidExists(spawnId))
                {
                    spawnId = Utils.LongRandom(0, long.MaxValue, rnd);

                    currentTry++;
                    if (currentTry >= maxTries)
                        return;
                }

                NPC data = NPCDatabase.getInstance.getNpc(id);
                if (data == null)
                    return;

                Database.getInstance.SpawnNPC(spawnId, id, respawnTime, pos, rot, currentHealth, currentMana);
            }

            GameObject go = Instantiate(NPCDatabase.getInstance.getNpc(id).serverPrefab, pos, rot);
            go.name = NPCDatabase.getInstance.getNpc(id).Name;
            WorldNPC npc = go.GetComponent<WorldNPC>();
            if (npc == null)
            {
                Destroy(go);
                return;
            }

            npc.Spawn(spawnId, pos, respawnTime, NPCDatabase.getInstance.getNpc(id));
            npc.npc.vitals.CurrentHealth = currentHealth;
            npc.npc.vitals.CurrentMana = currentMana;
            this.npc.Add(npc);

            foreach (var i in npc.currentZone.players)
                Server.getInstance.SpawnNPC(npc, i.player.getClient());
        }
        public void DespawnNPC(int id)
        {
            foreach (var i in npc)
            {
                if (i.spawnID == id)
                {
                    Database.getInstance.DespawnNPC(i.spawnID);
                    Server.getInstance.DespawnNPC(id, i.currentZone);
                    Destroy(i.gameObject);
                    npc.Remove(i);
                }
            }
        }
        bool npcSpawnidExists(long id)
        {
            foreach (var i in npc)
                if (i.spawnID == id)
                    return true;

            return false;
        }
        public WorldNPC getNPC(long id)
        {
            foreach (var i in npc)
                if (i.spawnID == id)
                    return i;

            return null;
        }
    }
}