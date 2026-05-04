using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for Button colors
using TMPro;           // Required for TextMeshPro

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI difficultyStatusText;
    public Button easyBtn, normalBtn, hardBtn;

    public void SetEasy() 
    {
        DifficultySettings.MonsterSpeedMultiplier = 0.7f;
        UpdateUI("EASY", Color.green, easyBtn);
        Debug.Log("Difficulty set to Easy");
    }

    public void SetNormal() 
    {
        DifficultySettings.MonsterSpeedMultiplier = 1.0f;
        UpdateUI("NORMAL", Color.white, normalBtn);
        Debug.Log("Difficulty set to Normal");
    }

    public void SetHard() 
    {
        DifficultySettings.MonsterSpeedMultiplier = 1.5f;
        UpdateUI("HARD", Color.red, hardBtn);
        Debug.Log("Difficulty set to Hard");
    }

    //  visual confirmation for me
    private void UpdateUI(string level, Color statusColor, Button activeBtn)
    {
        
        difficultyStatusText.text = "MODE: " + level;
        difficultyStatusText.color = statusColor;

      

        easyBtn.image.color = Color.gray;
        normalBtn.image.color = Color.gray;
        hardBtn.image.color = Color.gray;

        // 3. Make the selected one pop
        activeBtn.image.color = statusColor;
    }

    public void PlayGame()
    {
        //  GameScene is Index 1 in Build Settings
        SceneManager.LoadScene(1); 
    }

    public void QuitGame()
    {
        Debug.Log("Game Exited");
        Application.Quit();
    }
}