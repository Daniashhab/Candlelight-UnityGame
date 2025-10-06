using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlip : MonoBehaviour
{
    public enum MovementType { VelocityBased, PositionBased }
    public MovementType movementType;

    internal bool isFacingRight = true;
    private Vector2 lastPosition; // Used for position-based flipping
    private Rigidbody2D rb;
    private SpriteRenderer sr;

 
    void Start()
    {
        lastPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = 0f;

        if (movementType == MovementType.VelocityBased)
        {
            rb = GetComponent<Rigidbody2D>();
            horizontal = rb.velocity.x;
        }
        else if (movementType == MovementType.PositionBased)
        {
            horizontal = (transform.position.x - lastPosition.x);
            lastPosition = transform.position; // Store last position for next frame
        }

        Flip(horizontal);
    }

    private void Flip(float horizontal)
    {
        if ((horizontal > 0f && !isFacingRight) || (horizontal < 0f && isFacingRight))
        {
            isFacingRight = !isFacingRight;

           

            // Check if there are any children
            if (transform.childCount > 0)
            {
                transform.localScale = new Vector3 (transform.localScale.x * -1, transform.localScale.y,transform.localScale.z);
            }
            else
            {
                // Flip the parent SpriteRenderer
                sr.flipX = !sr.flipX;
            }
        }
    }


}
