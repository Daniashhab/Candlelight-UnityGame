using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCollision : MonoBehaviour
{
    public Slider healthBar;
    [SerializeField] float maxHealth = 100;
    private float health = 100;
    [SerializeField] float healthDecreaseFactor = 0.01f;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] GameObject gameOverScreen;
    public GameObject playerInventory;

    [SerializeField] static float groundCheckRadius = 0.5f;
    private Transform groundCheck;

    private static bool isOnPlatform = false;
    private static Transform currentPlatform = null;
    private static Vector3 lastPlatformPosition;
    private PlayerController playerController;
    private Rigidbody2D rb;
    private Animator playerAnimatior;
    bool hasDead = false;
    public GameObject playerHead;

    private bool isHealTutorialComplete = false;
    private bool isItemToturialComplete = false;
    public TextMeshProUGUI tutText;


    void Start()
    {
        groundCheck = transform.Find("groundCheck");
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        playerAnimatior = GetComponent<Animator>();
    }

    void Update()
    {
        health -= Time.deltaTime * healthDecreaseFactor;
        healthBar.value = health / maxHealth;

        if (health <=0) 
        {
            health = 0;// Clamp health to 0
            
            hasDead = true;
            // hide the separate head sprite immediately
            if (playerHead != null)
                playerHead.SetActive(false);


            playerAnimatior.SetBool("IsDead", true);
            
           
            //bool isDead = true;
            // playerAnimatior.SetBool("IsDead", isDead);



            //GameOver();
        }
    }
    private void GameOver()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        Destroy(gameObject); // Destroy the player. add timer for death animation to play.
    }

    public void Heal(float heal)
    {
        health += heal;
    }

    public void Damage(float damage)
    {
        health += damage;
    }
    

    // Updated IsGrounded function to check for moving platforms
    public bool IsGrounded()
    {
        bool isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, LayerMask.GetMask("ground"));

        if (isOnPlatform)
        {
            isGrounded = true; // Treat platform as ground
        }

        return isGrounded;
    }

    public void PushPlayer(float verticleForce, float horizontalForce, int direction, float stunDuration)
    {
        playerController.isStunned = true;
        rb.velocity = new Vector2(horizontalForce * direction, verticleForce);
        StartCoroutine(Unstun(stunDuration));
    }
    

    IEnumerator Unstun(float time)
    {
        yield return new WaitForSeconds(time);
        playerController.isStunned = false;
    }

    void FixedUpdate()
    {
        if (isOnPlatform && currentPlatform != null)
        {
            // Apply platform movement to player
            Vector3 platformMovement = currentPlatform.position - lastPlatformPosition;
            transform.position += platformMovement;
            lastPlatformPosition = currentPlatform.position;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "bee projectile")
        {
            GameObject beeProj = collision.gameObject;
            BeeProjectile beeProjScript = beeProj.GetComponent<BeeProjectile>();
            health -= beeProjScript.damage;
            Destroy(beeProj);
        }

        if (collision.gameObject.name == "stone golemite")
        {
            GameObject golem = collision.gameObject;
            StoneGolemite script = golem.GetComponent<StoneGolemite>();
            if (script.resinStage == 4) { return; }
            SpriteFlip spriteFlip = golem.GetComponent<SpriteFlip>();
            health -= script.damage;
            int direction = spriteFlip.isFacingRight ? 1 : -1;
            PushPlayer(50, 35, direction, 0.4f);
        }

        if (collision.gameObject.CompareTag("enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            health -= enemy.damage;
        }

        // Detect and handle moving platforms
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            isOnPlatform = true;
            currentPlatform = collision.transform;
            lastPlatformPosition = currentPlatform.position;
        }

        if (collision.gameObject.name == "stone projectile")
        {
            StoneProjectile stoneP = collision.gameObject.GetComponent<StoneProjectile>();
            health -= stoneP.damage;
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("SharpObject")) 
        {
            health -= 10; 

            Rigidbody2D rb = GetComponent<Rigidbody2D>(); 
            if (rb != null)
            {
                // Calculate knockback direction
                Vector2 knockbackDirection = ((transform.position - collision.transform.position) + new Vector3(0, 1f)).normalized;

                float knockbackForce = 35f; 

                rb.velocity = Vector2.zero; 
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.name == "portal")
        {
            playerController.teleporter = collision.gameObject.GetComponent<Portal>();

        }

        if (collision.gameObject.CompareTag("items"))
        {
            Item itemComponent = collision.gameObject.GetComponentInParent<Item>();

            HandelItemToturial(itemComponent);

            if (itemComponent.isPickable && (!itemComponent.isActive || itemComponent.canBePickedUpWhileActive))
            {
                int totalItems = 0;
                foreach (Item item in inventoryManager.inventory)
                {
                    if (item != Item.NoneItem) totalItems++;
                }

                if (itemComponent != null && totalItems < inventoryManager.inventorySize)
                {
                    for (int i = 0; i < inventoryManager.inventory.Count; i++)
                    {
                        if (inventoryManager.inventory[i] == Item.NoneItem)
                        {
                            itemComponent.transform.SetParent(playerInventory.transform);
                            itemComponent.transform.localPosition = Vector2.zero;
                            itemComponent.rb.velocity = Vector2.zero;
                            itemComponent.transform.rotation = Quaternion.identity;
                            inventoryManager.inventory[i] = itemComponent;
                            itemComponent.PickedUpState();
                            return;
                        }
                    }
                }
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "door")
        {
            playerController.teleporter = null;
        }

        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            isOnPlatform = false;
            currentPlatform = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private void ReloadSceneOnDie() 
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    public void OnDeathAnimationComplete()
    {
        gameOverScreen.SetActive(true);
        
    }

    private void HandelItemToturial(Item item)
    {
        if (!isItemToturialComplete && (item is Pinecone || item is Stick || item is ResinBomb))
        {
            tutText.gameObject.SetActive(true);
            tutText.text = "you have picked up a weapon. right click = activate. left click = use";
            isItemToturialComplete = true;
            Invoke("DisableText", 3f);
        }
        else if (!isHealTutorialComplete && (item is Honeycomb || item is InsectWax))
        {
            tutText.gameObject.SetActive(true);
            tutText.text = "you can use wax to heal. left click to instantly use";
            isHealTutorialComplete = true;
            Invoke("DisableText", 3f);
        }
    }

    private void DisableText()
    {
        tutText.gameObject.SetActive(false);
    }


}
