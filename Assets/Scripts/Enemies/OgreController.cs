using UnityEngine;

public class OgreController : MonoBehaviour
{
    [Header("Status Display")]
    // Public variables to display information about the Ogre's status in the Unity Inspector
    public int currentHealth;                   // Current health of the Ogre
    public FacingDirection facingDirection;     // Enum to keep track of the Ogre's facing direction
    public enum FacingDirection                 // Enum definition for the Ogre's facing direction
    {
        Left, Right
    }
    public bool onGround;                       // Boolean to check if the Ogre is on the ground
    public float moveSpeed;                     // Speed at which the Ogre moves
    public int currentPassiveState;             // The current passive state of the Ogre
    public float changeStateCounter;            // Time counter to change the passive state
    public bool agroToPlayerState;              // Boolean to check if the Ogre is in an aggressive state towards the player
    public bool playerWithinAttackRange;        // Boolean to check if the player is within the Ogre's attack range
    public float attackCounter;                 // Time counter to determine when the Ogre can attack
    public bool inKnockBackState;               // Boolean to check if the Ogre is in knockback state
    public float knockbackCounter;              // Time counter for the knockback state
    public bool isDead;                         // Boolean to check if the Ogre is dead
    public float destroyCounter;                // Time counter to destroy the Ogre's game object


    [Header("Enemy Customization")]
    // Public variables to customize the Ogre's attributes in the Unity Inspector
    public int maxHealth = 100;                 // Maximum health of the Ogre
    public int damageDealt = 10;                // Damage dealt by the Ogre's attack
    public float walkSpeed = 2;                 // Walking speed of the Ogre
    public float agroSpeed = 3;                 // Speed of the Ogre in aggressive state
    public float jumpForce = 4;                 // Force of the Ogre's jump
    public float stateTimeMin = 1f;             // Minimum time for the passive state
    public float stateTimeMax = 5f;             // Maximum time for the passive state
    public float agroStateRange = 10f;          // Range for the Ogre to enter aggressive state towards player
    public float agroAttackRange = 2f;          // Range for the Ogre to attack the player
    public float attackDelay = 1f;              // Delay between attacks
    public float knockbackStateDelay = 0.4f;    // Delay for the knockback state

    public InvSlotClass[] droppedItems;         // Array of items that the Ogre will drop when killed

    [HideInInspector] public Vector2 movement;  // Movement vector of the Ogre


    [Header("Parents and Prefabs")]
    // References to other game objects in the scene
    private PlayerController player;            // Reference to the PlayerController
    private TerrainGeneration terrain;          // Reference to the TerrainGeneration
    private GameObject tileDropCircle;          // Reference to the tile drop circle prefab

    private Animator animator;                  // Reference to the animator component of the Ogre
    public Rigidbody2D rb2d;                    // Reference to the Rigidbody2D component of the Ogre
    public LayerMask autoJumpOnLayers;          // Layer mask to automatically jump over obstacles

    public Transform hitDetection;              // Transform of the Ogre's hit detection object
    public float hitDetectionSize;              // Size of the Ogre's hit detection object
    public LayerMask hitPlayer;                 // Layer mask to detect the Player for attacking


    [Header("Audio Clips")]
    // Public audio sources for the Ogre's audio clips
    public AudioSource sound_Idle1;             // Idle sound 1
    public AudioSource sound_Idle2;             // Idle sound 2
    public AudioSource sound_TakeDamage;        // Take damage default splat
    public AudioSource sound_TakeDamageOgre;    // Take damage ogre cry
    public AudioSource sound_DealDamage;        // Deal damage sound
    public AudioSource sound_Dead;              // Death sound

    ///////////////////

    void Start()
    {
        // Find Player and Terrain in the scene and load TileDropCircle prefab
        player = PlayerController.FindObjectOfType<PlayerController>();
        terrain = TerrainGeneration.FindObjectOfType<TerrainGeneration>();
        tileDropCircle = Resources.Load<GameObject>("TileDropCircle");

        // Set Ogre's animator and rigidbody components
        animator = this.GetComponent<Animator>();
        rb2d = this.transform.parent.GetComponent<Rigidbody2D>();

        // Set starting values for the Ogre's variables
        currentHealth = maxHealth;
        facingDirection = FacingDirection.Right;
        agroToPlayerState = false;
        isDead = false;
        destroyCounter = 0;
    }

