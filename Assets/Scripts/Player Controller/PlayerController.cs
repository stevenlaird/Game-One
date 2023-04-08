using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // General variables
    [HideInInspector] public Vector2 spawnPosition;
    [HideInInspector] private Rigidbody2D rb;
    [HideInInspector] private Animator animator;
    // Managers and objects related to the player
    [HideInInspector] public InventoryManager inventoryManager;
    [HideInInspector] public TerrainGeneration terrainGenerator;
    [HideInInspector] public DayNightCycle dayNightCycle;
    // Health bar UI object
    [HideInInspector] public GameObject healthBar;
    // Held objects for the player
    [HideInInspector] public GameObject heldTool;
    [HideInInspector] public GameObject heldTile;
    [HideInInspector] public GameObject heldMisc;
    [HideInInspector] public GameObject heldItemName;
    // Torch-related objects
    [HideInInspector] public RuntimeAnimatorController torchAnim;
    [HideInInspector] public GameObject torchLight;

    // Crafting-related variables
    [Header("Items & Crafting")]
    public bool inRangeOfCraftingTable;
    public bool inRangeOfAnvil;
    public bool inRangeOfOven;

    // Player settings/customization
    [Header("Player Settings")]
    public float moveSpeed = 5;
    public float jumpForce = 10;

    public int playerRange = 4;

    public bool autoJump = true;
    public LayerMask autoJumpOnLayers;

    // Combat-related variables
    [Header("Combat")]
    public float maxHealth = 100;
    public float currentHealth;
    public bool isDead = false;
    public float destroyCounter = 0;
    public bool inKnockBackState;
    public float knockbackCounter;
    public float knockbackDelay = 0.5f;
    private float timeBetweenClick = 0.5f;
    public float singleClickDelay;

    public int baseDamage = 5;
    public int currentDamage;
    public Transform hitDetectionNoWeapon;
    public Transform hitDetectionWeapon;
    private float hitDetectionSize;
    public LayerMask hitEnemies;

    // Input display variables
    [Header("Input Display")]
    public Vector2Int mousePosition;
    public Vector2Int playerHeadPosition;
    public Vector2Int playerFeetPosition;
    public bool onGround;
    public FacingDirection facingDirection;
    public enum FacingDirection
    {
        Left, Right
    }
    public float horizontal;
    public float jump;
    public float vertical;
    public float clickCounter;
    public bool leftClickContinuous;
    public bool leftClickSingle;
    public bool rightClickContinuous;
    public bool rightClickSingle;

    // Player armor-related variables
    [Header("Player Armor")]
    private Sprite playerHead;
    private Sprite playerChest;
    private Sprite playerFrontArm;
    private Sprite playerBackArm;
    private Sprite playerFrontLeg;
    private Sprite playerBackLeg;
    private Sprite playerFrontFoot;
    private Sprite playerBackFoot;
    public InvSlotClass helmetSlot;
    public InvSlotClass chestSlot;
    public InvSlotClass legSlot;
    public InvSlotClass gloveSlot;
    public InvSlotClass bootSlot;
    public Sprite[] playerNoArmor;
    public Sprite[] playerArmor;

    //new Vector2 (Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y)

    ///////////////////

    // Place the Player at the spawn position and initialize necessary component references
    public void PlaceAtSpawn()
    {
        this.gameObject.transform.position = spawnPosition;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    public void Start()
    {
        // Initialize references to necessary manager scripts and UI objects
        inventoryManager = FindObjectOfType<InventoryManager>();
        terrainGenerator = FindObjectOfType<TerrainGeneration>();
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        healthBar = inventoryManager.transform.GetChild(10).transform.GetChild(1).gameObject;
        heldTool = this.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject;
        heldTile = this.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject;
        heldMisc = this.transform.GetChild(2).gameObject.transform.GetChild(2).gameObject;
        heldItemName = inventoryManager.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject;

        // Load torch-related objects from resources
        torchLight = Resources.Load<GameObject>("TorchLight");
        torchAnim = Resources.Load<RuntimeAnimatorController>("Torch");

        // Load player and armor sprites from resources
        playerNoArmor = Resources.LoadAll("Player", typeof(Sprite)).Cast<Sprite>().ToArray();
        playerArmor = Resources.LoadAll("Armor", typeof(Sprite)).Cast<Sprite>().ToArray();

        // Set the current health to the maximum health at the start
        currentHealth = maxHealth;
        // Set isDead to FALSE
        isDead = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // Update input-related variables based on user input
        leftClickSingle = Input.GetMouseButtonDown(0);
        rightClickSingle = Input.GetMouseButtonDown(1);
        leftClickContinuous = Input.GetMouseButton(0);
        rightClickContinuous = Input.GetMouseButton(1);

        horizontal = Input.GetAxis("Horizontal");
        jump = Input.GetAxisRaw("Jump");
        vertical = Input.GetAxisRaw("Vertical");


        // Get and store Player's head and feet positions
        playerHeadPosition.x = Mathf.RoundToInt(transform.position.x - 0.5f);
        playerHeadPosition.y = Mathf.RoundToInt(transform.position.y + 0.5f);

        playerFeetPosition.x = Mathf.RoundToInt(transform.position.x - 0.5f);
        playerFeetPosition.y = Mathf.RoundToInt(transform.position.y - 0.5f);


        // Get and store mouse position in world coordinates
        mousePosition.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePosition.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);

        // Update animations using input related variables and calling InventoryManager
        animator.SetFloat("Horizontal Movement", horizontal);
            //animator.SetBool("Swing Arm Once", leftClickSingle || rightClickSingle);
        animator.SetBool("Swing Arm", leftClickContinuous || rightClickContinuous);
        animator.SetBool("Using Inventory", inventoryManager.usingInventory);

        // Call methods within script
        RefreshHealthBar();
        RefreshHeldItem();
        RefreshEquippedArmor();
        RefreshDamageDealer();

        SingleClickDelay();
        HitDetection();
        KnockbackCounter();
        DestroyCounter();

        // Update crafting range variables on Player side
        // Each crafting table, anvil, and oven placed contains a script that will update these variables accordingly
        inRangeOfCraftingTable = false;
        inRangeOfAnvil = false;
        inRangeOfOven = false;

        // Initialize movement Vector2
        Vector2 movement;

        // Knockback effect
        if (inKnockBackState == true)
        {
            movement = rb.velocity;
        }
        else
        {
            movement = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }

        // Input right movement, update sprite and variable to be facing right
        if (horizontal > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingDirection = FacingDirection.Right;
        }
        // Input left movement, update sprite and variable to be facing left
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingDirection = FacingDirection.Left;
        }

        // Input jump, Player jump
        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGround)
            { movement.y = jumpForce; }
        }

        // Can't move outside left world border
        if (playerHeadPosition.x < 0)
        {
            movement.x = 1;
        }

        // Can't move outside right world border
        if (playerHeadPosition.x > terrainGenerator.worldWidth - 1)
        {
            movement.x = -1;
        }

        // Auto jump logic
        if (autoJump)
        {
            if (GroundRaycast() && !FrontRaycast() && !FrontRaycast2() && !UpRaycast() && horizontal != 0 && jump == 0)
            {
                if (onGround)
                {
                    movement.y = jumpForce * 0.7f;
                }
            }
        }

        // Update rigidbody velocity using movement variable definined in previous IF statements
        rb.velocity = movement;

        // Play swing sound if the left or right mouse button are held down when the singleClickDelay hits 0.5 and game not paused
        if ((leftClickContinuous || rightClickContinuous) && singleClickDelay == 0.5f && !AudioSettings.gamePaused)
        {
            FindObjectOfType<AudioManager>().Play("Player_Swing");
        }

    }

    // Perform a raycast below the Player to determine if they are on the ground or above an obstacle
    public bool GroundRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 2f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 0.5f);
        return hit;
    }
    // Perform a raycast in front of the Player to determine if they are facing an obstacle/wall 
    public bool FrontRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 2f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 0.5f);
        return hit;
    }
    // Perform a second raycast in front of the player to determine if they are facing an obstacle/wall
    public bool FrontRaycast2()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 1.5f), -Vector2.right * transform.localScale.x, 2f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 1.5f), -Vector2.right * transform.localScale.x, Color.white, 0.5f);
        return hit;
    }
    // Perform a raycast above the player to determine if they are below an obstacle/wall
    public bool UpRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), Vector2.up, 1f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.up, Color.white, 1f);
        return hit;
    }

    // Refresh the health bar UI color and size based on the Player's current health percentage
    public void RefreshHealthBar()
    {
        float healthPercentage = currentHealth / maxHealth;

        healthBar.transform.localScale = new Vector3(healthPercentage, 1, 1);

        if (healthPercentage <= 1.0f && healthPercentage > 0.50f)
        {
            healthBar.GetComponent<Image>().color = Color.green;
        }
        else if (healthPercentage <= 0.50f && healthPercentage > 0.25f)
        {
            healthBar.GetComponent<Image>().color = Color.yellow;
        }
        else if (healthPercentage <= 0.25)
        {
            healthBar.GetComponent<Image>().color = Color.red;
        }
    }

    // Refresh the visuals of the held item based on the selected item in the Player's inventory
    public void RefreshHeldItem()
    {
        // Holding nothing
        if (inventoryManager.selectedItem.GetItem() == null)
        {
            // Update heldItemName text
            heldItemName.transform.GetComponent<TextMeshProUGUI>().text = "";
            // Update heldItem Sprites
            heldTool.GetComponent<SpriteRenderer>().sprite = null;
            heldTile.GetComponent<SpriteRenderer>().sprite = null;
            heldMisc.GetComponent<SpriteRenderer>().sprite = null;
            // Not holding a torch anymore, remove the animator and prefab child
            if (heldMisc.transform.childCount > 0)
            {
                Component.Destroy(heldMisc.GetComponent<Animator>());
                GameObject.Destroy(heldMisc.transform.GetChild(0).gameObject);
            }
            // Can remove ground tiles at a slow speed when the Player is holding nothing but removed tiles won't drop a tile drop prefab
            // This prevents the Player from getting stuck in holes/caves if they lose their pickaxe item
            // But also encourages the Player to craft tools to break tiles faster
            if (Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange)
            {
                if (leftClickContinuous && terrainGenerator.groundTilePositions.Contains(new Vector2Int(mousePosition.x, mousePosition.y)))
                {
                    clickCounter += Time.deltaTime;
                    if (clickCounter > 0.05)
                    {
                        terrainGenerator.PlayerRemoveGroundTileHealth(mousePosition.x, mousePosition.y, inventoryManager.startingItems[0].GetItem().GetTool().toolDamage / 2, inventoryManager.startingItems[0].GetItem().GetTool().itemName);
                        clickCounter = 0;
                    }
                }
                else
                { clickCounter = 0; }
            }
        }
        else
        {
            // Update heldItemName text
            heldItemName.transform.GetComponent<TextMeshProUGUI>().text = inventoryManager.selectedItem.GetItem().itemName;
            // Not holding a torch anymore, remove the animator and prefab child
            if (inventoryManager.selectedItem.GetItem().itemName != "Torch" && heldMisc.transform.childCount > 0)
            {
                Component.Destroy(heldMisc.GetComponent<Animator>());
                GameObject.Destroy(heldMisc.transform.GetChild(0).gameObject);
            }
            // Holding armor
            if (inventoryManager.selectedItem.GetItem().GetArmor() != null) 
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }
            // Holding misc
            else if (inventoryManager.selectedItem.GetItem().GetMisc() != null) 
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }
            // Holding raw resource
            else if (inventoryManager.selectedItem.GetItem().GetRawResource() != null) 
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }
            // Holding consumable
            else if (inventoryManager.selectedItem.GetItem().GetConsumable() != null) 
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }
            // Holding tile
            else if (inventoryManager.selectedItem.GetItem().GetTile() != null) 
            {
                // Holding tile - torch
                if (inventoryManager.selectedItem.GetItem().GetTile().itemName == "Torch") 
                {
                    if (heldMisc.transform.childCount < 1)
                    {
                        heldTool.GetComponent<SpriteRenderer>().sprite = null;
                        heldTile.GetComponent<SpriteRenderer>().sprite = null;
                        heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
                        heldMisc.AddComponent<Animator>().runtimeAnimatorController = torchAnim;
                        GameObject tileTorchLight = Instantiate(torchLight, heldMisc.transform.position, Quaternion.identity);
                        tileTorchLight.transform.parent = heldMisc.transform;
                    }
                }
                // Holding tile -  crafting table
                else if (inventoryManager.selectedItem.GetItem().GetTile().itemName == "Crafting Table") 
                {
                    heldTool.transform.localScale = new Vector2(-0.5f, 0.5f);
                    heldTool.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
                    heldTile.GetComponent<SpriteRenderer>().sprite = null;
                    heldMisc.GetComponent<SpriteRenderer>().sprite = null;
                }
                // Holding tile -  other
                else
                {
                    heldTool.transform.localScale = new Vector2(-1f, 1f);
                    heldTool.GetComponent<SpriteRenderer>().sprite = null;
                    heldTile.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
                    heldMisc.GetComponent<SpriteRenderer>().sprite = null;
                }

                // Holding tile -  Ground tile
                // Within Player's range, but not behind the Player
                if (inventoryManager.selectedItem.GetItem().GetTile().tileLocation == TileClass.TileLocation.Ground &&
                    Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange &&
                    Vector2.Distance(playerHeadPosition, mousePosition) > 0.8f &&
                    Vector2.Distance(playerFeetPosition, mousePosition) > 0.8f)
                {
                    if (rightClickContinuous)
                    {
                        inventoryManager.selectedItem.GetItem().Use(this); // Use item - Place tile
                    }
                }
                // Holding tile -  Surface or Wall tile
                // Within Player's range, can be placed on behind the Player
                else if (Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange)
                {
                    if (rightClickContinuous)
                    {
                        inventoryManager.selectedItem.GetItem().Use(this); // Use item - Place tile
                    }
                }
            }
            // Holding tool
            else if (inventoryManager.selectedItem.GetItem().GetTool() != null && 
                Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange)
            {
                heldTool.transform.localScale = new Vector2(1f, 1f);
                heldTool.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = null;

                // Holding tool - pickaxe
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.pickaxe)
                {
                    if (leftClickContinuous && terrainGenerator.groundTilePositions.Contains(new Vector2Int(mousePosition.x, mousePosition.y)))
                    {
                        clickCounter += Time.deltaTime;
                        if (clickCounter > 0.05)
                        {
                            terrainGenerator.PlayerRemoveGroundTileHealth(mousePosition.x, mousePosition.y, inventoryManager.selectedItem.GetItem().GetTool().toolStrength, inventoryManager.selectedItem.GetItem().itemName);
                            clickCounter = 0;
                        }
                    }
                    else
                        clickCounter = 0;
                }
                // Holding tool - axe
                else if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.axe)
                {
                    if (leftClickContinuous && terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(mousePosition.x, mousePosition.y)))
                    {
                        clickCounter += Time.deltaTime;
                        if (clickCounter > 0.05)
                        {
                            terrainGenerator.PlayerRemoveSurfaceTileHealth(mousePosition.x, mousePosition.y, inventoryManager.selectedItem.GetItem().GetTool().toolStrength);
                            clickCounter = 0;
                        }
                    }
                    else
                        clickCounter = 0;
                }
                // Holding tool - hammer
                else if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.hammer)
                {
                    if (leftClickContinuous && terrainGenerator.wallTilePositions.Contains(new Vector2Int(mousePosition.x, mousePosition.y)) &&
                        (!terrainGenerator.wallTilePositions.Contains(new Vector2Int(mousePosition.x, mousePosition.y + 1)) ||
                        !terrainGenerator.wallTilePositions.Contains(new Vector2Int(mousePosition.x, mousePosition.y - 1)) ||
                        !terrainGenerator.wallTilePositions.Contains(new Vector2Int(mousePosition.x + 1, mousePosition.y)) ||
                        !terrainGenerator.wallTilePositions.Contains(new Vector2Int(mousePosition.x - 1, mousePosition.y))))
                    {
                        clickCounter += Time.deltaTime;
                        if (clickCounter > 0.05)
                        {
                            terrainGenerator.PlayerRemoveWallTileHealth(mousePosition.x, mousePosition.y, inventoryManager.selectedItem.GetItem().GetTool().toolStrength);
                            clickCounter = 0;
                        }
                    }
                    else
                    { clickCounter = 0; }
                }
                
            }
        }
    }

    // Update the Player's armor visuals based on the inventory's equipped armor pieces
    public void RefreshEquippedArmor()
    {
        // Update equipped armor slots by calling the InventoryManager
        helmetSlot = inventoryManager.equippedArmor[0];
        chestSlot = inventoryManager.equippedArmor[1];
        legSlot = inventoryManager.equippedArmor[2];
        gloveSlot = inventoryManager.equippedArmor[3];
        bootSlot = inventoryManager.equippedArmor[4];

        // Set head/helmet variable to corresponding equipped head/helmet Sprite from array
        if (helmetSlot.GetItem() == null)
        { playerHead = playerNoArmor[7]; }
        else if (helmetSlot.GetItem().itemName == "Wood Helmet")
        { playerHead = playerArmor[39]; }
        else if (helmetSlot.GetItem().itemName == "Stone Helmet")
        { playerHead = playerArmor[31]; }
        else if (helmetSlot.GetItem().itemName == "Iron Helmet")
        { playerHead = playerArmor[23]; }
        else if (helmetSlot.GetItem().itemName == "Gold Helmet")
        { playerHead = playerArmor[15]; }
        else if (helmetSlot.GetItem().itemName == "Diamond Helmet")
        { playerHead = playerArmor[7]; }
        // Set chest variable to corresponding equipped chest Sprite from array
        if (chestSlot.GetItem() == null)
        { playerChest = playerNoArmor[3]; }
        else if (chestSlot.GetItem().itemName == "Wood Chestplate")
        { playerChest = playerArmor[35]; }
        else if (chestSlot.GetItem().itemName == "Stone Chestplate")
        { playerChest = playerArmor[27]; }
        else if (chestSlot.GetItem().itemName == "Iron Chestplate")
        { playerChest = playerArmor[19]; }
        else if (chestSlot.GetItem().itemName == "Gold Chestplate")
        { playerChest = playerArmor[11]; }
        else if (chestSlot.GetItem().itemName == "Diamond Chestplate")
        { playerChest = playerArmor[3]; }
        // Set leg variable to corresponding equipped leg Sprite from array
        if (legSlot.GetItem() == null)
        { playerFrontLeg = playerNoArmor[6]; playerBackLeg = playerNoArmor[2]; }
        else if (legSlot.GetItem().itemName == "Wood Leggings")
        { playerFrontLeg = playerArmor[38]; playerBackLeg = playerArmor[34]; }
        else if (legSlot.GetItem().itemName == "Stone Leggings")
        { playerFrontLeg = playerArmor[30]; playerBackLeg = playerArmor[26]; }
        else if (legSlot.GetItem().itemName == "Iron Leggings")
        { playerFrontLeg = playerArmor[22]; playerBackLeg = playerArmor[18]; }
        else if (legSlot.GetItem().itemName == "Gold Leggings")
        { playerFrontLeg = playerArmor[14]; playerBackLeg = playerArmor[10]; }
        else if (legSlot.GetItem().itemName == "Diamond Leggings")
        { playerFrontLeg = playerArmor[6]; playerBackLeg = playerArmor[2]; }
        // Set glove variable to corresponding equipped glove Sprite from array
        if (gloveSlot.GetItem() == null)
        { playerFrontArm = playerNoArmor[4]; playerBackArm = playerNoArmor[0]; }
        else if (gloveSlot.GetItem().itemName == "Wood Gloves")
        { playerFrontArm = playerArmor[36]; playerBackArm = playerArmor[32]; }
        else if (gloveSlot.GetItem().itemName == "Stone Gloves")
        { playerFrontArm = playerArmor[28]; playerBackArm = playerArmor[24]; }
        else if (gloveSlot.GetItem().itemName == "Iron Gloves")
        { playerFrontArm = playerArmor[20]; playerBackArm = playerArmor[16]; }
        else if (gloveSlot.GetItem().itemName == "Gold Gloves")
        { playerFrontArm = playerArmor[12]; playerBackArm = playerArmor[8]; }
        else if (gloveSlot.GetItem().itemName == "Diamond Gloves")
        { playerFrontArm = playerArmor[4]; playerBackArm = playerArmor[0]; }
        // Set boot variable to corresponding equipped boot Sprite from array
        if (bootSlot.GetItem() == null)
        { playerFrontFoot = playerNoArmor[5]; playerBackFoot = playerNoArmor[1]; }
        else if (bootSlot.GetItem().itemName == "Wood Boots")
        { playerFrontFoot = playerArmor[37]; playerBackFoot = playerArmor[33]; }
        else if (bootSlot.GetItem().itemName == "Stone Boots")
        { playerFrontFoot = playerArmor[29]; playerBackFoot = playerArmor[25]; }
        else if (bootSlot.GetItem().itemName == "Iron Boots")
        { playerFrontFoot = playerArmor[21]; playerBackFoot = playerArmor[17]; }
        else if (bootSlot.GetItem().itemName == "Gold Boots")
        { playerFrontFoot = playerArmor[13]; playerBackFoot = playerArmor[9]; }
        else if (bootSlot.GetItem().itemName == "Diamond Boots")
        { playerFrontFoot = playerArmor[5]; playerBackFoot = playerArmor[1]; }

        // Refresh Player's Sprites using the variables assigned in previous IF statements
        this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = playerHead;
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = playerChest;
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = playerFrontArm;
        this.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = playerBackArm;
        this.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = playerFrontLeg;
        this.transform.GetChild(4).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = playerFrontFoot;
        this.transform.GetChild(5).GetComponent<SpriteRenderer>().sprite = playerBackLeg;
        this.transform.GetChild(5).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = playerBackFoot;
    }

    // Refresh the Player's damage and hit detection sphere depending on the item held
    // Also includes functionality for calling item class use methods
    // Unused lines of code are for easy implementation of varied held item actions
    public void RefreshDamageDealer()
    {
        // Holding nothing
        if (inventoryManager.selectedItem.GetItem() == null)
        {
            // Weak attack and radius
            currentDamage = baseDamage;

            if (rightClickSingle)
            { } // Do nothing
        }
        else
        {
            // Weak attack and radius
            currentDamage = baseDamage;
            hitDetectionSize = 1.5f;

            // Holding armor
            if (inventoryManager.selectedItem.GetItem().GetArmor() != null)
            { }
            // Holding consumable
            else if (inventoryManager.selectedItem.GetItem().GetConsumable() != null)
            {
                if (rightClickSingle)
                {
                    // Consume consumable, can only be consumed each delay
                    inventoryManager.selectedItem.GetItem().Use(this);
                    singleClickDelay = timeBetweenClick;
                }
            }
            // Holding misc
            else if (inventoryManager.selectedItem.GetItem().GetMisc() != null)
            { }
            // Holding raw resource
            else if (inventoryManager.selectedItem.GetItem().GetRawResource() != null)
            { }
            // Holding tile
            else if (inventoryManager.selectedItem.GetItem().GetTile() != null)
            { }
            // Holding tool
            else if (inventoryManager.selectedItem.GetItem().GetTool() != null)
            {
                // Set currentDamage to toolDamage and larger hit detection size
                currentDamage = inventoryManager.selectedItem.GetItem().GetTool().toolDamage;
                hitDetectionSize = 2.0f;

                // Holding pickaxe
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.pickaxe)
                { }
                // Holding axe
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.axe)
                { }
                // Holding hammer
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.hammer)
                { }
                // Holding weapon
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.weapon)
                { }
            }
        }
    }

    // Update the single click delay counter
    public bool SingleClickDelay()
    {
        if (singleClickDelay <= 0)
        {
            return true;
        }
        else
        {
            singleClickDelay -= Time.fixedDeltaTime;
            return false;
        }
    }

    // Handle interactions with enemy game objects based on single or continuous left-click input
    public void HitDetection()
    {
        if (SingleClickDelay() && inventoryManager.usingInventory == false)
        {
            if (leftClickSingle || leftClickContinuous || rightClickSingle || rightClickContinuous)
            {
                Collider2D[] enemiesToDamage;

                if (inventoryManager.selectedItem.GetItem() == null || inventoryManager.selectedItem.GetItem().GetTool() == null)
                { enemiesToDamage = Physics2D.OverlapCircleAll(hitDetectionNoWeapon.position, hitDetectionSize, hitEnemies); }
                else
                { enemiesToDamage = Physics2D.OverlapCircleAll(hitDetectionWeapon.position, hitDetectionSize, hitEnemies); }

                for (int i = 0; i < enemiesToDamage.Length; i++)
                {
                    if (enemiesToDamage[i].GetComponent<OgreController>() != null)
                        enemiesToDamage[i].GetComponent<OgreController>().TakeDamage(currentDamage, this.transform.position);
                    else if (enemiesToDamage[i].GetComponent<WalkingBarrelController>() != null)
                        enemiesToDamage[i].GetComponent<WalkingBarrelController>().TakeDamage(currentDamage, this.transform.position);
                    Debug.Log("Hit " + enemiesToDamage[i].name + " for " + currentDamage);
                }
                singleClickDelay = timeBetweenClick;
            }
        }
    }

    // Draw 2 gizmo spheres in the Editor around the Player's hit detection areas for testing
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitDetectionNoWeapon.position, 1.5f);
        Gizmos.DrawWireSphere(hitDetectionWeapon.position, 2f);
    }

    // This method is called by the OgreController script when the player takes damage.
    // It takes in two parameters: the amount of damage taken and the position where the damage came from.
    public void TakeDamage(float damageTaken, Vector3 hitFromPosition)
    {
        // Triggers the "Take Damage" animation in the player's animator component
        animator.SetTrigger("Take Damage");
        // Plays the "Player_TakeDamage" sound by calling the AudioManager script
        FindObjectOfType<AudioManager>().Play("Player_TakeDamage");

        // Checks each equipped armor slot to see if there is an item equipped.
        // IF an item is equipped, subtracts 1 from its durability.
        if (helmetSlot.GetItem() != null)
        { helmetSlot.SubtractDurability(1); }
        if (chestSlot.GetItem() != null)
        { chestSlot.SubtractDurability(1); }
        if (legSlot.GetItem() != null)
        { legSlot.SubtractDurability(1); }
        if (gloveSlot.GetItem() != null)
        { gloveSlot.SubtractDurability(1); }
        if (bootSlot.GetItem() != null)
        { bootSlot.SubtractDurability(1); }

        // Initializes a variable to store the amount of damage taken after reduction from armor.
        float reducedDamage = damageTaken;

        // Loops through each equipped armor slot and checks the item name to determine its armor rating.
        // Reduces the damage taken based on the armor rating of the equipped items.
        for (int i = 0; i < inventoryManager.equippedArmor.Length; i++)
        {
            if (inventoryManager.equippedArmor[i].GetItem() != null)
            {
                if (inventoryManager.equippedArmor[i].GetItem().itemName.Contains("Wood"))
                { reducedDamage = reducedDamage - 0.5f; }
                if (inventoryManager.equippedArmor[i].GetItem().itemName.Contains("Stone"))
                { reducedDamage = reducedDamage - 0.5f; }
                if (inventoryManager.equippedArmor[i].GetItem().itemName.Contains("Iron"))
                { reducedDamage = reducedDamage - 0.75f; }
                if (inventoryManager.equippedArmor[i].GetItem().itemName.Contains("Gold"))
                { reducedDamage = reducedDamage - 1f; }
                if (inventoryManager.equippedArmor[i].GetItem().itemName.Contains("Diamond"))
                { reducedDamage = reducedDamage - 1.25f; }
            }
        }

        // Logs the amount of damage taken after reduction from armor.
        Debug.Log("Hit for " + reducedDamage);

        // Subtracts the reduced damage from the player's current health.
        currentHealth -= reducedDamage;

        // Calculates the knockback direction based on the hitFromPosition parameter.
        Vector2 knockbackDirection = (transform.position - hitFromPosition).normalized;
        // Calculates the knockback direction in the x-axis.
        Vector2 knockbackX = new Vector2(knockbackDirection.x, 0f);

        // Resets the knockbackCounter to the knockbackDelay.
        knockbackCounter = knockbackDelay;
        // Applies knockback force to the player using the Rigidbody2D component.
        rb.AddForce(knockbackX * 10, ForceMode2D.Impulse);
            //rb.AddForce(knockbackDirection * 20, ForceMode2D.Impulse);

        // IF the player's current health is less than or equal to 0, the player is dead.
        // Triggers the "Is Dead" animation, disables the player's capsule collider, and drops all items in the inventory.
        if (currentHealth <= 0)
        {
            animator.SetTrigger("Is Dead");
            Debug.Log("Player Dead");
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            this.GetComponent<CapsuleCollider2D>().enabled = false;
            inventoryManager.DropAllItems();
            isDead = true;
        }
    }

    // Update the knockback counter and state variable
    private void KnockbackCounter()
    {
        if (knockbackCounter <= 0)
        {
            inKnockBackState = false;
        }
        else
        {
            knockbackCounter -= Time.fixedDeltaTime;
            inKnockBackState = true;
        }
    }

    // Update the destroy counter and destroy the player's game object if the Player's health reaches zero
    // Respawn and reset the Player's health, rigidbody, collider, and days survived
    // Drop all of the Player's items on the ground when he dies
    public void DestroyCounter()
    {
        if (isDead == true)
        {
            destroyCounter += Time.fixedDeltaTime;

            if (destroyCounter >= 3f)
            {
                isDead = false;
                destroyCounter = 0;
                this.transform.position = spawnPosition; // respawn
                currentHealth = maxHealth;
                this.GetComponent<CapsuleCollider2D>().enabled = true;
                //rb.constraints = RigidbodyConstraints2D.None;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                dayNightCycle.daysSurvived = 0;
                for (int i = 0; i < inventoryManager.startingItems.Length; i++)
                {
                    if (inventoryManager.startingItems[i].GetItem() != null)
                    {
                        inventoryManager.AddToInventory(inventoryManager.startingItems[i].GetItem(), 
                                                        inventoryManager.startingItems[i].GetQuantity(), 
                                                        inventoryManager.startingItems[i].GetStartingDurability(), 
                                                        inventoryManager.startingItems[i].GetCurrentDurability());
                    }
                }
            }
        }
    }

    // Collision triggers checks to determine if the Player is on ground
    // Commented line of code were used to determine if the Player was near crafting tables or anvils
    // But this did not work when multiple tables/anvils were placed
    // Functionality was moved to individual scripts attached to every crafting related tile in the world
    public void OnTriggerEnter2D(Collider2D collider)
    {
        /*if (collider.gameObject.CompareTag("Anvil")) //if tile touches player
        {
            inRangeOfAnvil = true;
        }
        if (collider.gameObject.CompareTag("CraftingTable")) //if tile touches player
        {
            inRangeOfCraftingTable = true;
        }*/
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Ground"))
        { onGround = true; }

        /*if (collider.gameObject.CompareTag("Anvil")) //if tile touches player
        {
            inRangeOfAnvil = true;
        }
        if (collider.gameObject.CompareTag("CraftingTable")) //if tile touches player
        {
            inRangeOfCraftingTable = true;
        }*/
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Ground"))
        { onGround = false; }

        /*if (collider.gameObject.CompareTag("Anvil")) //if tile touches player
        {
            inRangeOfAnvil = false;
        }
        if (collider.gameObject.CompareTag("CraftingTable")) //if tile touches player
        {
            inRangeOfCraftingTable = false;
        }*/
    }
}