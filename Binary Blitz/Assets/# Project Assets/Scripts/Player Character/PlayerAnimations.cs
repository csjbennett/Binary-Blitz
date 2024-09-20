using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private PlayerMove player;
    public Animator armAnm;
    public Animator legAnm;
    public Transform torso;
    public Transform legs;
    public float legAnmSpeedDivisor;

    private bool canWalk = true;
    private bool isWalking = false;
    private bool isRunning = false;

    private Vector3 rightFacing = new Vector3(0, 0, 0);
    private Vector3 leftFacing = new Vector3(0, 180, 0);

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerMove>();
        player.onStateChange.AddListener(UpdateAnimatorState);
    }

    // Update is called once per frame
    void Update()
    {
        // Store player's velocity
        var vel = player.GetVelocity();

        if (canWalk)
        {
            float xAbs = Mathf.Abs(vel.x);
            if (xAbs == 0)
                legAnm.Play("Idle");
            else if (xAbs > 0f && xAbs <= 5f)
                legAnm.Play("Walk");
            else if (xAbs > 5f)
                legAnm.Play("Run");

            legAnm.speed = xAbs / legAnmSpeedDivisor;
        }

        if (player.playerState != PlayerMove.State.airborn)
        {
            if (vel.x > 0)
                legs.eulerAngles = rightFacing;
            if (vel.x < 0)
                legs.eulerAngles = leftFacing;
        }
    }

    private void UpdateAnimatorState()
    {
        // Enable/disable player walking animation system
        if (player.playerState == PlayerMove.State.grounded)
            canWalk = true;
        else
        {
            canWalk = false;
            isWalking = false;
            isRunning = false;
        }
        
        if (player.playerState == PlayerMove.State.airborn)
        {
            legAnm.Play("Jump");
        }
        else if (player.playerState == PlayerMove.State.groundSliding)
        {
            legAnm.Play("Slide");
        }
        else if (player.playerState == PlayerMove.State.wallSlidingLeft)
        {
            legAnm.Play("Jump");
        }
        else if (player.playerState == PlayerMove.State.wallSlidingRight)
        {
            legAnm.Play("Jump");
        }
    }

    public enum LegDirection { right, left };
    public void ForceLegDirection(LegDirection direction)
    {
        if (direction == LegDirection.right)
            legs.eulerAngles = rightFacing;
        else
            legs.eulerAngles = leftFacing;
    }
}
