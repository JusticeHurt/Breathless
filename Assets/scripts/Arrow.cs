using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasHit;
    public float baseDamage = 50f; // Set your base damage here

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Makes the arrow point in the direction it's flying
        if (!hasHit && rb.linearVelocity != Vector3.zero)
        {
            transform.forward = rb.linearVelocity;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        // 1. Try to find a DeerPart on the specific cube we hit
        DeerPart hitPart = collision.gameObject.GetComponent<DeerPart>();

        if (hitPart != null)
        {
            // 2. Apply damage through the hitbox system
            hitPart.ApplyDamage(baseDamage);
        }

        // Stop the physics so it "sticks"
        rb.isKinematic = true; 
        
        // Disable the collider so it doesn't bump into other things while stuck
        GetComponent<Collider>().enabled = false;

        // Parent it to what it hit so if the target moves, the arrow stays in it
        transform.SetParent(collision.transform); 
    }
}