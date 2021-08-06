using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundState
{
    private GameObject player;
    private float width;
    private float height;
    private float length;

    private Collider2D playerCollider;

    // GroundState constructor.  Sets offsets for raycasting.
    public GroundState(GameObject playerRef)
    {
        player = playerRef;
        playerCollider = player.GetComponent<Collider2D>();
        width = playerCollider.bounds.extents.x + 0.1f;
        height = playerCollider.bounds.extents.y + 0.2f;
        length = 0.05f;
    }

    // Returns whether or not player is touching wall.
    public bool isWall()
    {
        bool left = Physics2D.Raycast(new Vector2(player.transform.position.x - width,
                                                  player.transform.position.y),
                                                  -Vector2.right, length);
        bool right = Physics2D.Raycast(new Vector2(player.transform.position.x + width,
                                                   player.transform.position.y),
                                                   Vector2.right, length);

        return left || right;
    }

    // Returns whether or not player is touching ground.
    public bool isGround()
    {
        // Directly down
        return Physics2D.Raycast(new Vector2(player.transform.position.x,
                                             player.transform.position.y - height),
                                             -Vector2.up, length);
    }

    // Returns whether or not player is touching wall or ground.
    public bool isTouching()
    {
        return isGround() || isWall();
    }

    //Returns direction of wall.
    public int wallDirection()
    {
        // If there is a wall to the left.
        if (Physics2D.Raycast(new Vector2(player.transform.position.x - width,
                                                  player.transform.position.y),
                                                  -Vector2.right, length))
            return -1;

        // If there is a wall to the right.
        if (Physics2D.Raycast(new Vector2(player.transform.position.x + width,
                                                  player.transform.position.y),
                                                  Vector2.right, length))
            return 1;

        // No wall to either side.
        return 0;
    }
}
