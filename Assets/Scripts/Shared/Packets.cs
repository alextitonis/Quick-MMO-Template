public class Packets
{
    public static readonly ushort Welcome = 0;
    public static readonly ushort Disconnect = 1;
    public static readonly ushort ClientDisconnected = 2;

    public static readonly ushort Login = 3;
    public static readonly ushort Register = 4;
    public static readonly ushort ForgotPassowrd = 5;

    public static readonly ushort RequestCharacters = 6;
    public static readonly ushort CreateCharacter = 7;
    public static readonly ushort DeleteCharacter = 8;
    public static readonly ushort EnterGame = 9;
    public static readonly ushort ExitGame = 10;

    public static readonly ushort GameServer = 11;
    public static readonly ushort RequestGameServers = 12;
    public static readonly ushort ServerSelected = 13;

    public static readonly ushort TimeOut = 14;

    public static readonly ushort PlayerMovement = 15;
    public static readonly ushort Jump = 16;
    public static readonly ushort PlayerAnimation = 17;
    public static readonly ushort PlayAnimation = 18;

    public static readonly ushort GameServerDisconnected = 19;

    public static readonly ushort Version = 20;

    public static readonly ushort _SendMessage = 21;

    public static readonly ushort InventoryData = 22;
    public static readonly ushort SlotData = 23;
    public static readonly ushort RemoveItem = 24;

    public static readonly ushort MaxHealth = 25;
    public static readonly ushort CurrentHealth = 26;
    public static readonly ushort MaxMana = 27;
    public static readonly ushort CurrentMana = 28;
    public static readonly ushort Vitals = 29;

    public static readonly ushort SpawnNPC = 30;
    public static readonly ushort DespawnNPC = 31;
    public static readonly ushort MoveNPC = 32;
    public static readonly ushort CurrentHealthNPC = 33;

    public static readonly ushort NewZone = 34;

    public static readonly ushort MoveWithMouse = 35;
    public static readonly ushort SetTargetPlayer = 36;
    public static readonly ushort SetTargetNPC = 37;

    public static readonly ushort SendBaseStats = 38;
    public static readonly ushort SendStats = 39;
    public static readonly ushort SendLevel = 40;

    public static readonly ushort ReportAProblem = 41;

    public static readonly ushort EquipItem = 42;
    public static readonly ushort UseItem = 43;

    public static readonly ushort UpdatePlayerSpell = 44;
    public static readonly ushort UseSpell = 45;
}