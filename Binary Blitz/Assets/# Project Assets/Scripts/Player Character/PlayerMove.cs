using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // Show debug tools like gizmos
    public bool debugMode;

    [Header("Player State")]
    public State playerState = State.grounded;
    public enum State { grounded, airborn, clambering, groundSliding, wallSlidingRight, wallSlidingLeft };

    [Header("Movement Traits")]
    public float moveForce;
    public float clamberReach;
    public float jumpForceInitial;
    public float jumpForceSustained;
    public Vector2 wallJumpVelocity;
    public float groundedDrag = 2.5f;
    public float airbornDrag = 0;
    public float minTimeUntilAdditionalJumpForces = 0.1f;
    public float jumpTime = 0.25f;
    private float airtime = 0f;
    public float extraGravity;
    public float airbornManeuverability;

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

    // Misc. public variables
    [HideInInspector]
    public float xVel;

    // Misc. variables variables
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~`
    // Movement variables
    private bool canJump = true;
    private bool canStand = true;
    private bool groundSliding = false;

    // Physics
    private Rigidbody2D rigBod;
    private CapsuleCollider2D capsuleCollider;
    private float maxAirbornOrSlidingVel = 7.6f;
    // Collider stuff
    private Vector2 standingColSize = new Vector2(0.75f, 1.85f);
    private Vector2 standingColOffset = new Vector2(0, -0.075f);
    private Vector2 slidingColSize = new Vector2(0.75f, 0.95f);
    private Vector2 slidingColOffset = new Vector2(0, -0.525f);
    // Clamber stuff
    private Vector2 clamberTarget = Vector2.zero;

    // Input variables
    private float x = 0;
    private float y = 0;
    private float j = 0;
    private float c = 0;

    // Start is called before the first frame update
    void Start()
    {
        rigBod = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Set public variables
        xVel = rigBod.velocity.x;

        // Set private variables
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        j = Input.GetAxis("Jump");
        c = Input.GetAxis("Clamber");

        // Update player state before performing movement actions
        UpdateState();
    }

    // Apply movement physics
    private void FixedUpdate()
    {
        if (playerState == State.grounded)
        {
            // Move force
            rigBod.AddForce(Vector2.right * x * moveForce * Time.fixedDeltaTime, ForceMode2D.Force);

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
                airtime += Time.fixedDeltaTime;

                if (airtime >= minTimeUntilAdditionalJumpForces)
                {
                    // Sustain the jump force when button is held
                    if (j > 0)
                        rigBod.AddForce(Vector3.up * jumpForceSustained * Time.fixedDeltaTime, ForceMode2D.Force);
                    // Start fast-falling once jump button is released
                    else
                        airtime = jumpTime;

                    // Airborn movement
                    float xVel = Mathf.Abs(rigBod.velocity.x);
                    if (xVel < maxAirbornOrSlidingVel)
                        rigBod.AddForce(Vector3.right * x * airbornManeuverability * Time.fixedDeltaTime, ForceMode2D.Force);
                }
            }
            // Apply extra gravity to make the player fall quicker
            else
                rigBod.AddForce(Vector3.up * -extraGravity * Time.fixedDeltaTime, ForceMode2D.Force);

            rigBod.AddForce(Vector3.right * x);
        }
        else if (playerState == State.groundSliding)
        {
            // Airborn movement
            float xVel = Mathf.Abs(rigBod.velocity.x);
            if (xVel < maxAirbornOrSlidingVel)
                rigBod.AddForce(Vector3.right * x * airbornManeuverability * Time.fixedDeltaTime, ForceMode2D.Force);

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
                WalljumpToTheLeft();
            // Wallslide (add extra gravity)
            else
                rigBod.AddForce(Vector2.up * -extraGravity * Time.fixedDeltaTime, ForceMode2D.Force);
        }
        else if (playerState == State.wallSlidingLeft)
        {
            // Walljump
            if (j > 0 && canJump)
                WalljumpToTheRight();
            // Wallslide (add extra gravity)
            else
                rigBod.AddForce(Vector2.up * -extraGravity * Time.fixedDeltaTime, ForceMode2D.Force);
        }
        else if (playerState == State.clambering)
        {

        }
        else
            Debug.LogWarning("No valid state selected for player!");
    }

    // State updater
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Updates state based on wall checks and inputs (horizontal, vertical, jump)
    private void UpdateState()
    {
        // Cannot change state while on cooldown
        if (canChangeState)
        {
            // On the ground ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
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
                // Wallslide right ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                if (CheckArea(wallCheckRA, wallCheckRB))
                {
                    // Start clamber
                    if (c > 0)
                    {
                        var clamberCheck = CanClamber(ClamberDirection.right);

                        if (clamberCheck.canClamber)
                        {
                            float xTargetPos = clamberCheck.wallPos.x + (standingColSize.x / 2f);
                            float yTargetPos = clamberCheck.floorPos.y + (standingColSize.y / 2f);
                            clamberTarget = new Vector2(xTargetPos, yTargetPos);

                            return;
                        }
                    }

                    // Start wallslide
                    if ((x > 0 || rigBod.velocity.x > 0.1f) || (playerState == State.wallSlidingRight && x! < 0))
                        playerState = State.wallSlidingRight;
                    else
                        ChangeState(State.airborn);

                    // Walljump
                    if (j > 0 && canJump)
                    {
                        airtime = 0;
                        WalljumpToTheLeft();
                    }
                }
                // Wallslide left ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                else if (CheckArea(wallCheckLA, wallCheckLB))
                {
                    // Start clamber
                    if (c > 0)
                    {
                        var clamberCheck = CanClamber(ClamberDirection.left);

                        if (clamberCheck.canClamber)
                        {
                            float xTargetPos = clamberCheck.wallPos.x - (standingColSize.x / 2f);
                            float yTargetPos = clamberCheck.floorPos.y + (standingColSize.y / 2f);
                            clamberTarget = new Vector2(xTargetPos, yTargetPos);

                            return;
                        }
                    }

                    // Start wallslide
                    if ((x < 0 || rigBod.velocity.x < -0.1f) || (playerState == State.wallSlidingLeft && x! > 0))
                        playerState = State.wallSlidingLeft;
                    else
                        ChangeState(State.airborn);

                    if (j > 0 && canJump)
                    {
                        airtime = 0;
                        WalljumpToTheRight();
                    }
                }
                // Airborn (no wallslide) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                else
                    playerState = State.airborn;

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

        // Set collider size and offset based on current state
        if (playerState == State.groundSliding)
        {
            // Start groundslide
            if (!groundSliding)
            {
                capsuleCollider.size = slidingColSize;
                capsuleCollider.offset = slidingColOffset;

                groundSliding = true;
            }
        }
        else
        {
            // Stop groundslide
            if (groundSliding)
            {
                capsuleCollider.size = standingColSize;
                capsuleCollider.offset = standingColOffset;

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

    // Walljump functions
    private void WalljumpToTheLeft()
    {
        // Add jump force                    Reverse jump direction
        rigBod.velocity = wallJumpVelocity * new Vector2(-1, 1);

        // Change state
        ChangeState(State.airborn);
        canJump = false;
    }

    private void WalljumpToTheRight()
    {
        // Add jump force
        rigBod.velocity = wallJumpVelocity;

        // Change state
        ChangeState(State.airborn);
        canJump = false;
    }

    // Clamber functions / class
    private void StartClamber()
    {
        rigBod.gravityScale = 0;
        capsuleCollider.size = slidingColSize;
        capsuleCollider.offset = Vector2.zero;
    }

    private void StopClamber()
    {
        rigBod.gravityScale = 1;
        capsuleCollider.size = standingColSize;
        capsuleCollider.offset = standingColOffset;
    }

    private enum ClamberDirection { right, left };
    private ClamberCheck CanClamber(ClamberDirection direction)
    {
        var clamberPacket = new ClamberCheck();

        if (direction == ClamberDirection.right)
        {

        }
        else // direction == ClamberDirection.left
        {

        }        

        return clamberPacket;
    }

    public class ClamberCheck
    {
        public bool canClamber = false;
        public Vector2 wallPos = Vector2.zero;
        public Vector2 floorPos = Vector2.zero;
    }
}
