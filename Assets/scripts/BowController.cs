using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BowController : MonoBehaviour
{
    public bool isAiming; 

    [Header("Settings")]
    public float launchForce = 40f;
    public float holdingBreathMultiplier = 1.5f; 
    public float reloadTime = 1.0f;
    public float shotNoiseLevel = 45f; 

    [Header("Aiming & Zoom")]
    public float normalFOV = 60f;
    public float aimFOV = 35f;
    public float zoomSpeed = 10f;

    [Header("Sway Settings")]
    public float baseSway = 0.05f;
    public float breathSwayReduction = 0.2f; 
    public float swaySpeed = 2f;

    [Header("Recoil Settings")]
    public float recoilAmount = 0.2f; 
    public float recoilRotation = 10f; 
    public float recoilRecoverySpeed = 5f; 

    [Header("Trajectory Line")]
    public LineRenderer lineRenderer;
    public int linePoints = 25;      // Smoothness of the line
    public float timeStep = 0.1f;    // Length of the prediction
    [Header("Trajectory Colors")]
    public Color normalColor = Color.green;
    public Color targetColor = Color.red;

    [Header("References")]
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public GameObject fakeArrow;
    public PlayerBreath breathSystem; 
    private Hunter hunterScript;

    [Header("Audio")]
    AudioSource audioSource;
    public AudioClip bowShotSound; 
    public float bowVolume = 0.5f;



    private bool isReloading = false;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private Camera mainCam;

    private float currentRecoilZ = 0f;
    private float currentRecoilRotX = 0f;

    void Awake() 
    {
        mainCam = Camera.main; 

        //AudioSource initialization
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) 
        {
            breathSystem = playerObj.GetComponent<PlayerBreath>();
            hunterScript = playerObj.GetComponent<Hunter>();
        }

        // Cleanup LineRenderer on start
        if (lineRenderer != null) lineRenderer.positionCount = 0;
    }

    void Update()
    {
        HandleZoom();
        HandleRecoilRecovery();
        HandleSway();

        // Update the aiming state
        isAiming = !isReloading && Mouse.current.rightButton.isPressed;

        if (isAiming)
        {
            fakeArrow.SetActive(true);
            DrawTrajectory(); //Visualize the arc
            if (Mouse.current.leftButton.wasPressedThisFrame) Shoot();
        }
        else 
        {
            if (lineRenderer != null) lineRenderer.positionCount = 0; // Hide line
            if (!isReloading) fakeArrow.SetActive(false);
        }
    }

    void DrawTrajectory()
    {
        if (lineRenderer == null || shootPoint == null) return;

        lineRenderer.positionCount = linePoints;
        Vector3 startPosition = shootPoint.position;
        
        float forceToSimulate = (breathSystem != null && breathSystem.isHoldingBreath) ? launchForce * holdingBreathMultiplier : launchForce;
        
        Vector3 startVelocity = shootPoint.forward * forceToSimulate;
        Vector3 lastPoint = startPosition;
        bool hitDeer = false;

        // Set initial color
        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeStep;
            Vector3 currentPoint = startPosition + startVelocity * time + 0.5f * Physics.gravity * time * time;
            
            lineRenderer.SetPosition(i, currentPoint);

            // Check if this segment of the line hits a deer
            RaycastHit hit;
            if (Physics.Linecast(lastPoint, currentPoint, out hit))
            {
                if (hit.collider.CompareTag("Deer"))
                {
                    hitDeer = true;
                }
                // Stop drawing the line if hitS anything (walls, ground, etc.)
                lineRenderer.positionCount = i + 1;
                break;

            }

            lastPoint = currentPoint;
        }

        // Update color based on if DEER FOUND
        if (hitDeer)
        {
            lineRenderer.startColor = targetColor;
            lineRenderer.endColor = targetColor;
        }
    }

    void HandleZoom()
    {
        float targetFOV = Mouse.current.rightButton.isPressed ? aimFOV : normalFOV;
        mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    void Shoot()
    {
        isReloading = true;
        GameObject newArrow = Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);
        Rigidbody rb = newArrow.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            float finalForce = (breathSystem != null && breathSystem.isHoldingBreath) ? launchForce * holdingBreathMultiplier : launchForce;
            rb.AddForce(shootPoint.forward * finalForce, ForceMode.Impulse);
        }

        if (hunterScript != null) hunterScript.MakeNoise(shotNoiseLevel);

        if (audioSource != null && bowShotSound != null)
        {
            audioSource.PlayOneShot(bowShotSound, bowVolume); 
        }

        currentRecoilZ = recoilAmount; 
        currentRecoilRotX = recoilRotation; 

        if (breathSystem != null) breathSystem.heartRate += 10f;
        StartCoroutine(ReloadCooldown());
    }

    void HandleSway()
    {
        float multiplier = (breathSystem != null && breathSystem.isHoldingBreath) ? breathSwayReduction : 1f;
        float swayX = Mathf.Sin(Time.time * swaySpeed) * (baseSway * multiplier);
        float swayY = Mathf.Cos(Time.time * swaySpeed) * (baseSway * multiplier);

        transform.localPosition = initialLocalPos + new Vector3(swayX, swayY, -currentRecoilZ);
    }

    void HandleRecoilRecovery()
    {
        currentRecoilZ = Mathf.Lerp(currentRecoilZ, 0, Time.deltaTime * recoilRecoverySpeed);
        currentRecoilRotX = Mathf.Lerp(currentRecoilRotX, 0, Time.deltaTime * recoilRecoverySpeed);
        transform.localRotation = initialLocalRot * Quaternion.Euler(-currentRecoilRotX, 0, 0);
    }

    IEnumerator ReloadCooldown()
    {
        fakeArrow.SetActive(false);
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;
        if (Mouse.current.rightButton.isPressed) fakeArrow.SetActive(true);
    }
}