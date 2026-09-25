using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public PlayerController player;

    // player 在画面中的位置（视口坐标，0~1，左下角为 (0,0)）
    [Range(0f, 1f)] public float screenX = 0.25f;          // 靠左
    [Range(0f, 1f)] public float groundScreenY = 0.25f;    // 在 ground 上：左下
    [Range(0f, 1f)] public float ceilingScreenY = 0.75f;   // 在 ceiling 上：左上

    public float smoothTimeX = 0.1f;
    public float smoothTimeY = 0.3f;   // 竖直方向慢一点，翻转时镜头平滑过渡

    Camera cam;
    Vector3 velocity;

    // 竖直方向只在 player 落地（站在 ground 或 ceiling 上）时更新，
    // 跳跃、翻转途中都保持不动
    float anchorY;
    bool anchorOnCeiling;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (player == null) return;

        anchorY = player.transform.position.y;
        anchorOnCeiling = player.IsFlipped;
        transform.position = TargetPosition();   // 开局直接到位，不要从远处滑过来
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (player.IsGrounded())
        {
            anchorY = player.transform.position.y;
            anchorOnCeiling = player.IsFlipped;
        }

        Vector3 target = TargetPosition();
        Vector3 pos = transform.position;
        pos.x = Mathf.SmoothDamp(pos.x, target.x, ref velocity.x, smoothTimeX);
        pos.y = Mathf.SmoothDamp(pos.y, target.y, ref velocity.y, smoothTimeY);
        transform.position = pos;
    }

    Vector3 TargetPosition()
    {
        // 正交相机：半高 = orthographicSize，半宽 = 半高 * 宽高比
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float screenY = anchorOnCeiling ? ceilingScreenY : groundScreenY;

        // player 要出现在视口 (screenX, screenY)，反推相机中心应在哪里
        Vector2 offset = new Vector2((screenX - 0.5f) * 2f * halfW,
                                     (screenY - 0.5f) * 2f * halfH);

        // X 实时跟随；Y 用最近一次落地时的高度
        return new Vector3(player.transform.position.x - offset.x,
                           anchorY - offset.y,
                           transform.position.z);
    }
}
