using UnityEngine;
using UnityEngine.UIElements;

public class WaxEgg : MonoBehaviour
{
    [SerializeField] private int numAnts = 3;
    [SerializeField] private float spawnDistance = 3f;
    [SerializeField] private float positionOffset = 0.5f;
    [SerializeField] private GameObject insectwaxPrefab;
    [SerializeField] private GameObject antPrefab;

    private Vector2[] spawnPositions;

    public void DestroyEggAndSpawnAnts()
    {
        int numAnts = Random.Range(0, 4);

        SpawnAnts(numAnts);

        DropInsectwax();
        Invoke("DestroyEgg", 0.1f);
    }

    private void DestroyEgg()
    {
        Destroy(gameObject);
    }

    private void DropInsectwax()
    {
        Rigidbody2D rb = Instantiate(insectwaxPrefab, transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
        InsectWax insectWax = rb.GetComponent<InsectWax>();
        insectWax.isPickable = false;
        float direction = UnityEngine.Random.Range(-4, 5);
        rb.velocity = new Vector2(direction, 4);
    }

    private void SpawnAnts(int antAmount)
    {
        for (int i = antAmount; i > 0; i--)
        {
            GameObject antObject = Instantiate(antPrefab, transform.position, Quaternion.identity);
            antObject.name = "ant";
            Rigidbody2D rb = antObject.GetComponent<Rigidbody2D>();
            Ant ant = antObject.GetComponent<Ant>();

            rb.velocity = new Vector2(Random.Range(-6, 7), Random.Range(5, 8));
            ant.isInvincible = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "attack hitbox")
        {
            DestroyEggAndSpawnAnts();
        }
    }

    //public Vector2[] GetAntPositions(int antCount)
    //{
    //    if (antCount == 0) return;

    //    Vector2[] positions = new Vector2[antCount];
    //    float angleStep = 180f / (antCount + 1);
    //    float startAngle = -90f + angleStep;

    //    for (int i = 0; i < antCount; i++)
    //    {
    //        float angle = startAngle + (i * angleStep);
    //        float radian = angle * Mathf.Deg2Rad;
    //        Vector2 basePosition = new Vector2(transform.position.x + spawnDistance * Mathf.Sin(radian),
    //                                           transform.position.y + spawnDistance * Mathf.Cos(radian));
            
    //        // Add a small random offset to make positions slightly different
    //        float offsetX = Random.Range(-positionOffset, positionOffset);
    //        float offsetY = Random.Range(-positionOffset, positionOffset);
    //        positions[i] = basePosition + new Vector2(offsetX, offsetY);
    //    }

    //    return positions;
    //}
}
