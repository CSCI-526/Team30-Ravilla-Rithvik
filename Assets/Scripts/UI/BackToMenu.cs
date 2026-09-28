using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Press Esc in a level to go back to the start screen
public class BackToMenu : MonoBehaviour
{
    public string menuScene = "MainMenu";

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
            && Application.CanStreamedLevelBeLoaded(menuScene))
        {
            SceneManager.LoadScene(menuScene);
        }
    }
}
