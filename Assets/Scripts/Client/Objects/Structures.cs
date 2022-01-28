public struct CharacterSettings
{
    public string Name;
    public Races Race;
    public Classes Class;
    public Genders Gender;
    public string ID;
    public BaseStats baseStats;
    public Vitals vitals;
    public Stats stats;
}

public struct Vitals
{
    public float MaxHealth;
    public float CurrentHealth;
    public float MaxMana;
    public float CurrentMana;
}

public struct BaseStats
{
    public ushort Strength; //STR
    public ushort Agility; //AGI
    public ushort Stamina; //STA
    public ushort Intelligence; //INT
    public ushort Wit; //WIT
    public ushort Mental; //MEN
}

public struct Stats
{
    public float PhysicalAttack;
    public float MagicalAttack;

    public float AttackSpeed;
    public float CastingSpeed;

    public float PhysicalArmor;
    public float MagicalArmor;

    public float PhysicalCritical;
    public float MagicalCritical;

    public float Cooldown;

    public float MovementSpeed;

    public float HitChance;
}