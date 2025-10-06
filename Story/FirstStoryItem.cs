using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FirstStoryItem : MonoBehaviour
{

    [Header("Animation Frames")]
    [Tooltip("Drag your sliced 'Temporary running_0..n' sprites here in order")]
    [SerializeField] private Sprite[] runFrames;

    [Header("Animation Settings")]
    [Tooltip("How many frames per second to play the run animation")]
    [SerializeField] private float frameRate = 12f;

    [Header("Movement Settings")]
    [Tooltip("Units per second to move left")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("How long (in seconds) to run+move before destroying")]
    [SerializeField] private float moveDuration = 1.5f;

    private SpriteRenderer _sr;
    private bool _started;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (runFrames == null || runFrames.Length == 0)
            Debug.LogWarning("No run frames assigned!", gameObject);
    }

    private void Update()
    {
        // if we've triggered the run, slide left every frame
        if (_started)
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_started) return;               // only once
        if (!other.CompareTag("Player"))    // must hit the player
            return;

        _started = true;
        _sr.flipX = true;
        StartCoroutine(RunAndDestroy());
    }

    private IEnumerator RunAndDestroy()
    {
        // schedule the object to die after moveDuration
        Destroy(gameObject, moveDuration);

        if (runFrames == null || runFrames.Length == 0)
            yield break;

        float timePerFrame = 1f / frameRate;
        float elapsed = 0f;
        int frameIndex = 0;

        // loop until we've run for the total duration
        while (elapsed < moveDuration)
        {
            // swap to the next sprite
            _sr.sprite = runFrames[frameIndex];
            frameIndex = (frameIndex + 1) % runFrames.Length;

            // wait exactly one frame-interval
            yield return new WaitForSeconds(timePerFrame);
            elapsed += timePerFrame;
        }
    }
}
