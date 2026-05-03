using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Hitboxes & Multipliers")]
    public Collider[] weakPointColliders;   // Head, Heart
    public float weakPointMultiplier = 3.0f;

    public Collider[] strongPointColliders; //  Limbs, Torso
    public float strongPointMultiplier = 0.2f;


    void Start()
    {
        currentHealth = maxHealth;
    }



    public void TakeDamage(float baseDamage, Collider hitCollider)
    {
        float actualDamage = baseDamage; // Default to 1x damage if it isn't in either list

        // check if the hit collider is a weak point
        foreach (Collider col in weakPointColliders)
        {
            if (col == hitCollider)
            {
                actualDamage *= weakPointMultiplier;
                break;
            }
        }

        // check if the hit collider is a strong point (minimal damage)
        foreach (Collider col in strongPointColliders)
        {
            if (col == hitCollider)
            {
                actualDamage *= strongPointMultiplier;
                break;
            }
        }

        currentHealth -= actualDamage;
        Debug.Log("<color=orange>[Monster Health]</color> Took " + actualDamage + " damage! (Remaining: " + currentHealth + ")");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // decrement deer count / check win condition
        Hunter hunter = Object.FindAnyObjectByType<Hunter>();
        if (hunter != null)
        {
            hunter.OnDeerKilled(); 
        }

        // sestroy the monster
        Destroy(gameObject);
    }
}