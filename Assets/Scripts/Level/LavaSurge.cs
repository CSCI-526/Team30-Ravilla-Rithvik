using BeatTiming;
using UnityEngine;

// Lava that floods over its banks on one beat of every bar.
// It flashes on the beat before as a warning, then surges up and spills over both edges.
// Anyone caught in the surge dies, so jumps and dashes over lava have to be timed around it.
[RequireComponent(typeof(SpriteRenderer))]
public class LavaSurge : MonoBehaviour
{
    public int beatsPerBar = 4;
    // Which beat of the bar it floods on (0 = the first beat)
    public int surgeBeat = 0;

    // How far the surface rises, and how far it spills over each bank
    public float surgeHeight = 2f;
    public float surgeSpread = 0.75f;

    // Timing in beats: starts rising just before the beat, stays up for a bit, then drains
    public float riseBeats = 0.15f;
    public float holdBeats = 0.5f;
    public float fallBeats = 0.35f;

    public Color warnColor = new Color(1f, 0.85f, 0.3f, 1f);
    public Color surgeColor = new Color(1f, 0.25f, 0.05f, 1f);

    // Share of the full surge height above which it's deadly
    [Range(0f, 1f)] public float deadlyFrom = 0.3f;

    private SpriteRenderer spriteRenderer;
    private Vector3 basePosition;
    private Vector3 baseScale;
    private Color baseColor;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        basePosition = transform.localPosition;
        baseScale = transform.localScale;
        baseColor = spriteRenderer.color;
    }

    void Update()
    {
        float surge = 0f;
        bool warning = false;

        BeatConductor conductor = BeatConductor.Instance;
        if (conductor != null && conductor.IsRunning && conductor.SongBeats >= 0)
        {
            int bar = Mathf.Max(1, beatsPerBar);
            float beats = (float)conductor.SongBeats;
            // Beats since the last surge beat, 0..bar
            float since = Mathf.Repeat(beats - surgeBeat, bar);

            if (since > bar - riseBeats) surge = (since - (bar - riseBeats)) / riseBeats;
            else if (since < holdBeats) surge = 1f;
            else if (since < holdBeats + fallBeats) surge = 1f - (since - holdBeats) / fallBeats;
            else warning = since > bar - 1f;

            // Flash four times per beat while warning
            if (warning) warning = Mathf.Repeat(beats * 4f, 1f) < 0.5f;
        }

        // Grow up and out; the bottom edge stays put
        Vector3 scale = baseScale;
        scale.x = baseScale.x + surgeSpread * 2f * surge;
        scale.y = baseScale.y + surgeHeight * surge;
        transform.localScale = scale;
        transform.localPosition = basePosition + Vector3.up * (surgeHeight * surge * 0.5f);

        spriteRenderer.color = warning ? warnColor : Color.Lerp(baseColor, surgeColor, surge);

        if (surge >= deadlyFrom) KillPlayersInside();
    }

    // Checked directly instead of waiting for collision events, which don't fire for a player standing still
    private void KillPlayersInside()
    {
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(transform.position, transform.lossyScale, 0f))
        {
            PlayerHealth player = hit.GetComponentInParent<PlayerHealth>();
            if (player != null) player.Kill();
        }
    }
}
