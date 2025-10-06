using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectWax : Item
{
    [SerializeField] float healAmount = 10;
    public override void Activate()
    {

    }

    public override void Use()
    {
        PlayerCollision playerCollision = gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<PlayerCollision>();
        playerCollision.Heal(healAmount);
        Destroy(gameObject);
    }

    public override void EndOfLifeTime()
    {


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            isPickable = true;
        }
    }
}
