using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private PlayerMove player;
    public Animator armAnm;
    public Animator legAnm;

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
        if (canWalk)
        {

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

        }
        else if (player.playerState == PlayerMove.State.groundSliding)
        {

        }
        else if (player.playerState == PlayerMove.State.wallSlidingLeft)
        {

        }
        else if (player.playerState == PlayerMove.State.wallSlidingRight)
        {

        }
    }
}
