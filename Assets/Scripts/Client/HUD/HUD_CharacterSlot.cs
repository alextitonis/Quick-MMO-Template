using UnityEngine;
using UnityEngine.UI;

public class HUD_CharacterSlot : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Image panel;
    [SerializeField] Image img;

    public CharacterSettings character;

    public void SetUp(CharacterSettings character)
    {
        this.character = character;
        nameText.text = character.Name;
        img.sprite = CharacterSelectManager.getInstance.imgExample;
    }

    public void OnSelect()
    {
        CharacterSelectManager.getInstance.Select(character);
        panel.color = Color.yellow;
    }

    public void OnDeselect()
    {
        panel.color = Color.white;
    }
}