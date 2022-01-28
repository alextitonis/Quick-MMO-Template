using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Fade : MonoBehaviour
{
    [SerializeField] Text text;

    public IEnumerator FadeIn(float t = 1f)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        while (text.color.a < 1.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime / t));
            yield return null;
        }

        StartCoroutine(FadeOut());
    }
    public IEnumerator FadeOut(float t = 1f)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        while (text.color.a < 1.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }
}