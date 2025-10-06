using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class StoneGolemite : Enemy
{
    public Transform pointA;
    public Transform pointB;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;


    private Transform currentPoint;
    public float speed = 1.5f;
    public float idleSpeed = 3f;
    private float threshold = 0.2f;
    [SerializeField] private GameObject stoneProjectilePrefab;
    [SerializeField] private StoneGolemSoundManager soundManager;

    [SerializeField] private float stayKitedTimer = 3;
    [SerializeField] private float kitedRadius = 8;
    private bool isKited = false;
    private bool isGrounded = true;

    [SerializeField] private float jumpPower = 8;
    [SerializeField] private float jumpDistanceCheckHorizontal = 1.5f;
    [SerializeField] private float jumpDistanceCheckVertical = 2.5f;
    private bool canJump = false;
    private float actualSpeed;

    [SerializeField] private float attackCooldown = 3f;
    private float timeSinceLastAttack = 0;
    private bool isAttacking = false;
    private bool isMeleeAttacking = false;
    private bool canAttack => (isKited && (timeSinceLastAttack >= attackCooldown) && Mathf.Abs(transform.position.x - playerTransform.position.x) > 6);

    private bool hasSetNewIdle = false;

    [SerializeField] private float leapOffFactor = 1.7f;
    [SerializeField] private float leapDistance = 5f;
    [SerializeField] private float wallCheckFactor = 1.2f;
    [SerializeField] private float requiredDepthForLeap = 5f;
    [SerializeField] private float leapSpeed = 8f;
    private bool canLeap = false;
    private Vector2 stopLeapPosition;
    private bool hasStoppedLeap = true;
    private bool isStuck = false;

    private float unstuckTimer = 0;

    private enum CharacterState
    {
        idle,
        walking,
        jump,
        fall,
        melee,
        summon
    }

    private CharacterState currentState = CharacterState.walking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteR = GetComponent<SpriteRenderer>();  
        flip = GetComponent<SpriteFlip>();
        col = GetComponent<Collider2D>();
        currentPoint = pointB;
        actualSpeed = speed;

        if (soundManager == null)
            soundManager = GetComponent<StoneGolemSoundManager>();
    }

    void FixedUpdate()
    {
        ResinCheck();

        if (resinStage > 2) return;

        timeSinceLastAttack += Time.deltaTime;
        unstuckTimer += Time.deltaTime;

        if (unstuckTimer > 1) { unstuckTimer = 0; isStuck = false; }

        LookForPlayer();
        UpdateIsKited();

        if ((((stopLeapPosition.x < transform.position.x) && flip.isFacingRight) || ((stopLeapPosition.x > transform.position.x) && !flip.isFacingRight)) && stopLeapPosition != new Vector2(0, 0) && !hasStoppedLeap)
        {
            stopLeapPosition = new Vector2(0, 0);
            rb.velocity = new Vector2(0, rb.velocity.y);
            speed = actualSpeed;
            hasStoppedLeap = true;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            SetNewIdlePoints();
            //Debug.Log("Is Kited: " + isKited + " X velocity: " + rb.velocity.x);
            //Debug.Log("Attack cool down: " + (timeSinceLastAttack >= attackCooldown) + " || Distance: " + (Mathf.Abs(transform.position.x - playerTransform.position.x) < 6));
            //Debug.Log("Is Attacking: " + isAttacking + " Is Stuck: " + isStuck);
            //Debug.Log("Stopped Leap: " + hasStoppedLeap + " Stop leap condition: " + ((((stopLeapPosition.x < transform.position.x) && flip.isFacingRight) || ((stopLeapPosition.x > transform.position.x) && !flip.isFacingRight)) && stopLeapPosition != new Vector2(0, 0) && !hasStoppedLeap));
        }

        SetAnimation(GetNextState());

        if (isKited)
        {
            LeapCheck();
            JumpCheck();

            if (canJump)
            {
                Jump();
            }
            else if (canLeap)
            {
                Leap();
            }
            else
            {
                if (!isAttacking && !isStuck && resinStage == 0) 
                { 
                    FollowPlayer(); 
                }

            }
        }
        else if (!isAttacking)
        {
            if (Vector3.Distance(pointA.position, pointB.position) < 5f) return;
            Idle();
        }

    }

    private CharacterState GetNextState()
    {
        if (isAttacking)
            return CharacterState.summon;

        if (rb.velocity.y > 0.1f)
            return CharacterState.jump;

        if (rb.velocity.y < -0.1f)
            return CharacterState.fall;

        if (!isMeleeAttacking && canAttack && resinStage < 3)
            return CharacterState.summon;

        if (Mathf.Abs(rb.velocity.x) > 0.1f && !isMeleeAttacking)
            return CharacterState.walking;

        if (isMeleeAttacking)
            return CharacterState.melee;

        return CharacterState.idle;
    }

    private void SetAnimation(CharacterState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        anim.SetTrigger(currentState.ToString().ToLower());

        if (newState == CharacterState.walking)
        {
            soundManager?.PlaySound("GolemWalk");
        }
        else
        {
            soundManager?.StopSound();
        }
    }


    private void LeapCheck()
    {
        if (!isGrounded)  return; 

            int direction = flip.isFacingRight ? 1 : -1;
        Vector2 startPosition = new Vector2(transform.position.x + (direction * col.bounds.extents.x * leapOffFactor), transform.position.y);

        float distanceIncrease = 0;
        int emergencyBreakAfterXAttempts = 0;

        while (distanceIncrease < leapDistance)
        {
            // raycast 1: check under starting from in front of the enemy for ground, if null then there is a hole to jump over
            RaycastHit2D check1 = Physics2D.Raycast(new Vector2(startPosition.x + distanceIncrease * direction, startPosition.y), Vector2.down, col.bounds.extents.y + requiredDepthForLeap, groundLayer);
            Debug.DrawRay(new Vector2(startPosition.x + distanceIncrease * direction, startPosition.y), Vector2.down * (col.bounds.extents.y + requiredDepthForLeap), Color.red, 1f);

            if (distanceIncrease < 0.1f && check1.collider != null)
            {
                return;
            }

            distanceIncrease += 0.5f;

            if (check1.collider == null && distanceIncrease > 0.5f)
            {
                isStuck = true;
            }

            if (distanceIncrease > 1.2f)
            {
                if (check1.collider != null)
                {
                    isStuck = false;
                    break;
                }

            }

            emergencyBreakAfterXAttempts++;
            if (emergencyBreakAfterXAttempts >= 50)
            {
                Debug.Log("Too many attempts to find ground and never reached own position"); break;
            }
        }

        if (isStuck) return;

        // raycast 4: check weather enemy is trying to jump into a tall wall or not 
        RaycastHit2D wallCheck = Physics2D.Raycast(startPosition, Vector2.right * direction, distanceIncrease + wallCheckFactor, groundLayer);
        Debug.DrawRay(startPosition, new Vector2(direction * distanceIncrease + wallCheckFactor, 0), Color.blue, 1);
        if (wallCheck) { isStuck = true; return; }

        stopLeapPosition = new Vector2(transform.position.x + direction*distanceIncrease*1.2f, transform.position.y);
        canLeap = true;
    }


    public void AttackBoolHandler(int i) // call at animation start and animation end
    {
        bool b = i == 0;
        isAttacking = b;
    }

    public void AttackMeleeBoolHandler() // call at animation start and animation end
    {
        isMeleeAttacking = false;
    }

    private void Attack()
    {
        timeSinceLastAttack = 0;
        GameObject projObj = Instantiate(stoneProjectilePrefab, transform.position, Quaternion.identity);
        projObj.name = stoneProjectilePrefab.name;
        StoneProjectile proj = projObj.GetComponent<StoneProjectile>();
        proj.playerTransform = playerTransform;
    }
    private void Idle()
    {
        Vector2 direction = new Vector2(currentPoint.position.x - transform.position.x, 0).normalized;
        rb.velocity = direction * idleSpeed;

        if (Vector2.Distance(transform.position, currentPoint.position) < threshold)
        {
            currentPoint = (currentPoint == pointA) ? pointB : pointA;
        }
    }

    internal void UpdateIsKited()
    {
        isKited = isKited ? true : isPlayerVisible;

        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, kitedRadius, playerLayer);
        if (detectedPlayer != null && isKited)
        {
            hasSetNewIdle = false;
            isKited = true;
        }
        else StartCoroutine(NoLongerKited());
    }

    private void FollowPlayer()
    {
        int direction = (playerTransform.position.x - transform.position.x) > 0 ? 1 : -1;
        if (((Mathf.Abs(transform.position.x - playerTransform.position.x) < 4.5f) && hasStoppedLeap) || isMeleeAttacking)
        {
            rb.velocity = new Vector2(direction * 0.01f, rb.velocity.y);
            return;
        }
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
    }

    private void JumpCheck()
    {
        if (!isGrounded) return;
        if (Mathf.Abs(transform.position.x - playerTransform.position.x) < 4) return;

        Vector2 raycastDirection = flip.isFacingRight ? Vector2.right : Vector2.left;
        int direction = flip.isFacingRight? 1 : -1;

        Vector2 rayPos = new Vector2(transform.position.x, transform.position.y - (col.bounds.extents.y * 0.5f));
        Debug.DrawRay(rayPos, raycastDirection * jumpDistanceCheckHorizontal, Color.yellow, 1);
        if (Physics2D.CircleCast(rayPos, 0.25f, raycastDirection, jumpDistanceCheckHorizontal, groundLayer))
        {
            Vector2 raycastOrigin = new Vector2(rayPos.x, rayPos.y + jumpDistanceCheckVertical);

            Debug.DrawRay(raycastOrigin, raycastDirection * (jumpDistanceCheckHorizontal + 0.3f), Color.yellow, 1);
            if (!(Physics2D.CircleCast(raycastOrigin, 0.25f, raycastDirection, jumpDistanceCheckHorizontal + 0.3f, groundLayer)))
            {
                canJump = true;
            }
            else isStuck = true;
        }
    }

    private void Leap()
    {
        hasStoppedLeap = false;
        canLeap = false;
        speed = leapSpeed; 
        int direction = flip.isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(speed * direction, jumpPower);
    }

    private void SetNewIdlePoints()
    {
        List<Vector2> directions = new List<Vector2>();
        directions.Add(Vector2.right); directions.Add(Vector2.left);

        foreach (Vector2 direction in directions)
        {
            int intDirection = (direction == Vector2.right) ? 1 : -1; // direction in the form of an integer

            float wallRaycastDistance = 10; // distance to check for a wall, also the distance away from the enemy that we set one of the idle points, decreases to avoid walls.
            float distanceToWall = wallRaycastDistance; // the adjusted wallRaycastDistance that we use for setting the point position

            Vector2 wallRaycastPos = new Vector2(col.bounds.center.x + (intDirection * col.bounds.extents.x), col.bounds.center.y - (col.bounds.extents.y * 0.8f)); 
            RaycastHit2D wallRaycast = Physics2D.CircleCast(wallRaycastPos, 0.2f, direction, wallRaycastDistance, groundLayer); // raycast to check for a wall
            Debug.DrawRay(wallRaycastPos, direction * wallRaycastDistance, Color.blue, 1); 
            if (wallRaycast) { distanceToWall = Mathf.Abs(wallRaycast.distance); } // get the distance to the surface of the wall

            int holeRaycastDistance = 100; // just as wallRaycastSize but for holes. Multiplied by 10 then later divided by 10 to avoid floating point inprecision
            Collider2D holeRaycast = null; // raycast to check for a hole
            Vector2 holeCheckPosition; // to check at slighlty under the ground level, and holeRaycastDistance away from the enemy

            Vector3 positionOfPoint; // will be assigned the true idle point later on depending on which obsticle is closer
            int distanceToHole = 0;

            int emergencyBreakAfterXAttempts = 0;

            while (holeRaycastDistance != 0)
            {
                holeCheckPosition = new Vector2(transform.position.x - (-intDirection * (holeRaycastDistance / 10)) + (intDirection * col.bounds.extents.x), transform.position.y - GetComponent<Collider2D>().bounds.extents.y - 0.1f); // setting and updating the position
                holeRaycast = Physics2D.OverlapCircle(holeCheckPosition, 0.1f, groundLayer); // using OverlapCircle rather than raycast since raycast cant really check what is specifically at the end of it, so spawning a tiny overlap circle is better.

                Debug.DrawRay(new Vector2(holeCheckPosition.x - 1, holeCheckPosition.y), 2 * Vector2.right, Color.white, 1);
                Debug.DrawRay(new Vector2(holeCheckPosition.x, holeCheckPosition.y - 1), 2 * Vector2.up, Color.white, 1);

                // if we detect ground, that means no hole, and we can set the idle point as far as the holeRaycastDistance. But if we dont detect any ground, meaning there is a hole, set the distance to 0 until we find some ground.
                // Also only update the distance if there was a hole, then ground again
                if (holeRaycast != null) 
                {
                    if (distanceToHole == 0) distanceToHole = holeRaycastDistance;
                }
                else
                {
                    distanceToHole = 0;
                }

                holeRaycastDistance -= 10; // keep inching closer til there is no longer a hole

                emergencyBreakAfterXAttempts++;
                if (emergencyBreakAfterXAttempts >= 70)
                {
                    Debug.Log("Too many attempts to find ground and never reached own position"); break;
                }

            }

            Vector3 positionOfPointDistanceToWall = new Vector3(transform.position.x + (intDirection * (wallRaycastDistance - (wallRaycastDistance - distanceToWall) - col.bounds.size.x / 10)), transform.position.y, 0);
            Vector3 positionOfPointDistanceToHole = new Vector3(transform.position.x + (intDirection * (distanceToHole / 10)), transform.position.y, 0);

            // pointB is always to the right of the monster, pointA is always to the left
            positionOfPoint = (distanceToWall > (distanceToHole / 10f)) ? positionOfPointDistanceToHole : positionOfPointDistanceToWall;

            // assign position to the correct idle point
            if (direction == Vector2.right)
            {
                pointB.position = positionOfPoint;
            }
            else
            {
                pointA.position = positionOfPoint;
            }

        }

    }

    private void Jump()
    {
        speed = actualSpeed * 2;
        int direction = flip.isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(speed * direction, jumpPower);
        canJump = false;
    }

    IEnumerator NoLongerKited()
    {
        yield return new WaitForSeconds(stayKitedTimer);

        if (isPlayerVisible)
        {
            isKited = true;
        }
        else
        {
            isKited = false;
            if (!hasSetNewIdle) { SetNewIdlePoints(); hasSetNewIdle = true; }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadiusPlayer);
        Gizmos.DrawWireSphere(transform.position, kitedRadius);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            isMeleeAttacking = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            speed = actualSpeed;
            hasStoppedLeap = true;
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            isGrounded = false;
        }
    }
}
