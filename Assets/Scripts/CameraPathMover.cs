using UnityEngine;
using UnityEngine.SceneManagement;

// The camera moves along waypoints on its own and the player must keep up; leaving the camera view is a loss and restarts the level
[RequireComponent(typeof(Camera))]
public class CameraPathMover : MonoBehaviour
{
    public PlayerController player;

    // Waypoints: place empty objects in the scene and drag them in order. The camera center passes through them in sequence
    public Transform[] waypoints;
    public float speed = 3f;          // Camera speed (world units per second)
    public float startDelay = 0f;     // Seconds to wait before moving at the start, giving the player time to react

    // How far (world units) the player must be outside the view to lose, so touching the edge isn't instant death
    public float outOfViewMargin = 0.5f;

    Camera cam;
    int nextIndex = 1;   // Next waypoint to move toward
    float delayTimer;
    bool gameOver;

    void Start()
    {
        cam = GetComponent<Camera>();
        delayTimer = startDelay;

        // Start directly at the first waypoint
        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
            transform.position = WithCameraZ(waypoints[0].position);
    }

    void Update()
    {
        if (gameOver || (player != null && player.HasWon)) return;   // Camera stops after the level is cleared

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        MoveAlongPath();
    }

    void LateUpdate()
    {
        if (gameOver || player == null || player.HasWon) return;

        if (IsPlayerOutOfView())
        {
            gameOver = true;
            Lose();
        }
    }

    void MoveAlongPath()
    {
        if (waypoints == null) return;

        // Several close waypoints may be passed in one frame; keep using the remaining distance so speed stays constant
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
        // Orthographic camera: half height = orthographicSize, half width = half height * aspect
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector2 d = player.transform.position - transform.position;
        return Mathf.Abs(d.x) > halfW + outOfViewMargin ||
               Mathf.Abs(d.y) > halfH + outOfViewMargin;
    }

    void Lose()
    {
        // Reloading the current scene = going back to the first frame of the game
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // Waypoints only set x/y; keep the camera's own z (otherwise it would sit at z=0 and see nothing)
    Vector3 WithCameraZ(Vector3 p) => new Vector3(p.x, p.y, transform.position.z);

    // Draw the path in the Scene view for easier tweaking
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
