using UnityEngine;
using UnityEngine.UI;

public class HUD_GameServer : MonoBehaviour
{
    public _GameServer gameServer;

    [SerializeField] Text gsNameText;
    [SerializeField] Color normal, clicked;
    public Image img;

    public void SetUp(_GameServer gs, string ping)
    {
        normal = img.color;
        gameServer = gs;
        gsNameText.text = gs.Name + " | Ping: " + ping;
    }

    public void Select()
    {
        ServerSelectionManager.getInstance.OnSelectGameServer(gameServer);
    }

    public void UpdateImg(bool reset)
    {
        if (!reset)
            img.color = clicked;
        else
            img.color = clicked;
    }
}