using UnityEngine;

public class PlatformController : MonoBehaviour
{
    // Variables to hold references to PlayerController and PlatformEffector2D components
    private PlayerController player;
    private PlatformEffector2D platformEffector2D;
    
    // Variables to adjust settings of this platform
    public float waitTime;
    public float distanceToPlayer;
    
    // Variables to check the state of the player's input and movement
    public bool playerMovingDown;
    public float playerVerticalInput;
    public float playerJumpInput;

    ///////////////////

    void Start()
    {
        // Find the PlayerController component in the scene
        player = PlayerController.FindObjectOfType<PlayerController>();
        
        // Set the tag of this game object to "Ground"
        this.gameObject.tag = "Ground";
        
        // Add a BoxCollider2D component to this game object and set its size and offset
        this.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.0f, 0.3f);
        this.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0.0f, 0.35f);
        this.gameObject.GetComponent<BoxCollider2D>().usedByEffector = true;
        
        // Add a PlatformEffector2D component to this game object and set its surfaceArc
        platformEffector2D = this.gameObject.AddComponent<PlatformEffector2D>();
        platformEffector2D.surfaceArc = 90f;
        
        // Set the initial value of waitTime and playerMovingDown
        waitTime = 0.1f;
        playerMovingDown = false;
    }

    void Update()
    {
        // Get the player's input for debugging purposes
        playerVerticalInput = player.vertical;
        playerJumpInput = player.jump;

        // Calculate the distance between this platform and the player's feet
        distanceToPlayer = Vector2.Distance(this.transform.position, (player.transform.position + Vector3.down));

        // IF the player is currently moving down and the wait time hasn't elapsed yet
        if (playerMovingDown && waitTime > 0f)
        {
            // Countdown the wait time
            waitTime -= Time.fixedDeltaTime;
        }
        // ELSE The player is no longer moving down or the wait time has elapsed
        else
        {
            // Set playerMovingDown to FALSE
            playerMovingDown = false;
            // Reset the wait time counter so the player can fall through the platform again
            waitTime = 0.1f;
        }

        // IF the player is trying to move down
        if (player.jump < -0.1f || player.vertical < -0.1f)
        {
            // IF the wait time hasn't elasped yet AND the Player is close enough to the platform
            if (waitTime > 0f && distanceToPlayer < 0.8f)
            {
                // Flip the PlatformEffector Component's Rotational Offset so the player falls through the platform
                platformEffector2D.rotationalOffset = 180f;
                // Set playerMovingDown to TRUE
                playerMovingDown = true;
            }

            // If the wait time has elapsed
            if (waitTime <= 0)
            {
                // Set playerMovingDown to FALSE
                playerMovingDown = false;
                // Reset the PlatformEffector Component's Rotational Offset so the player recognizes the platform as ground again
                platformEffector2D.rotationalOffset = 0f;
            }
        }

        // If the player is trying to jump
        if (player.jump > 0.1f || player.vertical > 0.1f)
        {
            // Reset the PlatformEffector Component's Rotational Offset so the player recognizes the platform as ground again
            platformEffector2D.rotationalOffset = 0f;
        }
        // If the player is not trying to jump or move down
        if (player.jump == 0f && player.vertical == 0f)
        {
            if (playerMovingDown == false)
            {
                // Reset the PlatformEffector Component's Rotational Offset so the player recognizes the platform as ground again
                platformEffector2D.rotationalOffset = 0f;
            }
        }
    }
}