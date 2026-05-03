using UnityEngine;
using TMPro;
using System.Collections;

public class UIFade : MonoBehaviour
{
    public float displayTime = 5f; 
    public float fadeDuration = 2f; 
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Start the timer
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        // Wait for the player to read it
        yield return new WaitForSeconds(displayTime);

        // fade the alpha to 0
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        // disable the object when done to save performance
        gameObject.SetActive(false);
    }
}