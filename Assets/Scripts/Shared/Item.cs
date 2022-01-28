using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Custom/Item", order = 1)]
public class Item : ScriptableObject
{
    [Header("Core Information")]
    public int ID;
    public string Name;
    public string Description;
    public ItemType Type;
    public EquipmentLocation EquipmentLocation;

    [Header("Base Stats")]
    public ushort Strength; //STR
    public ushort Agility; //AGI
    public ushort Stamina; //STA
    public ushort Intelligence; //INT
    public ushort Wit; //WIT
    public ushort Mental; //MEN

    [Header("Stats")]
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

    public float MaxHealth;
    public float MaxMana;

    public float Range;

    [Header("Visuals")]
    public GameObject clientObject;
    public GameObject serverObject;
    public SkinnedMeshRenderer EquipmentMesh;
    public EquipmentMeshRegion[] CoveredRegions;
    public Sprite Sprite;

    public bool isUsable { get { return Type != ItemType.Currency && Type != ItemType.Material && Type != ItemType.None && Type != ItemType.Quest; } }
}