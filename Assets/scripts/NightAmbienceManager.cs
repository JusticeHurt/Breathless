using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NightAmbienceManager : MonoBehaviour
{
    [Header("References")]
    public DayNightCycle dayNightCycle;
    public Hunter hunter;

    [Header("Ambience Settings")]
    public float maxVolume = 1.0f;
    
    [Tooltip("How fast the crickets return when it gets quiet.")]
    public float fadeInSpeed = 0.5f; 
    
    [Tooltip("How fast the crickets cut out when you make noise.")]
    public float fadeOutSpeed = 5.0f; 
    
    [Tooltip("Noise level at which the crickets go completely silent.")]
    public float hunterNoiseThreshold = 40f; 

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (dayNightCycle == null) dayNightCycle = Object.FindFirstObjectByType<DayNightCycle>();
        if (hunter == null) hunter = Object.FindFirstObjectByType<Hunter>();
        
        audioSource.loop = true;
        audioSource.volume = 0f; 
        
        if (!audioSource.isPlaying) 
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        // Safety check
        if (dayNightCycle == null || hunter == null) return;

        bool isNight = (dayNightCycle.timeOfDay <= 0.23f || dayNightCycle.timeOfDay >= 0.78f);

        float targetVolume = 0f;
        
        if (isNight)
        {
            // 1 Get the noise percentage
            float noisePercentage = Mathf.Clamp01(hunter.currentNoise / hunterNoiseThreshold);
            
            // 2 Invert it
            float noiseRatio = 1f - noisePercentage;
            
            // 3 
            // Apply the ratio to max volume
            targetVolume = maxVolume * noiseRatio;
        }

        //   speed to use based on whether we are getting louder or quieter
        float currentSpeed;
        if (targetVolume > audioSource.volume)
        {
            currentSpeed = fadeInSpeed; // Volume is going up
        }
        else
        {
            currentSpeed = fadeOutSpeed; // Volume is going down
        }

        //. fade using the chosen speed
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, currentSpeed * Time.deltaTime);
    }
}