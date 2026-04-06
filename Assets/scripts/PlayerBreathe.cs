using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerBreath : MonoBehaviour
{
    [Header("UI References")]
    public Slider breathBar;
    public Transform playerCamera; 

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
    public float inhaleVolume = 0.5f; 
    public float gaspVolume = 0.8f;
    
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
                Debug.Log("Inhale Sound Triggered!"); // This will tell us if the code is working
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
            float breathPercentage = 1f - ((heartRate - 60f) / (maxHeartRate - 60f));
            breathBar.value = breathPercentage;

            if (breathBar.fillRect != null)
            {
                Image fillImage = breathBar.fillRect.GetComponent<Image>();
                fillImage.color = Color.Lerp(Color.red, Color.cyan, breathPercentage);
            }
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
        if (playerCamera != null) StartCoroutine(JoltCamera());
        
        Debug.Log("Player gasped! Noise level spiked to 30.");

            if (audioSource != null && gaspSound != null)
        {
            audioSource.PlayOneShot(gaspSound, gaspVolume); 
        }
    }

    IEnumerator JoltCamera()
    {
        float elapsed = 0f;
        float duration = 0.1f;
        Quaternion startRot = playerCamera.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(-5f, 0, 0);

        while (elapsed < duration)
        {
            playerCamera.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        duration = 0.2f;
        while (elapsed < duration)
        {
            playerCamera.localRotation = Quaternion.Slerp(targetRot, startRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }




}