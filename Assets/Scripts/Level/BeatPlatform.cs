using BeatTiming;
using UnityEngine;

// A platform that appears and disappears in time with the beat.
// It's solid for onBeats out of every periodBeats, blinks during the last warnBeats before vanishing,
// and shows as a faint outline while gone. Give neighbouring platforms different offsets so the
// player hops from one to the next as it appears.
[RequireComponent(typeof(Collider2D))]
public class BeatPlatform : MonoBehaviour
{
    public int periodBeats = 4;
    public float onBeats = 3f;

    // Shifts this platform's cycle, e.g. 2 = appears two beats later than a platform with 0
    public float offsetBeats = 0f;

    // Blinks for this many beats before disappearing, as a warning
    public float warnBeats = 1f;

    public Color solidColor = new Color(0.75f, 0.45f, 1f, 1f);
    [Range(0f, 1f)] public float hiddenAlpha = 0.15f;

    public bool IsSolid { get; private set; } = true;

    private Collider2D col;
    private SpriteRenderer spriteRenderer;


    void Awake()
    {
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Apply(true, false);
    }

    void Update()
    {
        BeatConductor conductor = BeatConductor.Instance;

        // Stay solid until the beat starts, so the level is safe while the music is being scheduled
        if (conductor == null || !conductor.IsRunning || conductor.SongBeats < 0)
        {
            Apply(true, false);
            return;
        }

        float period = Mathf.Max(1, periodBeats);
        float t = Mathf.Repeat((float)conductor.SongBeats - offsetBeats, period);
        bool solid = t < onBeats;
        bool warning = solid && t >= onBeats - warnBeats;

        Apply(solid, warning);
    }

    private void Apply(bool solid, bool warning)
    {
        IsSolid = solid;
        col.enabled = solid;

        if (spriteRenderer == null) return;

        Color c = solidColor;
        if (!solid)
        {
            c.a = hiddenAlpha;
        }
        else if (warning)
        {
            // Flash on and off four times per beat
            BeatConductor conductor = BeatConductor.Instance;
            float beats = conductor != null ? (float)conductor.SongBeats : 0f;
            c.a = Mathf.Repeat(beats * 4f, 1f) < 0.5f ? 1f : 0.35f;
        }
        spriteRenderer.color = c;
    }
}
