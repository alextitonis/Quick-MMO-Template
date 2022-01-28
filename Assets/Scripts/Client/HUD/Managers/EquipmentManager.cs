using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemDatabase;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] HUD_EquipmentSlot left_hand, right_hand, head, chest, legs, feet, gloves;
    [SerializeField] GameObject obj;

    [HideInInspector] public bool isActive;

    public void UpdateUI(Player player)
    {
        isActive = !isActive;
        obj.SetActive(isActive);

        if (isActive)
            SetUp(player.getEquipment(), player);
    }

    public void SetUp(Dictionary<EquipmentLocation, Item> equipment, Player player)
    {
        foreach (var i in equipment)
        {
            switch (i.Key)
            {
                case EquipmentLocation.Chest:
                    chest.SetUp(i.Key, i.Value, player);
                    break;
                case EquipmentLocation.Feet:
                    feet.SetUp(i.Key, i.Value, player);
                    break;
                case EquipmentLocation.Gloves:
                    gloves.SetUp(i.Key, i.Value, player);
                    break;
                case EquipmentLocation.Head:
                    head.SetUp(i.Key, i.Value, player);
                    break;
                case EquipmentLocation.Left_Hand:
                    left_hand.SetUp(i.Key, i.Value, player);
                    break;
                case EquipmentLocation.Legs:
                    legs.SetUp(i.Key, i.Value, player);
                    break;
                case EquipmentLocation.Right_Hand:
                    right_hand.SetUp(i.Key, i.Value, player);
                    break;
            }
        }
    }
    void Clear()
    {
        chest.Clear();
        feet.Clear();
        gloves.Clear();
        head.Clear();
        left_hand.Clear();
        legs.Clear();
        right_hand.Clear();
    }
}