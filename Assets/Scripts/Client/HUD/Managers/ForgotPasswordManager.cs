using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ForgotPasswordManager : MonoBehaviour
{
    [SerializeField] Text Log_Text;
    [SerializeField] InputField EmailInput;

    public static ForgotPasswordManager getInstance;
    void Awake() { getInstance = this; }

    public void ForgotPassword()
    {
        string email = EmailInput.text;

        CleanUp();

        if (!email.Contains("@") || !email.Contains(".") || string.IsNullOrEmpty(email))
        {
            Log("Invalid email!");
            return;
        }

        Client.getInstance.ForgotPassword(email);
    }
    public void Back()
    {
        CleanUp();
        MenuManager.getInstance.ChangeScreen(Screen.Login);
    }

    public void CleanUp()
    {
        Log_Text.text = EmailInput.text = string.Empty;
    }
    public void Log(string txt)
    {
        StopCoroutine(ClearLog());
        Log_Text.text = txt;
        StartCoroutine(ClearLog());
    }
    IEnumerator ClearLog()
    {
        yield return new WaitForSeconds(4f);
        Log_Text.text = string.Empty;
    }
}