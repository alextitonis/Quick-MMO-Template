using DarkRift;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemDatabase;
using static SpellDatabase;

public class HandleData : MonoBehaviour
{

    public static HandleData getInstance;
    void Awake()
    {
        getInstance = this;
    }

    [SerializeField] Text Client_ID_Text;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] Transform PlayerParent;
    [SerializeField] float version = 1.0f;

    [HideInInspector] public Dictionary<ushort, GameObject> Players = new Dictionary<ushort, GameObject>();
    [HideInInspector] public Player localPlayer;
    [HideInInspector] public Dictionary<long, GameObject> NPCs = new Dictionary<long, GameObject>();

    public void ClearGame()
    {
        foreach (var i in Players)
            Destroy(i.Value.gameObject);

        foreach (var i in NPCs)
            Destroy(i.Value.gameObject);

        if (localPlayer != null)
            Destroy(localPlayer);

        Players.Clear();
        NPCs.Clear();
        localPlayer = null;
    }
    public void Handle(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        using (Message msg = e.GetMessage())
        {
            using (DarkRiftReader reader = msg.GetReader())
            {
                DarkRiftReader newReader = Cryptography.DecryptReader(reader);

                if (msg.Tag == Packets.Welcome)
                {
                    Client.localID = newReader.ReadUInt16();
                    Client.maxCharacters = newReader.ReadInt32();
                }
                else if (msg.Tag == Packets.Disconnect)
                {
                    ushort disconnected = newReader.ReadUInt16();

                    if (Client.localID == disconnected)
                    {
                        foreach (var player in Players)
                            Destroy(player.Value);

                        Players.Clear();

                        Utils.getInstance.Exit();
                        Camera.main.GetComponent<CameraController>().SetTarget(null);
                    }
                    else
                    {
                        if (Players.ContainsKey(disconnected))
                        {
                            Destroy(Players[disconnected]);
                            Players.Remove(disconnected);
                        }
                    }
                }
                else if (msg.Tag == Packets.Login)
                {
                    bool done = newReader.ReadBoolean();
                    string response = newReader.ReadString();
                    string email = newReader.ReadString();
                    string password = newReader.ReadString();

                    Debug.Log("Login Response: Done: " + done + " Response: " + response);

                    if (done)
                    {
                        LoginManager.getInstance.email = email;
                        LoginManager.getInstance.password = password;
                        LoginManager.getInstance.CleanUp();
                        MenuManager.getInstance.ChangeScreen(Screen.ServerSelection);
                    }
                    else
                    {
                        LoginManager.getInstance.CleanUp();
                        LoginManager.getInstance.Log(response);
                    }

                    Utils.getInstance.Loading(false);
                }
                else if (msg.Tag == Packets.Register)
                {
                    bool done = newReader.ReadBoolean();
                    string response = newReader.ReadString();

                    Debug.Log("Register Response: Done: " + done + " Response: " + response);

                    if (done)
                    {
                        RegistrationManager.getInstance.CleanUp();
                        MenuManager.getInstance.ChangeScreen(Screen.Login);
                    }
                    else
                    {
                        RegistrationManager.getInstance.CleanUp();
                        RegistrationManager.getInstance.Log(response);
                    }

                    Utils.getInstance.Loading(false);
                }
                else if (msg.Tag == Packets.CreateCharacter)
                {
                    bool done = newReader.ReadBoolean();
                    string response = newReader.ReadString();

                    CharacterCreateManager.getInstance.Response(done, response);

                    Utils.getInstance.Loading(false);
                }
                else if (msg.Tag == Packets.DeleteCharacter)
                {
                    bool done = newReader.ReadBoolean();
                    string response = newReader.ReadString();
                    string deletedID = newReader.ReadString();

                    CharacterSelectManager.getInstance.DeleteCharacterResponse(done, response, deletedID);

                    Utils.getInstance.Loading(false);
                }
                else if (msg.Tag == Packets.RequestCharacters)
                {
                    int length = newReader.ReadInt32();
                    List<CharacterSettings> chars = new List<CharacterSettings>();

                    for (int i = 0; i < length; i++)
                    {
                        CharacterSettings settings = new CharacterSettings();
                        settings.Name = newReader.ReadString();
                        settings.Race = (Races)newReader.ReadInt32();
                        settings.Class = (Classes)newReader.ReadInt32();
                        settings.Gender = (Genders)newReader.ReadInt32();
                        settings.ID = newReader.ReadString();

                        chars.Add(settings);
                    }

                    CharacterSelectManager.getInstance.Enter(chars);

                    Utils.getInstance.Loading(false);
                }
                else if (msg.Tag == Packets.EnterGame)
                {
                    ushort clientId = newReader.ReadUInt16();
                    string charName = newReader.ReadString();
                    Vector3 charPosition = new Vector3(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());
                    Quaternion charRotation = new Quaternion(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());
                    Races race = (Races)reader.ReadInt32();
                    Classes _class = (Classes)reader.ReadInt32();
                    Genders gender = (Genders)reader.ReadInt32();
                    float maxHealth = newReader.ReadSingle();
                    float currentHealth = newReader.ReadSingle();

                    if (clientId == Client.localID)
                        MenuManager.getInstance.ChangeScreen(Screen.InGame);

                    GameObject player = Instantiate(PlayerPrefab, charPosition, charRotation);
                    player.name = charName;
                    player.tag = "Player";
                    player.GetComponent<Player>().SetUp(clientId, charName, race, _class, gender, charPosition, charRotation, maxHealth, currentHealth);
                    Players.Add(clientId, player);
                    if (clientId == Client.localID)
                        localPlayer = player.GetComponent<Player>();
                }
                else if (msg.Tag == Packets.ExitGame)
                {
                    ushort id = newReader.ReadUInt16();

                    if (Players.ContainsKey(id))
                    {
                        Destroy(Players[id]);
                        Players.Remove(id);
                    }
                }
                else if (msg.Tag == Packets.RequestGameServers)
                {
                    int length = newReader.ReadInt32();
                    List<_GameServer> gss = new List<_GameServer>();
                    for (int i = 0; i < length; i++)
                    {
                        _GameServer gs = new _GameServer();
                        gs.ID = newReader.ReadUInt16();
                        gs.Name = newReader.ReadString();
                        gs.IP = newReader.ReadString();
                        gs.Port = newReader.ReadInt32();
                        bool isIpV4 = newReader.ReadBoolean();

                        if (isIpV4)
                            gs.ipVersion = IPVersion.IPv4;
                        else
                            gs.ipVersion = IPVersion.IPv6;

                        gss.Add(gs);
                    }

                    ServerSelectionManager.getInstance.GameServers = gss;
                    ServerSelectionManager.getInstance.SetUp(false);

                    Utils.getInstance.Loading(false);
                }
                else if (msg.Tag == Packets.ForgotPassowrd)
                {
                    string response = newReader.ReadString();

                    ForgotPasswordManager.getInstance.Log(response);

                    Utils.getInstance.Loading(false);
                }
                else if (msg.Tag == Packets.TimeOut)
                {
                    bool isOnline = newReader.ReadBoolean();

                    if (isOnline)
                        StartCoroutine(Client.getInstance.TimeOutCheckRoutine());
                    else
                        MenuManager.getInstance.ChangeScreen(Screen.Offline);
                }
                else if (msg.Tag == Packets.PlayerMovement)
                {
                    ushort clientId = newReader.ReadUInt16();
                    Vector3 position = new Vector3(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());
                    Quaternion rotation = new Quaternion(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());

                    if (Players.ContainsKey(clientId))
                    {
                        Players[clientId].transform.position = position;
                        Players[clientId].transform.rotation = rotation;
                    }
                }
                else if (msg.Tag == Packets.PlayerAnimation)
                {
                    ushort animated = newReader.ReadUInt16();
                    AnimationType type = (AnimationType)newReader.ReadInt32();
                    string animationName = newReader.ReadString();
                    string value = newReader.ReadString();

                    if (Players.ContainsKey(animated))
                        Players[animated].GetComponent<Player>().ApplyAnimation(type, animationName, value);

                }
                else if (msg.Tag == Packets.Version)
                {
                    float version = newReader.ReadSingle();

                    if (version != this.version)
                    {
                        Debug.Log("incompatible Client! Disconnecting...");
                        Client.getInstance.CloseConnection();
                        MenuManager.getInstance.ChangeScreen(Screen.Offline);
                    }
                }
                else if (msg.Tag == Packets.PlayAnimation)
                {
                    ushort clientID = newReader.ReadUInt16();
                    string anim = newReader.ReadString();

                    Players[clientID].GetComponent<Player>().PlayAnimation(anim);
                }
                else if (msg.Tag == Packets._SendMessage)
                {
                    _Message _msg = new _Message(newReader.ReadString(), newReader.ReadString(), (_MessageType)reader.ReadInt32());

                    ChatManager.getInstance.GetMessage(_msg);
                }
                else if (msg.Tag == Packets.InventoryData)
                {
                    int count = newReader.ReadInt32();
                    List<InventoryItem> inventory = new List<InventoryItem>();

                    for (int i = 0; i < count; i++)
                    {
                        InventoryItem item = new InventoryItem();
                        item.item = ItemDatabase.getInstance.getItem(newReader.ReadInt32());
                        item.quantity = newReader.ReadInt32();
                        item.slot = newReader.ReadInt32();

                        inventory.Add(item);
                    }

                    foreach (var i in inventory)
                        localPlayer.AddItem(i);
                }
                else if (msg.Tag == Packets.SlotData)
                {
                    Player p = localPlayer;
                    int itemId = newReader.ReadInt32();
                    int quantity = newReader.ReadInt32();
                    int slot = newReader.ReadInt32();

                    if (itemId == -1)
                        p.RemoveItem(slot);
                    else
                    {
                        InventoryItem item = new InventoryItem();
                        item.item = ItemDatabase.getInstance.getItem(itemId);
                        item.quantity = quantity;
                        item.slot = slot;

                        p.AddItem(item);
                    }
                }
                else if (msg.Tag == Packets.MaxHealth)
                {
                    ushort player = newReader.ReadUInt16();
                    float health = newReader.ReadSingle();

                    if (player == Client.localID)
                        VitalsManager.getInstance.SetMaxHealth(health);

                    if (Players.ContainsKey(player))
                        Players[player].GetComponent<Player>().SetMaxHealth(health);
                }
                else if (msg.Tag == Packets.CurrentHealth)
                {
                    ushort player = newReader.ReadUInt16();
                    float health = newReader.ReadSingle();

                    if (player == Client.localID)
                        VitalsManager.getInstance.SetCurrentHealth(health);

                    if (Players.ContainsKey(player))
                        Players[player].GetComponent<Player>().SetCurrentHealth(health);
                }
                else if (msg.Tag == Packets.MaxMana)
                {
                    float mm = newReader.ReadSingle();

                    VitalsManager.getInstance.SetMaxMana(mm);

                    localPlayer.SetMaxMana(mm);
                }
                else if (msg.Tag == Packets.CurrentMana)
                {
                    float cm = newReader.ReadSingle();

                    VitalsManager.getInstance.SetCurrentMana(cm);

                    localPlayer.SetMaxMana(cm);
                }
                else if (msg.Tag == Packets.Vitals)
                {
                    float mh = newReader.ReadSingle();
                    float ch = newReader.ReadSingle();
                    float mm = newReader.ReadSingle();
                    float cm = newReader.ReadSingle();

                    VitalsManager.getInstance.SetMaxHealth(mh);
                    VitalsManager.getInstance.SetCurrentHealth(ch);
                    VitalsManager.getInstance.SetMaxMana(mm);
                    VitalsManager.getInstance.SetCurrentMana(cm);

                    localPlayer.SetMaxHealth(mh);
                    localPlayer.SetCurrentHealth(ch);
                    localPlayer.SetMaxMana(mm);
                    localPlayer.SetCurrentMana(ch);
                }
                else if (msg.Tag == Packets.SpawnNPC)
                {
                    int id = newReader.ReadInt32();
                    long spawnID = newReader.ReadInt64();

                    Vector3 pos = new Vector3(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());
                    Quaternion rot = new Quaternion(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());

                    GameObject go = Instantiate(NPCDatabase.getInstance.getObject(id), pos, rot);
                    go.name = NPCDatabase.getInstance.getNPCName(id);
                    go.tag = "Player";
                    go.GetComponent<WorldNPC>().SetUp(NPCDatabase.getInstance.getNPC(id), spawnID);

                    if (NPCs.ContainsKey(id))
                    {
                        Destroy(NPCs[id]);
                        NPCs.Remove(id);
                    }

                    Debug.Log("Spawning npc with id: " + spawnID);
                    NPCs.Add(spawnID, go);
                }
                else if (msg.Tag == Packets.DespawnNPC)
                {
                    long spawnID = newReader.ReadInt64();
                    print("Despawning npc with id: " + spawnID);

                    if (NPCs.ContainsKey(spawnID))
                    {
                        Destroy(NPCs[spawnID]);
                        NPCs.Remove(spawnID);
                    }
                }
                else if (msg.Tag == Packets.MoveNPC)
                {
                    long spawnID = newReader.ReadInt64();

                    Vector3 pos = new Vector3(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());
                    Quaternion rot = new Quaternion(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());

                    if (NPCs.ContainsKey(spawnID))
                    {
                        NPCs[spawnID].transform.position = pos;
                        NPCs[spawnID].transform.rotation = rot;
                    }
                }
                else if (msg.Tag == Packets.NewZone)
                {
                    string zoneName = newReader.ReadString();

                    foreach (var i in Players)
                        Destroy(i.Value);

                    Players.Clear();

                    foreach (var i in NPCs)
                        Destroy(i.Value);

                    NPCs.Clear();
                }
                else if (msg.Tag == Packets.CurrentHealthNPC)
                {
                    long spawnId = newReader.ReadInt64();
                    float currentHealth = newReader.ReadSingle();

                    if (NPCs.ContainsKey(spawnId))
                        NPCs[spawnId].GetComponent<WorldNPC>().data.vitals.CurrentHealth = currentHealth;
                }
                else if (msg.Tag == Packets.SendBaseStats)
                {
                    BaseStats stats = new BaseStats();
                    stats.Strength = newReader.ReadUInt16();
                    stats.Agility = newReader.ReadUInt16();
                    stats.Stamina = newReader.ReadUInt16();
                    stats.Intelligence = newReader.ReadUInt16();
                    stats.Wit = newReader.ReadUInt16();
                    stats.Mental = newReader.ReadUInt16();

                    Players[Client.localID].GetComponent<Player>().baseStats = stats;
                }
                else if (msg.Tag == Packets.SendStats)
                {
                    Stats stats = new Stats();
                    stats.PhysicalAttack = newReader.ReadSingle();
                    stats.MagicalAttack = newReader.ReadSingle();
                    stats.AttackSpeed = newReader.ReadSingle();
                    stats.CastingSpeed = newReader.ReadSingle();
                    stats.PhysicalArmor = newReader.ReadSingle();
                    stats.MagicalArmor = newReader.ReadSingle();
                    stats.PhysicalCritical = newReader.ReadSingle();
                    stats.MagicalCritical = newReader.ReadSingle();
                    stats.Cooldown = newReader.ReadSingle();
                    stats.MovementSpeed = newReader.ReadSingle();
                    stats.HitChance = newReader.ReadSingle();

                    Players[Client.localID].GetComponent<Player>().stats = stats;
                }
                else if (msg.Tag == Packets.SendLevel)
                {
                    ushort level = newReader.ReadUInt16();

                    Players[Client.localID].GetComponent<Player>().level = level;
                }
                else if (msg.Tag == Packets.EquipItem)
                {
                    ushort playerId = newReader.ReadUInt16();

                    bool equip = newReader.ReadBoolean();

                    int itemId = -1;
                    EquipmentLocation location = EquipmentLocation.None;
                    if (equip) itemId = newReader.ReadInt32();
                    else location = (EquipmentLocation)reader.ReadInt32();

                    if (Players.ContainsKey(playerId))
                    {
                        if (equip) Players[playerId].GetComponent<Player>().Equip(ItemDatabase.getInstance.getItem(itemId));
                        else Players[playerId].GetComponent<Player>().UnEquip(location);
                    }
                }
                else if (msg.Tag == Packets.UpdatePlayerSpell)
                {
                    Spell spell = SpellDatabase.getInstance.getSpell(newReader.ReadInt32());
                    SpellPacketUpdateType type = (SpellPacketUpdateType)newReader.ReadInt32();
                    float cooldown = 0f;

                    if (type != SpellPacketUpdateType.Remove)
                        cooldown = newReader.ReadSingle();

                    switch (type)
                    {
                        case SpellPacketUpdateType.Add:
                            localPlayer.AddSpell(spell, cooldown);
                            break;
                        case SpellPacketUpdateType.Remove:
                            localPlayer.RemoveSpell(spell);
                            break;
                        case SpellPacketUpdateType.UpdateCooldown:
                            localPlayer.UpdateCooldown(spell, cooldown);
                            break;
                    }
                }
                else if (msg.Tag == Packets.UseSpell)
                {
                    ushort playerId = newReader.ReadUInt16();
                    if (Players.ContainsKey(playerId))
                    {
                        Player player = Players[playerId].GetComponent<Player>();
                        if (player == null)
                            return;

                        Spell spell = SpellDatabase.getInstance.getSpell(newReader.ReadInt32());
                        if (spell == null)
                            return;

                        Vector3 target = new Vector3(newReader.ReadSingle(), newReader.ReadSingle(), newReader.ReadSingle());
                        player.UseSpell(spell, true, target);
                    }
                }
            }
        }
    }
}