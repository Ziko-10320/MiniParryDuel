using UnityEngine;
using UnityEngine.InputSystem;
using FirstGearGames.SmoothCameraShaker;
public class VikingMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Jump")]
    public bool canJump = false;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Dash")]
    public bool canDash = false;
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    [Header("Player Assignment")]
    public bool isPlayer1 = false;
    public bool isPlayer2 = false;
    // ── Private ──────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator animator;

    private float moveInput;
    private bool isGrounded;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;
    [Header("Attack")]
    public Transform attackPoint;
    public Vector2 attackBox = new Vector2(1f, 1f);
    public float attackDamage = 10f;
    public LayerMask enemyLayer;

    private bool isAttacking;

    public ShakeData CameraShake ;
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
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentPosture = maxPosture;
        vikingHealth = GetComponent<VikingHealth>();
    }
    public void OnSpecialPerformed()
    {
        if (canJump && !canDash) TryJump();
        else if (canDash && !canJump) TryDash();
    }
    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Dash cooldown tick
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // Skip normal movement logic while dashing
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
            return;
        }
        // Stun timer
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                animator.SetBool("isStunned", false);
            }
            return;
        }

        // Posture regen when not blocking
        if (!isBlocking && currentPosture < maxPosture)
            currentPosture += postureRegen * Time.deltaTime;
        if (isInParryWindow)
        {
            parryWindowTimer -= Time.deltaTime;
            if (parryWindowTimer <= 0f)
                isInParryWindow = false;
        }
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
        moveInput = value;
    }
    // ── Animation ─────────────────────────────────────────────
    void HandleAnimation()
    {
        animator.SetBool("isRunning", moveInput != 0f);
    }

    // ── Jump ──────────────────────────────────────────────────
    void TryJump()
    {
        if (!isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // ── Dash ──────────────────────────────────────────────────
    void TryDash()
    {
        if (dashCooldownTimer > 0f) return;

        // Dash in the direction the sprite is currently facing
        dashDirection = transform.localScale.x; // +1 right, -1 left

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
    }
    public void TriggerAttack()
    {
        if (isStunned) return;
        if (isAttacking) return;
        isAttacking = true;
        animator.SetTrigger("Attack");
    }
    public void DealDamage()
    {
        Collider2D hit = Physics2D.OverlapBox(attackPoint.position, attackBox, 0f, enemyLayer);
        if (hit != null)
        {
            KnightHealth knightHealth = hit.GetComponent<KnightHealth>();
            if (knightHealth != null)
                knightHealth.TakeDamage(attackDamage, transform.position);
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
    public void StartBlock()
    {
        if (isStunned) return;
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
        isBlocking = false;
        animator.SetTrigger("EndBlock");
    }

    public bool IsBlocking() => isBlocking && !isStunned;

    public void AbsorbBlockedHit(float damage, Vector2 attackerPosition)
    {
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
        isStunned = true;
        isBlocking = false;
        stunTimer = parryStunDuration;
        animator.SetTrigger("EndBlock");
        animator.SetBool("isStunned", true);
        animator.SetTrigger("GetParried");

        Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }
    public void ReceiveParryPostureDamage(float damage)
    {
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
        animator.SetBool("isStunned", true);
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
