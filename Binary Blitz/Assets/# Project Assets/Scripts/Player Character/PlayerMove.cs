using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public bool debugMode;

    [Header("Movement Traits")]
    public float moveSpeed;
    public float jumpForceInitial;
    public float jumpForceSustained;
    public float groundedDrag = 2.5f;
    public float airbornDrag = 0;
    public float jumpTime = 0.25f;
    private float airtime = 0f;
    public float extraGravity;

    [Header("Physics")]
    public Rigidbody2D rigBod;
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

    [Header("Ground Check Layermask")]
    public State playerState = State.grounded;
    public enum State { grounded, airborn, groundSliding, wallSlidingRight, wallSlidingLeft };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float j = Input.GetAxis("Jump");

        UpdateState(x, y, j);

        if (playerState == State.grounded)
        {
            // Ground movement force
            rigBod.AddForce(Vector2.right * x * moveSpeed * Time.deltaTime, ForceMode2D.Force);

            // Jump
            if (j > 0)
            {
                // Jump force
                rigBod.AddForce(Vector3.up * jumpForceInitial, ForceMode2D.Impulse);

                // Change state
                playerState = State.airborn;
                StartStateCooldown();
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

        }
        else if (playerState == State.wallSlidingRight)
        {

        }
        else if (playerState == State.wallSlidingLeft)
        {

        }
        else
            Debug.LogWarning("No valid state selected for player!");
    }

    // State updates
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Updates state with given inputs (horizontal, vertical, jump)
    private void UpdateState(float x, float y, float j)
    {
        if (canChangeState)
        {
            // On the ground
            if (CheckArea(groundCheckA, groundCheckB))
            {
                if (y >= 0)
                {
                    playerState = State.grounded;
                }
                else
                {
                    playerState = State.groundSliding;
                }
            }
            // In the air
            else
            {
                // Wallslide right
                if (CheckArea(wallCheckRA, wallCheckRB) && x > 0)
                {
                    playerState = State.wallSlidingRight;
                }
                // Wallslide left
                else if (CheckArea(wallCheckLA, wallCheckLB) && x < 0)
                {
                    playerState = State.wallSlidingLeft;
                }
                // Airborn (no wallslide)
                else
                {
                    playerState = State.airborn;
                }
            }
        }

        if (playerState == State.airborn)
        {
            rigBod.drag = airbornDrag;
        }
        else
        {
            rigBod.drag = groundedDrag;
            airtime = 0;
        }
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

    // Forces a specific state, usually from an outside source
    public void ChangeState(State newState)
    {
        playerState = newState;
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
