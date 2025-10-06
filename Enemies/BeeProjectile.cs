using System.Collections;
using UnityEngine;

public class BeeProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 15f; // Time before self-destruction
    public float damage = 12;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(DestroyAfterTime()); // Start the self-destruct timer
    }

    void Update()
    {
        RotateTowardsMovement(); // Adjust rotation to match movement direction
    }

    void RotateTowardsMovement()
    {
        if (rb.velocity.sqrMagnitude > 0.01f) // Only rotate if moving
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
