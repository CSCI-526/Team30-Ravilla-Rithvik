using BeatTiming;
using UnityEngine;

// Spins an object around its Z axis (e.g. a saw blade's sprite).
// With spinOnBeat, while the beat is running it whirls round on each beat and slows down in between,
// so the spin ticks with the music.
public class Spinner : MonoBehaviour
{
    // Degrees per second when not spinning on the beat; negative spins clockwise
    public float speed = -360f;

    // Off by default so existing saws spin as before; Level 2's saws turn it on
    public bool spinOnBeat = false;
    // How far it turns each beat; negative spins clockwise
    public float degreesPerBeat = -180f;
    // Share of the beat spent turning (the rest it holds still)
    [Range(0.05f, 1f)] public float turnFraction = 0.4f;

    private float baseAngle;


    void Awake()
    {
        baseAngle = transform.localEulerAngles.z;
    }

    void Update()
    {
        BeatConductor conductor = BeatConductor.Instance;
        if (spinOnBeat && conductor != null && conductor.IsRunning && conductor.SongBeats >= 0)
        {
            double beats = conductor.SongBeats;
            int beat = (int)System.Math.Floor(beats);
            float t = Mathf.Clamp01((float)(beats - beat) / turnFraction);
            // Fast start, soft stop
            t = 1f - (1f - t) * (1f - t) * (1f - t);

            float turns = Mathf.Repeat(beat * degreesPerBeat, 360f);
            transform.localRotation = Quaternion.Euler(0f, 0f, baseAngle + turns + t * degreesPerBeat);
            return;
        }

        transform.Rotate(0f, 0f, speed * Time.deltaTime);
    }
}
