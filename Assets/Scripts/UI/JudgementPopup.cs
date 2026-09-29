using BeatTiming;
using UnityEngine;

// Pops PERFECT / GOOD / MISS above the player on every beat press,
// so the timing feedback shows up where the player is already looking
public class JudgementPopup : MonoBehaviour
{
    public Vector2 offset = new Vector2(0f, 1.7f);
    public float riseDistance = 0.5f;
    public float duration = 0.6f;
    public float textSize = 0.1f;
    public int sortingOrder = 100;

    public Color perfectColor = new Color(1f, 0.85f, 0.2f);
    public Color goodColor = new Color(0.35f, 0.9f, 1f);
    public Color missColor = new Color(1f, 0.3f, 0.3f);

    private TimingJudge judge;
    private TextMesh text;
    private MeshRenderer textRenderer;
    private Color color;
    private float shownAt = -1f;


    void Awake()
    {
        // Kept as a separate object (not a child) so it isn't affected by the player's scale
        var go = new GameObject("JudgementPopup Text");
        text = go.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 64;
        text.characterSize = textSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontStyle = FontStyle.Bold;

        textRenderer = go.GetComponent<MeshRenderer>();
        textRenderer.sharedMaterial = text.font.material;
        textRenderer.sortingOrder = sortingOrder;
        textRenderer.enabled = false;
    }

    // Start rather than OnEnable, so the Beat System has already set TimingJudge.Instance
    void Start()
    {
        judge = TimingJudge.Instance;
        if (judge != null) judge.OnJudged += Show;
    }

    void OnDestroy()
    {
        if (judge != null) judge.OnJudged -= Show;
        if (text != null) Destroy(text.gameObject);
    }

    private void Show(Judgement judgement)
    {
        switch (judgement.Tier)
        {
            case Accuracy.Perfect: text.text = "PERFECT"; color = perfectColor; break;
            case Accuracy.Good: text.text = "GOOD"; color = goodColor; break;
            default: text.text = "MISS"; color = missColor; break;
        }
        shownAt = Time.time;
        textRenderer.enabled = true;
    }

    void LateUpdate()
    {
        if (shownAt < 0f) return;

        float t = (Time.time - shownAt) / duration;
        if (t >= 1f)
        {
            textRenderer.enabled = false;
            shownAt = -1f;
            return;
        }

        // Rise up, pop in slightly larger, and fade out over the last 40%
        text.transform.position = transform.position + (Vector3)offset + Vector3.up * riseDistance * t;
        text.transform.localScale = Vector3.one * Mathf.Lerp(1.3f, 1f, Mathf.Clamp01(t * 6f));
        Color c = color;
        c.a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        text.color = c;
    }
}
