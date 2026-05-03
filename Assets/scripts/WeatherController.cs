using UnityEngine;
using System.Collections;

public class WeatherController : MonoBehaviour
{
    private ParticleSystem rain;
    private AudioSource rainSound;

    [Header("Timing Settings")]
    public float minClearTime = 60f; 
    public float maxClearTime = 180f;
    public float minRainTime = 60f;
    public float maxRainTime = 360f;

    [Header("Audio Settings")]
    public float fadeDuration = 3f; 
    public float maxVolume = 0.3f;   // full volume of the rain

    void Start()
    {
        rain = GetComponent<ParticleSystem>();
        rainSound = GetComponent<AudioSource>();
        
        //volume at 0 for my start
        if (rainSound != null) rainSound.volume = 0;

        StartCoroutine(WeatherRoutine());
    }

   IEnumerator WeatherRoutine()
    {
        while (true)
        {
            Debug.Log("Rain Starting...");//start rain
            rain.Play();
            if (rainSound != null && !rainSound.isPlaying) rainSound.Play();
            
            yield return StartCoroutine(FadeVolume(maxVolume, fadeDuration));


            yield return new WaitForSeconds(Random.Range(minRainTime, maxRainTime));//rain for random amount of time

            // stop rain
            Debug.Log("Rain Stopping...");
            rain.Stop();
            
            // Fade OUT the audio. 
            yield return StartCoroutine(FadeVolume(0, fadeDuration));

            // Stay clear for a random amount of time
            yield return new WaitForSeconds(Random.Range(minClearTime, maxClearTime));
        }
    }

    // Tsmooth fade
    IEnumerator FadeVolume(float targetVolume, float duration)
    {

        if (rainSound == null) yield break;

        float startVolume = rainSound.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            rainSound.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        rainSound.volume = targetVolume;
        if (targetVolume <= 0) rainSound.Stop();
    }
}