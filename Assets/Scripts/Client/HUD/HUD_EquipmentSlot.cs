using UnityEngine;
using UnityEngine.UI;
using static ItemDatabase;

public class HUD_EquipmentSlot : MonoBehaviour
{
    [SerializeField] EquipmentLocation slot;
    [SerializeField] Image image;

    Player player;
    Item item;

    public void SetUp(EquipmentLocation slot, Item item, Player player)
    {
        if (this.slot != slot)
            return;

        this.item = item;
        this.player = player;

        if (item != null)
        {
            image.sprite = item.Sprite;
        }
    }
    public void Clear()
    {
        image.sprite = null;
        item = null;
        player = null;
    }

    public void Use()
    {
        if (item == null)
            return;

        if (player == null)
            return;

        if (!player.isLocal())
            return;

        if (slot == EquipmentLocation.None)
            return;

        Client.getInstance.UnEquipItem(slot);
    }
}