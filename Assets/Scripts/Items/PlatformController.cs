using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlatformController : MonoBehaviour
{
    private PlayerController player;
    private PlatformEffector2D platformEffector2D;
    public float waitTime;
    public bool playerMovingDown;
    public float playerVerticalInput;
    public float playerJumpInput;
    public float distanceToPlayer;

    ///////////////////

    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
        this.gameObject.tag = "Ground";
        this.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.0f, 0.3f);
        this.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0.0f, 0.35f);
        this.gameObject.GetComponent<BoxCollider2D>().usedByEffector = true;
        platformEffector2D = this.gameObject.AddComponent<PlatformEffector2D>();
        platformEffector2D.surfaceArc = 90f;
        waitTime = 0.1f;
        playerMovingDown = false;
    }

    void Update()
    {
        playerVerticalInput = player.vertical;  // Was used to display Player's Input in inspector when fine tuning this script
        playerJumpInput = player.jump;          // Was used to display Player's Input in inspector when fine tuning this script
        
        // Calculate distanceToPlayer using the Player's position. Added Vector3.Down to get a distance from the Player's Feet
        distanceToPlayer = Vector2.Distance(this.transform.position, (player.transform.position + Vector3.down));

        if (playerMovingDown && waitTime > 0f)  // IF the Player presses a key or button to MOVE DOWN AND waitTime hasn't counted down to 0
        {
            waitTime -= Time.fixedDeltaTime;        // Countdown waitTime
        }
        else                                    // Player isn't pressing a key or button to MOVE DOWN OR waitTime has counted down to 0
        {
            playerMovingDown = false;               // Player is not falling through this Platform
            waitTime = 0.1f;                        // Reset the waitTime counter so Player can fall through it again
        }

        if (player.jump < -0.1f || player.vertical < -0.1f) // IF Player presses a key or button to MOVE DOWN
        {
            if (waitTime > 0f && distanceToPlayer < 0.8f)       // IF waitTime counter is greater than 0 AND this Platform is close enough to the Player's Feet
            {
                platformEffector2D.rotationalOffset = 180f;         // PlatformEffector Component's Rotational Offset is flipped allowing Player to fall through
                playerMovingDown = true;                            // Player is falling through this Platform
            }

            if (waitTime <= 0)                                  // IF waitTime counter hits 0, or lower for buffer
            {
                playerMovingDown = false;                           // Player is no longer falling through this Platform
                platformEffector2D.rotationalOffset = 0f;           // PlatformEffector Component's Rotational Offset is reset allowing Player to recognize it as Ground again
            }

        }
        if (player.jump > 0.1f || player.vertical > 0.1f)   // IF Player presses a key or button to JUMP
        {
            platformEffector2D.rotationalOffset = 0f;           // PlatformEffector Component's Rotational Offset is reset allowing Player to recognize it as Ground
        }
        if (player.jump == 0f && player.vertical == 0f)     // If Player is not pressing a key/button to JUMP or MOVE DOWN
        {
            if (playerMovingDown == false)                      // IF Player is no longer falling through this Platform
            {
                platformEffector2D.rotationalOffset = 0f;           // PlatformEffector Component's Rotational Offset is reset allowing Player to recognize it as Ground
            }
        }
    }
}
