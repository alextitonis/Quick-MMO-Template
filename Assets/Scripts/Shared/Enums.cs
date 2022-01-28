public enum Screen
{
    Starting = 0,
    Login = 1,
    Register = 2,
    InGame = 3,
    Offline = 4,
    ForgotPassword = 5,
    CharacterSelect = 6,
    CharacterCreate = 7,
    ServerSelection = 8,
}

public enum Classes
{
    Fighter = 0,
    Mage = 1,
}

public enum Races
{
    Human = 0,
    Elf = 1,
    DarkElf = 2,
    Orc = 3,
}

public enum Genders
{
    Male = 0,
    Female = 1,
}

public enum AnimationType
{
    Bool = 0,
    Trigger,
    Float,
    Integer,
}


public enum SpellCostType
{
    None = 0,
    Mana = 1,
    Health = 2,
}
public enum SpellOperateType
{
    Passive = 0,
    Active = 1,
}
public enum SpellType
{
    None = -1,
    Buff = 0,
    Debuff = 1,
    Aggessive = 2,
    Heal = 3,
}
public enum SpellEffect
{
    None = 0,
    Burn = 1,
    Freeze = 2,
    Slow = 3,
    Stun = 4,
    Sleep = 5,
    Root = 6,
}
public enum SpellTarget
{
    None = -1,
    Self = 0,
    Target = 1,
    AOE = 3,
}
public enum SpellPacketUpdateType
{
    Add = 0,
    Remove = 1,
    UpdateCooldown = 2,
}

public enum ItemType
{
    None = 0,
    Quest = 1,
    Equipment = 2,
    Potion = 3,
    Currency = 4,
    Material = 5,
}
public enum EquipmentLocation
{
    None = -1,

    Left_Hand = 0,
    Right_Hand = 1,

    Head = 2,
    Chest = 3,
    Legs = 4,
    Feet = 5,
    Gloves = 6,
}
public enum EquipmentMeshRegion
{
    Legs, Arms, Torso,
}

public enum ObjectType { Player, AI }

public enum NPC_Type { Friendly = 0, Enemy = 1 }