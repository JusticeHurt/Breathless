using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    [Header("Cursor Settings")]
    public Texture2D cursorTexture;

    void Start()
    {
        // runs as soon as the menu loads
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene"); 
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!"); 
        Application.Quit();   
    }
}