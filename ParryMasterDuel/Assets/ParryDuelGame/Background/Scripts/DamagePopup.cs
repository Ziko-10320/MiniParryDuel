using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    public static DamagePopup Instance { get; private set; }

    [Header("Prefab")]
    public GameObject popupPrefab;

    [Header("Player Canvases (World Space)")]
    public Canvas p1Canvas;
    public Canvas p2Canvas;

    [Header("Settings")]
    public float floatSpeed = 1.5f;
    public float fadeDuration = 0.3f;
    public float displayDuration = 0.5f;
    public float spawnRangeX = 0.5f;
    public float spawnRangeY = 0.3f;

    void Awake() => Instance = this;

    public void ShowP1(float amount) => Show(amount, p1Canvas);
    public void ShowP2(float amount) => Show(amount, p2Canvas);

    void Show(float amount, Canvas canvas)
    {
        if (popupPrefab == null || canvas == null) return;

        GameObject popup = Instantiate(popupPrefab, canvas.transform);
        RectTransform rt = popup.GetComponent<RectTransform>();

        // Spawn at random position within canvas
        rt.anchoredPosition = new Vector2(
            Random.Range(-spawnRangeX, spawnRangeX) * 100f,
            Random.Range(-spawnRangeY, spawnRangeY) * 100f
        );

        TextMeshProUGUI text = popup.GetComponent<TextMeshProUGUI>();
        text.text = "-" + Mathf.RoundToInt(amount);

        StartCoroutine(AnimatePopup(popup, rt, text));
    }

    IEnumerator AnimatePopup(GameObject popup, RectTransform rt, TextMeshProUGUI text)
    {
        Vector2 startPos = rt.anchoredPosition;
        Color c = text.color;

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            text.color = c;
            yield return null;
        }

        // Hold and float up
        t = 0f;
        c.a = 1f;
        text.color = c;
        while (t < displayDuration)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = startPos + Vector2.up * (floatSpeed * t * 100f);
            yield return null;
        }

        // Fade out
        t = 0f;
        Vector2 floatedPos = rt.anchoredPosition;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = 1f - Mathf.Clamp01(t / fadeDuration);
            text.color = c;
            rt.anchoredPosition = floatedPos + Vector2.up * (floatSpeed * t * 100f);
            yield return null;
        }

        Destroy(popup);
    }
}