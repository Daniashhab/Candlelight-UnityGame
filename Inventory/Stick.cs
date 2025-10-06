using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stick : Item
{
    [SerializeField] private Collider2D attackCollider;
    private bool isAttacking = false;
    public override void Activate()
    {
        isPickable = false;
        isActive = true;
    }

    public override void Use()
    {
        if (isAttacking) return;
        isAttacking = true;
        attackCollider.enabled = true;
        StartCoroutine(EndAttack());
    }

    public override void EndOfLifeTime()
    {

    }

    private IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(0.3f);
        attackCollider.enabled = false;
        isAttacking = false;
    }

    private void Update()
    {
        LifeTimeLogic();
    }
}

