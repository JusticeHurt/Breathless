using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerBreath : MonoBehaviour
{
    [Header("UI References")]
    public Slider breathBar;
    public Transform playerCamera; 
    public CanvasGroup breathVignette; // The blue one
    public CanvasGroup bloodVignette;  // The red one

    [Header("Breath Settings")]
    public float heartRate = 60f;
    public float maxHeartRate = 120f;
    public bool isHoldingBreath = false;

    [Header("Rates")]
    public float breathDrainSpeed = 15f; 
    public float recoverySpeed = 10f;    

    [Header("Audio")]
    public AudioSource audioSource; 
    public AudioClip gaspSound;  
    public AudioClip inhaleSound;
    public float inhaleVolume = 0.3f; 
    public float gaspVolume = 2.0f;

    
    private Hunter hunterScript;

    void Start()
    {
        hunterScript = GetComponent<Hunter>();
    }

    void Update()
    {
        HandleInput();
        UpdateHeartRate();
        UpdateUI();
        SyncWithHunter();
    }

    void HandleInput()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && heartRate < maxHeartRate)
        {
            if (audioSource != null && inhaleSound != null)
            {
                audioSource.PlayOneShot(inhaleSound, inhaleVolume);
                //Debug.Log("Inhale Sound Triggered!"); 
            }
        }

        // HOLDING LOGIC
        if (Keyboard.current.leftShiftKey.isPressed && heartRate < maxHeartRate)
        {
            isHoldingBreath = true;
        }
        else
        {
            isHoldingBreath = false;
        }
    }

    void UpdateHeartRate()
    {
        if (isHoldingBreath)
        {
            heartRate += breathDrainSpeed * Time.deltaTime;

            if (heartRate >= maxHeartRate)
            {
                TriggerGasp();
            }
        }
        else
        {
            // Return to resting 60 BPM
            heartRate = Mathf.MoveTowards(heartRate, 60f, recoverySpeed * Time.deltaTime);
        }

        heartRate = Mathf.Clamp(heartRate, 60f, maxHeartRate);
    }

    void SyncWithHunter()
    {
        if (hunterScript == null) return;

        hunterScript.isHoldingBreath = isHoldingBreath;

        // Pass heart rate hum to Hunter
        float heartNoise = (heartRate - 60f) / 12f;
        hunterScript.UpdateHeartNoise(heartNoise);
    }


    void UpdateUI()
    {
        if (breathBar != null)
        {
            // 1.0 is full breath, 0.0 is gasping
            float breathPercentage = 1f - ((heartRate - 60f) / (maxHeartRate - 60f));
            breathBar.value = breathPercentage;

            // VIGNETTE LOGIC
            if (breathVignette != null)
            {
                //shows when 40% of breathe is used
                float threshold = 0.4f; 
                float targetAlpha = Mathf.Max(0, (1f - breathPercentage - threshold) / (1f - threshold));

                // Stop flickering if the heart rate jumps
                breathVignette.alpha = Mathf.Lerp(breathVignette.alpha, targetAlpha, Time.deltaTime * 1.5f);
            }
            // --------------------------

            if (breathBar.fillRect != null)
            {
                Image fillImage = breathBar.fillRect.GetComponent<Image>();
                fillImage.color = Color.Lerp(Color.red, Color.cyan, breathPercentage);
            }
        }
        if (bloodVignette != null && bloodVignette.alpha > 0)
        {
            // This slowly drains the red alpha back to 0 over about half a second
            bloodVignette.alpha = Mathf.MoveTowards(bloodVignette.alpha, 0f, Time.deltaTime * 2f);
        }
    }

    void TriggerGasp()
    {
        isHoldingBreath = false;
        heartRate = 90f; // Penalty floor
        
        // spike the player noise level. 
        // Deer and Monsters are already checking hunterScript.currentNoise!
        if (hunterScript != null) 
        {
            hunterScript.MakeNoise(30f); 
        }
        // In PlayerBreath.cs inside TriggerGasp() gasp isnt red so i do a null
        if (playerCamera != null) StartCoroutine(JoltCamera(-5f, 0.1f, false));
        
        //Debug.Log("Player gasped! Noise level spiked to 30.");

            if (audioSource != null && gaspSound != null)
        {
            audioSource.PlayOneShot(gaspSound, gaspVolume); 
        }
    }

    // public for my attack script
    // 
    //parameters for kickAmount, duration, and an optional color
    public IEnumerator JoltCamera(float kickAmount = -5f, float duration = 0.1f, bool isAttack = false)
    {
        float elapsed = 0f;
        float halfDuration = duration / 2f;
        Quaternion startRot = playerCamera.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(kickAmount, Random.Range(-2f, 2f), 0);

        // If it's an attack, we spike the blood vignette alpha
        if (isAttack && bloodVignette != null)
        {
            bloodVignette.alpha = 1f; 
        }

        // --- Jolt Logic ---
        while (elapsed < halfDuration)
        {
            playerCamera.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            playerCamera.localRotation = Quaternion.Slerp(targetRot, startRot, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.localRotation = startRot;
    }





}