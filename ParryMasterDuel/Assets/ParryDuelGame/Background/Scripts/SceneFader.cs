using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    private Image img;

    void Awake() => img = GetComponent<Image>();

    void Start() => StartCoroutine(FadeIn());

    public IEnumerator FadeIn()
    {
        float t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            SetAlpha(t / fadeDuration);
            yield return null;
        }
        SetAlpha(0f);
    }

    public IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(t / fadeDuration);
            yield return null;
        }
        SetAlpha(1f);
    }

    void SetAlpha(float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}