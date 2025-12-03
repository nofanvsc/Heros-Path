using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Health")]
    public int maxHealth = 150;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Weapon Hit Detection (Box Hitbox)")]
    public Transform attackPoint;          // Position/rotation of hitbox
    public Vector3 hitBoxSize = new Vector3(1f, 1f, 1f);   // Editable size
    public LayerMask enemyLayer;

    public int lightDamage = 30;
    public int heavyDamage = 50;

    private bool lightActive = false;
    private bool heavyActive = false;

    // --- Death System ---
    public bool isDead = false;
    public PlayerController controller;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // ----------------------------------------------------------
    // PLAYER TAKES DAMAGE
    // ----------------------------------------------------------
    public void TakeDamage(int dmg)
    {
        if (isDead) return; // prevent more damage after death

        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            isDead = true;
            controller.OnPlayerDeath();   // tell controller to handle death
            Debug.Log("Player Dead");
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // ----------------------------------------------------------
    // ANIMATION EVENTS
    // ----------------------------------------------------------
    public void EnableLightAttack()
    {
        lightActive = true;
        PerformAttack();
    }

    public void DisableLightAttack()
    {
        lightActive = false;
    }

    public void EnableHeavyAttack()
    {
        heavyActive = true;
        PerformAttack();
    }

    public void DisableHeavyAttack()
    {
        heavyActive = false;
    }

    // ----------------------------------------------------------
    // BOX HIT DETECTION
    // ----------------------------------------------------------
    private void PerformAttack()
    {
        Collider[] hits = Physics.OverlapBox(
            attackPoint.position,
            hitBoxSize * 0.5f,
            attackPoint.rotation,
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            EnemyCombat enemy = hit.GetComponent<EnemyCombat>();
            if (enemy != null)
            {
                if (lightActive)
                    enemy.TakeDamage(lightDamage);

                if (heavyActive)
                    enemy.TakeDamage(heavyDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = attackPoint.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
    }
}
