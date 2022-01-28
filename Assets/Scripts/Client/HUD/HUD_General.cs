using UnityEngine;

public class HUD_General : MonoBehaviour
{
    public void GoTo(Screen screen)
    {
        MenuManager.getInstance.ChangeScreen(screen);
    }
}