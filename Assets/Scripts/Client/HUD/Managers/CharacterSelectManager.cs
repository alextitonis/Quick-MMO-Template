using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    List<CharacterSettings> Characters = new List<CharacterSettings>();

    List<GameObject> Slots = new List<GameObject>();
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform spawnLocation;
    [SerializeField] Text logText;

    public Sprite imgExample;
    public CharacterSettings SelectedCharacter = new CharacterSettings { Name = "" };

    public static CharacterSelectManager getInstance;
    void Awake() { getInstance = this; }

    public void Enter(List<CharacterSettings> Characters)
    {
        logText.text = "";
        this.Characters = Characters;

        foreach (var go in Slots)
            Destroy(go);
        Slots.Clear();

        foreach (var c in Characters)
        {
            GameObject go = Instantiate(slotPrefab, spawnLocation);
            go.GetComponent<HUD_CharacterSlot>().SetUp(c);
            Slots.Add(go);
        }
    }

    public void Select(CharacterSettings settings)
    {
        foreach (var c in Slots)
            c.GetComponent<HUD_CharacterSlot>().OnDeselect();

        SelectedCharacter = settings;
    }

    public void DeleteCharacter()
    {
        if (SelectedCharacter.Name == "")
            return;

        Client.getInstance.DeleteCharacter(SelectedCharacter.ID);
    }

    public void Play()
    {
        if (SelectedCharacter.Name == "" || Characters.Count == 0)
        {
            logText.text = "You need to create a character first, to enter the game!";
            return;
        }

        Client.getInstance.EnterGame(SelectedCharacter.ID);
    }

    public void CreateCharacter()
    {
        if (Characters.Count > Client.maxCharacters)
        {
            logText.text = "You cant exceed the number of " + Client.maxCharacters + " of characters! First delete one!";
            return;
        }

        MenuManager.getInstance.ChangeScreen(Screen.CharacterCreate);
    }

    public void DeleteCharacterResponse(bool done, string response, string deletedId)
    {
        foreach (var c in Slots)
        {
            if (c.GetComponent<HUD_CharacterSlot>().character.ID == deletedId)
            {
                Destroy(c);
                Slots.Remove(c);
                return;
            }
        }

        Debug.Log("character not found!");
    }

    public void Back()
    {
        MenuManager.getInstance.ChangeScreen(Screen.Login);
        Client.getInstance.CloseConnection();
        Client.getInstance.Connect();
    }
}