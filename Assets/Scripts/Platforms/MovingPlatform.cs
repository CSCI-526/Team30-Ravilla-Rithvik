using BeatTiming;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public Vector2 travel = new Vector2(4f, 0f);
    public float speed = 2f;
    public float waitTime = 0.5f;

    // Lock the platform to the beat: it steps along once per beat and pauses on each beat.
    // The trip and the wait are rounded to whole beats from speed and waitTime, so it keeps about the same pace.
    // Off by default so existing platforms move as before; Level 2's ferries turn it on.
    public bool syncToBeat = false;

    // Share of each beat spent moving while stepping (the rest it pauses)
    private const float StepMoveFraction = 0.6f;

    public Vector2 Velocity { get; private set; }

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 endPos;
    private bool movingToEnd = true;
    private float waitTimer;


    void Reset()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        startPos = rb.position;
        endPos = startPos + travel;
    }

    void FixedUpdate()
    {
        BeatConductor conductor = BeatConductor.Instance;
        if (syncToBeat && conductor != null)
        {
            MoveTo(BeatPosition(conductor));
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            Velocity = Vector2.zero;
            return;
        }

        Vector2 target = movingToEnd ? endPos : startPos;
        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);

        MoveTo(next);

        if (next == target)
        {
            movingToEnd = !movingToEnd;
            waitTimer = waitTime;
        }
    }

    private void MoveTo(Vector2 next)
    {
        Velocity = (next - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    // Where the platform should be right now, worked out from the beat clock so it never drifts from the music
    private Vector2 BeatPosition(BeatConductor conductor)
    {
        if (!conductor.IsRunning || conductor.SongBeats < 0) return startPos;

        double secondsPerBeat = conductor.SecondsPerBeat;
        float moveSeconds = speed > 0f ? travel.magnitude / speed : 0f;
        int moveBeats = Mathf.Max(1, Mathf.RoundToInt((float)(moveSeconds / secondsPerBeat)));
        int waitBeats = Mathf.Max(0, Mathf.RoundToInt((float)(waitTime / secondsPerBeat)));
        int tripBeats = waitBeats + moveBeats;

        double beats = conductor.SongBeats;
        int trip = (int)System.Math.Floor(beats / tripBeats);
        double intoTrip = beats - (double)trip * tripBeats;

        // Wait at the end first, then take one step per beat, each step landing on the beat
        float progress = 0f;
        if (intoTrip >= waitBeats)
        {
            double stepping = intoTrip - waitBeats;
            int step = (int)System.Math.Floor(stepping);
            float phase = (float)(stepping - step);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((phase - (1f - StepMoveFraction)) / StepMoveFraction));
            progress = (step + t) / moveBeats;
        }

        return trip % 2 == 0 ? Vector2.Lerp(startPos, endPos, progress) : Vector2.Lerp(endPos, startPos, progress);
    }
}
