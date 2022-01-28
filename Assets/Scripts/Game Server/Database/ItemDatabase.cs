using UnityEngine;
using System.Collections.Generic;

namespace GameServer
{
    public class ItemDatabase : MonoBehaviour
    {
        public static ItemDatabase getInstance;
        void Awake() { getInstance = this; }

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

        public List<Item> getItems()
        {
            List<Item> res = new List<Item>();
            for (int i = 0; i < items.Length; i++)
                res.Add(items[i]);

            return res;
        }
    }

    public class InventoryItem
    {
        public int slot;
        public Item item;
        public int Quantity;
    }
}