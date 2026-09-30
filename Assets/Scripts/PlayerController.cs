using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float velocity = 5f;
    public float jumpPress = 14f;

    public LayerMask groundLayer;          // Remember to put both Ground and Ceiling on this layer
    public float groundCheckDistance = 0.05f;

    Rigidbody2D rb;
    Collider2D col;

    float baseGravityScale;
    bool isFlipped = false;
    public bool IsFlipped => isFlipped;    // Read by external scripts such as the camera

    bool hasWon = false;
    public bool HasWon => hasWon;          // True after reaching the destination; the camera uses it to stop moving and skip the lose check

    // 1 under normal gravity, -1 when flipped
    float GravityDir => isFlipped ? -1f : 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        baseGravityScale = Mathf.Abs(rb.gravityScale);
    }

    void Update()
    {
        if (hasWon) return;   // Ignore input after the level is cleared

        FlipGravity();
        Move();
        Jump();
    }

    void Move()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x = -velocity;
        else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x = velocity;

        rb.linearVelocity = new Vector2(x, rb.linearVelocityY);
    }

    void Jump()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            // Normal gravity: jump up (+); flipped: jump down (-)
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPress * GravityDir);
        }
    }

    void FlipGravity()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (!kb.gKey.wasPressedThisFrame) return;   // Each press of G toggles gravity

        isFlipped = !isFlipped;
        rb.gravityScale = baseGravityScale * GravityDir;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);   // Clear old vertical velocity so the flip feels snappier

        // Turn the character upside down so its feet face the "new ground"
        Vector3 s = transform.localScale;
        s.y = Mathf.Abs(s.y) * GravityDir;
        transform.localScale = s;
    }

    // Called by Destination: freeze the player and show the level complete screen
    public void ReachDestination()
    {
        if (hasWon) return;
        hasWon = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;   // No longer affected by gravity; stays at the destination

        LevelCompleteUI.Show(Time.timeSinceLevelLoad);
    }

    public bool IsGrounded()
    {
        // Check along the current gravity direction: down when normal, up when flipped
        Vector2 dir = isFlipped ? Vector2.up : Vector2.down;
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * 0.9f, b.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, dir, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }
}