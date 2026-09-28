using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// A level-select button. It's greyed out if its scene isn't in Build Settings yet (level not made).
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    public string sceneName;
    public Text label;
    public string levelTitle = "Level 1";
    public string lockedText = "Coming soon";

    private Button button;


    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Load);
    }

    void OnEnable()
    {
        bool available = !string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
        button.interactable = available;
        if (label != null) label.text = available ? levelTitle : $"{levelTitle}\n<size=22>{lockedText}</size>";
    }

    private void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}
