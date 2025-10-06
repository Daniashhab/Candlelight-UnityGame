using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class Bee : Enemy
{
    internal Vector2 goToPosition;
    internal bool hasPositionToGoTo = false;
    private bool hasWentToPosition = false;

    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float infinitySpeed = 1f;
    [SerializeField] private float infinitySize = 1f;
    [SerializeField] private float xAxisMult = 1f;
    [SerializeField] private float randomnessFactor = 0.2f; // Small random variation in direction
    [SerializeField] private float beeDetectionRadius = 3f; 

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float outOfSightRange = 12f;
    private bool isKited = false;

    [SerializeField] private float attackCoolDown = 2f;
    private bool canAttack = true;

    private float infinityTime = 0f;

    void Update()
    {
        if(hasWentToPosition) LookForPlayer();
        if (playerTransform != null)
        {
            if (Mathf.Abs(transform.position.x - playerTransform.position.x) < detectionRadiusPlayer)
            {
                isKited = true;
            }
            if (Mathf.Abs(transform.position.x - playerTransform.position.x) > outOfSightRange)
            {
                isKited = false;
            }
        }
        if (!isPlayerVisible) isKited = false;

        if (isKited)
        {
            FollowPlayer();
            if (canAttack)
            {
                canAttack = false;
                attack();
                StartCoroutine(AttackCoolDownTimer(attackCoolDown));
            }
        }

        if (!hasWentToPosition)
        {
            MoveToPosition();
        }
        else
        {
            AvoidOtherBees();
            MoveAwayFromPlayerX();
            MoveAwayFromPlayerY();
            AvoidGround();
            IdleMovement();
        }
    }

    private IEnumerator AttackCoolDownTimer(float time)
    {
        yield return new WaitForSeconds(time);
        canAttack = true;
    }


    void attack()
    {
        Vector2 aim = (playerTransform.position - transform.position).normalized; // Get direction
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.name = "bee projectile";

        // Apply velocity to move the projectile
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = aim * 7f; // Adjust speed as needed
        }
    }


    void AvoidGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 4, groundLayer);

        if (hit.collider != null)
        {
            transform.position += Vector3.up * movementSpeed * Time.deltaTime; // Move upwards
        }
    }

    void MoveToPosition()
    {
        if (!hasPositionToGoTo)
        {
            hasWentToPosition = true; // If no position to go to, immediately start idle movement
            return;
        }

        // Move towards goToPosition
        transform.position = Vector2.MoveTowards(transform.position, goToPosition, movementSpeed * Time.deltaTime);

        // Check if the bee has reached its position
        if (Vector2.Distance(transform.position, goToPosition) < 0.1f)
        {
            hasWentToPosition = true;
            hasPositionToGoTo = false;
        }
    }

    void FollowPlayer()
    {
        float distanceToPlayer = Mathf.Abs(transform.position.x - playerTransform.position.x);

        if (distanceToPlayer > 6f && distanceToPlayer < outOfSightRange)
        {
            Vector2 aim = (playerTransform.position - transform.position).normalized;
            transform.position += (Vector3)(aim * movementSpeed * Time.deltaTime);
        }

        // Check if the bee is too high (more than 10 units above the player)
        if (transform.position.y - playerTransform.position.y > 10f)
        {
            float targetY = playerTransform.position.y + 3f; // Move toward 3 units above player
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 0.5f), 0);
        }
    }

    void MoveAwayFromPlayerX()
    {
        if (playerTransform != null && isPlayerVisible)
        {
            float moveDirection = transform.position.x > playerTransform.position.x ? 1 : -1;

            // Keep moving the bee until it's at least 5 units away from the player
            if (Mathf.Abs(transform.position.x - playerTransform.position.x) < 5)
            {
                transform.position += new Vector3(moveDirection * (movementSpeed + 1.5f) * Time.deltaTime, 0, 0);
            }
        }
    }

    void MoveAwayFromPlayerY()
    {
        if (playerTransform != null)
        {
            if (playerTransform == null) return;
            float targetY = playerTransform.position.y + 3f;

            // Smoothly move toward target Y position
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 0.5f), 0);
        }
    }


    void AvoidOtherBees()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, beeDetectionRadius);

        foreach (Collider2D obj in nearbyObjects)
        {
            if (obj.gameObject != gameObject && obj.gameObject.name == "bee") // Detect other bees
            {
                Vector2 awayDirection = (transform.position - obj.transform.position).normalized;

                // Introduce randomness for a more natural movement
                float randomX = Random.Range(-randomnessFactor, randomnessFactor);
                float randomY = Random.Range(-randomnessFactor, randomnessFactor);
                Vector2 randomOffset = new Vector2(randomX, randomY);

                // Move slightly away from the other bee
                transform.position += (Vector3)(awayDirection + randomOffset) * movementSpeed * Time.deltaTime;
            }
        }
    }

    void IdleMovement()
    {
        infinityTime += Time.deltaTime * infinitySpeed; // Time increases naturally

        float x = 0;
        x = Mathf.Sin(infinityTime) * infinitySize;
        float y = Mathf.Sin(infinityTime * xAxisMult) * infinitySize;

        transform.position = new Vector3(transform.position.x + x, transform.position.y + y, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "pinecone")
        {
            Destroy(gameObject);
        }
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            if (hasPositionToGoTo) hasWentToPosition = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, beeDetectionRadius);
        Gizmos.color = Color.black; 
        Gizmos.DrawWireSphere(transform.position, detectionRadiusPlayer); 
    }
}
