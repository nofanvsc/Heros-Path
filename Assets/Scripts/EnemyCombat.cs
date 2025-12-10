using UnityEngine;
using UnityEngine.UI;

public class EnemyCombat : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public Image healthFill;

    [Header("Attack Settings")]
    public int damage = 15;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("References")]
    public Animator animator;
    public EnemyAStarAI ai;
    public Transform attackPoint;       
    public float hitboxRadius = 0.6f;    
    public LayerMask playerLayer;

    private Transform player;
    private PlayerCombat playerCombat;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private bool hitboxActive = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        GameObject pgo = GameObject.FindWithTag("Player");
        if (pgo != null)
        {
            player = pgo.transform;
            playerCombat = player.GetComponent<PlayerCombat>();
            if (playerCombat == null)
                Debug.LogWarning("EnemyCombat: PlayerCombat not found on Player object.");
        }
        else
        {
            Debug.LogWarning("EnemyCombat: No GameObject with tag 'Player' found in scene.");
        }


        if (ai == null) ai = GetComponent<EnemyAStarAI>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        TryAttack();
    }

 
    void TryAttack()
    {
        if (isAttacking) return;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            isAttacking = true;
            if (ai != null) ai.canMove = false;

            if (animator != null)
            {
                animator.SetBool("isAttacking", true);
                animator.SetTrigger("AttackTrigger");
            }
        }
    }


    public void EnableAttack()
    {
        hitboxActive = true;
    }


    public void DisableAttack()
    {
        hitboxActive = false;
        isAttacking = false;

        if (animator != null) animator.SetBool("isAttacking", false);
        if (ai != null) ai.canMove = true;
    }

    void FixedUpdate()
    {
        if (!hitboxActive) return;
        if (attackPoint == null) return;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, hitboxRadius, playerLayer);

        foreach (var h in hits)
        {
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(damage);
            }
            else
            {
                var pc = h.GetComponent<PlayerCombat>();
                if (pc != null) pc.TakeDamage(damage);
            }

         
            hitboxActive = false;
            break;
        }
    }


    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
        if (currentHealth <= 0) Die();
    }

    void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        Destroy(gameObject);
    }

 
    void OnDrawGizmosSelected()
    {
        if (attackPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, hitboxRadius);
        }
    }
}
