using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public bool debugMode;

    [Header("Movement Traits")]
    public float moveSpeed;
    public float jumpSpeed;

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

    public enum State { grounded, airborn, groundSliding, wallSlidingRight, wallSlidingLeft };
    public State playerState = State.grounded;

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
            float yVel = rigBod.velocity.y;
            rigBod.velocity = new Vector2(x * moveSpeed, yVel);
        }
        else if (playerState == State.airborn)
        {

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
