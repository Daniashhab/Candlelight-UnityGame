using UnityEngine;

public class SoundDistance : MonoBehaviour
{
    public string playerTag = "Player"; // Set this to match your player's tag
    public float maxDistance = 10f;
    public float maxVolume = 1f; // New: maximum volume when player is very close

    private AudioSource audioSource;
    private Transform playerTransform;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameObject player = GameObject.FindGameObjectWithTag(playerTag); // Find player by tag
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        float normalized = Mathf.Clamp01(1 - (distance / maxDistance));
        audioSource.volume = normalized * maxVolume; // Apply max volume
    }

    // Draw Gizmos in Scene View
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // Optional: This works at runtime only
        if (Application.isPlaying && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}
