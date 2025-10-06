using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinecone : Item
{
    public override void Activate()
    {
        isPickable = false;
        isActive = true;
    }

    public override void Use()
    {
        SpriteFlip flip = gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<SpriteFlip>();
        GroundState();
        int direction = flip.isFacingRight ? 1 : -1; // to know which direction to drop the item in
        rb.velocity = new Vector2(direction * 4, 10);
    }

    public override void EndOfLifeTime()
    {

    }
    private void Update()
    {
        LifeTimeLogic();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ground") && isActive)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.GetMask("ground"))
        {
            isGrounded = false;
        }
    }
}
