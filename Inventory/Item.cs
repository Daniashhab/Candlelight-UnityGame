using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public abstract class Item : MonoBehaviour
{
    public enum ItemType { None, Stick, Pinecone, ResinBomb, Leaf, Heal, Termite }
    public ItemType itemType; // Each item prefab assigns this in the Inspector
    public bool isPickedUp = false;
    public bool isPickable = true;
    private Collider2D triggerCol;
    private Collider2D mainCol;
    internal Rigidbody2D rb;
    public float lifeTime;
    internal float lifeLeft;
    internal bool canBeDropped = true;
    internal bool isLifeOver = false;
    public Sprite icon;
    internal SpriteRenderer sr;

    public bool isActive = false;
    public bool isConsumable;
    public bool canBePickedUpWhileActive = false;

    internal bool isGrounded = false; // currently only works for resin bomb and pinecone since they are projectiles and actually need this for dragonfly logic

    private void Start()
    {
        triggerCol = transform.GetChild(0).gameObject.GetComponent<Collider2D>();
        mainCol = gameObject.GetComponent<Collider2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        lifeLeft = lifeTime;
    }

    public void GroundState()
    {
        sr.enabled = true;
        isPickedUp = false;
        mainCol.enabled = true;
        rb.isKinematic = false;
        transform.SetParent(null);
        StartCoroutine(EnableTrigger());
    }

    public void PickedUpState()
    {
        sr.enabled = false;
        isPickedUp = true;
        triggerCol.enabled = false;
        mainCol.enabled = false;
        rb.isKinematic = true;
    }

    public void LifeTimeLogic()
    {
        if (isActive)
        {
            lifeLeft -= Time.deltaTime;
            if (lifeLeft <= 0)
            {
                canBeDropped = false;
            }
            if (!canBeDropped)
            {
                isActive = false;
                EndOfLifeTime();
                AtEndOfBurning(); //we should instead play an animation at this stage for the icon if its still in the inventory, otherwise
                //play an animation for the gameObject itself, and at the end of the animaton, only there do we call this method
            }
        }
    }

    public void AtEndOfBurning()
    {
        isLifeOver = true;
        if (!isPickedUp)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator EnableTrigger()
    {
        yield return new WaitForSeconds(1.2f); 
        triggerCol.enabled = true;
    }

    public abstract void EndOfLifeTime();
    public abstract void Activate();
    public abstract void Use();


    public static Item NoneItem = new EmptyItem();

    private class EmptyItem : Item
    {
        public EmptyItem()
        {
            itemType = ItemType.None;
        }

        public override void EndOfLifeTime() { /* nothing */ }
        public override void Activate() { /* Do nothing */ }
        public override void Use() { /* Do nothing */ }
    }
}

public class ExampleItem : Item 
{
    public override void Activate()
    {
        Debug.Log("Activated something!");
    }

    public override void Use()
    {
        return; // Does nothing
    }

    public override void EndOfLifeTime()
    {
        return;
    }
}