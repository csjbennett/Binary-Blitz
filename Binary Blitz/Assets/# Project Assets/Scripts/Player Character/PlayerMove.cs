using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMove : MonoBehaviour
{
    // Show debug tools like gizmos
    public bool debugMode;

    [Header("Player States")]
    public State playerState = State.grounded;
    public enum State { grounded, airborn, clambering, groundSliding, wallSlidingRight, wallSlidingLeft };

    public Direction playerDirection = Direction.right;
    public enum Direction { right, left };

    [Space(10)][Header("Movement Traits")]
    public float moveForce;
    public float moveForceMod = 1f;
    [Space()]
    public float clamberReach;
    public float clamberSpeed;
    public float clamberSpeedMod = 1f;
    [Space()]
    public float jumpForceInitial;
    public float jumpForceSustained;
    public float jumpForceMod = 1f;
    public Vector2 wallJumpVelocity;
    [Space()]
    public float groundedDrag = 2.5f;
    public float airbornDrag = 0;
    public float minTimeUntilAdditionalJumpForces = 0.1f;
    public float jumpTime = 0.25f;
    private float airtime = 0f;
    public float extraGravity;
    public float airbornManeuverability;
    public float airbornManeuverabilityMod = 1f;

    [Space(10)][Header("Ground Check Layermask")]
    public LayerMask groundAndWallCheckLayers;

    // Prevents state from changing (prevents double jumps/glitches)
    public float stateChangeCooldown = 0.1f;
    private bool canChangeState = true;
    private bool canChangeDirection = true;

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

    [Space(10)]
    [Header("Player Pivot")]
    public Transform playerPivot;
    public float playerPivotGroundslideYOffset;

    // Misc. public variables
    [HideInInspector]
    public float xVel;

    // Misc. variables
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Movement variables
    private bool canJump = true;
    private bool canStand = true;
    private bool groundSliding = false;

    // Animation controller
    private PlayerAnimations playerAnimator;

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

    // State change event
    [HideInInspector]
    public UnityEvent onStateChange;

    // Initialize components
    void Start()
    {
        rigBod = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        playerAnimator = GetComponent<PlayerAnimations>();
    }

    // Handle inputs
    void Update()
    {
        // Set public variables
        xVel = rigBod.linearVelocity.x;

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
            rigBod.AddForce(Vector2.right * x * moveForce * moveForceMod * Time.fixedDeltaTime, ForceMode2D.Force);

            // Jump
            if (j > 0 && canJump)
            {
                // Jump force
                rigBod.AddForce(Vector3.up * jumpForceInitial * jumpForceMod, ForceMode2D.Impulse);

                // Change state
                ChangeState(State.airborn);
                canJump = false;
            }

            // Direction change
            if (rigBod.linearVelocity.x < 0)
                playerDirection = Direction.left;
            if (rigBod.linearVelocity.x > 0)
                playerDirection = Direction.right;
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
                        rigBod.AddForce(Vector3.up * jumpForceSustained * jumpForceMod * Time.fixedDeltaTime, ForceMode2D.Force);
                    // Start fast-falling once jump button is released
                    else
                        airtime = jumpTime;

                    // Airborn movement
                    float xVel = Mathf.Abs(rigBod.linearVelocity.x);
                    if (xVel < maxAirbornOrSlidingVel)
                        rigBod.AddForce(Vector3.right * x * airbornManeuverability * Time.fixedDeltaTime, ForceMode2D.Force);
                }
            }
            // Apply extra gravity to make the player fall quicker
            else
                rigBod.AddForce(Vector3.up * -extraGravity * Time.fixedDeltaTime, ForceMode2D.Force);

            rigBod.AddForce(Vector3.right * x);

            // Change player direction based on velocity
            if (rigBod.linearVelocity.x < 0)
                playerDirection = Direction.left;
            else if (rigBod.linearVelocity.x > 0)
                playerDirection = Direction.right;
        }
        else if (playerState == State.groundSliding)
        {
            // Airborn movement
            float xVel = Mathf.Abs(rigBod.linearVelocity.x);
            if (xVel < maxAirbornOrSlidingVel)
                rigBod.AddForce(Vector3.right * x * airbornManeuverability * Time.fixedDeltaTime, ForceMode2D.Force);

            if (j > 0 && canJump && canStand)
            {
                // Jump force
                rigBod.AddForce(Vector3.up * jumpForceInitial * jumpForceMod, ForceMode2D.Impulse);

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
            float xDist = Mathf.Abs(transform.position.x - clamberTarget.x);
            float yDist = Mathf.Abs(transform.position.y - clamberTarget.y);

            // Match clamberTarget's y position
            if (yDist > 0.5f && xDist < 0.8f)
            {
                rigBod.linearVelocity = new Vector2(0, clamberTarget.y - transform.position.y).normalized * clamberSpeed * clamberSpeedMod;
            }
            // Match clamberTarget's x position
            else if (xDist > 0.125f)
            {
                rigBod.linearVelocity = new Vector2(clamberTarget.x - transform.position.x, clamberTarget.y - transform.position.y).normalized * clamberSpeed * clamberSpeedMod;
            }
            else
                StopClamber();
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
            // Ground-independent state checks ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // Start clamber, cannot begin clamber while ground sliding
            if (c > 0 && CanClamber())
            {
                StartClamber();
                return;
            }

            // On the ground ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            if (CheckAreaRelative(groundCheckA, groundCheckB))
            {
                // Grounded and walking
                if (y >= 0 && canStand)
                {
                    ChangeState(State.grounded);
                }
                // Grounded and sliding
                else
                {
                    ChangeState(State.groundSliding);

                    if (CheckAreaRelative(crouchCheckA, crouchCheckB))
                        canStand = false;
                    else
                        canStand = true;
                }
            }
            // In the air
            else
            {
                // Wallslide right ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                if (CheckAreaRelative(wallCheckRA, wallCheckRB))
                {
                    // Start wallslide
                    if (playerState != State.wallSlidingRight && ((x > 0 || rigBod.linearVelocity.x > 0.1f) || (playerState == State.wallSlidingRight && x! < 0)))
                    {
                        // Disable changing directions
                        canChangeDirection = false;
                        playerDirection = Direction.right;
                        ChangeState(State.wallSlidingRight);
                    }
                    else if (x < 0)
                    {
                        ChangeState(State.airborn);
                        return;
                    }

                    // Walljump
                    if (j > 0 && canJump)
                    {
                        airtime = 0;
                        WalljumpToTheLeft();
                    }
                }
                // Wallslide left ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                else if (CheckAreaRelative(wallCheckLA, wallCheckLB))
                {
                    // Start wallslide
                    if (playerState != State.wallSlidingLeft && ((x < 0 || rigBod.linearVelocity.x < -0.1f) || (playerState == State.wallSlidingLeft && x! > 0)))
                    {
                        // Disable changing directions
                        canChangeDirection = false;
                        playerDirection = Direction.left;
                        ChangeState(State.wallSlidingLeft);
                    }
                    else if (x > 0)
                    {
                        ChangeState(State.airborn);
                        return;
                    }

                    if (j > 0 && canJump)
                    {
                        airtime = 0;
                        WalljumpToTheRight();
                    }
                }
                // Airborn (no wallslide) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                else
                {
                    ChangeState(State.airborn);
                    canChangeDirection = true;
                }

                // Reset canStand in edge cases
                canStand = true;
            }
        }
        // Stop trying to change state if player is clambering
        else if (playerState == State.clambering)
            return;

        // Set player drag and airtime
        // Airborn
        if (playerState == State.airborn)
        {
            rigBod.linearDamping = airbornDrag;
        }
        // Grounded
        else
        {
            // Not sliding (increase friction)
            if (playerState != State.groundSliding)
            {
                rigBod.linearDamping = groundedDrag;
                airtime = 0;
            }
            // Sliding (decrease friction)
            else
            {
                rigBod.linearDamping = airbornDrag;
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
                playerPivot.localPosition = Vector2.up * playerPivotGroundslideYOffset;

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
                playerPivot.localPosition = Vector2.zero;

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
        if (playerState == newState)
            return;

        playerState = newState;
        onStateChange.Invoke();
        //Debug.Log($"player state: {newState}\ntime: {Time.time}");
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
    private bool CheckAreaRelative(Vector2 offsetA, Vector2 offsetB)
    {
        Vector2 pos = transform.position;
        return Physics2D.OverlapArea(pos + offsetA, pos + offsetB, groundAndWallCheckLayers);
    }
    private bool CheckArea(Vector2 pos, Vector2 offsetA, Vector2 offsetB)
    {
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

    // Gets the center and size of two offsets
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
        // Add jump force                                   Reverse jump direction
        rigBod.linearVelocity = wallJumpVelocity * jumpForceMod * new Vector2(-1, 1);
        // Flip legs
        playerAnimator.ForceLegDirection(PlayerAnimations.LegDirection.left);
        // Change state
        ChangeState(State.airborn);
        canJump = false;
    }

    private void WalljumpToTheRight()
    {
        // Add jump force
        rigBod.linearVelocity = wallJumpVelocity * jumpForceMod;
        // Flip legs
        playerAnimator.ForceLegDirection(PlayerAnimations.LegDirection.right);
        // Change state
        ChangeState(State.airborn);
        canJump = false;
    }

    // Clamber functions / class
    private void StartClamber()
    {
        rigBod.gravityScale = 0;
        rigBod.linearDamping = groundedDrag;
        capsuleCollider.size = slidingColSize;
        capsuleCollider.offset = Vector2.zero;
        // State must be manually changed here to prevent state changes from occurring while clambering
        playerState = State.clambering;
        canChangeState = false;
        canChangeDirection = false;
        onStateChange.Invoke();
        StopAllCoroutines();
    }
    private void StopClamber()
    {
        rigBod.gravityScale = 1;
        rigBod.linearDamping = groundedDrag;
        capsuleCollider.size = standingColSize;
        capsuleCollider.offset = standingColOffset;
        clamberTarget = Vector2.zero;
        canChangeState = true;
        canChangeDirection = true;
        onStateChange.Invoke();
        StopAllCoroutines();
    }
    private bool CanClamber()
    {
        Vector2 wallPos;
        Vector2 floorCheckPos;
        RaycastHit2D wallHit;
        RaycastHit2D floorHit;

        // Check for clamber-able walls
        wallHit = GetWallHit();

        if (wallHit.collider != null)
        {
            wallPos = wallHit.point;

            if (playerDirection == Direction.right)
                floorCheckPos = wallPos + new Vector2(0.5f, clamberReach);
            else
                floorCheckPos = wallPos + new Vector2(-0.5f, clamberReach);
        }
        else
            return false;

        // Check for a floor where clamber is being attempted
        floorHit = Physics2D.Raycast(floorCheckPos, Vector2.down * 2f, 2.5f, groundAndWallCheckLayers);
        if (floorHit.collider != null)
        {
            // Get position of the corner where clamber is being attempted
            float wallX = wallHit.point.x;
            float floorY = floorHit.point.y;
            Vector2 clamberTarget = new Vector2(wallX, floorY);

            // Get the offset from the corner to check if the area is open
            Vector2 clamberTargetOffset = standingColSize / 2f;
            if (playerDirection == Direction.left)
                clamberTargetOffset.x = -clamberTargetOffset.x;

            // Add offset to clamberTarget
            clamberTarget += clamberTargetOffset;

            // Check if clamberTarget is open
            Vector2 offset = Vector2.up * 0.125f;
            if (!CheckArea(clamberTarget, (standingColSize / 2f) + standingColOffset + offset, (-standingColSize / 2f) + standingColOffset + offset))
            {
                // Check if there is anything blocking you on the clamber's path
                if (PathIsClamberable(transform.position, clamberTarget))
                {
                    this.clamberTarget = clamberTarget;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }
    // Height offsets used for wall checks during a clamber check
    private Vector3[] clamberHeightOffsets = new Vector3[4]
    {
        new Vector3(0, 0.525f),
        new Vector3(0, 0),
        new Vector3(0, -0.525f),
        new Vector3(0, -0.875f)
    };
    // Checks at 3 different heights to see if there is a wall anywhere
    private RaycastHit2D GetWallHit()
    {
        float x;
        RaycastHit2D hit = new RaycastHit2D();

        if (playerDirection == Direction.right)
            x = 1;
        else
            x = -1;

        // Check side player is facing first
        if (CheckSideForClamber(x, out hit))
            return hit;
        // Check other side just in case
        else if (CheckSideForClamber(-x, out hit))
            return hit;

        return hit;
    }
    private bool CheckSideForClamber(float x, out RaycastHit2D hit)
    {
        for (int i = 0; i < clamberHeightOffsets.Length; i++)
        {
            hit = Physics2D.Raycast(transform.position + clamberHeightOffsets[i], new Vector2(x, 0), clamberReach, groundAndWallCheckLayers);

            if (hit.collider != null)
            {
                return true;
            }
        }

        hit = new RaycastHit2D();
        return false;
    }
    // Returns a boolean that tells you if the clamber's path is open
    private bool PathIsClamberable(Vector2 playerPos, Vector2 targetPos)
    {
        float yAmount = targetPos.y - playerPos.y;
        float xAmount = targetPos.x - playerPos.x;

        // Check vertical path to target
        if (Physics2D.RaycastAll(playerPos, Vector3.up * yAmount, yAmount, groundAndWallCheckLayers).Length == 0)
        {
            // Check horizontal path to target from vertical offset
            if (Physics2D.RaycastAll(playerPos + (Vector2.up * yAmount), Vector2.right * xAmount, xAmount, groundAndWallCheckLayers).Length == 0)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }

    // Allows other scripts to check if player can change direction
    public bool CanChangeDirection()
    { return canChangeDirection; }

    // Variable getters
    public float GetX()
    {
        return x;
    }
    public float GetY()
    {
        return y;
    }
    public float GetJ()
    {
        return j;
    }
    public float GetC()
    {
        return c;
    }
    public Vector2 GetVelocity()
    {
        return rigBod.linearVelocity;
    }
}
