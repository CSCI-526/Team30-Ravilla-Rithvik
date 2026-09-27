using BeatTiming;
using UnityEngine;

// Changes the music tempo when the player walks through it. Place one at each checkpoint
// so the tempo matches the section the player respawns into.
[RequireComponent(typeof(Collider2D))]
public class TempoZone : MonoBehaviour
{
    public float bpm = 105f;


    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;

        BeatConductor conductor = BeatConductor.Instance;
        if (conductor != null && !Mathf.Approximately(conductor.Bpm, bpm)) conductor.SetBpm(bpm);
    }
}
