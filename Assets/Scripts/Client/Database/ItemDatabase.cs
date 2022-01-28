using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase getInstance;
    void Awake() { getInstance = this; }

    [System.Serializable]
    public class InventoryItem
    {
        public Item item;
        public int quantity;
        public int slot;
    }

    [SerializeField] Item[] items;

    public Item getItem(int ID)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].ID == ID)
                return items[i];
        }

        return null;
    }
}