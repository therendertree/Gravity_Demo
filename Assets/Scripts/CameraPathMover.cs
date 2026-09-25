using UnityEngine;
using UnityEngine.SceneManagement;

// 相机沿路径点自行移动，player 必须跟上；player 离开相机视野就判负并重开关卡
[RequireComponent(typeof(Camera))]
public class CameraPathMover : MonoBehaviour
{
    public PlayerController player;

    // 路径点：在场景里放若干空物体，按顺序拖进来。相机中心会依次经过它们
    public Transform[] waypoints;
    public float speed = 3f;          // 相机移动速度（世界单位/秒）
    public float startDelay = 0f;     // 开局等待几秒再开始移动，给玩家反应时间

    // player 超出视野多少才判负（世界单位），避免刚碰到边缘就死
    public float outOfViewMargin = 0.5f;

    Camera cam;
    int nextIndex = 1;   // 下一个要前往的路径点
    float delayTimer;
    bool gameOver;

    void Start()
    {
        cam = GetComponent<Camera>();
        delayTimer = startDelay;

        // 开局直接放到第一个路径点
        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
            transform.position = WithCameraZ(waypoints[0].position);
    }

    void Update()
    {
        if (gameOver) return;

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        MoveAlongPath();
    }

    void LateUpdate()
    {
        if (gameOver || player == null) return;

        if (IsPlayerOutOfView())
        {
            gameOver = true;
            Lose();
        }
    }

    void MoveAlongPath()
    {
        if (waypoints == null) return;

        // 一帧内可能走过多个很近的路径点，把剩余距离继续用掉，保证速度恒定
        float remaining = speed * Time.deltaTime;
        while (remaining > 0f && nextIndex < waypoints.Length)
        {
            if (waypoints[nextIndex] == null) { nextIndex++; continue; }

            Vector3 target = WithCameraZ(waypoints[nextIndex].position);
            float dist = Vector3.Distance(transform.position, target);

            if (dist <= remaining)
            {
                transform.position = target;
                remaining -= dist;
                nextIndex++;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, target, remaining);
                remaining = 0f;
            }
        }
    }

    bool IsPlayerOutOfView()
    {
        // 正交相机：半高 = orthographicSize，半宽 = 半高 * 宽高比
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector2 d = player.transform.position - transform.position;
        return Mathf.Abs(d.x) > halfW + outOfViewMargin ||
               Mathf.Abs(d.y) > halfH + outOfViewMargin;
    }

    void Lose()
    {
        // 重新加载当前场景 = 回到游戏开始时的那一帧
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // 路径点只决定 x/y，z 保持相机自己的（否则相机会贴到 z=0 看不到东西）
    Vector3 WithCameraZ(Vector3 p) => new Vector3(p.x, p.y, transform.position.z);

    // 在 Scene 视图里画出路径，方便调整
    void OnDrawGizmos()
    {
        if (waypoints == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
