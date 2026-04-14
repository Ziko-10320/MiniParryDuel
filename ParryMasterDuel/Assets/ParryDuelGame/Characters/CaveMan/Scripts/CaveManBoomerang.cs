using System.Collections;
using UnityEngine;

public class CaveManBoomerang : MonoBehaviour
{
    [Header("Movement")]
    public float travelSpeed = 15f;
    public float maxDistance = 8f;
    public float returnSpeed = 12f;
    public float pickupRadius = 0.8f;

    [Header("Damage")]
    public float attackDamage = 10f;
    public float attackRadius = 0.6f;
    public LayerMask enemyLayer;
    public Transform attackPoint;

    [Header("Rotation")]
    public float spinSpeed = 720f;

    public Transform owner;
    private Transform spawnPoint;
    private Vector2 travelDirection;
    private float distanceTravelled;
    private bool returning;
    private bool hasHitOnWay;
    private bool hasHitOnReturn;

    private GhostTrail ghostTrail;
    
    public MeshRenderer meshRenderer;

    private void Awake()
    {
        ghostTrail = GetComponent<GhostTrail>();
        
    }
    void Start()
    {
        if (ghostTrail != null) ghostTrail.StartTrail();
    }
    public void Init(Transform ownerTransform, Transform spawnPointTransform)
    {
        owner = ownerTransform;
        spawnPoint = spawnPointTransform;
        travelDirection = new Vector2(ownerTransform.localScale.x, 0f).normalized;
    }

    void Update()
    {
        if (owner == null) return;

        // Spin the boomerang
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        if (!returning)
        {
            // Travel forward
            transform.position += (Vector3)(travelDirection * travelSpeed * Time.deltaTime);
            distanceTravelled += travelSpeed * Time.deltaTime;

            // Deal damage going out
            if (!hasHitOnWay)
                CheckDamage(ref hasHitOnWay);

            if (distanceTravelled >= maxDistance)
                returning = true;
        }
        else
        {
            // Return to spawnPoint (follows player)
            Vector2 dir = ((Vector2)spawnPoint.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * returnSpeed * Time.deltaTime);

            // Deal damage coming back
            if (!hasHitOnReturn)
                CheckDamage(ref hasHitOnReturn);

            // Check pickup radius
            float dist = Vector2.Distance(transform.position, spawnPoint.position);
            if (dist <= pickupRadius)
            {
                StartCoroutine(GracefulDestroy());
            }
        }
    }

    private IEnumerator GracefulDestroy()
    {
        // --- STEP 1: BECOME A GHOST ---
        // Disable all components that make the boomerang visible or interactable.
        // This makes it "dead" to the game world.
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        meshRenderer.enabled = false; // If you have a MeshRenderer for the boomerang's visual, disable it too.

        // Stop the boomerang's own movement and logic.
        this.enabled = false;

        // Tell the trail to stop spawning NEW ghosts.
        if (ghostTrail != null)
        {
            ghostTrail.StopTrail();
        }

        // Tell the owner they got the boomerang back.
        CaveManMovement caveMan = owner.GetComponent<CaveManMovement>();
        if (caveMan != null)
        {
            caveMan.OnBoomerangReturned();
        }

        // --- STEP 2: WAIT FOR THE ORPHANS TO FADE ---
        // We wait for the lifetime of the last ghost + a small buffer.
        // This gives the FadeGhost coroutines time to finish.
        float waitTime = (ghostTrail != null) ? ghostTrail.ghostLifetime + 0.1f : 0.1f;
        yield return new WaitForSeconds(waitTime);

        // --- STEP 3: THE FINAL DEATH ---
        // Now that all trails have faded, we can safely destroy the parent object.
        Destroy(gameObject);
    }
    void CheckDamage(ref bool hasHit)
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            KnightHealth knight = hit.GetComponent<KnightHealth>();
            if (knight != null) { knight.TakeDamage(attackDamage, transform.position); hasHit = true; }

            VikingHealth viking = hit.GetComponent<VikingHealth>();
            if (viking != null) { viking.TakeDamage(attackDamage, transform.position); hasHit = true; }

            NinjaHealth ninja = hit.GetComponent<NinjaHealth>();
            if (ninja != null) { ninja.TakeDamage(attackDamage, transform.position); hasHit = true; }

            SoldierHealth soldier = hit.GetComponent<SoldierHealth>();
            if (soldier != null) { soldier.TakeDamage(attackDamage, transform.position); hasHit = true; }

            CaveManHealth caveMan = hit.GetComponent<CaveManHealth>();
            if (caveMan != null && hit.transform != owner && hit.transform.parent != owner)
            { caveMan.TakeDamage(attackDamage, transform.position); hasHit = true; }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}