using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResinPuddle : MonoBehaviour
{
    private List<Vector2> speed = new List<Vector2>();
    private List<Rigidbody2D> rigidbodies = new List<Rigidbody2D>();
    private List<Enemy> enemies = new List<Enemy>();

    private void Start()
    {
        StartCoroutine(DestroySelf());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        Enemy e = rb.GetComponent<Enemy>();
        if (e == null) return;
        rigidbodies.Add(rb);
        enemies.Add(e);
        speed.Add(new Vector2(rb.velocity.x / 3, 0));
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        for (int i = 0; i < speed.Count; i++)
        {
            rigidbodies[i].velocity = speed[i];
            enemies[i].resinTime += Time.deltaTime;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        int i = rigidbodies.IndexOf(rb);
        if (i >= 0 && i < rigidbodies.Count) // Ensure the index is valid before removing
        {
            rigidbodies.RemoveAt(i);
            speed.RemoveAt(i); 
        }
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}
