using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public bool debugMode;

    [Header("Movement Traits")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Physics Layermask")]
    public LayerMask groundAndWallCheckLayers;

    [Header("Ground Checks")]
    public Vector2 groundCheckA;
    public Vector2 groundCheckB;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            // Ground check
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            // Get center
            float xPos = transform.position.x + ((groundCheckA.x + groundCheckB.x) / 2f);
            float yPos = transform.position.y + ((groundCheckA.y + groundCheckB.y) / 2f);
            Vector3 center = new Vector3(xPos, yPos);
            // Get size
            float xSize = Mathf.Abs(groundCheckB.x - groundCheckA.x);
            float ySize = Mathf.Abs(groundCheckB.y - groundCheckA.y);
            Vector3 size = new Vector2(xSize, ySize);
            // Draw bounds
            Gizmos.DrawCube(center, size);

            // Wall check
        }
    }
}
