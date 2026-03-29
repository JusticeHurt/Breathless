using UnityEngine;

public class DeerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 100f;
    
    [Header("Effects")]
    public ParticleSystem bloodParticles;

    [Header("Audio")]
    AudioSource audioSource;
    public AudioClip ArrowHitSound; 
    public float HitVolume = 0.5f;

    private DeerAI deerAI;
    private bool isDead = false;

    void Start()
    {
        deerAI = GetComponent<DeerAI>();
        audioSource = GetComponent<AudioSource>();

        if (bloodParticles != null) 
        {
            bloodParticles.Stop();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        // plays hit sound immediately when damage is taken
        if (audioSource != null && ArrowHitSound != null)
        {
            audioSource.PlayOneShot(ArrowHitSound, HitVolume);
        }

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
                GameObject player = GameObject.FindWithTag("Player");

                if (player != null)
                {
                    // clear the flee state first so it doesn't 'return' inside DeerAI
                    deerAI.currentState = DeerAI.DeerState.Idle;
                    deerAI.OnHeardNoise(player.transform.position); 
                    
                    //Debug.Log("Deer told to flee from Player at: " + player.transform.position);
                }
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