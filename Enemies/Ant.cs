using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ant : Enemy
{
    private Rigidbody2D rb;
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float randomnessFactor = 0.2f;
    [SerializeField] private float attackCoolDown = 3f;
    [SerializeField] private float dashSpeed = 8f;
    private bool canAttack => attackTimer >= attackCoolDown;
    internal bool isInvincible = false;
    private float attackTimer = 0;
    private bool finishedAttack = true;
    private bool isGrounded = false;

    public Animator anim;
    private CharacterState currentState = CharacterState.idle;
    private enum CharacterState
    {
        walk,
        jump,
        idle
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        flip = GetComponent<SpriteFlip>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isInvincible) return;
        if (finishedAttack) attackTimer += Time.deltaTime;

        SetAnimation(GetNextState());
        LookForPlayer();

        if (isPlayerVisible && finishedAttack)
        {
            FollowPlayer();

            if (canAttack)
            {
                Attack();
            }
        }
    }

    private CharacterState GetNextState()
    {
        if (!isGrounded)
            return CharacterState.jump;

        if (rb.velocity.magnitude > 0.5f)
            return CharacterState.walk;

        return CharacterState.idle;
    }

    private void SetAnimation(CharacterState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        anim.SetTrigger(currentState.ToString().ToLower());
    }

    void FollowPlayer()
    {
        if (playerTransform == null) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * movementSpeed, rb.velocity.y);
        //rb.AddForce(new Vector2(rb.velocity.x, jumpForce));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "attack hitbox" && !isInvincible)
        {
            Destroy(gameObject);
        }
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            isInvincible = false;
            finishedAttack = true;
            rb.gravityScale = 3;
            isGrounded = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            isGrounded = false;
        }
    }

    private void Attack()
    {
        int direction = flip.isFacingRight ? 1 : -1;
        attackTimer = 0;
        finishedAttack = false;
        rb.velocity = new Vector2 (dashSpeed * direction, jumpForce);
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject); // Destroy the enemy when called
    }

    //void AvoidOtherAnts()
    //{
    //    Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, antDetectionRadius);

    //    foreach (Collider2D obj in nearbyObjects)
    //    {
    //        if (obj.gameObject != gameObject && obj.gameObject.name == "Ant") // Ignore self
    //        {
    //            Vector2 awayDirection = (transform.position - obj.transform.position).normalized;

    //            // Introduce randomness for a more natural movement
    //            float randomX = Random.Range(-randomnessFactor, randomnessFactor);
    //            float randomY = Random.Range(-randomnessFactor, randomnessFactor);
    //            Vector2 randomOffset = new Vector2(randomX, randomY);

    //            // Move slightly away from the other bee
    //            transform.position += (Vector3)(awayDirection + randomOffset) * movementSpeed * Time.deltaTime;
    //        }
    //    }
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, detectionRadiusPlayer);
    }

}