using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public float detectionRadiusPlayer; // Radius to detect player
    public LayerMask playerLayer;       // Set this to "Player" in the Inspector
    public LayerMask groundLayer;
    internal Transform playerTransform;
    public float damage;
    public int resinStage = 0;
    internal SpriteFlip flip;


    [SerializeField] GameObject resinCubePrefab;
    [SerializeField] Sprite resinStageOne;
    [SerializeField] Sprite resinStageTwo;
    [SerializeField] Sprite resinStageThree;
    [SerializeField] Sprite resinStageFour;

    private GameObject resinCube = null;
    internal SpriteRenderer spriteR = null;
    internal float resinTime = 0;


    public bool isPlayerVisible => IsPlayerVisible(playerTransform, transform, detectionRadiusPlayer);

    internal void ResinCheck()
    {
        if (resinTime > 1 && resinStage < 4)
        {
            resinTime = 0;
            Resinify();
        }
    }

    private void Resinify()
    {
        resinStage++;
        if (resinStage == 1)
        {
            resinCube = Instantiate(resinCubePrefab, transform.position, Quaternion.identity);
            spriteR = resinCube.GetComponent<SpriteRenderer>();

            Vector3 newScale = spriteR.transform.localScale * 1.2f;
            resinCube.transform.localScale = newScale;

            // Calculate the new position to align the bottom of both objects
            float thisBottomY = transform.position.y - (spriteR.bounds.size.y / 2);
            float newHeight = spriteR.bounds.size.y;
            resinCube.transform.position = new Vector3(transform.position.x, thisBottomY + (newHeight / 2), transform.position.z);

            resinCube.transform.SetParent(transform);
        }


        switch (resinStage)
        {
            case 1:
                spriteR.sprite = resinStageOne;
                break;
            case 2:
                spriteR.sprite = resinStageTwo;
                break;
            case 3:
                spriteR.sprite = resinStageThree;
                flip.enabled = false;
                break;
            case 4:
                spriteR.sprite = resinStageFour;
                resinCube.GetComponent<Collider2D>().enabled = true;
                resinCube.layer = Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2));
                break;
        }
    }

    internal void LookForPlayer()
    {
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, detectionRadiusPlayer, playerLayer);
        if (detectedPlayer != null)
        {
            playerTransform = detectedPlayer.transform;
        }
    }
    bool IsPlayerVisible(Transform playerTransform, Transform ownTransform, float detectionRadius)
    {
        if (playerTransform == null) return false;

        Vector2 directionToPlayer = (playerTransform.position - ownTransform.position).normalized;

        int excludedLayers = LayerMask.GetMask("CameraBorders");
        RaycastHit2D[] hits = Physics2D.RaycastAll(ownTransform.position, directionToPlayer, detectionRadius, ~excludedLayers);
        //Debug.DrawRay(ownTransform.position, directionToPlayer * detectionRadius, Color.red, 1);


        int? playerIndex = null;
        List<int> obstacleIndex = new List<int>();

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("player"))
            {
                playerIndex = i;
            }
            else if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("ground") ||
                     hits[i].collider.CompareTag("hiding spot"))
            {
                obstacleIndex.Add(i);
            }
        }

        if (playerIndex != null)
        {
            if (obstacleIndex.Count == 0)
            {
                return true;
            }
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("ground")) return false;
                if (hits[i].collider.CompareTag("hiding spot"))
                {
                    float overlap = PhysicsCalculations.CalculateOverlapPercentage(playerTransform.GetComponent<Collider2D>(), hits[i].collider);
                    return overlap < 87;
                }
                if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("player")) return true;
            }
        }

        return false; // Otherwise, player is visible
    }
}
