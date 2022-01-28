using UnityEngine;
using UnityEngine.UI;

public class SelectedObjectManager : MonoBehaviour
{
    public static SelectedObjectManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] GameObject obj;
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Slider healthSlider;

    bool isOpen = false;
    bool isPlayer;
    Player player;
    WorldNPC npc;

    public void OpenAsPlayer(Player player)
    {
        print(player.name);
        obj.SetActive(true);
        this.player = player;
        isOpen = true;
        isPlayer = true;
        nameText.text = player.CharName;
        levelText.text = player.level.ToString();
        healthSlider.maxValue = player.vitals.MaxHealth;
        healthSlider.value = player.vitals.CurrentHealth;
    }
    public void OpenAsNPC(WorldNPC npc)
    {
        print(npc.name);
        obj.SetActive(true);
        this.npc = npc;
        isPlayer = false;
        isOpen = true;
        nameText.text = npc.data.Name;
        levelText.text = npc.level.ToString();
        healthSlider.maxValue = npc.data.vitals.MaxHealth;
        healthSlider.value = npc.data.vitals.CurrentHealth;
    }
    public void Close()
    {
        npc = null;
        player = null;
        isOpen = false;
        obj.SetActive(false);
    }

    void Update()
    {
        if (isOpen)
        {
            if (isPlayer)
            {
                nameText.text = player.CharName;
                levelText.text = player.level.ToString();
                healthSlider.maxValue = player.vitals.MaxHealth;
                healthSlider.value = player.vitals.CurrentHealth;
            }
            else
            {
                nameText.text = npc.data.Name;
                levelText.text = npc.level.ToString();
                healthSlider.maxValue = npc.data.vitals.MaxHealth;
                healthSlider.value = npc.data.vitals.CurrentHealth;
            }
        }
    }
}