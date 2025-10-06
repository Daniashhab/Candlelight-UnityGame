using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class Dragonfly : Enemy
{
    private Rigidbody2D rb;

    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float maintainYSpeed = 1f;
    [SerializeField] private float idleSpeed = 0.5f;
    [SerializeField] private float idleSize = 1f;
    [SerializeField] private float groundAvoidanceHeight = 4f; // Distance at which the dragonfly avoids the ground
    [SerializeField] private float minHoverHeight = 2.5f; // Minimum height above the player
    [SerializeField] private float maxHoverHeight = 5f; // Maximum height above the player

    private float idleTime = 0f;
    [SerializeField] private float divingSpeedX = 5;
    [SerializeField] private float divingSpeedY = 5;
    [SerializeField] private float accelerationFactor = 5;
    [SerializeField] private float diveTime = 1.5f; // Duration of the dive
    [SerializeField] private float YSpeedKited = 4f;
    [SerializeField] private float attackDelay = 1f;
    private float attackTimeCounter = 0;
    private float distanceToGround;
    public Animator anim;

    private bool isDiving = false;
    private float divingToPos;
    private float diveDirectionX;
    private float diveAccelerationTime;

    private bool isMaintainingHeight = false;

    private bool isIdle = true;

    private Vector2 direction = new Vector2(1, 0);


    private List<GameObject> projectiles = new List<GameObject>();
    private bool isAvoidingPinecone = false;

    private bool isCaughtByResinbomb = false;

    private CharacterState currentState = CharacterState.fly;
    private enum CharacterState
    {
        fly,
        dive
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceToGround = groundAvoidanceHeight;
        anim = GetComponent<Animator>();
        spriteR = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (isCaughtByResinbomb) return;
        LookForPlayer();
        LookForProjectile();
        AvoidProjectile();
        SetAnimation(GetNextState());

        if (!isMaintainingHeight) AvoidGround();

        if (rb.velocity.x > 10) rb.velocity = new Vector2(10, rb.velocity.y);
        if (rb.velocity.x < -10) rb.velocity = new Vector2(-10, rb.velocity.y);


        if (isAvoidingPinecone) return;
        if (!isDiving)
        {
            diveAccelerationTime = 0;
            if (isPlayerVisible)
            {
                attackTimeCounter += Time.deltaTime;
            }
            else
            {
                attackTimeCounter = 0;
            }

            if (attackTimeCounter > attackDelay)
            {
                StartDive();
                attackTimeCounter = 0;
            }
            else
            {
                if (isPlayerVisible)
                {
                    if (distanceToGround >= groundAvoidanceHeight)
                    {
                        FollowPlayer();
                    }
                }
                else
                {
                    if (rb.velocity.x > 0.1)
                    {
                        rb.velocity = new Vector2(rb.velocity.x - (rb.velocity.x * 0.04f), rb.velocity.y);
                    }
                    else rb.velocity = new Vector2(0, rb.velocity.y); 
                }
                if (isIdle) IdleMovement(); else idleTime = 0;
            }

        }
        else
        {
            PerformDive();
        }
        FlipTowardsPlayer();
    }

    private CharacterState GetNextState()
    {
        if (isDiving)
            return CharacterState.dive;

        return CharacterState.fly;
    }

    private void SetAnimation(CharacterState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        anim.SetTrigger(currentState.ToString().ToLower());
    }

    private IEnumerator ChangeIsAvoidingPinecone(bool b)
    {
        yield return new WaitForSeconds(1.2f);
        isAvoidingPinecone = b;
    }
    private void LookForProjectile()
    {
        projectiles.Clear();
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, 2.5f);
        foreach (Collider2D obj in detectedObjects)
        {
            if (obj.name == "pinecone")
            {
                Pinecone pine = obj.gameObject.GetComponentInParent<Pinecone>();
                if (pine.isActive && !pine.isGrounded)
                {
                    projectiles.Add(obj.gameObject);
                }
            }
            if (obj.name == "resin bomb")
            {
                ResinBomb resinb = obj.gameObject.GetComponentInParent<ResinBomb>();
                if (resinb.isActive && !resinb.isGrounded)
                {
                    projectiles.Add(obj.gameObject);
                }
            }
        }
    }

    private void AvoidProjectile()
    {
        if (projectiles.Count == 0)
        {
            StartCoroutine(ChangeIsAvoidingPinecone(false));
            return;
        }

        isAvoidingPinecone = true;

        float closestDistance = 1000;
        GameObject closestPinecone = null;

        foreach (GameObject projectile in projectiles)
        {
            float distance = Vector2.Distance(transform.position, projectile.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPinecone = projectile; 
            }
        }

        int direction = transform.position.x > closestPinecone.transform.position.x ? 1 : -1;
        rb.velocity = new Vector2(direction * movementSpeed * 1.3f, 0);

    }

    void StartDive()
    {
        if (playerTransform != null)
        {
            divingToPos = playerTransform.position.y;
            diveDirectionX = (playerTransform.position.x - transform.position.x) > 0 ? 1 : -1; // Move left or right based on player position
            isDiving = true;
            StartCoroutine(EndDiveAfterTime(diveTime));
            rb.velocity = new Vector2(rb.velocity.x, divingSpeedY * -1);
        }
    }

    IEnumerator EndDiveAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        rb.velocity = new Vector2(rb.velocity.x, divingSpeedY);

        isDiving = false;
    }

    void PerformDive()
    {
        if (transform.position.y <= divingToPos)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        diveAccelerationTime += Time.deltaTime;

        float acceleration = Mathf.Clamp01(diveAccelerationTime * accelerationFactor);

        rb.velocity = new Vector2(diveDirectionX * divingSpeedX * acceleration, rb.velocity.y);
    }


    void FollowPlayer()
    {
        if (!isPlayerVisible) return;
        if (Mathf.Abs(transform.position.x - playerTransform.position.x) >= 4.5f)
        {
            direction = new Vector2((playerTransform.position.x - transform.position.x), 0).normalized;
        }
        rb.velocity = new Vector2(direction.x * movementSpeed, rb.velocity.y);
    }

    // Moves the dragonfly upwards if it's too close to the ground
    void AvoidGround()
    {
        float groundAvH = isDiving ? groundAvoidanceHeight / 3 : groundAvoidanceHeight;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundAvH, groundLayer);
        if (hit.collider != null)
        {
            distanceToGround = Mathf.Abs(hit.collider.transform.position.y - transform.position.y);
            if (hit.collider.transform.position.y < transform.position.y)
            {
                isIdle = false;
                rb.velocity += new Vector2(rb.velocity.x, movementSpeed * Time.fixedDeltaTime * 2); // Move up if too close to ground
                isMaintainingHeight = false;
            }
        }
        else
        {
            if (isMaintainingHeight) StartCoroutine(SetMaintainingHeightToFalse());
            isIdle = true;
        }
    }

    private IEnumerator SetMaintainingHeightToFalse()
    {
        yield return new WaitForSeconds(0.5f);
        isMaintainingHeight = false;
    }

    // Ensures the dragonfly stays within the set hover height range above the player
    void MaintainHoverHeight()
    {
        float targetY = playerTransform.position.y + minHoverHeight;
        float maxY = playerTransform.position.y + maxHoverHeight;

        if (transform.position.y < targetY)
        {
            rb.velocity += new Vector2(rb.velocity.x, maintainYSpeed);
            isIdle = false;
        }
        else if (transform.position.y > maxY)
        {
            rb.velocity -= new Vector2(rb.velocity.x, maintainYSpeed);
            isIdle = false;
        }
        else
        {
            isIdle = true;
        }
    }

    // Handles idle movement using a sine wave to create a natural up-and-down motion
    void IdleMovement()
    {
        idleTime += Time.fixedDeltaTime;
        float verticalVelocity = Mathf.Sin(idleTime * idleSpeed) * idleSize;

        rb.velocity = new Vector2(rb.velocity.x, verticalVelocity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "attack hitbox")
        {
            Destroy(gameObject);
        }
        if (collision.transform.parent != null && collision.transform.parent.TryGetComponent(out Pinecone p))
        {
            if (p.isActive && !p.isGrounded)
            {
                Destroy(gameObject);
            }
        }
        if (collision.transform.parent != null && collision.transform.parent.TryGetComponent(out ResinBomb r))
        {
            if (r.isActive && !r.isGrounded)
            {
                isCaughtByResinbomb = true;
                Rigidbody2D rrb = collision.transform.parent.GetComponent<Rigidbody2D>();
                rb.velocity = rrb.velocity;
                rb.gravityScale = rrb.gravityScale;
            }
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("ground") && isCaughtByResinbomb)
        {
            Destroy(gameObject);
        }
    }


    // Draws debug visualization for the detection radius in the Unity editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadiusPlayer);
    }
    void FlipTowardsPlayer()
    {
        if (playerTransform == null) return;
        spriteR.flipX = playerTransform.position.x < transform.position.x;
    }
}
