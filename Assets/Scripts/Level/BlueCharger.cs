using UnityEngine;

// The blue charger: a BlueEnemy that marches on the beat (with a BeatMover) and fights back.
// Touching it, or hitting it with a dash that isn't Perfect, hurts and shoves the player away.
// A Perfect dash still breaks it through BlueEnemy.
[RequireComponent(typeof(BlueEnemy))]
public class BlueCharger : MonoBehaviour
{
    public int touchDamage = 1;
    // Push given to the player, away from the charger
    public Vector2 shove = new Vector2(7f, 1.5f);
    public float shoveTime = 0.25f;


    private void OnCollisionEnter2D(Collision2D collision) => CheckHit(collision);

    // Also checked on Stay, for when the player keeps pushing into it
    private void OnCollisionStay2D(Collision2D collision) => CheckHit(collision);

    private void CheckHit(Collision2D collision)
    {
        PlayerDash dash = collision.collider.GetComponentInParent<PlayerDash>();
        if (dash == null || dash.IsPerfectDash) return;

        // PlayerHealth's invincibility time also spaces out the shoves
        PlayerHealth health = dash.GetComponent<PlayerHealth>();
        if (health == null || health.IsInvincible || health.IsDead) return;

        float away = Mathf.Sign(dash.transform.position.x - transform.position.x);
        PlayerMovement movement = dash.GetComponent<PlayerMovement>();
        if (movement != null) movement.Knockback(new Vector2(shove.x * away, shove.y), shoveTime);
        health.TakeDamage(touchDamage);
    }
}
