using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BeeNest : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;
    private bool hasDetectedPlayer = false;

    [SerializeField] private int numBees = 3;
    [SerializeField] private float spawnDistance = 3f;
    private Vector2[] spawnPositions;

    [SerializeField] private GameObject beePrefab;

    [SerializeField] private SpriteRenderer bar;
    [SerializeField] private GameObject barFill;
    [SerializeField] private GameObject honeycombPrefab;
    [SerializeField] private int honeycombAmount = 3;
    private int currentHoneycombAmount;
    [SerializeField] private float stealingRadius = 3f;
    [SerializeField] private float stealingDuration = 2.5f;
    private float stealingTimer = 0;

    private float barFillScaleX;
    private Vector2 barFillStartPos;

    private void Start()
    {
        currentHoneycombAmount = honeycombAmount;
        barFillScaleX = barFill.transform.localScale.x;
        barFillStartPos = barFill.transform.position;
        barFill.GetComponent<SpriteRenderer>().enabled = false;
    }

    void Update()
    {
        if (IsPlayerDetected() && !hasDetectedPlayer)
        {
            hasDetectedPlayer = true;
            spawnPositions = GetBeePositions(numBees);
            int x = 0;
            foreach (var position in spawnPositions)
            {
                StartCoroutine(SpawnBees(x * 0.5f, position));
                x++;
            }
        }

        if (Keyboard.current.eKey.isPressed && currentHoneycombAmount > 0 && IsPlayerWithinStealingRadius())
        {
            bar.enabled = true;
            barFill.GetComponent<SpriteRenderer>().enabled = true;

            stealingTimer += Time.deltaTime;

            float depletionRatio = stealingTimer / stealingDuration;
            float newScaleX = Mathf.Lerp(
                ((float)currentHoneycombAmount / honeycombAmount) * barFillScaleX,
                ((float)(currentHoneycombAmount - 1) / honeycombAmount) * barFillScaleX,
                depletionRatio
            );

            Vector3 currentPos = barFill.transform.position;
            barFill.transform.localScale = new Vector3(newScaleX, barFill.transform.localScale.y, 1);

            // Adjust position to make bar shrink from the left
            float shiftAmount = (barFillScaleX - newScaleX);
            barFill.transform.position = new Vector2(barFillStartPos.x - shiftAmount, currentPos.y);

            if (stealingTimer >= stealingDuration)
            {
                currentHoneycombAmount--;
                stealingTimer = 0;

                newScaleX = ((float)currentHoneycombAmount / honeycombAmount) * barFillScaleX;
                barFill.transform.localScale = new Vector3(newScaleX, barFill.transform.localScale.y, 1);

                barFill.transform.position = new Vector2(barFillStartPos.x - (barFillScaleX - newScaleX) / 2, currentPos.y);

                Rigidbody2D rb = Instantiate(honeycombPrefab, transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
                Honeycomb honey = rb.GetComponent<Honeycomb>();
                honey.isPickable = false;
                float direction = UnityEngine.Random.Range(-4, 5);
                rb.velocity = new Vector2(direction, 4);
            }
        }
        else
        {
            stealingTimer = 0;
            bar.enabled = false;
            barFill.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private IEnumerator SpawnBees(float waitForSeconds, Vector2 pos)
    {
        yield return new WaitForSeconds(waitForSeconds);
        GameObject beeObject = Instantiate(beePrefab, transform.position, Quaternion.identity);
        beeObject.name = "bee";
        Bee bee = beeObject.GetComponent<Bee>();
        bee.goToPosition = pos;
        bee.hasPositionToGoTo = true;
    }

    private bool IsPlayerDetected()
    {
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        return detectedPlayer != null && IsPlayerVisible(detectedPlayer.transform);
    }

    private bool IsPlayerWithinStealingRadius()
    {
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, stealingRadius, playerLayer);
        return detectedPlayer != null;
    }

    private bool IsPlayerVisible(Transform playerTransform)
    {
        int excludedLayers = playerLayer | LayerMask.GetMask("CameraBorders");

        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                                             (playerTransform.position - transform.position).normalized,
                                             detectionRadius,
                                             ~excludedLayers);
        return hit.collider == null;
    }

    public Vector2[] GetBeePositions(int beeCount)
    {
        Vector2[] positions = new Vector2[beeCount];
        float angleStep = 180f / (beeCount + 1);
        float startAngle = -90f + angleStep;

        for (int i = 0; i < beeCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            float radian = angle * Mathf.Deg2Rad;
            positions[i] = new Vector2(transform.position.x + spawnDistance * Mathf.Sin(radian),
                                       transform.position.y + spawnDistance * Mathf.Cos(radian));
        }

        return positions;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
