using UnityEngine;

public class DeerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 100f;
    
    [Header("Effects")]
    public ParticleSystem bloodParticles;

    private DeerAI deerAI;
    private bool isDead = false;

    void Start()
    {
        deerAI = GetComponent<DeerAI>();
        if (bloodParticles != null) 
        {
            bloodParticles.Stop();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // start the blood trail if the deer is wounded but alive
            if (bloodParticles != null && !bloodParticles.isPlaying)
            {
                bloodParticles.Play();
            }
            
            // deer flees immediately when hit
            if (deerAI != null)
            {
                // Forces the deer to run away from its current position 
                deerAI.OnHeardNoise(transform.position);
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Deer Down!");

        Hunter player = Object.FindFirstObjectByType<Hunter>();
        if (player != null)
        {
            player.OnDeerKilled(); //increments the count AND updates the text
        }

        Destroy(gameObject, 0.1f);
    }
}