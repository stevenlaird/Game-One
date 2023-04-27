using UnityEngine;

public class WalkingBarrelController : MonoBehaviour
{
    [Header("Status Display")]
    // Public variables to display information about the Barrel's status in the Unity Inspector
    public int currentHealth;                       // Current health of the barrel
    public FacingDirection facingDirection;         // Direction the Barrel is facing
    public enum FacingDirection
    {
        Left, Right
    }
    public bool onGround;                           // Whether the Barrel is currently on the ground
    public bool runningFromPlayerState;             // Whether the Barrel is currently running from the player
    public bool inKnockBackState;                   // Whether the Barrel is currently in the knockback state
    public float knockbackCounter;                  // Counter for knockback state delay
    public bool isDead;                             // Whether the Barrel is dead
    public float destroyCounter;                    // Counter for time since Barrel's death


    [Header("Enemy Customization")]
    // Public variables to customize the Barrel's attributes in the Unity Inspector
    public int maxHealth = 100;                     // Maximum health of the Barrel
    public float runSpeed = 3.5f;                   // Speed at which the Barrel runs
    public float jumpForce = 4;                     // Force with which the Barrel jumps
    public float runningFromPlayerRange = 10f;      // Range within which the Barrel runs from the Player
    public float knockbackStateDelay = 0.4f;        // Delay before Barrel can move again after knockback

    public InvSlotClass[] droppedItems;             // Items dropped by the barrel when destroyed

    [HideInInspector] public Vector2 movement;      // Movement vector of the barrel


    [Header("Parents and Prefabs")]
    // References to other game objects in the scene
    private PlayerController player;                // Reference to the PlayerController
    private TerrainGeneration terrain;              // Reference to the TerrainGenerator
    private GameObject tileDropCircle;              // Prefab for the dropped items circle

    private Animator animator;                      // Reference to the animator component
    public Rigidbody2D rb2d;                        // Reference to the rigidbody component
    public LayerMask autoJumpOnLayers;              // Layer mask for auto-jump detection


    // Public audio sources for the Barrel's audio clips
    [Header("Audio Clips")]
    public AudioSource sound_TakeDamage;            // Take damage default splat
    public AudioSource sound_TakeDamageBarrel;      // Take damage Barrel cry

    ///////////////////

    void Start()
    {
        // Find Player and Terrain in the scene and load TileDropCircle prefab
        player = PlayerController.FindObjectOfType<PlayerController>();
        terrain = TerrainGeneration.FindObjectOfType<TerrainGeneration>();
        tileDropCircle = Resources.Load<GameObject>("TileDropCircle");

        // Set Barrel's animator and rigidbody components
        animator = this.GetComponent<Animator>();
        rb2d = this.transform.parent.GetComponent<Rigidbody2D>();

        // Set starting values for the Barrel's variables
        currentHealth = maxHealth;
        facingDirection = FacingDirection.Left;
        runningFromPlayerState = false;
        isDead = false;
        destroyCounter = 0;
    }

    void Update()
    {
        // Calculate the distance between the Barrel and the Player
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Check if the Player is within range and set the corresponding variable
        if (distanceToPlayer <= runningFromPlayerRange)
            { runningFromPlayerState = true; }
        else if (distanceToPlayer > runningFromPlayerRange)
            { runningFromPlayerState = false; }

        // IF Barrel is not dead
        if (isDead == false)
            // Enable running animation when the Barrel is running from the Player
            { animator.SetBool("Running", runningFromPlayerState); }

        // Call Counters
        KnockbackCounter();
        DestroyCounter();

        // Debug code to draw various rays for testing
        //Debug.DrawRay(this.transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 1.5f), -Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.up, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (-Vector3.right * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (-Vector3.right * 2.1f * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
    }

    // This method is called every fixed frame and updates the Barrel's movement
    private void FixedUpdate()
    {
        // IF the Barrel is dead
        if (isDead == true)
        {
            // Freeze the Barrel's rigidbody and disable its collider
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            this.GetComponent<CapsuleCollider2D>().enabled = false;
            return;
        }
        // IF the Barrel is not running from the Player, stop movement
        if (runningFromPlayerState == false)
        {
            movement = new Vector2(0, rb2d.velocity.y);
        }

        // IF the Barrel is running from the Player and not in the Knockback state
        if (runningFromPlayerState == true && inKnockBackState == false)
        {
            // IF the Player is to the left of the Ogre, move the Ogre left and face it left
            if (transform.position.x + 0.1f < player.transform.position.x)
            {
                facingDirection = FacingDirection.Left;
                movement = new Vector2(-runSpeed, rb2d.velocity.y);
                transform.localScale = new Vector3(1, 1, 1); //change sprite left
            }
            // ELSE IF the Player is to the right of the Barrel, move the Barrel right and face it right
            else if (transform.position.x - 0.1f > player.transform.position.x)
            {
                facingDirection = FacingDirection.Right;
                movement = new Vector2(runSpeed, rb2d.velocity.y);
                transform.localScale = new Vector3(-1, 1, 1); //change sprite right
            }
            // ELSE no movement
            else
            {
                movement = new Vector2(0, rb2d.velocity.y);
            }

            // IF the Ogre is on the ground and there is no obstacle in front of it, and it is not currently jumping, make it jump
            if (GroundRaycast() && !FrontRaycast() && !FrontRaycast2() && !UpRaycast()) // AutoJump
            {
                if (onGround)
                {
                    movement.y = jumpForce;
                }
            }
            if (GroundRaycast() && FrontRaycast()) // AutoJump
            {
                movement.x = 0;

                if (onGround)
                {
                    movement.y = jumpForce;
                }
            }
        }

        // Set rigidbody velocity using movement variable assigned in previous IF statements
        rb2d.velocity = movement;
    }

