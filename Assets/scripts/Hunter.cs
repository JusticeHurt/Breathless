using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class Hunter : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float jumpPower = 5f;
    public float gravityAccel = -13f;

    [Header("Stealth & Noise")]
    public bool isHoldingBreath;
    public float currentNoise;
    public float noiseBuildRate = 1.5f;
    public float maxMovementNoise = 15f;
    public float heartNoiseContribution;

    [Header("Advanced Recovery (Accelerating)")]
    public float initialRecoveryRate = 5f;   // starts dropping slow
    public float maxRecoveryRate = 40f;      // eventually drops very fast
    public float recoveryAcceleration = 15f; // speed at which recovery gains momentum
    private float currentRecoveryMomentum;

    [Header("UI Reference")]
    public Slider noiseSlider;
    public Gradient noiseGradient; // The color map (Yellow to Red)
    public Image noiseFillImage;
    public TextMeshProUGUI deerCounterText; 
    public GameObject winScreenUI;         

    [Header("Win Settings")]
    public int winCondition;

    [Header("Physics Setup")]
    public Transform groundCheck;
    public LayerMask groundMask;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    // Static so it persists, but we reset it in Start()
    public static int deerKilled = 0;

    void Start()
    {
    
        Time.timeScale = 1f;
        cc = GetComponent<CharacterController>();
        deerKilled = 0;
        currentRecoveryMomentum = initialRecoveryRate;

        // SYNC WITH DAY NIGHT CYCLE
        DayNightCycle cycle = Object.FindFirstObjectByType<DayNightCycle>();
        if (cycle != null)
        {
            winCondition = cycle.totalDeerGoal;
        }

        //hide win screen
        if (winScreenUI != null) winScreenUI.SetActive(false);
        UpdateDeerUI();
    }

    void Update()
    {
        HandleStealthNoise();
        UpdateNoiseUI();
    }

    // --- Gameplay Events ---

    public void OnDeerKilled() 
    {
        deerKilled++;
        
        //Debug.Log("<color=green>Deer Killed Event!</color> Total now: " + deerKilled);
        
        UpdateDeerUI();
        
        if (deerKilled >= winCondition)
        {
            WinGame();
        }
    }

    void UpdateDeerUI()
    {
        if(deerCounterText != null)
        {
            deerCounterText.text = "Deer Hunted: " + deerKilled + " / " + winCondition;
            //Debug.Log("UI Updated to: " + deerCounterText.text);
        }
    }

    void WinGame()
    {
        // bring up the win panel and freeze time
        if (winScreenUI != null) winScreenUI.SetActive(true);
        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- Movement Logic ---

    public void Move(Vector3 moveInput)
    {
        // 1. Physics Check
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        // 2. Horizontal Movement
        Vector3 move = moveInput * speed;

        // 3. Gravity
        velocity.y += gravityAccel * Time.deltaTime;

        // 4. Combine and Apply
        Vector3 finalMovement = move + (Vector3.up * velocity.y);
        cc.Move(finalMovement * Time.deltaTime);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = jumpPower;
        }
    }

    // --- Stealth & UI Logic ---

    private void HandleStealthNoise()
    {
        float moveSpeed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;
        float movementCeiling = maxMovementNoise + heartNoiseContribution;

        // if moving, reset recovery momentum. If still, accelerate it.
        if (moveSpeed < 0.1f)
        {
            currentRecoveryMomentum = Mathf.MoveTowards(currentRecoveryMomentum, maxRecoveryRate, recoveryAcceleration * Time.deltaTime);
        }
        else
        {
            currentRecoveryMomentum = initialRecoveryRate;
        }

        // Determine target noise (0 if holding breath, otherwise heart hum)
        float targetNoise = isHoldingBreath ? 0f : heartNoiseContribution;

        // DROPPING NOISE (Standing still OR holding breath)
        if (isHoldingBreath || moveSpeed < 0.1f)
        {
            currentNoise = Mathf.MoveTowards(currentNoise, targetNoise, currentRecoveryMomentum * Time.deltaTime);
        }
        // BUILDING NOISE (Moving and NOT holding breath)
        else if (moveSpeed > 0.1f)
        {
            if (currentNoise < movementCeiling)
            {
                currentNoise += (moveSpeed * noiseBuildRate) * Time.deltaTime;
            }
        }

        currentNoise = Mathf.Clamp(currentNoise, 0f, 100f);
    }

    private void UpdateNoiseUI()
    {
        if (noiseSlider != null)
        {
            noiseSlider.maxValue = 100f;
            noiseSlider.value = Mathf.Lerp(noiseSlider.value, currentNoise, Time.deltaTime * 10f);

            if (noiseFillImage != null && noiseGradient != null)
            {
                float normalizedNoise = noiseSlider.value / noiseSlider.maxValue;
                noiseFillImage.color = noiseGradient.Evaluate(normalizedNoise);
            }
        }
    }

    public void MakeNoise(float amount)
    {
        currentNoise += amount;
    }

    public void UpdateHeartNoise(float amount)
    {
        heartNoiseContribution = amount;
    }
}