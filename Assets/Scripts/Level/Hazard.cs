using System.Collections.Generic;
using UnityEngine;

// Hurts the player on contact. Put it on spikes, saws, lava or enemies.
// Works with both trigger and solid colliders.
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public int damage = 1;

    // Kill the player outright instead of dealing damage (lava)
    public bool killInstantly = false;

    // Players currently touching this hazard. Tracked with Enter/Exit instead of Stay,
    // because Stay stops firing once the player's Rigidbody falls asleep standing still.
    private readonly HashSet<PlayerHealth> touching = new HashSet<PlayerHealth>();


    // Staying in contact keeps hurting; PlayerHealth's invincibility time spaces out the hits
    void FixedUpdate()
    {
        touching.RemoveWhere(player => player == null);
        // Copy first: killing the player removes them from the set
        foreach (PlayerHealth player in new List<PlayerHealth>(touching))
        {
            Hurt(player);
        }
    }

    void OnDisable()
    {
        touching.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other) => AddPlayer(other);
    private void OnTriggerExit2D(Collider2D other) => RemovePlayer(other);
    private void OnCollisionEnter2D(Collision2D collision) => AddPlayer(collision.collider);
    private void OnCollisionExit2D(Collision2D collision) => RemovePlayer(collision.collider);

    private void AddPlayer(Collider2D other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        touching.Add(player);
        Hurt(player);
    }

    private void Hurt(PlayerHealth player)
    {
        if (killInstantly)
        {
            // The player respawns away from here, so stop tracking them
            touching.Remove(player);
            player.Kill();
        }
        else
        {
            player.TakeDamage(damage);
        }
    }

    private void RemovePlayer(Collider2D other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null) touching.Remove(player);
    }
}
