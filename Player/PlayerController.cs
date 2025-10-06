using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float runSpeed;
    [SerializeField] float jumpPower;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration = 1f;
    [SerializeField] float floatingCheckRadius = 2f;
    [SerializeField] float floatPower;
    [SerializeField] float jumpBufferTime = 0.2f;
    [SerializeField]private float jumpBufferCounter = 0;
    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteCounter;
    [SerializeField] float gravityScale;
    [SerializeField] float dashCoolDown;
    private bool canDash = true;
    private bool isWalkingSoundPlaying = false;
    internal float airBalloonTime = 0;

    private PlayerCollision playerCollision;
    internal bool isStunned = false;

    private bool isLeafActive => airBalloonTime > 0; 
    internal bool isFloating = false;
    internal Portal teleporter; //can be used as a bool too, if its null. think of it as isInsideTeleporter, if teleporter is null, then isInsideTeleporter is false, otherwise true
    Vector2 moveInput;
    internal Rigidbody2D myRigidbody;

    CapsuleCollider2D myCapsulecollider; //??? - when we get the sprites we might use a different type of collider so its better than we dont base code around a capsule collider i think

     private Animator playerAnimatior;
     public DashBar dashBar;
    public ParticleSystem dashParticals;
    public bool isFacingRight { get; private set; } = true;



    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();

        myRigidbody.gravityScale = gravityScale;

        myCapsulecollider = GetComponent<CapsuleCollider2D>();
        playerCollision = GetComponent<PlayerCollision>();
        playerAnimatior = GetComponent<Animator>();
        //dashBar.SetDashBar(1f); // Start full
    }


    void Update()
    {
        airBalloonTime -= Time.deltaTime;
        if (!isLeafActive && isFloating)
        {
            isFloating = false;
            myRigidbody.gravityScale = gravityScale;
        }


        Run();
        if (isFloating)
        {
            if (Physics2D.Raycast(transform.position, Vector2.down, floatingCheckRadius, LayerMask.GetMask("ground")))
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, 2f); // Apply upward force
            }
        }
        if (Keyboard.current.spaceKey.isPressed && isLeafActive)
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, floatPower);
        }
        // the jump 
        // 1) Decrement the jump buffer
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        // 2) Handle coyote time: reset if grounded, otherwise tick down
        if (playerCollision.IsGrounded())
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // 3) If we still have a buffered jump AND we have coyote time left, jump now
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            Jump();
        }

        bool grounded = playerCollision.IsGrounded();

        if (grounded)
        {
            // Check horizontal speed only if grounded
            bool isRunning = Mathf.Abs(myRigidbody.velocity.x) > 0.1f;
            playerAnimatior.SetBool("IsRunning", isRunning);
            playerAnimatior.SetBool("IsJumping", false);
        }
        else
        {
            // In the air, so no running, but jumping
            playerAnimatior.SetBool("IsRunning", false);
            playerAnimatior.SetBool("IsJumping", true);
        }

       

    }

    void OnInteract()
    {
        if(teleporter != null)
        {
            teleporter.Teleport(transform);
        }
    }

    // Handling the basic movement W,A,S,D
    void  OnMove(InputValue value) 
    {
        Vector2 input = value.Get<Vector2>();
        // If any horizontal key is pressed, use its sign (1 or -1), otherwise 0.
        float horizontalInput = input.x != 0 ? Mathf.Sign(input.x) : 0;
        moveInput = new Vector2(horizontalInput, 0f);
    }
    void Run() 
    {
        if (isStunned) return;
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.velocity.y) ;
        myRigidbody.velocity = playerVelocity;

        // Set animation parameter
        //bool isRunning = Mathf.Abs(myRigidbody.velocity.x) > 0.1f;
        //playerAnimatior.SetBool("IsRunning", isRunning);

        if (moveInput.x > 0)
            isFacingRight = true;
        else if (moveInput.x < 0)
            isFacingRight = false;

        if (playerCollision.IsGrounded() && Mathf.Abs(moveInput.x) > 0)
        {
            if (!isWalkingSoundPlaying)
            {
                SoundManager.Instance.PlaySound2D("Run");
                isWalkingSoundPlaying = true;
            }
            return;
        }


        if (isWalkingSoundPlaying)
        {
            SoundManager.Instance.StopSound2D();
            isWalkingSoundPlaying = false;
        }
    }
    
    void OnCancelFloat(InputValue value)
    {
        if (value.isPressed)
        {
            isFloating = false;
            airBalloonTime = 0f;
            myRigidbody.gravityScale = gravityScale; // Reset gravity when floating stops
        }
    }
    void OnAirballoon(InputValue value)
    {
        if (value.isPressed && isLeafActive) // eneter the player into a state of floating (hot airballooning)
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, floatPower);

        }
    }


    void OnDash(InputValue value) 
    {
        if (isStunned) return;
        if (value.isPressed && canDash) 
        {
            // We use StartCoroutine to make an action last for a specific time
            canDash = false;
            StartCoroutine(DashCoroutine());

           
        }
        
    }
    // DashCoroutine is the method that control the dash 
    // IEnumerator in Unity it has a special role: it’s used as the return type for coroutines. Coroutines let you write functions that can pause their execution with yield return and then resume later, allowing you to perform actions over multiple frames without freezing the game.
    private IEnumerator DashCoroutine()
    {
        // Position the particle system at the player's position (optionally with an offset)
        dashParticals.transform.position = transform.position;
        dashParticals.transform.parent = null;  // detach from player
        dashParticals.Play();
        SoundManager.Instance.PlaySound2D("Dash");
        float originalSpeed = runSpeed;
        float originalGravity = myRigidbody.gravityScale; // Store the original gravity

        runSpeed = dashSpeed;
        myRigidbody.gravityScale = 0f; // Disable gravity during dash

        // Ensure dash is only horizontal
        myRigidbody.velocity = new Vector2(isFacingRight ? dashSpeed : -dashSpeed, 0);

        dashBar.SetDashBar(0f);

        yield return new WaitForSeconds(dashDuration);

        dashParticals.Stop();
        runSpeed = originalSpeed;
        myRigidbody.gravityScale = originalGravity; // Restore gravity after dash

        StartCoroutine(DashCoolDown(dashCoolDown));
    }
    private IEnumerator DashCoolDown(float coolDown)
    {
        float elapsed = 0f;
        while (elapsed < dashCoolDown) 
        {
            elapsed += Time.deltaTime;
            dashBar.SetDashBar(elapsed / dashCoolDown);
            yield return null;
        }

        canDash = true;
    }
    // handling the jump 
    private void Jump()
    {
        if (isStunned) return;
        // Clear both counters so we don’t trigger multiple jumps at once
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;

        // Set vertical velocity to jumpPower
        myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpPower);

       
        playerAnimatior.SetBool("IsJumping", true);
       
        SoundManager.Instance.PlaySound2D("Jump");
        
    }
    // This is where we buffer the jump input
    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            // Store the jump input for a short time
            jumpBufferCounter = jumpBufferTime;

        }
    }

    
    }