    // Handle damage taken by the Barrel and apply knockback
    public void TakeDamage(int damageTaken, Vector3 hitFromPosition)
    {
        // Play default hit splat sound
        sound_TakeDamage.Play();

        // IF the Ogre is not dead
        if (isDead == false)
        {
            // Subtract the damage taken from current health, play animation and sound
            currentHealth -= damageTaken;
            animator.SetTrigger("Take Damage");
            sound_TakeDamageBarrel.Play();

            // Apply knockback and reset knockback counter
            Vector2 knockbackDirection = (transform.position - hitFromPosition).normalized;
            Vector2 knockbackX = new Vector2(knockbackDirection.x, 0f);

            knockbackCounter = knockbackStateDelay;
            rb2d.AddForce(knockbackX * 10, ForceMode2D.Impulse);

            // IF player is holding an item, and its not made of wood, subtract it's durability by 1
            if (player.inventoryManager.inventoryItems[player.inventoryManager.selectedSlot].GetItem() != null)
            {
                if (!player.inventoryManager.inventoryItems[player.inventoryManager.selectedSlot].GetItem().itemName.Contains("Wood"))
                {
                    player.inventoryManager.inventoryItems[player.inventoryManager.selectedSlot].SubtractDurability(1);
                }
            }

            // IF current health hits 0, play animation and sound, freeze rigidbody, and set isDead to TRUE
            if (currentHealth <= 0)
            {
                Debug.Log(this.gameObject.name + " is Dead");
                animator.SetTrigger("Is Dead");
                rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
                this.GetComponent<CapsuleCollider2D>().enabled = false;
                isDead = true;
            }
        }
    }

    // Apply a delay to the Barrel's knockback state
    private void KnockbackCounter()
    {
        if (knockbackCounter <= 0)
        {
            inKnockBackState = false;
        }
        else
        {
            inKnockBackState = true;
            knockbackCounter -= Time.fixedDeltaTime;
        }
    }

    // Destroy the Barrel after a set amount of time has passed since its death
    public void DestroyCounter()
    {
        if (isDead == true)
        {
            destroyCounter += Time.fixedDeltaTime;

            if (destroyCounter >= 2f)
            {
                //this.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                CreateTileDrops();
                Destroy(this.transform.parent.gameObject);
            }
        }
    }

    // Instantiate dropped items when the Barrel is destroyed
    public void CreateTileDrops()
    {
        float chunkCoord = (Mathf.Round(this.transform.parent.transform.position.x / terrain.chunkSize) * terrain.chunkSize);
        chunkCoord /= terrain.chunkSize;

        for (int i = 0; i < droppedItems.Length; i++)
        {
            GameObject newTileDrop;

            newTileDrop = Instantiate(tileDropCircle, this.transform.position, Quaternion.identity);
            newTileDrop.transform.parent = terrain.worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
            newTileDrop.GetComponent<CircleCollider2D>().enabled = true;
            newTileDrop.GetComponent<SpriteRenderer>().sprite = droppedItems[i].GetItem().itemSprites[0];
            newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = droppedItems[i].GetItem().itemSprites[0];
            newTileDrop.GetComponent<TileDropController>().invSlotClass = droppedItems[i];
        }
    }

    // Raycast checks to determine if there is a ground tile, used to autojump and change directions at walls and gaps
    public bool GroundRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 1f, autoJumpOnLayers);
        return hit;
    }
    public bool FrontRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 1f, autoJumpOnLayers);
        return hit;
    }
    public bool FrontRaycast2()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 1.5f), -Vector2.right * transform.localScale.x, 1f, autoJumpOnLayers);
        return hit;
    }
    public bool UpRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), Vector2.up, 2f, autoJumpOnLayers);
        return hit;
    }
    public bool DownRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (-Vector3.right * transform.localScale.x) - Vector3.up, -Vector2.up, 3f, autoJumpOnLayers);
        return hit;
    }
    public bool DownRaycast2()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (-Vector3.right * 2.1f * transform.localScale.x) - Vector3.up, -Vector2.up, 3f, autoJumpOnLayers);
        return hit;
    }

    // Collision triggers checks to determine if the Barrel is on ground
    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Ground"))
        { onGround = true; }
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Ground"))
        { onGround = false; }
    }
}
