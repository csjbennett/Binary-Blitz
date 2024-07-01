using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // Show debug tools like gizmos
    public bool debugMode;

    [Header("Player State")]
    public State playerState = State.grounded;
    public enum State { grounded, airborn, groundSliding, wallSlidingRight, wallSlidingLeft };

    [Header("Movement Traits")]
    public float moveForce;
    public float slideMoveForce;
    public float sprintSpeedMultiplier;
    public float jumpForceInitial;
    public float jumpForceSustained;
    public Vector2 wallJumpVelocity;
    public float groundedDrag = 2.5f;
    public float airbornDrag = 0;
    public float jumpTime = 0.25f;
    private float airtime = 0f;
    public float extraGravity;

    [Header("Ground Check Layermask")]
    public LayerMask groundAndWallCheckLayers;
    
    // Prevents state from changing (prevents double jumps/glitches)
    public float stateChangeCooldown = 0.1f;
    private bool canChangeState = true;

    [Header("Ground Checks")]
    public Vector2 groundCheckA;
    public Vector2 groundCheckB;

    [Header("Wall Checks")]
    [Header("Right")]
    public Vector2 wallCheckRA;
    public Vector2 wallCheckRB;
    [Header("Left")]
    public Vector2 wallCheckLA;
    public Vector2 wallCheckLB;
    [Header("Crouch")]
    public Vector2 crouchCheckA;
    public Vector2 crouchCheckB;

    // Etc. private variables
    private bool canJump = true;
    private bool canStand = true;
    private bool groundSliding = false;
    private Rigidbody2D rigBod;
    private CapsuleCollider2D capsuleCollider;

    // Start is called before the first frame update
    void Start()
    {
        rigBod = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float j = Input.GetAxis("Jump");
        float s = Input.GetAxis("Sprint");

        UpdateState(x, y, j);

        if (playerState == State.grounded)
        {
            // Ground movement force
            // Sprint
            if (s != 0)
                x *= sprintSpeedMultiplier;

            // Move force
            rigBod.AddForce(Vector2.right * x * moveForce * Time.deltaTime, ForceMode2D.Force);

            // Jump
            if (j > 0 && canJump)
            {
                // Jump force
                rigBod.AddForce(Vector3.up * jumpForceInitial, ForceMode2D.Impulse);

                // Change state
                ChangeState(State.airborn);
                canJump = false;
            }
        }
        else if (playerState == State.airborn)
        {
            // Allows for short-hops and long-hops depending on how long jump key is held
            if (airtime < jumpTime)
            {
                airtime += Time.deltaTime;
                
                // Sustain the jump force when button is held
                if (j > 0)
                    rigBod.AddForce(Vector3.up * jumpForceSustained * Time.deltaTime, ForceMode2D.Force);
            }
            // Apply extra gravity to make the player fall quicker
            else
                rigBod.AddForce(Vector3.up * -extraGravity * Time.deltaTime, ForceMode2D.Force);
        }
        else if (playerState == State.groundSliding)
        {
            // Apply force
            rigBod.AddForce(Vector3.right * x * slideMoveForce);

            if (j > 0 && canJump && canStand)
            {
                // Jump force
                rigBod.AddForce(Vector3.up * jumpForceInitial, ForceMode2D.Impulse);

                // Change state
                ChangeState(State.airborn);
                canJump = false;
            }
        }
        else if (playerState == State.wallSlidingRight)
        {
            // Walljump
            if (j > 0 && canJump)
            {
                // Add jump force
                // Reverse jump direction
                Vector2 jump = wallJumpVelocity;
                jump.x *= -1;
                rigBod.velocity = jump;

                // Change state
                ChangeState(State.airborn);
                canJump = false;
            }
            // Wallslide (add extra gravity)
            else
            {
                rigBod.AddForce(Vector2.up * -extraGravity * Time.deltaTime, ForceMode2D.Force);
            }
        }
        else if (playerState == State.wallSlidingLeft)
        {
            // Walljump
            if (j > 0 && canJump)
            {
                // Add jump force
                rigBod.velocity = wallJumpVelocity;

                // Change state
                ChangeState(State.airborn);
                canJump = false;
            }
            // Wallslide (add extra gravity)
            else
            {
                rigBod.AddForce(Vector2.up * -extraGravity * Time.deltaTime, ForceMode2D.Force);
            }
        }
        else
            Debug.LogWarning("No valid state selected for player!");
    }

    // State updates
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Updates state with given inputs (horizontal, vertical, jump)
    private void UpdateState(float x, float y, float j)
    {
        // Cannot change state while on cooldown
        if (canChangeState)
        {
            // On the ground
            if (CheckArea(groundCheckA, groundCheckB))
            {
                // Grounded and walking
                if (y >= 0 && canStand)
                {
                    playerState = State.grounded;
                }
                // Grounded and sliding
                else
                {
                    playerState = State.groundSliding;

                    if (CheckArea(crouchCheckA, crouchCheckB))
                        canStand = false;
                    else
                        canStand = true;
                }
            }
            // In the air
            else
            {
                // Wallslide right                          // Stick onto wall if given input or velocity above threhsold, and sustain if already started
                if ((CheckArea(wallCheckRA, wallCheckRB) && (x > 0 || rigBod.velocity.x > 0.1f)) || (playerState == State.wallSlidingRight && x !< 0))
                {
                    playerState = State.wallSlidingRight;
                }
                // Wallslide left                                // Stick onto wall if given input or velocity above threhsold, and sustain if already started
                else if ((CheckArea(wallCheckLA, wallCheckLB) && (x < 0 || rigBod.velocity.x < -0.1f)) || (playerState == State.wallSlidingLeft && x !> 0))
                {
                    playerState = State.wallSlidingLeft;
                }
                // Airborn (no wallslide)
                else
                {
                    playerState = State.airborn;
                }

                // Reset canStand in edge cases
                canStand = true;
            }
        }

        // Set player drag and airtime
        // Airborn
        if (playerState == State.airborn)
        {
            rigBod.drag = airbornDrag;
        }
        // Grounded
        else
        {
            // Not sliding (increase friction)
            if (playerState != State.groundSliding)
            {
                rigBod.drag = groundedDrag;
                airtime = 0;
            }
            // Sliding (decrease friction)
            else
            {
                rigBod.drag = airbornDrag;
                airtime = 0;
            }
        }

        // Set player height based on current state
        if (playerState == State.groundSliding)
        {
            if (!groundSliding)
            {
                capsuleCollider.size = new Vector2(0.95f, 0.95f);
                capsuleCollider.offset = new Vector2(0, -0.525f);

                groundSliding = true;
            }
        }
        else
        {
            if (groundSliding)
            {
                capsuleCollider.size = new Vector2(1, 1.95f);
                capsuleCollider.offset = new Vector2(0, -0.025f);

                groundSliding = false;
            }
        }

        // Reset canJump
        if (j == 0)
            canJump = true;
    }

    // Forces a specific state, usually from an outside source
    public void ChangeState(State newState)
    {
        playerState = newState;

        StartStateCooldown();
    }

    // Start cooldown for changing state again
    private void StartStateCooldown()
    {
        StartCoroutine(StateCooldown());
        canChangeState = false;
    }

    // Allows changing of states when complete
    private IEnumerator StateCooldown()
    {
        yield return new WaitForSeconds(stateChangeCooldown);

        canChangeState = true;
    }

    // Does an OverlapArea check between two vectors for state changes
    private bool CheckArea(Vector2 offsetA, Vector2 offsetB)
    {
        Vector2 pos = transform.position;
        return Physics2D.OverlapArea(pos + offsetA, pos + offsetB, groundAndWallCheckLayers);
    }

    // Gizmos
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Draw bounding boxes for OverlapArea checks
    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            // Ground check
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            var groundCheck = GetCenterAndSize(groundCheckA, groundCheckB);
            Gizmos.DrawCube(groundCheck.Item1, groundCheck.Item2);

            // Right wall check
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            var rightWallCheck = GetCenterAndSize(wallCheckRA, wallCheckRB);
            Gizmos.DrawCube(rightWallCheck.Item1, rightWallCheck.Item2);

            // Left wall check
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            var leftWallCheck = GetCenterAndSize(wallCheckLA, wallCheckLB);
            Gizmos.DrawCube(leftWallCheck.Item1, leftWallCheck.Item2);

            // Crouch check
            Gizmos.color = new Color(0, 1f, 1f, 0.5f);
            var crouchCheck = GetCenterAndSize(crouchCheckA, crouchCheckB);
            Gizmos.DrawCube(crouchCheck.Item1, crouchCheck.Item2);
        }
    }

    private (Vector2, Vector2) GetCenterAndSize(Vector2 offsetA, Vector2 offsetB)
    {
        // Get center
        float xPos = transform.position.x + ((offsetA.x + offsetB.x) / 2f);
        float yPos = transform.position.y + ((offsetA.y + offsetB.y) / 2f);
        Vector3 center = new Vector3(xPos, yPos);

        // Get size
        float xSize = Mathf.Abs(offsetB.x - offsetA.x);
        float ySize = Mathf.Abs(offsetB.y - offsetA.y);
        Vector3 size = new Vector2(xSize, ySize);

        return (center, size);
    }
}
