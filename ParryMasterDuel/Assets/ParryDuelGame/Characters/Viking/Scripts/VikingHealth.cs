using UnityEngine;
using FirstGearGames.SmoothCameraShaker;
public class VikingHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] hitSounds;
    public AudioClip[] blockSounds;
    public AudioClip[] parrySounds;
    [Header("VFX")]
    public GameObject bloodPrefab;
    public Transform bloodSpawnPoint;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isKnockedBack;
    private float knockbackTimer;
    public ShakeData CameraShake;
    public ShakeData CameraShakeParry;
    public bool IsKnockedBack => isKnockedBack;
    private VikingMovement movement;

    private bool isFinishable;
    public bool IsFinishable => isFinishable;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        movement = GetComponent<VikingMovement>();
    }

    void Update()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
                isKnockedBack = false;
        }
    }
    void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }
    public void TakeDamage(float amount, Vector2 attackerPosition)
    {
        if (isFinishable) return;
        if (movement.IsInParryWindow())
        {
            // Successful parry — spawn parry VFX
            if (movement.parryVFXPrefab != null && movement.parryVFXSpawnPoint != null)
                Instantiate(movement.parryVFXPrefab, movement.parryVFXSpawnPoint.position, movement.parryVFXPrefab.transform.rotation);

            // Play parry animation
            CameraShakerHandler.Shake(CameraShakeParry);
            animator.SetTrigger("Parry");
            PlayRandom(parrySounds);
            KnightMovement attacker = FindAttacker<KnightMovement>(attackerPosition);
            if (attacker != null)
            {
                attacker.TriggerParryStun(transform.position);
                attacker.ReceiveParryPostureDamage(movement.parryPostureDamage);
            }
            NinjaMovement attackerninja = FindAttacker<NinjaMovement>(attackerPosition);
            if (attackerninja != null)
            {
                attackerninja.TriggerParryStun(transform.position);
                attackerninja.ReceiveParryPostureDamage(movement.parryPostureDamage);
            }
            VikingMovement attackerViking2 = FindAttacker<VikingMovement>(attackerPosition);
            if (attackerViking2 != null && attackerViking2 != movement)
            {
                attackerViking2.TriggerParryStun(transform.position);
                attackerViking2.ReceiveParryPostureDamage(movement.parryPostureDamage);
            }
            SoldierMovement attackersoldier = FindAttacker<SoldierMovement>(attackerPosition);
            if (attackersoldier != null)
            {
                attackersoldier.TriggerParryStun(transform.position);
                attackersoldier.ReceiveParryPostureDamage(movement.parryPostureDamage);
            }
            CaveManMovement attackerCM = FindAttacker<CaveManMovement>(attackerPosition);
            if (attackerCM != null)
            {
                attackerCM.TriggerParryStun(transform.position);
                attackerCM.ReceiveParryPostureDamage(movement.parryPostureDamage);
            }
            movement.parryInstantBlock = true;
            return; // parry absorbs everything, no damage, no posture damage
        }
        if (movement.IsBlocking())
        {
            Vector2 knockbackDire = ((Vector2)transform.position - attackerPosition).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDire * knockbackForce, ForceMode2D.Impulse);
            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
            if (bloodPrefab != null && bloodSpawnPoint != null)
                Instantiate(bloodPrefab, bloodSpawnPoint.position, Quaternion.identity);
            movement.AbsorbBlockedHit(amount, attackerPosition);
            PlayRandom(blockSounds);
            float reducedDamage = amount * (1f - movement.blockDamageReduction);
            currentHealth -= reducedDamage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            Debug.Log($"Knight blocked! Took reduced damage: {reducedDamage}. HP left: {currentHealth}");
            if (currentHealth <= 0f) Die();
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        CameraShakerHandler.Shake(CameraShake);
        animator.SetTrigger("TakeDamage");
        PlayRandom(hitSounds);
        if (bloodPrefab != null && bloodSpawnPoint != null)
            Instantiate(bloodPrefab, bloodSpawnPoint.position, Quaternion.identity);
        Debug.Log($"Viking took {amount} damage. HP left: {currentHealth}");
        if (currentHealth <= 0f)
            Die();
    }
    T FindAttacker<T>(Vector2 position) where T : Component
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, 1f);
        foreach (var h in hits)
        {
            T comp = h.GetComponent<T>();
            if (comp != null) return comp;
        }
        return null;
    }
    void Die()
    {
        if (isFinishable) return;
        isFinishable = true;
        movement.EnterFinishableState();
      

    }
    public void CheckFinishable()
    {
        if (!isFinishable && currentHealth <= 0f)
            Die();
    }
}