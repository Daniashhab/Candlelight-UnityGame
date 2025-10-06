using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResinBomb : Item
{
    [SerializeField] private GameObject puddlePrefab;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ground") && isActive)
        {
            Instantiate(puddlePrefab, new Vector2(transform.position.x, transform.position.y - 0.3f), Quaternion.identity);
            Destroy(gameObject);
        }
        if (collision.gameObject.layer == LayerMask.GetMask("ground") && isActive)
        {
            isGrounded = true;
        }

    }

    public override void EndOfLifeTime()
    {
        Instantiate(puddlePrefab, new Vector2(transform.position.x, transform.position.y - 0.3f), Quaternion.identity);

    }

    private void Update()
    {
        LifeTimeLogic();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "stone golemite")
        {
            Instantiate(puddlePrefab, new Vector2(transform.position.x, transform.position.y - 0.3f), Quaternion.identity);
            Destroy(gameObject);
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
