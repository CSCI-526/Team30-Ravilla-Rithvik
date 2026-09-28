using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Start screen: Start goes straight to the first level, Select Level opens the level list
public class MainMenu : MonoBehaviour
{
    // Level scenes in play order. Start loads the first one that's in Build Settings.
    public string[] levelScenes = { "Level1", "Level2", "Level3" };

    public GameObject mainPanel;
    public GameObject levelSelectPanel;

    // Selected when each panel opens, so the menu also works with the keyboard (arrows + Enter)
    public GameObject mainFirstSelected;
    public GameObject levelSelectFirstSelected;


    void Start()
    {
        ShowMain();
    }

    public void StartGame()
    {
        foreach (string scene in levelScenes)
        {
            if (Application.CanStreamedLevelBeLoaded(scene))
            {
                SceneManager.LoadScene(scene);
                return;
            }
        }

        Debug.LogWarning("No level scenes are in Build Settings yet.", this);
    }

    public void ShowLevelSelect()
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        Select(levelSelectFirstSelected);
    }

    public void ShowMain()
    {
        levelSelectPanel.SetActive(false);
        mainPanel.SetActive(true);
        Select(mainFirstSelected);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Select(GameObject target)
    {
        if (EventSystem.current == null || target == null) return;

        // Levels that aren't made yet are disabled, so fall back to the first button that can be pressed
        Selectable selectable = target.GetComponent<Selectable>();
        if (selectable != null && !selectable.interactable && target.transform.parent != null)
        {
            foreach (Selectable other in target.transform.parent.GetComponentsInChildren<Selectable>())
            {
                if (other.interactable)
                {
                    target = other.gameObject;
                    break;
                }
            }
        }

        EventSystem.current.SetSelectedGameObject(target);
    }
}
