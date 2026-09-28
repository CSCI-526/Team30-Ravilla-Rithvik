using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(0f, 1f);
    public float smoothTime = 0.15f;

    public bool useBounds = false;
    public Vector2 minPosition = new Vector2(-10f, -5f);
    public Vector2 maxPosition = new Vector2(10f, 5f);

    // Shifts the view this far ahead of the way the player is facing, so a landing spot across a long gap
    // is on screen before the jump. 0 = keep the player centred.
    public float lookAhead = 0f;
    // Slower than smoothTime so turning around briefly doesn't swing the view
    public float lookAheadSmoothTime = 0.5f;

    private Vector3 velocity;
    private PlayerMovement targetMovement;
    private float lookAheadX;
    private float lookAheadVelocity;


    void Start()
    {
        if (target != null)
        {
            targetMovement = target.GetComponent<PlayerMovement>();
            lookAheadX = GetLookAheadTarget();
            transform.position = GetTargetPosition();
        }
    }

    // LateUpdate runs after the player has moved this frame
    void LateUpdate()
    {
        if (target == null) return;

        lookAheadX = Mathf.SmoothDamp(lookAheadX, GetLookAheadTarget(), ref lookAheadVelocity, lookAheadSmoothTime);
        transform.position = Vector3.SmoothDamp(transform.position, GetTargetPosition(), ref velocity, smoothTime);
    }

    // Jump straight to the target with no smoothing, e.g. after the player respawns
    public void SnapToTarget()
    {
        if (target == null) return;

        lookAheadX = GetLookAheadTarget();
        lookAheadVelocity = 0f;
        transform.position = GetTargetPosition();
        velocity = Vector3.zero;
    }

    private float GetLookAheadTarget()
    {
        float facing = targetMovement != null ? targetMovement.FacingDirection : 1f;
        return facing * lookAhead;
    }

    private Vector3 GetTargetPosition()
    {
        Vector2 pos = (Vector2)target.position + offset;
        pos.x += lookAheadX;

        if (useBounds)
        {
            pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        }

        // Keep the camera's own z so it stays in front of the 2D scene
        return new Vector3(pos.x, pos.y, transform.position.z);
    }
}
