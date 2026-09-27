using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Finish line of the last level: stops the player, shows GAME COMPLETE, then ends the game.
// In the editor it exits Play mode; in a build (e.g. WebGL, which can't quit) R restarts from the first scene.
[RequireComponent(typeof(Collider2D))]
public class GameComplete : MonoBehaviour
{
    public string message = "GAME COMPLETE";
    public float endDelay = 3f;

    public bool IsFinished { get; private set; }

    private float timer;
    private Text label;


    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsFinished) return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        IsFinished = true;

        // Freeze the player at the finish line
        player.enabled = false;
        PlayerDash dash = player.GetComponent<PlayerDash>();
        if (dash != null) dash.enabled = false;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        label = CreateLabel();
        label.text = message;
    }

    void Update()
    {
        if (!IsFinished) return;

        timer += Time.unscaledDeltaTime;
        if (timer < endDelay) return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        label.text = message + "\n<size=36>Press R to play again</size>";
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) SceneManager.LoadScene(0);
#endif
    }

    // Plain centered text on its own overlay canvas
    private Text CreateLabel()
    {
        var canvasObject = new GameObject("GameComplete Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        var textObject = new GameObject("Message");
        textObject.transform.SetParent(canvasObject.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 96;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.85f, 0.3f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return text;
    }
}
