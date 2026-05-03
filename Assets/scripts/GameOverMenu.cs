using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameOverMenu : MonoBehaviour
{
    // Reloads whatever scene you are currently in
    public void RestartGame()
    {
        //  set Time.timeScale = 0
        Time.timeScale = 1f; 
        
       
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Load the current active scene
    }

    // sends the player back to the title screen
    public void GoToMainMenu()
    {
        // unfreeze time here also 
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }
}