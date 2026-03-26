using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healRate = 2f; //slowly recovers health when not being hit

    [Header("UI References (Bars & Vignettes)")]
    public Slider healthSlider;      //vertical slider on the left
    public CanvasGroup bloodVignette; //red image covering the screen
    public CanvasGroup fadeOverlay;   // black image for transitions

    [Header("Health Bar Polish")]
    public Gradient healthGradient; // in inspector
    public Image fillImage;

    [Header("Death & Game Over")]
    public GameObject gameOverUI;    // The panel with the restart button
    public float deathTimeDilation = 0.2f; 
    public float fadeDuration = 1.5f;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // I\initialize Vertical Health Bar
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        // ensure Game Over UI is hidden at start
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // start the game by fading IN from black
        if (fadeOverlay != null) 
        {
            fadeOverlay.alpha = 1;
            fadeOverlay.blocksRaycasts = true;
            StartCoroutine(Fade(1, 0)); 
        }
    }

    void Update()
    {
        if (isDead) return;

        // passive healing logic
        if (currentHealth < maxHealth)
        {
            currentHealth += healRate * Time.deltaTime;
            UpdateUI();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {

            healthSlider.value = currentHealth;
            
            //change color based on 0.0 - 1.0 percentage
            if (fillImage != null && healthGradient != null)
            {
                float normalizedHealth = currentHealth / maxHealth;
                fillImage.color = healthGradient.Evaluate(normalizedHealth);
            }
        }

        if (bloodVignette != null)
        {
            bloodVignette.alpha = 1 - (currentHealth / maxHealth);
        }
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        currentHealth = 0;
        UpdateUI();

        // 1. Time Dilation (Bounty Point!)
        Time.timeScale = deathTimeDilation;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; 

        // 2. Show the Restart Menu
        if (gameOverUI != null) gameOverUI.SetActive(true);

        // 3. Unlock Mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;
    }

    // link restart button on click
    public void RestartButton()
    {
        StartCoroutine(ReloadWithFade());
    }

    IEnumerator ReloadWithFade()
    {
        // smooth Fade to black before restart
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(Fade(0, 1));
        }

        // reset time scales before loading next scene
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0;
        
        // fading TO black, block the mouse. 
        // fading TO clear, unblock it at the end.
        if (fadeOverlay != null) fadeOverlay.blocksRaycasts = true;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            }
            yield return null;
        }

        if (fadeOverlay != null) 
        {
            fadeOverlay.alpha = endAlpha;
            if (endAlpha <= 0) 
            {
                fadeOverlay.blocksRaycasts = false;
            }
        }
    }

}



