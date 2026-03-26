using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float sensitivity = 100f;
    public float aimMultiplier = 0.4f; // 40% of normal speed when aiming
    public Transform cameraTransform; 
    
    // reference to bow script
    private BowController bow;

    float xRotation = 0f;
    float yRotation = 0f;

    void Start()
    {
        // Find the bow in the scene (or on the player)
        bow = GetComponentInChildren<BowController>();
    }

    public void AdjustRotation(float xDelta, float yDelta)
    {
        float currentSensitivity = sensitivity;

        // check if the bow exists and if the player is currently aiming
        if (bow != null && bow.isAiming)
        {
            currentSensitivity *= aimMultiplier;
        }

        // apply the sensitivity
        xDelta *= currentSensitivity * Time.deltaTime;
        yDelta *= currentSensitivity * Time.deltaTime;

        xRotation += xDelta;
        yRotation -= yDelta;

        yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0f);
    }
}