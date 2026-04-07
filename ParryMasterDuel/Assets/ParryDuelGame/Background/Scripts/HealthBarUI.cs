using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider mainSlider;       // follows health instantly
    public Slider delayedSlider;    // the white ghost bar that lags behind

    [Header("Delayed Bar Settings")]
    public float delayTime = 0.5f;  // seconds before delayed bar starts moving
    public float lerpSpeed = 3f;    // how fast the delayed bar catches up

    [Header("Character Info")]
    public Image characterPortrait;
    public TextMeshProUGUI characterNameText;

    // ?? Private ???????????????????????????????????????????????
    private float maxHealth;
    private float currentHealth;
    private float delayedHealth;
    private float delayTimer;

    // The actual health script — we'll poll it every frame
    private MonoBehaviour healthScript;

    // ?? Called by GameManager after spawning ??????????????????
    public void SetCharacter(GameObject player, Sprite portrait, string charName)
    {
        Debug.Log("SetCharacter called. Player is: " + (player == null ? "NULL" : player.name));
        if (player == null) return;
        // Find whichever health script is on this character
        healthScript = GetHealthScript(player);

        if (healthScript == null)
        {
            Debug.LogError("HealthBarUI: No health script found on " + player.name);
            return;
        }

        // Read max health dynamically
        maxHealth = GetMaxHealth(healthScript);
        currentHealth = maxHealth;
        delayedHealth = maxHealth;

        // Set slider ranges
        mainSlider.minValue = 0;
        mainSlider.maxValue = maxHealth;
        mainSlider.value = maxHealth;

        delayedSlider.minValue = 0;
        delayedSlider.maxValue = maxHealth;
        delayedSlider.value = maxHealth;

        // Set portrait and name
        if (characterPortrait != null && portrait != null)
            characterPortrait.sprite = portrait;
        if (characterNameText != null)
            characterNameText.text = charName;
    }

    void Update()
    {
        if (healthScript == null) return;

        // Poll current health from whichever script it is
        float newHealth = GetCurrentHealth(healthScript);

        if (newHealth != currentHealth)
        {
            currentHealth = newHealth;
            mainSlider.value = currentHealth;
            delayTimer = delayTime; // reset delay countdown
        }

        // Delayed bar logic
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        else
        {
            delayedHealth = Mathf.Lerp(delayedHealth, currentHealth, lerpSpeed * Time.deltaTime);
            delayedSlider.value = delayedHealth;
        }
        if (currentHealth <= 0f)
        {
            CanvasGroup mainCG = mainSlider.GetComponent<CanvasGroup>();
            CanvasGroup delayedCG = delayedSlider.GetComponent<CanvasGroup>();
            if (mainCG != null)
                mainCG.alpha = Mathf.Lerp(mainCG.alpha, 0f, Time.deltaTime * 3f);
            if (delayedCG != null)
                delayedCG.alpha = Mathf.Lerp(delayedCG.alpha, 0f, Time.deltaTime * 3f);
        }
    }

    // ?? Helpers ???????????????????????????????????????????????
    MonoBehaviour GetHealthScript(GameObject go)
    {
        MonoBehaviour m;
        m = go.GetComponentInChildren<KnightHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<VikingHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<NinjaHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<SoldierHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<CaveManHealth>(); if (m != null) return m;
        Debug.Log("Searching for health script on: " + go.name + " children count: " + go.GetComponentsInChildren<MonoBehaviour>().Length);
        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>())
            Debug.Log("  Found component: " + mb.GetType().Name);
        return null;
    }

    float GetMaxHealth(MonoBehaviour m)
    {
        if (m is KnightHealth k) return k.maxHealth;
        if (m is VikingHealth v) return v.maxHealth;
        if (m is NinjaHealth n) return n.maxHealth;
        if (m is SoldierHealth s) return s.maxHealth;
        if (m is CaveManHealth c) return c.maxHealth;
        return 100f;
    }

    float GetCurrentHealth(MonoBehaviour m)
    {
        if (m is KnightHealth k) return k.CurrentHealth;
        if (m is VikingHealth v) return v.CurrentHealth;
        if (m is NinjaHealth n) return n.CurrentHealth;
        if (m is SoldierHealth s) return s.CurrentHealth;
        if (m is CaveManHealth c) return c.CurrentHealth;
        return 0f;
    }
}