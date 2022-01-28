using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [SerializeField] InputField EmailInput, PasswordInput;
    [SerializeField] Text Log_Text;

    public static LoginManager getInstance;
    void Awake()
    {
        CleanUp();
        getInstance = this;
    }

    public void Login()
    {
        string email = EmailInput.text;

        if (!email.Contains("@") || !email.Contains(".") || string.IsNullOrEmpty(email))
        {
            Log_Text.text = "Invalid Email!";
            CleanUp();
            return;
        }

        string password = PasswordInput.text;

        if (string.IsNullOrEmpty(password))
        {
            Log_Text.text = "Invalid Password!";
            CleanUp();
            return;
        }

        CleanUp();
        Client.getInstance.Login(email, password);
    }
    public void Register()
    {
        CleanUp();
        MenuManager.getInstance.ChangeScreen(Screen.Register);
    }
    public void ForgotPassword()
    {
        CleanUp();
        MenuManager.getInstance.ChangeScreen(Screen.ForgotPassword);
    }

    public void CleanUp()
    {
        EmailInput.text = PasswordInput.text = Log_Text.text = string.Empty;
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
        Log_Text.tag = string.Empty;
    }

    public string email = "";
    public string password = "";
}