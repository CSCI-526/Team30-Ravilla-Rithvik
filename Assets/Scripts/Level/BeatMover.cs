using BeatTiming;
using UnityEngine;

// Moves an object between waypoints in time with the beat (e.g. a saw blade).
// It waits at a waypoint, then slides to the next one and lands exactly on the beat,
// so the player can read its movement from the rhythm.
[RequireComponent(typeof(Rigidbody2D))]
public class BeatMover : MonoBehaviour
{
    // Waypoints relative to the starting position. The first one is usually (0, 0).
    public Vector2[] waypoints = { Vector2.zero, new Vector2(3f, 0f) };

    // How many beats each move takes (1 = move to the next waypoint every beat)
    public int beatsPerStep = 1;

    // Share of each step spent moving (the rest is spent waiting at the waypoint)
    [Range(0.05f, 1f)] public float moveFraction = 0.4f;

    // true: A -> B -> C -> B -> A, false: A -> B -> C -> A
    public bool pingPong = true;

    // Runs the movement this many beats early. 0 = arrive exactly on the beat.
    // A crusher uses this so it's already open a moment before the beat the player dashes on.
    public float beatOffset = 0f;

    private Rigidbody2D rb;
    private Vector2 startPos;


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
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        rb.MovePosition(startPos + GetOffset());
    }

    private Vector2 GetOffset()
    {
        BeatConductor conductor = BeatConductor.Instance;
        if (conductor == null || !conductor.IsRunning || conductor.SongBeats < 0 || waypoints.Length == 1)
        {
            return waypoints[0];
        }

        // Worked out from the beat clock every frame, so it never drifts from the music
        double steps = (conductor.SongBeats + beatOffset) / Mathf.Max(1, beatsPerStep);
        int step = (int)System.Math.Floor(steps);
        float phase = (float)(steps - step);

        // Wait first, then move during the last part of the step so we arrive on the beat
        float t = Mathf.Clamp01((phase - (1f - moveFraction)) / moveFraction);
        t = Mathf.SmoothStep(0f, 1f, t);

        return Vector2.Lerp(waypoints[WaypointIndex(step)], waypoints[WaypointIndex(step + 1)], t);
    }

    private int WaypointIndex(int step)
    {
        int count = waypoints.Length;
        if (!pingPong) return step % count;

        // Bounce back and forth: 0 1 2 1 0 1 2 ...
        int cycle = (count - 1) * 2;
        int i = step % cycle;
        return i < count ? i : cycle - i;
    }

    // Show the path in the Scene view
    void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;

        Vector2 origin = Application.isPlaying ? startPos : (Vector2)transform.position;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawWireSphere(origin + waypoints[i], 0.15f);
            if (i > 0) Gizmos.DrawLine(origin + waypoints[i - 1], origin + waypoints[i]);
        }
    }
}
