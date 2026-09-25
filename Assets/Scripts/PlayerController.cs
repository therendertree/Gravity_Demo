using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float velocity = 5f;
    public float jumpPress = 14f;

    public LayerMask groundLayer;          // 记得把 Ground 和 Ceiling 都放进这个 Layer
    public float groundCheckDistance = 0.05f;

    Rigidbody2D rb;
    Collider2D col;

    float baseGravityScale;
    bool isFlipped = false;
    public bool IsFlipped => isFlipped;    // 供相机等外部脚本读取

    // 正常重力为 1，翻转后为 -1
    float GravityDir => isFlipped ? -1f : 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        baseGravityScale = Mathf.Abs(rb.gravityScale);
    }

    void Update()
    {
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
            // 正常重力：向上跳 (+)；翻转后：向下跳 (-)
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPress * GravityDir);
        }
    }

    void FlipGravity()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (!kb.gKey.wasPressedThisFrame) return;   // 每按一次 G 切换一次重力

        isFlipped = !isFlipped;
        rb.gravityScale = baseGravityScale * GravityDir;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);   // 清掉旧的竖直速度，翻转更干脆

        // 让角色上下颠倒，脚朝向“新的地面”
        Vector3 s = transform.localScale;
        s.y = Mathf.Abs(s.y) * GravityDir;
        transform.localScale = s;
    }

    public bool IsGrounded()
    {
        // 朝当前重力方向检测：正常时往下，翻转时往上
        Vector2 dir = isFlipped ? Vector2.up : Vector2.down;
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * 0.9f, b.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, dir, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }
}