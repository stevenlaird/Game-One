using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OgreController : MonoBehaviour
{
    [Header("Status Display")]
    public int currentHealth;
    public FacingDirection facingDirection;
    public enum FacingDirection
    {
        Left, Right
    }
    public bool onGround;
    public float moveSpeed;
    public int currentPassiveState;
    public float changeStateCounter;
    public bool agroToPlayerState;
    public bool playerWithinAttackRange;
    public float attackCounter;
    public bool inKnockBackState;
    public float knockbackCounter;
    public bool isDead;
    public float destroyCounter;


    [Header("Enemy Customization")]
    public int maxHealth = 100;
    public int damageDealt = 10;
    public float walkSpeed = 2;
    public float agroSpeed = 3;
    public float jumpForce = 4;
    public float stateTimeMin = 1f;
    public float stateTimeMax = 5f;
    public float agroStateRange = 10f;
    public float agroAttackRange = 2f;
    public float attackDelay = 1f;
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

    public Transform hitDetection;
    public float hitDetectionSize;
    public LayerMask hitPlayer;


    [Header("Audio Clips")]
    public AudioSource sound_Idle1;
    public AudioSource sound_Idle2;
    public AudioSource sound_TakeDamage;
    public AudioSource sound_TakeDamageOgre;
    public AudioSource sound_DealDamage;
    public AudioSource sound_Dead;

    ///////////////////

    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
        terrain = TerrainGeneration.FindObjectOfType<TerrainGeneration>();
        tileDropCircle = Resources.Load<GameObject>("TileDropCircle");

        animator = this.GetComponent<Animator>();
        rb2d = this.transform.parent.GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        facingDirection = FacingDirection.Right;
        agroToPlayerState = false;
        isDead = false;
        destroyCounter = 0;

        float chunkCoord = (Mathf.Round(this.transform.parent.transform.position.x / terrain.chunkSize) * terrain.chunkSize);
        chunkCoord /= terrain.chunkSize;
        //this.transform.parent.SetParent(terrain.worldChunks[(int)chunkCoord].transform.GetChild(4).transform);
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= agroStateRange && distanceToPlayer >= agroAttackRange)
        { agroToPlayerState = true; playerWithinAttackRange = false; }
        else if (distanceToPlayer > agroStateRange)
        { agroToPlayerState = false; playerWithinAttackRange = false; }
        else if (distanceToPlayer < agroAttackRange)
        { agroToPlayerState = true; playerWithinAttackRange = true; }

        animator.SetFloat("Horizontal Movement", movement.x);
        animator.SetBool("Following Player", agroToPlayerState);

        ChangeStateCounter();
        KnockbackCounter();
        DestroyCounter();

        //Debug.DrawRay(this.transform.position - (Vector3.up * 0.5f), Vector2.right * transform.localScale.x, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 1.5f), Vector2.right * transform.localScale.x, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.up, Color.white, 1f);
        //Debug.DrawRay(this.transform.position + (Vector3.right * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
        //Debug.DrawRay(this.transform.position + (Vector3.right * 2.1f * transform.localScale.x) - Vector3.up, -Vector2.up, Color.white, 2f);
    }

    private void FixedUpdate()
    {
        if (isDead == true)
        {
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            this.GetComponent<CapsuleCollider2D>().enabled = false;
            return;
        }

        if (inKnockBackState == false)
        {
            if (agroToPlayerState == false && playerWithinAttackRange == false)
            {
                moveSpeed = walkSpeed;

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

                if (facingDirection == FacingDirection.Right)
                {
                    transform.localScale = new Vector3(1, 1, 1); //change sprite right
                }
                else if (facingDirection == FacingDirection.Left)
                {
                    transform.localScale = new Vector3(-1, 1, 1); //change sprite left
                }

                if (GroundRaycast() && !FrontRaycast() && !FrontRaycast2() && !UpRaycast() && movement.x != 0) // autojump
                {
                    if (onGround)
                    {
                        movement.y = jumpForce; //jump
                    }
                }
                if ((GroundRaycast() && FrontRaycast()) || FrontRaycast2() || (!DownRaycast() && !DownRaycast2()))
                {
                    ChangeDirection();
                }

                /*float distanceToPlayerSpawn = Vector2.Distance(transform.position, player.spawnPosition);
                if (distanceToPlayerSpawn < 10)
                {
                    ChangeDirection();
                }*/
            }

            else if (agroToPlayerState == true && playerWithinAttackRange == false)
            {
                moveSpeed = agroSpeed;

                if (transform.position.x + 1f < player.transform.position.x)
                {
                    facingDirection = FacingDirection.Right;
                    movement = new Vector2(moveSpeed, rb2d.velocity.y);
                    transform.localScale = new Vector3(1, 1, 1); //change sprite right
                }
                else if (transform.position.x - 1f > player.transform.position.x)
                {
                    facingDirection = FacingDirection.Left;
                    movement = new Vector2(-moveSpeed, rb2d.velocity.y);
                    transform.localScale = new Vector3(-1, 1, 1); //change sprite left
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

            else if (playerWithinAttackRange == true)
            {
                movement = new Vector2(rb2d.velocity.x, rb2d.velocity.y);
                DealDamage();
            }

            rb2d.velocity = movement;
        }
    }

    public void ChangeStateCounter()
    {
        if (changeStateCounter <= 0)
        {
            currentPassiveState = ChangeState(3);
            if (currentPassiveState == 0)
                sound_Idle1.Play();
            else if (currentPassiveState == 1)
                sound_Idle2.Play();
            changeStateCounter = UnityEngine.Random.Range(stateTimeMin, stateTimeMax);
        }
        else
        {
            changeStateCounter -= Time.fixedDeltaTime;
        }
    }

    public int ChangeState(int count)
    {
        int selectedState = UnityEngine.Random.Range(0, count);
        return selectedState;
    }

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

    public void IdleState()
    {
        movement = new Vector2(0, rb2d.velocity.y);
    }

    public void ChangeDirection()
    {
        if (facingDirection == FacingDirection.Right)
        { facingDirection = FacingDirection.Left; }
        else if (facingDirection == FacingDirection.Left)
        { facingDirection = FacingDirection.Right; }
        currentPassiveState = ChangeState(2);
    }

    public void DealDamage()
    {
        if (AttackDelay())
        {
            animator.SetTrigger("Attack");
            sound_DealDamage.Play();
            Collider2D[] detectPlayer = Physics2D.OverlapCircleAll(hitDetection.position, hitDetectionSize, hitPlayer);
            
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

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitDetection.position, hitDetectionSize);
    }

    public void TakeDamage(int damageTaken, Vector3 hitFromPosition)
    {
        sound_TakeDamage.Play();
        
        if (isDead == false)
        {
            currentHealth -= damageTaken;
            animator.SetTrigger("Take Damage");
            sound_TakeDamageOgre.Play();

            Vector2 knockbackDirection = (transform.position - hitFromPosition).normalized;
            Vector2 knockbackX = new Vector2(knockbackDirection.x, 0f);

            knockbackCounter = knockbackStateDelay;
            //rb2d.AddForce(knockbackDirection * 10, ForceMode2D.Impulse);
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
                sound_Dead.Play();
                rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
                this.GetComponent<CapsuleCollider2D>().enabled = false;
                isDead = true;
            }
        }
    }

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