    void Update()
    {
        // Calculate the distance between the Ogre and the Player
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Check if the Player is within aggro range or attack range and set the corresponding variables
        if (distanceToPlayer <= agroStateRange && distanceToPlayer >= agroAttackRange)
        { agroToPlayerState = true; playerWithinAttackRange = false; }
        else if (distanceToPlayer > agroStateRange)
        { agroToPlayerState = false; playerWithinAttackRange = false; }
        else if (distanceToPlayer < agroAttackRange)
        { agroToPlayerState = true; playerWithinAttackRange = true; }

        // Set animator parameters and call various counter methods
        animator.SetFloat("Horizontal Movement", movement.x);
        animator.SetBool("Following Player", agroToPlayerState);
        ChangeStateCounter();
        KnockbackCounter();
        DestroyCounter();

        // Debug code to draw various rays for testing
        //Debug.DrawRay(this.transform.position - (Vector3.up * 0.5f), Vector2.right * transform.localScale.x, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 1.5f), Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.up, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (Vector3.right * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.right * 2.1f * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
    }

    // This method is called every fixed frame and updates the Ogre's movement and actions
    private void FixedUpdate()
    {
        // IF the Ogre is dead
        if (isDead == true)
        {
            // Freeze the Ogre's rigidbody and disable its collider
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            this.GetComponent<CapsuleCollider2D>().enabled = false;
            return;
        }

        // IF Ogre is not in knockback state
        if (inKnockBackState == false)
        {
            // IF Ogre is not aggroed to the player and the player is not within attack range
            if (agroToPlayerState == false && playerWithinAttackRange == false)
            {
                moveSpeed = walkSpeed;

                // Depending on current passive state, call WalkingState(), IdleState(), or ChangeDirection()
                if (currentPassiveState == 0)
                {
                    WalkingState();
                }
                else if (currentPassiveState == 1)
                {
                    IdleState();
                }
                else if (currentPassiveState == 2)
                {
                    ChangeDirection();
                }

                // IF the Ogre is facing right, set its sprite facing right
                if (facingDirection == FacingDirection.Right)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
                // IF the Ogre is facing left, set its sprite facing left
                else if (facingDirection == FacingDirection.Left)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }

                // IF the Ogre is on the ground and there is no obstacle in front of it, and it is not currently jumping, make it jump
                if (GroundRaycast() && !FrontRaycast() && !FrontRaycast2() && !UpRaycast() && movement.x != 0) // AutoJump
                {
                    if (onGround)
                    {
                        movement.y = jumpForce;
                    }
                }

                // IF the Ogre is blocked by an obstacle in front or above, make it change direction
                if ((GroundRaycast() && FrontRaycast()) || FrontRaycast2() || (!DownRaycast() && !DownRaycast2()))
                {
                    ChangeDirection();
                }

                // UNUSED: Check the distance to the player's spawn position and make the Ogre change direction if the player is too close
                /*float distanceToPlayerSpawn = Vector2.Distance(transform.position, player.spawnPosition);
                if (distanceToPlayerSpawn < 10)
                {
                    ChangeDirection();
                }*/
            }

            // IF the Ogre is aggroed to the player but not within attack range
            else if (agroToPlayerState == true && playerWithinAttackRange == false)
            {
                moveSpeed = agroSpeed;

                // IF the Player is to the right of the Ogre, move the Ogre right and face it right
                if (transform.position.x + 1f < player.transform.position.x)
                {
                    facingDirection = FacingDirection.Right;
                    movement = new Vector2(moveSpeed, rb2d.velocity.y);
                    transform.localScale = new Vector3(1, 1, 1);
                }
                // ELSE IF the Player is to the left of the Ogre, move the Ogre left and face it left
                else if (transform.position.x - 1f > player.transform.position.x)
                {
                    facingDirection = FacingDirection.Left;
                    movement = new Vector2(-moveSpeed, rb2d.velocity.y);
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                // ELSE no movement
                else
                {
                    movement = new Vector2(0, rb2d.velocity.y);
                }

                // IF these ray cast are detected, autojump
                if (GroundRaycast() && !FrontRaycast() && !FrontRaycast2() && !UpRaycast())
                {
                    if (onGround)
                    {
                        movement.y = jumpForce;
                    }
                }
                if (GroundRaycast() && FrontRaycast())
                {
                    movement.x = 0;

                    if (onGround)
                    {
                        movement.y = jumpForce;
                    }
                }
            }

            // IF the Ogre is aggroed to the player and within attack range
            else if (playerWithinAttackRange == true)
            {
                // Stop Ogre's velocity stopping it near the player to attack
                movement = new Vector2(rb2d.velocity.x, rb2d.velocity.y);
                // Deal damage to the player
                DealDamage();
            }

            // Set rigidbody velocity using movement variable assigned in previous IF statements
            rb2d.velocity = movement;
        }
    }

