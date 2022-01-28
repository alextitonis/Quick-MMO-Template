using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class RegistrationManager : MonoBehaviour
{
    [SerializeField] InputField EmailInput, PasswordInput;
    [SerializeField] Text Log_Text;

    public static RegistrationManager getInstance;
    void Awake()
    {
        CleanUp();
        getInstance = this;
    }

    public void Register()
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
        Client.getInstance.Register(email, password);
    }
    public void Back()
    {
        CleanUp();
        MenuManager.getInstance.ChangeScreen(Screen.Login);
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
        Log_Text.text = string.Empty;
    }

    public void GenerateQRCode()
    {
        if (string.IsNullOrEmpty(PasswordInput.text) || string.IsNullOrEmpty(EmailInput.text))
            return;

        Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
        System.Drawing.Image img = qrcode.Draw(PasswordInput.text, 50);
        img.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + EmailInput.text + ".png");
    }

    bool passInputHiden = true;
    public void UpdatePasswordCharacters()
    {
        if (passInputHiden) PasswordInput.inputType = InputField.InputType.Standard;
        else PasswordInput.inputType = InputField.InputType.Password;

        passInputHiden = !passInputHiden;
    }

    public void GeneratePassword()
    {
        PasswordInput.text = Utils.getInstance.GetRandomString(10);

        if (passInputHiden)
            UpdatePasswordCharacters();
    }
}