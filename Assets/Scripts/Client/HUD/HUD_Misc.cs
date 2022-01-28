using UnityEngine;
using UnityEngine.SceneManagement;

public class HUD_Misc : MonoBehaviour
{
    public void OpenURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
            Application.OpenURL(url);
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    public void Exit()
    {
        Application.Quit();
    }
}