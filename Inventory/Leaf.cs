using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : Item
{
    [SerializeField] private float activationTime = 8;
    public override void Activate()
    {
        
    }

    public override void Use()
    {
        PlayerController playerController = gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<PlayerController>();
        playerController.airBalloonTime = activationTime; 
        playerController.isFloating = true;
        playerController.myRigidbody.gravityScale = 0.5f;
        Destroy(gameObject);
    }

    public override void EndOfLifeTime()
    {

    }
}
