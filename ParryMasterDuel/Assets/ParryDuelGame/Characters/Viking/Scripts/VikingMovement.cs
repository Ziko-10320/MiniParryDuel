using UnityEngine;
using UnityEngine.InputSystem;
using FirstGearGames.SmoothCameraShaker;
public class VikingMovement : MonoBehaviour, IFinishable
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip runLoopClip;
    public AudioClip jumpClip;
    public AudioClip[] rollSounds;
    public AudioClip[] attackSounds;
    [Header("Jump")]
    public bool canJump = false;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private float postureRegenDelay = 0f;
    public float postureRegenWaitTime = 2f;
    public float CurrentPosture => currentPosture;
    private bool isRunning;
    [Header("Roll")]
    public float rollSpeed = 12f;
    public float rollDuration = 0.4f;
    public float doubleTapWindow = 0.3f;

    private bool isRolling;
    private float rollTimer;
    private float lastTapTime;
    private float lastTapDirection;

    public float rollCooldown = 1f;
    private float rollCooldownTimer;

    [Header("Player Assignment")]
    public bool isPlayer1 = false;
    public bool isPlayer2 = false;
    // ── Private ──────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator animator;

    private float moveInput;
    private bool isGrounded;
 
    [Header("Attack")]
    public Transform attackPoint;
    public Vector2 attackBox = new Vector2(1f, 1f);
    public float attackDamage = 10f;
    public LayerMask enemyLayer;

    private bool isAttacking;

    public ShakeData CameraShake ;
    public ShakeData CameraShakeheavy;
    [Header("Block")]
    public float blockDamageReduction = 0.5f;
    public float maxPosture = 100f;
    public float postureRegen = 10f;
    public float stunDuration = 2f;
    public GameObject blockSparksPrefab;
    public Transform blockSparksSpawnPoint;

    private float currentPosture;
    private bool isBlocking;
    private bool isStunned;
    private float stunTimer;
    private VikingHealth vikingHealth;

    public float blockCooldown = 1f;
    private float blockCooldownTimer;
    public float parryWindow = 0.2f;
    public float parryStunDuration = 1.5f;
    public float parryPostureDamage = 30f;
    public GameObject parryVFXPrefab;
    public Transform parryVFXSpawnPoint;

    public float knockbackForce = 2f;
    private bool isInParryWindow;
    private float parryWindowTimer;
    public bool IsInParryWindow() => isInParryWindow && !isStunned;

    [Header("Lunge")]
    public float lungeForce = 8f;
    public float lungeDuration = 0.2f;
    public bool parryInstantBlock;
    private GhostTrail ghostTrail;

    private bool isFinishable;
    public bool IsFinishable() => isFinishable;

    [Header("Death")]
    public GameObject headPrefab;
    public Transform headSpawnPoint;
    public float headUpwardForce = 8f;
    public float headHorizontalForce = 4f;
    public float headTorque = 200f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentPosture = maxPosture;
        vikingHealth = GetComponent<VikingHealth>();
        ghostTrail = GetComponent<GhostTrail>();
    }
    public void PlayRandomAttackSound()
    {
        if (attackSounds == null || attackSounds.Length == 0) return;
        AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }
    void Update()
    {
        if (isFinishable) return;
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                if (ghostTrail != null) ghostTrail.StopTrail();
                isRolling = false;
            }
            return;
        }
        if (rollCooldownTimer > 0f)
            rollCooldownTimer -= Time.deltaTime;
        // Stun timer
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            currentPosture = Mathf.Lerp(0f, maxPosture, 1f - (stunTimer / stunDuration));
            if (stunTimer <= 0f)
            {
                isStunned = false;
                currentPosture = maxPosture;
                animator.SetBool("isStunned", false);
            }
            return;
        }

        // Posture regen when not blocking
        if (postureRegenDelay > 0f)
            postureRegenDelay -= Time.deltaTime;
        else if (!isBlocking && !isStunned && currentPosture < maxPosture)
            currentPosture += postureRegen * Time.deltaTime;
        if (isInParryWindow)
        {
            parryWindowTimer -= Time.deltaTime;
            if (parryWindowTimer <= 0f)
                isInParryWindow = false;
        }
        if (blockCooldownTimer > 0f)
            blockCooldownTimer -= Time.deltaTime;
        HandleMovement();
        HandleAnimation();
    }

    public void PerformLunge()
    {
        float lungeDirection = transform.localScale.x;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(lungeDirection * lungeForce, 0f), ForceMode2D.Impulse);
    }


    // ── Movement ──────────────────────────────────────────────
    void HandleMovement()
    {
        if (isStunned) return;
        if (isAttacking) return;
        if (vikingHealth.IsKnockedBack) return;
        if (isBlocking) return;
        if (GetComponent<VikingHealth>().IsKnockedBack) return;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
     
    }
    public void SetMoveInput(float value)
    {
        if (value != 0f && value != moveInput)
        {
            float currentDirection = Mathf.Sign(value);

            if (currentDirection == lastTapDirection && Time.time - lastTapTime < doubleTapWindow)
            {
                TryRoll(currentDirection);
                lastTapTime = -1f; // reset so triple tap doesn't chain
            }
            else
            {
                lastTapTime = Time.time;
                lastTapDirection = currentDirection;
            }
        }

        moveInput = value;
    }
    // ── Animation ─────────────────────────────────────────────
    void HandleAnimation()
    {
        bool running = moveInput != 0f && isGrounded && !isAttacking && !isBlocking;
        animator.SetBool("isRunning", moveInput != 0f);

        if (running && !isRunning)
        {
            isRunning = true;
            if (runLoopClip != null)
            {
                audioSource.clip = runLoopClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (!running && isRunning)
        {
            isRunning = false;
            audioSource.Stop();
        }
    }
    void TryRoll(float direction)
    {
        if (isFinishable) return;
        if (isStunned) return;
        if (isRolling) return;
        if (isBlocking) return;
        if (rollCooldownTimer > 0f) return;

        isRolling = true;
        rollCooldownTimer = rollCooldown;
        rollTimer = rollDuration;
        rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);

        // forward = rolling in the direction the sprite is facing
        // backward = rolling opposite to the direction the sprite is facing
        bool isForward = direction == Mathf.Sign(transform.localScale.x);

        if (isForward)
            animator.SetTrigger("Roll");
        else
            animator.SetTrigger("RollBack");

        if (ghostTrail != null) ghostTrail.StartTrail();
        if (rollSounds != null && rollSounds.Length > 0)
        {
            AudioClip clip = rollSounds[Random.Range(0, rollSounds.Length)];
            if (clip != null) audioSource.PlayOneShot(clip);
        }
    }
    // ── Jump ──────────────────────────────────────────────────
    void TryJump()
    {
        if (isFinishable) return;
        if (!isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (jumpClip != null) audioSource.PlayOneShot(jumpClip);
    }

    public void OnSpecialPerformed()
    {
        if (isFinishable) return;
        if (canJump) TryJump();
    }
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    public void TriggerAttack()
    {
        if (isFinishable) return;
        if (isStunned) return;
        if (isAttacking) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackBox, 0f, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            IFinishable finishable = hit.GetComponentInParent<IFinishable>();
            if (finishable != null && finishable.IsFinishable())
            {
                isAttacking = true;
                animator.SetTrigger("Finisher");
                finishable.GetFinished();
                return;
            }
        }

        isAttacking = true;
        animator.SetTrigger("Attack");
    }
    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackBox, 0f, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            if (hit.GetComponent<KnightHealth>() is KnightHealth kh) { kh.TakeDamage(attackDamage, transform.position); return; }
            if (hit.GetComponent<VikingHealth>() is VikingHealth vh) { vh.TakeDamage(attackDamage, transform.position); return; }
            if (hit.GetComponent<NinjaHealth>() is NinjaHealth nh) { nh.TakeDamage(attackDamage, transform.position); return; }
            if (hit.GetComponent<SoldierHealth>() is SoldierHealth sh) { sh.TakeDamage(attackDamage, transform.position); return; }
            if (hit.GetComponent<CaveManHealth>() is CaveManHealth ch) { ch.TakeDamage(attackDamage, transform.position); return; }
        }
    }
    public void EndAttack()
    {
        isAttacking = false;
    }
    public void CameraShakes()
    {
        CameraShakerHandler.Shake(CameraShake );
    }
    public void CameraShakeHeavy()
    {
        CameraShakerHandler.Shake(CameraShakeheavy);
    }
    public void StartBlock(bool ignoreParryCooldown = false)
    {
        if (isFinishable) return;
        if (isStunned) return;
        if (blockCooldownTimer > 0f && !parryInstantBlock) return;
        parryInstantBlock = false;
        animator.ResetTrigger("StartBlock");
        animator.ResetTrigger("EndBlock");
        isBlocking = true;
        isInParryWindow = true;
        parryWindowTimer = parryWindow;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetTrigger("StartBlock");
    }

    public void EndBlock()
    {
        if (!isBlocking) return;
        isBlocking = false;
        blockCooldownTimer = blockCooldown;
        animator.SetTrigger("EndBlock");
    }

    public bool IsBlocking() => isBlocking && !isStunned;

    public void AbsorbBlockedHit(float damage, Vector2 attackerPosition)
    {
        postureRegenDelay = postureRegenWaitTime;
        // Spawn sparks
        if (blockSparksPrefab != null && blockSparksSpawnPoint != null)
            Instantiate(blockSparksPrefab, blockSparksSpawnPoint.position, Quaternion.identity);
        CameraShakerHandler.Shake(CameraShake);
        // Damage posture
        currentPosture -= damage;

        if (currentPosture <= 0f)
        {
            currentPosture = 0f;
            TriggerStun();
        }
    }
    public void TriggerParryStun(Vector2 attackerPosition)
    {
        Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        animator.SetTrigger("GetParried");

        if (currentPosture <= 0f)
        {
            isStunned = true;
            isBlocking = false;
            stunTimer = stunDuration;
            animator.SetTrigger("EndBlock");
            animator.SetBool("isStunned", true);
        }
    }
    public void ReceiveParryPostureDamage(float damage)
    {
        postureRegenDelay = postureRegenWaitTime;
        currentPosture -= damage;
        if (currentPosture <= 0f)
        {
            currentPosture = 0f;
        }
    }
    void TriggerStun()
    {
        isStunned = true;
        isBlocking = false;
        stunTimer = stunDuration;
        animator.SetTrigger("EndBlock");
        animator.SetTrigger("GetStunned");
        animator.SetBool("isStunned", true);
        GetComponent<VikingHealth>()?.CheckFinishable(); // was wrongly calling NinjaHealth before
    }
    public void EnterFinishableState()
    {
        isFinishable = true;
        isBlocking = false;
        isAttacking = false;
        isRolling = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Finishable");
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerFinishable(gameObject);
    }

    public void GetFinished()
    {
        isFinishable = false;
        animator.SetTrigger("GetFinished");
        StartCoroutine(DisableAfterDelay(2.5f));
    }

    System.Collections.IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDisabled(gameObject);
        gameObject.SetActive(false);
    }
    public void SpawnHead()
    {
        if (headPrefab == null || headSpawnPoint == null) return;

        GameObject head = Instantiate(headPrefab, headSpawnPoint.position, headPrefab.transform.rotation);

        Rigidbody2D headRb = head.GetComponent<Rigidbody2D>();
        if (headRb == null) return;

        // horizontal direction is random left or right for variety
        float horizontalDir = Random.value > 0.5f ? 1f : -1f;

        headRb.AddForce(new Vector2(horizontalDir * headHorizontalForce, headUpwardForce), ForceMode2D.Impulse);
        headRb.AddTorque(headTorque * horizontalDir);
        Destroy(head, 2f);
    }
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackBox);
        }
    }
}
