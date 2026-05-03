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
    public float maxVolume = 0.5f;   // full volume of the rain

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
            StartCoroutine(FadeVolume(0, fadeDuration));
            rain.Stop();
            
            yield return new WaitForSeconds(Random.Range(minClearTime, maxClearTime));

            rain.Play();
            if (rainSound != null && !rainSound.isPlaying) rainSound.Play();
            StartCoroutine(FadeVolume(maxVolume, fadeDuration));

            yield return new WaitForSeconds(Random.Range(minRainTime, maxRainTime));
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