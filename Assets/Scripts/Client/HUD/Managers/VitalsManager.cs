using UnityEngine;
using UnityEngine.UI;

public class VitalsManager : MonoBehaviour
{
    public static VitalsManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] Slider healthSlider, manaSlider;
    [SerializeField] Text maxHealthText, currentHealthText, maxManaText, currentManaText;

    public void SetMaxHealth(float value)
    {
        maxHealthText.text = value.ToString();
        healthSlider.maxValue = value;
    }
    public void SetCurrentHealth(float value)
    {
        currentHealthText.text = value.ToString();
        healthSlider.value = value;
    }
    public void SetMaxMana(float value)
    {
        maxManaText.text = value.ToString();
        manaSlider.maxValue = value;
    }
    public void SetCurrentMana(float value)
    {
        currentManaText.text = value.ToString();
        manaSlider.value = value;
    }
}