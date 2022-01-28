using UnityEngine;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] Text zoneNameText;

    public void NewZone(string newZone)
    {
        zoneNameText.text = newZone;
        zoneNameText.GetComponent<HUD_Fade>().FadeIn();
    }
}