using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Player Movement
    public float moveSpeed = 8f;
    public float jumpForce = 8f;

    // Extra gravity while falling so jumps come down faster than they go up (1 = no extra)
    public float fallGravityMultiplier = 2.5f;
    public float maxFallSpeed = 20f;

    //Ground check to avoid infinite looping
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;


    // 1 = facing right, -1 = facing left
    public float FacingDirection { get; private set; } = 1f;

    //
    private Rigidbody2D rb;
    private PlayerDash dash;
    private float horizontalInput;
    private bool isGrounded;
    private MovingPlatform currentPlatform;
    private float knockbackTimer;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dash = GetComponent<PlayerDash>();
    }

    void Update()
    {
        horizontalInput = 0f;
        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput += 1f;
        if (horizontalInput != 0f) FacingDirection = horizontalInput;

        // Reads W or Up Arrow key press for Jump
        bool jumpPressed = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        // Ground detection circle cast at feet position
        if (groundCheck != null)
        {
            Collider2D ground = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            isGrounded = ground != null;
            currentPlatform = isGrounded ? ground.GetComponentInParent<MovingPlatform>() : null;
        }

        // PlayerDash controls velocity while dashing
        if (dash != null && dash.IsDashing) return;

        // Being shoved: let the push carry the player instead of the movement keys
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        // Ride along with a moving platform we're standing on
        Vector2 platformVelocity = currentPlatform != null ? currentPlatform.Velocity : Vector2.zero;
        float yVelocity = rb.linearVelocity.y;

        // Fall faster than we rise, and cap the fall speed
        if (!isGrounded && yVelocity < 0f)
        {
            yVelocity += Physics2D.gravity.y * rb.gravityScale * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            yVelocity = Mathf.Max(yVelocity, -maxFallSpeed);
        }

        // Match the platform's vertical motion unless we're jumping off it
        if (currentPlatform != null && yVelocity <= platformVelocity.y + 0.1f)
        {
            yVelocity = platformVelocity.y;
        }

        // Apply physics velocity for horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed + platformVelocity.x, yVelocity);
    }

    // Shove the player (e.g. hit by an enemy). Movement keys are ignored for 'duration' seconds.
    public void Knockback(Vector2 velocity, float duration)
    {
        if (dash != null) dash.ResetDash();
        rb.linearVelocity = velocity;
        knockbackTimer = duration;
    }
}
