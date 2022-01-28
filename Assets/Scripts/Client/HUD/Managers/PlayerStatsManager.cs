using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] GameObject obj;

    [SerializeField] Text playerNameText;
    [SerializeField] Text str, agi, sta, _int, wit, men;

    [SerializeField] Text PhysicalAttack;
    [SerializeField] Text MagicalAttack;
    [SerializeField] Text AttackSpeed;
    [SerializeField] Text CastingSpeed;
    [SerializeField] Text PhysicalArmor;
    [SerializeField] Text MagicalArmor;
    [SerializeField] Text PhysicalCritical;
    [SerializeField] Text MagicalCritical;
    [SerializeField] Text Cooldown;
    [SerializeField] Text MovementSpeed;
    [SerializeField] Text HitChance;

    public void UdpateUI(Player player)
    {
        obj.SetActive(!obj.activeSelf);

        if (player == null)
        {
            obj.SetActive(false);
            return;
        }

        if (obj.activeSelf)
        {
            playerNameText.text = "Stats - " + player.CharName;

            str.text = player.baseStats.Strength.ToString();
            agi.text = player.baseStats.Agility.ToString();
            sta.text = player.baseStats.Stamina.ToString();
            _int.text = player.baseStats.Intelligence.ToString();
            wit.text = player.baseStats.Wit.ToString();
            men.text = player.baseStats.Mental.ToString();

            PhysicalAttack.text = player.stats.PhysicalAttack.ToString();
            MagicalAttack.text = player.stats.MagicalAttack.ToString();
            AttackSpeed.text = player.stats.AttackSpeed.ToString();
            CastingSpeed.text = player.stats.CastingSpeed.ToString();
            PhysicalArmor.text = player.stats.PhysicalAttack.ToString();
            MagicalArmor.text = player.stats.MagicalArmor.ToString();
            PhysicalCritical.text = player.stats.PhysicalCritical.ToString() + "%";
            MagicalCritical.text = player.stats.MagicalCritical.ToString() + "%";
            Cooldown.text = player.stats.Cooldown.ToString() + "%";
            MovementSpeed.text = player.stats.MovementSpeed.ToString();
            HitChance.text = player.stats.HitChance.ToString() + "%";
        }
    }
}