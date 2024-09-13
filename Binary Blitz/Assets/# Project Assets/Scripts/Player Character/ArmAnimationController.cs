using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmAnimationController : MonoBehaviour
{
    public PlayerMove player;
    public Animator anm;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (player.playerState == PlayerMove.State.grounded)
        {

        }
        else if (player.playerState == PlayerMove.State.airborn)
        {

        }
        else if (player.playerState == PlayerMove.State.clambering)
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
