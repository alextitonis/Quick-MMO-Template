using System.Runtime.Remoting.Messaging;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Custom/Spell", order = 1)]
public class Spell : ScriptableObject
{
    [Header("Core Information")]
    public int ID;
    public string Name;
    public string Description;
    public Classes[] Classes;
    public ushort LevelNeeded;
    [Header("Stats")]
    public float CastDuration;
    public float Cooldown;
    public SpellCostType CostType;
    public float Cost;
    public SpellOperateType OperateType;
    public SpellType Type;
    public int Value;
    public SpellEffect Effect;
    public int EffectValue;
    public SpellTarget Target;
    public string Data;
    public ushort ActivationChance;
    [Header("Visuals")]
    public GameObject obj;
    public Sprite sprite;
}