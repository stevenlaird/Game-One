using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingBarrelController : MonoBehaviour
{
    [Header("Status Display")]
    public int currentHealth;
    public FacingDirection facingDirection;
    public enum FacingDirection
    {
        Left, Right
    }
    public bool onGround;
    public bool runningFromPlayerState;
    public bool inKnockBackState;
    public float knockbackCounter;
    public bool isDead;
    public float destroyCounter;


    [Header("Enemy Customization")]
    public int maxHealth = 100;
    public float runSpeed = 3.5f;
    public float jumpForce = 4;
    public float runningFromPlayerRange = 10f;
    public float knockbackStateDelay = 0.4f;

    public InvSlotClass[] droppedItems;

    [HideInInspector] public Vector2 movement;


    [Header("Parents and Prefabs")]
    private PlayerController player;
    private TerrainGeneration terrain;
    private GameObject tileDropCircle;

    private Animator animator;
    public Rigidbody2D rb2d;
    public LayerMask autoJumpOnLayers;


    [Header("Audio Clips")]
    public AudioSource sound_TakeDamage;
    public AudioSource sound_TakeDamageBarrel;

    ///////////////////

    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
        terrain = TerrainGeneration.FindObjectOfType<TerrainGeneration>();
        tileDropCircle = Resources.Load<GameObject>("TileDropCircle");

        animator = this.GetComponent<Animator>();
        rb2d = this.transform.parent.GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        facingDirection = FacingDirection.Left;
        runningFromPlayerState = false;
        isDead = false;
        destroyCounter = 0;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= runningFromPlayerRange)
        { runningFromPlayerState = true; }
        else if (distanceToPlayer > runningFromPlayerRange)
        { runningFromPlayerState = false; }

        if (isDead == false)
        { animator.SetBool("Running", runningFromPlayerState); }

        KnockbackCounter();
        DestroyCounter();

        //Debug.DrawRay(this.transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 1.5f), -Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.up, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (-Vector3.right * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (-Vector3.right * 2.1f * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
    }

    private void FixedUpdate()
    {
        if (isDead == true)
        {
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            this.GetComponent<CapsuleCollider2D>().enabled = false;
            return;
        }

        if (runningFromPlayerState == false)
        {
            movement = new Vector2(0, rb2d.velocity.y);
        }

        if (runningFromPlayerState == true && inKnockBackState == false)
        {
            if (transform.position.x + 0.1f < player.transform.position.x)
            {
                facingDirection = FacingDirection.Left;
                movement = new Vector2(-runSpeed, rb2d.velocity.y);
                transform.localScale = new Vector3(1, 1, 1); //change sprite left
            }
            else if (transform.position.x - 0.1f > player.transform.position.x)
            {
                facingDirection = FacingDirection.Right;
                movement = new Vector2(runSpeed, rb2d.velocity.y);
                transform.localScale = new Vector3(-1, 1, 1); //change sprite right
            }
            else
            {
                movement = new Vector2(0, rb2d.velocity.y);
            }

            if (GroundRaycast() && !FrontRaycast() && !FrontRaycast2() && !UpRaycast()) // autojump
            {
                if (onGround)
                {
                    movement.y = jumpForce; //jump
                }
            }
            if (GroundRaycast() && FrontRaycast())
            {
                movement.x = 0;

                if (onGround)
                {
                    movement.y = jumpForce; //jump
                }
            }
        }

        rb2d.velocity = movement;
    }

    public void TakeDamage(int damageTaken, Vector3 hitFromPosition)
    {
        sound_TakeDamage.Play();
                
        if (isDead == false)
        {
            currentHealth -= damageTaken;
            animator.SetTrigger("Take Damage");
            sound_TakeDamageBarrel.Play();

            Vector2 knockbackDirection = (transform.position - hitFromPosition).normalized;
            Vector2 knockbackX = new Vector2(knockbackDirection.x, 0f);

            knockbackCounter = knockbackStateDelay;
            rb2d.AddForce(knockbackX * 10, ForceMode2D.Impulse);

            if (player.inventoryManager.inventoryItems[player.inventoryManager.selectedSlot].GetItem() != null)
            {
                if (!player.inventoryManager.inventoryItems[player.inventoryManager.selectedSlot].GetItem().itemName.Contains("Wood"))
                {
                    player.inventoryManager.inventoryItems[player.inventoryManager.selectedSlot].SubtractDurability(1);
                }
            }

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
