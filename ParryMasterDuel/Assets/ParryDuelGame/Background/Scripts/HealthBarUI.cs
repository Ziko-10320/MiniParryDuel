using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Health Sliders")]
    public Slider mainSlider;
    public Slider delayedSlider;

    [Header("Posture Slider")]
    public Slider postureSlider; // The new posture slider
    public Slider postureDelayedSlider;
    private float delayedPosture;
    private float postureDelayTimer;
    [Header("Delayed Bar Settings")]
    public float delayTime = 0.5f;
    public float lerpSpeed = 3f;
    private float lastKnownPosture = -1f;
    [Header("Character Info")]
    public Image characterPortrait;
    public TextMeshProUGUI characterNameText;

    // Private variables
    private float maxHealth;
    private float currentHealth;
    private float delayedHealth;
    private float delayTimer;

    // We'll poll these scripts every frame
    private MonoBehaviour healthScript;
    private MonoBehaviour movementScript; // To hold the movement script

    // Called by GameManager after spawning
    public void SetCharacter(GameObject player, Sprite portrait, string charName)
    {
        if (player == null) return;

        // Find whichever health and movement scripts are on this character
        healthScript = GetHealthScript(player);
        movementScript = GetMovementScript(player);

        if (healthScript == null)
        {
            // This is the error you are seeing. It will be fixed now.
            Debug.LogError("HealthBarUI: No health script found on " + player.name, player);
            return;
        }
        if (movementScript == null)
        {
            Debug.LogError("HealthBarUI: No movement script found on " + player.name, player);
            return;
        }

        // Read max health and posture dynamically
        maxHealth = GetMaxHealth(healthScript);
        float maxPosture = GetMaxPosture(movementScript);

        currentHealth = maxHealth;
        delayedHealth = maxHealth;

        // Set health slider ranges
        mainSlider.minValue = 0;
        mainSlider.maxValue = maxHealth;
        mainSlider.value = maxHealth;
        delayedSlider.minValue = 0;
        delayedSlider.maxValue = maxHealth;
        delayedSlider.value = maxHealth;

        // Set posture slider range
        if (postureSlider != null)
        {
            postureSlider.minValue = 0;
            postureSlider.maxValue = maxPosture;
            postureSlider.value = maxPosture;
        }
        delayedPosture = maxPosture;
        if (postureDelayedSlider != null)
        {
            postureDelayedSlider.minValue = 0;
            postureDelayedSlider.maxValue = maxPosture;
            postureDelayedSlider.value = maxPosture;
        }
        // Set portrait and name
        if (characterPortrait != null && portrait != null)
            characterPortrait.sprite = portrait;
        if (characterNameText != null)
            characterNameText.text = charName;
        lastKnownPosture = maxPosture;
    }

    void Update()
    {
        // --- Health Logic ---
        if (healthScript != null)
        {
            float newHealth = GetCurrentHealth(healthScript);
            if (newHealth != currentHealth)
            {
                currentHealth = newHealth;
                mainSlider.value = currentHealth;
                delayTimer = delayTime;
            }
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
            }
            else
            {
                delayedHealth = Mathf.Lerp(delayedHealth, currentHealth, lerpSpeed * Time.deltaTime);
                delayedSlider.value = delayedHealth;
            }
        }

        // --- Posture Logic ---
        if (movementScript != null && postureSlider != null)
        {
            float newPosture = GetCurrentPosture(movementScript);
            postureSlider.value = newPosture;

            if (newPosture < lastKnownPosture)
            {
                // posture dropped — trigger delay
                postureDelayTimer = delayTime;
            }
            else if (newPosture > lastKnownPosture)
            {
                // posture going up (regen or stun recovery) — delayed bar follows instantly
                delayedPosture = newPosture;
                if (postureDelayedSlider != null)
                    postureDelayedSlider.value = delayedPosture;
            }

            lastKnownPosture = newPosture;

            if (postureDelayTimer > 0f)
                postureDelayTimer -= Time.deltaTime;
            else
            {
                delayedPosture = Mathf.Lerp(delayedPosture, newPosture, lerpSpeed * Time.deltaTime);
                if (postureDelayedSlider != null)
                    postureDelayedSlider.value = delayedPosture;
            }
        }
    }

    // =================================================================
    // CORRECT HELPER FUNCTIONS
    // =================================================================

    // --- Health Helpers (Your original, working code) ---
    MonoBehaviour GetHealthScript(GameObject go)
    {
        MonoBehaviour m;
        m = go.GetComponentInChildren<KnightHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<VikingHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<NinjaHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<SoldierHealth>(); if (m != null) return m;
        m = go.GetComponentInChildren<CaveManHealth>(); if (m != null) return m;
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

    // --- Posture Helpers (The new functions) ---
    MonoBehaviour GetMovementScript(GameObject go)
    {
        MonoBehaviour m;
        m = go.GetComponentInChildren<KnightMovement>(); if (m != null) return m;
        m = go.GetComponentInChildren<VikingMovement>(); if (m != null) return m;
        m = go.GetComponentInChildren<NinjaMovement>(); if (m != null) return m;
        m = go.GetComponentInChildren<SoldierMovement>(); if (m != null) return m;
        m = go.GetComponentInChildren<CaveManMovement>(); if (m != null) return m;
        return null;
    }

    float GetMaxPosture(MonoBehaviour m)
    {
        if (m is KnightMovement k) return k.maxPosture;
        if (m is VikingMovement v) return v.maxPosture;
        if (m is NinjaMovement n) return n.maxPosture;
        if (m is SoldierMovement s) return s.maxPosture;
        if (m is CaveManMovement c) return c.maxPosture;
        return 100f;
    }

    float GetCurrentPosture(MonoBehaviour m)
    {
        if (m is KnightMovement k) return k.CurrentPosture;
        if (m is VikingMovement v) return v.CurrentPosture;
        if (m is NinjaMovement n) return n.CurrentPosture;
        if (m is SoldierMovement s) return s.CurrentPosture;
        if (m is CaveManMovement c) return c.CurrentPosture;
        return 0f;
    }
}
