using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemDatabase;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] GameObject obj;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform spawnLocation;
    [SerializeField] GameObject descriptionPopUp;
    [SerializeField] Text description;

    [HideInInspector] public bool isActive = false;

    List<HUD_InventorySlot> slots = new List<HUD_InventorySlot>();

    public void UpdateUI(Player player)
    {
        if (isActive)
            Clear();

        descriptionPopUp.SetActive(false);

        isActive = !isActive;
        obj.SetActive(isActive);

        if (isActive)
            SetUp(player.getInventory(), player);
    }

    public void SetUp(List<InventoryItem> inventory, Player player)
    {
        Clear();

        foreach (var i in inventory)
        {
            GameObject go = Instantiate(slotPrefab, spawnLocation);
            HUD_InventorySlot slot = go.GetComponent<HUD_InventorySlot>();
            if (slot == null)
            {
                Destroy(go);
                continue;
            }

            slot.SetUp(i, player);
            slots.Add(slot);
        }
    }
    void Clear()
    {
        foreach (var i in slots)
            Destroy(i.gameObject);

        slots.Clear();
    }

    public void UpdateDescription(InventoryItem item, Player player, bool open)
    {
        if (item == null)
            return;

        if (player == null)
            return;

        if (!player.isLocal())
            return;

        descriptionPopUp.SetActive(open);

        if (open) ;
            description.text = item.item.Description;
    }
}