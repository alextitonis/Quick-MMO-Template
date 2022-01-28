using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] List<Sc> screens = new List<Sc>();

    public static MenuManager getInstance;
    void Awake()
    {
        getInstance = this;
        ChangeScreen(Screen.Starting);
    }

    [HideInInspector] public Screen currentScreen;

    public void ChangeScreen(Screen screen)
    {
        if (currentScreen == screen) return;

        if (currentScreen == Screen.InGame)
            if (screen != Screen.InGame)
                HandleData.getInstance.ClearGame();

        currentScreen = screen;

        foreach (var sc in screens)
            sc.go.SetActive(false);

        foreach (var sc in screens)
        {
            if (sc.screen == screen)
            {
                sc.go.SetActive(true);
                break;
            }
        }

        if (screen == Screen.ServerSelection)
            ServerSelectionManager.getInstance.SetUp();
    }

    [System.Serializable]
    public class Sc
    {
        public Screen screen;
        public GameObject go;
    }
}