using UnityEngine;

// Attach to the destination object: the level is cleared when the player touches it
// The destination needs a Collider2D with Is Trigger enabled (set automatically when the script is added)
[RequireComponent(typeof(Collider2D))]
public class Destination : MonoBehaviour
{
    public float bobHeight = 0.15f;   // Vertical bobbing amplitude to make the destination stand out; set to 0 to disable
    public float bobSpeed = 2f;

    Vector3 startPos;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (bobHeight > 0f)
            transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            player.ReachDestination();
    }
}
