using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "Custom/NPC", order = 1)]
public class NPC : ScriptableObject
{
    [Header("Core Information")]
    public int ID;
    public string Name;
    [Header("Stats")]
    public Vitals vitals;
    public BaseStats baseStats;
    public Stats stats;
    public int level;
    public NPC_Type type;
    [Header("Visuals")]
    public GameObject clientPrefab;
    public GameObject serverPrefab;
}