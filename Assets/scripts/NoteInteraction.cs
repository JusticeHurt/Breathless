using UnityEngine;

public class NoteInteraction : MonoBehaviour
{
    [Header("specific note's  UI")]
    public GameObject noteUI;

    void Start()
    {
        if (noteUI != null) noteUI.SetActive(false);
    }

    public void OpenNote()
    {
        
        noteUI.SetActive(true);
        Time.timeScale = 0f; // Ppause the game
        Cursor.lockState = CursorLockMode.None; // ulock mouse
        Cursor.visible = true;
    }

    public void CloseNote()
    {
        noteUI.SetActive(false);
        Time.timeScale = 1f; // Unpause the game
        Cursor.lockState = CursorLockMode.Locked; // lock mouse back to game
        Cursor.visible = false;
    }
}