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
    public Transform attackPoint;          
    public Vector3 hitBoxSize = new Vector3(1f, 1f, 1f);   
    public LayerMask enemyLayer;

    public int lightDamage = 30;
    public int heavyDamage = 50;

    private bool lightActive = false;
    private bool heavyActive = false;

 
    public bool isDead = false;
    public PlayerController controller;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }


    public void TakeDamage(int dmg)
    {
        if (isDead) return; 

        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            isDead = true;
            controller.OnPlayerDeath();   
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
