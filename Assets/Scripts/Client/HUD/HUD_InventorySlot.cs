using UnityEngine;
using UnityEngine.UI;
using static ItemDatabase;

public class HUD_InventorySlot : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Text itemInfo;

    InventoryItem item;
    Player player;

    public void SetUp(InventoryItem item, Player player)
    {
        this.item = item;
        this.player = player;

        image.sprite = item.item.Sprite;
        itemInfo.text = item.item.Name + " x " + item.quantity;
    }

    public void Use()
    {
        if (item == null)
            return;

        if (player == null)
            return;

        if (!player.isLocal())
            return;

        Client.getInstance.UseItem(item.item);
    }

    public void UpdatePopUpUI(bool open)
    {
        InventoryManager.getInstance.UpdateDescription(item, player, open);
    }
}