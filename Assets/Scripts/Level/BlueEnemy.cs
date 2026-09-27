using UnityEngine;

// Blue box that blocks the way. Only a Perfect dash breaks it; a Good dash or walking into it just gets blocked.
[RequireComponent(typeof(Collider2D))]
public class BlueEnemy : MonoBehaviour
{
    // Brief flash when hit by a dash that wasn't Perfect, so the player knows the timing was off
    public Color blockedFlashColor = Color.white;
    public float flashDuration = 0.12f;

    private SpriteRenderer spriteRenderer;
    private Collider2D boxCollider;
    private PlayerRespawn respawn;
    private Color baseColor;
    private float flashTimer;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
    }

    // Broken boxes come back when the player respawns, so a checkpoint before the box can't skip it
    void Start()
    {
        respawn = FindFirstObjectByType<PlayerRespawn>();
        if (respawn != null) respawn.OnRespawned += Restore;
    }

    void OnDestroy()
    {
        if (respawn != null) respawn.OnRespawned -= Restore;
    }

    void Update()
    {
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f && spriteRenderer != null) spriteRenderer.color = baseColor;
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckHit(collision);

    // Also checked on Stay, for when the player starts a dash while already touching the box
    private void OnCollisionStay2D(Collision2D collision) => CheckHit(collision);

    private void CheckHit(Collision2D collision)
    {
        PlayerDash dash = collision.collider.GetComponentInParent<PlayerDash>();
        if (dash == null || !dash.IsDashing) return;

        if (dash.IsPerfectDash)
        {
            Break();
        }
        else if (flashTimer <= 0f && spriteRenderer != null)
        {
            spriteRenderer.color = blockedFlashColor;
            flashTimer = flashDuration;
        }
    }

    private void Break()
    {
        // Turn the collider off right away so the dash carries on through, then hide the box until the next respawn
        boxCollider.enabled = false;
        gameObject.SetActive(false);
    }

    private void Restore()
    {
        flashTimer = 0f;
        if (spriteRenderer != null) spriteRenderer.color = baseColor;
        boxCollider.enabled = true;
        gameObject.SetActive(true);
    }
}