    // Handles the countdown for the Ogre's passive state change
    public void ChangeStateCounter()
    {
        if (changeStateCounter <= 0)
        {
            // Change the current passive state and play the corresponding sound effect
            currentPassiveState = ChangeState(3);
            if (currentPassiveState == 0)
                sound_Idle1.Play();
            else if (currentPassiveState == 1)
                sound_Idle2.Play();
            changeStateCounter = UnityEngine.Random.Range(stateTimeMin, stateTimeMax);
        }
        else
        {
            // Countdown
            changeStateCounter -= Time.fixedDeltaTime;
        }
    }

    // Change the Ogre's passive state at random intervals
    public int ChangeState(int count)
    {
        // Randomly select a new state for the Ogre to enter
        int selectedState = UnityEngine.Random.Range(0, count);
        return selectedState;
    }

    // Make the Ogre walk in its current facing direction
    public void WalkingState()
    {
        if (facingDirection == FacingDirection.Right)
        {
            movement = new Vector2(moveSpeed, rb2d.velocity.y);
        }
        else if (facingDirection == FacingDirection.Left)
        {
            movement = new Vector2(-moveSpeed, rb2d.velocity.y);
        }
    }

    // Make the Ogre idle (stop moving)
    public void IdleState()
    {
        movement = new Vector2(0, rb2d.velocity.y);
    }

    // Change the Ogre's facing direction
    public void ChangeDirection()
    {
        if (facingDirection == FacingDirection.Right)
        { facingDirection = FacingDirection.Left; }
        else if (facingDirection == FacingDirection.Left)
        { facingDirection = FacingDirection.Right; }
        currentPassiveState = ChangeState(2);
    }

    // Deal damage to the player within attack range
    public void DealDamage()
    {
        if (AttackDelay())
        {
            // Play attack animation and sound
            animator.SetTrigger("Attack");
            sound_DealDamage.Play();
            // Detect Player
            Collider2D[] detectPlayer = Physics2D.OverlapCircleAll(hitDetection.position, hitDetectionSize, hitPlayer);
            // Apply damage to Player and reset the counter for delay between attacks
            for (int i = 0; i < detectPlayer.Length; i++)
            {
                if (detectPlayer[i].tag == "Player")
                {
                    detectPlayer[i].GetComponent<PlayerController>().TakeDamage(damageDealt, this.transform.parent.position);
                    Debug.Log("Hit player for " + damageDealt);
                }
            }
            attackCounter = attackDelay;
        }
    }

    // Draw a gizmo sphere in the Editor around the Ogre's hit detection area for testing
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitDetection.position, hitDetectionSize);
    }

    // Handle damage taken by the Ogre and apply knockback
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
            sound_TakeDamageOgre.Play();

            // Apply knockback and reset knockback counter
            Vector2 knockbackDirection = (transform.position - hitFromPosition).normalized;
            Vector2 knockbackX = new Vector2(knockbackDirection.x, 0f);

            knockbackCounter = knockbackStateDelay;
                //rb2d.AddForce(knockbackDirection * 10, ForceMode2D.Impulse);
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
                sound_Dead.Play();
                rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
                this.GetComponent<CapsuleCollider2D>().enabled = false;
                isDead = true;
            }
        }
    }

    // Apply a delay between the Ogre's attacks
    private bool AttackDelay()
    {
        if (attackCounter <= 0)
        {
            return true;
        }
        else
        {
            attackCounter -= Time.fixedDeltaTime;
            return false;
        }
    }

    // Apply a delay to the Ogre's knockback state
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

    // Destroy the Ogre after a set amount of time has passed since its death
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

    // Instantiate dropped items when the Ogre is destroyed
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
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position - (Vector3.up * 0.5f), Vector2.right * transform.localScale.x, 1.5f, autoJumpOnLayers);
        return hit;
    }
    public bool FrontRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), Vector2.right * transform.localScale.x, 1.5f, autoJumpOnLayers);
        return hit;
    }
    public bool FrontRaycast2()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 1.5f), Vector2.right * transform.localScale.x, 1.5f, autoJumpOnLayers);
        return hit;
    }
    public bool UpRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), Vector2.up, 2f, autoJumpOnLayers);
        return hit;
    }
    public bool DownRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.right * transform.localScale.x) - Vector3.up, -Vector2.up, 3f, autoJumpOnLayers);
        return hit;
    }
    public bool DownRaycast2()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.right * 2.1f * transform.localScale.x) - Vector3.up, -Vector2.up, 3f, autoJumpOnLayers);
        return hit;
    }

    // Collision triggers checks to determine if the Ogre is on ground
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