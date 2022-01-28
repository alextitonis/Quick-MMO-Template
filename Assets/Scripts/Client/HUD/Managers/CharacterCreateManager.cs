using UnityEngine;
using UnityEngine.UI;

public class CharacterCreateManager : MonoBehaviour
{
    [SerializeField] Text RaceText, ClassText, GenderText;
    [SerializeField] Slider RaceSlider, ClassSlider, GenderSlider;
    [SerializeField] InputField NameInput;
    [SerializeField] Text logText;

    public static CharacterCreateManager getInstance;
    void Awake() { getInstance = this; }

    void OnEnable()
    {
        RaceText.text = ((Races)RaceSlider.value).ToString();
        ClassText.text = ((Classes)ClassSlider.value).ToString();
        GenderText.text = ((Genders)GenderSlider.value).ToString();
    }

    public void OnRaceChanged()
    {
        RaceText.text = ((Races)RaceSlider.value).ToString();
    }
    public void OnClassChanged()
    {
        ClassText.text = ((Classes)ClassSlider.value).ToString();
    }
    public void OnGenderChanged()
    {
        GenderText.text = ((Genders)GenderSlider.value).ToString();
    }

    public void Create()
    {
        string Name = NameInput.text;

        if (string.IsNullOrEmpty(Name))
            return;

        CharacterSettings c = new CharacterSettings();
        c.Name = Name;
        c.Race = (Races)RaceSlider.value;
        c.Class = (Classes)ClassSlider.value;
        c.Gender = (Genders)GenderSlider.value;

        Client.getInstance.CreateCharacter(c);
    }
    public void Back()
    {
        MenuManager.getInstance.ChangeScreen(Screen.CharacterSelect);
        Client.getInstance.RequestCharacters();
        logText.text = "";
    }

    public void Response(bool done, string response)
    {
        Debug.Log(response + ": " + done);

        if (!done)
            logText.text = response;
        else
            Back();
    }
}