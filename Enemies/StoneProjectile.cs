using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneProjectile : MonoBehaviour
{
    internal Transform playerTransform;
    public int damage = 15;

    [SerializeField] private float updatePlayerTransformCooldown = 0.2f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float homingDuration = 15f;
    [SerializeField] private float turnDuration = 0.5f; // How long the turn should last
    [SerializeField] private float DestroyAfterSeconds = 25;

    private float timeSincePlayerTransformUpdate = 0;
    private float homingTime = 0;
    private bool stoppedAiming = false;

    private Vector2 aim;
    private Vector2 direction;

    private bool isTurning = false;
    private float turnTime = 0f;
    private Vector2 turnStartDirection;

    void Start()
    {
        aim = playerTransform.position;
        StartCoroutine(DestroySelf());
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(DestroyAfterSeconds);
        Destroy(gameObject);
    }

    void Update()
    {
        // Homing timer
        homingTime += Time.deltaTime;
        if (homingTime >= homingDuration)
        {
            stoppedAiming = true;
        }

        timeSincePlayerTransformUpdate += Time.deltaTime;

        // Update the target direction while homing
        if (timeSincePlayerTransformUpdate >= updatePlayerTransformCooldown && !stoppedAiming)
        {
            timeSincePlayerTransformUpdate = 0;
            aim = playerTransform.position;
        }

        // Calculate the direction towards the target
        Vector2 newDirection = (aim - (Vector2)transform.position).normalized;

        // Detect if the player has moved across the projectile
        if (!isTurning && Mathf.Sign(playerTransform.position.x - transform.position.x) != Mathf.Sign(direction.x))
        {
            isTurning = true;
            turnTime = 0f;
            turnStartDirection = direction;
        }

        // Handle turning motion
        if (isTurning)
        {
            turnTime += Time.deltaTime / turnDuration;
            float curveFactor = Mathf.Sin(turnTime * Mathf.PI); // Creates a smooth turning effect

            direction = Vector2.Lerp(turnStartDirection, newDirection, turnTime);
            transform.position += (Vector3)direction.normalized * (speed * curveFactor * Time.deltaTime);

            if (turnTime >= 1f)
            {
                isTurning = false;
            }
        }
        else
        {
            direction = newDirection;
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }

        // Rotate the projectile to face the movement direction (adjusting for initial upward rotation)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 10f);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            Destroy(gameObject);
        }
    }
}
