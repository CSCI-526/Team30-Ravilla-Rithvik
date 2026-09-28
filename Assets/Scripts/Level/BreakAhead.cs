using UnityEngine;

// Breaks a blue box just before a Perfect dash reaches it, instead of on contact.
// Bumping into the box first costs the dash about one physics step of travel, which is enough to miss
// the landing when the box sits in the middle of a jump (Level 2's mid-air gate).
[RequireComponent(typeof(BlueEnemy))]
public class BreakAhead : MonoBehaviour
{
    // How far ahead of the box a Perfect dash breaks it, about one physics step of dash travel
    public float distance = 0.75f;

    private Collider2D col;


    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (!col.enabled) return;

        Bounds bounds = col.bounds;
        Vector2 size = new Vector2(bounds.size.x + distance * 2f, bounds.size.y);
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(bounds.center, size, 0f))
        {
            PlayerDash dash = hit.GetComponentInParent<PlayerDash>();
            if (dash != null && dash.IsPerfectDash)
            {
                // Let BlueEnemy break itself its own way (its Break method is private)
                SendMessage("Break");
                return;
            }
        }
    }
}
