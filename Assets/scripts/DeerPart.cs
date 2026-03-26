using UnityEngine;

public class DeerPart : MonoBehaviour
{
    public DeerHealth mainHealth; // main Deer object here
    public float damageMultiplier = 1f; // 2.0 for head, 1.0 for body

    public void ApplyDamage(float damage)
    {
        if (mainHealth != null)
        {
            mainHealth.TakeDamage(damage * damageMultiplier);
        }
    }
}