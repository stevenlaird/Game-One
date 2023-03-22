using JetBrains.Annotations;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Vector2 spawnPosition;
    [HideInInspector] private Rigidbody2D rb;
    [HideInInspector] private Animator animator;

    [HideInInspector] public InventoryManager inventoryManager;
    [HideInInspector] public TerrainGeneration terrainGenerator;
    [HideInInspector] public DayNightCycle dayNightCycle;

    [HideInInspector] public GameObject healthBar;

    [HideInInspector] public GameObject heldTool;
    [HideInInspector] public GameObject heldTile;
    [HideInInspector] public GameObject heldMisc;
    [HideInInspector] public GameObject heldItemName;

    [HideInInspector] public RuntimeAnimatorController torchAnim;
    [HideInInspector] public GameObject torchLight;


    [Header("Items & Crafting")]
    public bool inRangeOfCraftingTable;
    public bool inRangeOfAnvil;
    public bool inRangeOfOven;

    [Header("Player Settings")]
    public float moveSpeed = 5;
    public float jumpForce = 10;

    public int playerRange = 4;

    public bool autoJump = true;
    public LayerMask autoJumpOnLayers;

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

    public void PlaceAtSpawn()
    {
        this.gameObject.transform.position = spawnPosition;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        terrainGenerator = FindObjectOfType<TerrainGeneration>();
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        healthBar = inventoryManager.transform.GetChild(10).transform.GetChild(1).gameObject;
        heldTool = this.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject;
        heldTile = this.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject;
        heldMisc = this.transform.GetChild(2).gameObject.transform.GetChild(2).gameObject;
        heldItemName = inventoryManager.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject;

        torchLight = Resources.Load<GameObject>("TorchLight");
        torchAnim = Resources.Load<RuntimeAnimatorController>("Torch");

        playerNoArmor = Resources.LoadAll("Player", typeof(Sprite)).Cast<Sprite>().ToArray();
        playerArmor = Resources.LoadAll("Armor", typeof(Sprite)).Cast<Sprite>().ToArray();

        currentHealth = maxHealth;
        isDead = false;
    }

    private void Update()
    {
        leftClickSingle = Input.GetMouseButtonDown(0);
        rightClickSingle = Input.GetMouseButtonDown(1);
        leftClickContinuous = Input.GetMouseButton(0);
        rightClickContinuous = Input.GetMouseButton(1);

        horizontal = Input.GetAxis("Horizontal");
        jump = Input.GetAxisRaw("Jump");
        vertical = Input.GetAxisRaw("Vertical");

        playerHeadPosition.x = Mathf.RoundToInt(transform.position.x - 0.5f);
        playerHeadPosition.y = Mathf.RoundToInt(transform.position.y + 0.5f);

        playerFeetPosition.x = Mathf.RoundToInt(transform.position.x - 0.5f);
        playerFeetPosition.y = Mathf.RoundToInt(transform.position.y - 0.5f);

        mousePosition.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePosition.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);

        animator.SetFloat("Horizontal Movement", horizontal);
        //animator.SetBool("Swing Arm Once", leftClickSingle || rightClickSingle);
        animator.SetBool("Swing Arm", leftClickContinuous || rightClickContinuous);
        animator.SetBool("Using Inventory", inventoryManager.usingInventory);

        RefreshHealthBar();
        RefreshHeldItem();
        RefreshEquippedArmor();
        RefreshDamageDealer();

        SingleClickDelay();
        HitDetection();
        KnockbackCounter();
        DestroyCounter();

        inRangeOfCraftingTable = false;
        inRangeOfAnvil = false;
        inRangeOfOven = false;

        Vector2 movement;

        if (inKnockBackState == true)
        {
            movement = rb.velocity;
        }
        else
            movement = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (horizontal > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); //change sprite right
            facingDirection = FacingDirection.Right;
        }
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(1, 1, 1); //change sprite left
            facingDirection = FacingDirection.Left;
        }

        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGround)
                movement.y = jumpForce; //jump
        }

        if (playerHeadPosition.x < 0)
        {
            movement.x = 1; //can't move outside left world border
        }

        if (playerHeadPosition.x > terrainGenerator.worldWidth - 1)
        {
            movement.x = -1; //can't move outside right world border
        }

        if (autoJump)
        {
            if (GroundRaycast() && !FrontRaycast() && !FrontRaycast2() && !UpRaycast() && horizontal != 0 && jump == 0) // autojump
            {
                if (onGround)
                {
                    movement.y = jumpForce * 0.7f; //jump
                }
            }
        }

        rb.velocity = movement;

        if ((leftClickContinuous || rightClickContinuous) && singleClickDelay == 0.5f && !AudioSettings.gamePaused)
        {
            FindObjectOfType<AudioManager>().Play("Player_Swing");
        }

    }

    public bool GroundRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 2f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 0.5f);
        return hit;
    }
    public bool FrontRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 2f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, Color.white, 0.5f);
        return hit;
    }
    public bool FrontRaycast2()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 1.5f), -Vector2.right * transform.localScale.x, 2f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 1.5f), -Vector2.right * transform.localScale.x, Color.white, 0.5f);
        return hit;
    }
    public bool UpRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + (Vector3.up * 0.5f), Vector2.up, 1f, autoJumpOnLayers);
        //Debug.DrawRay(this.transform.position + (Vector3.up * 0.5f), Vector2.up, Color.white, 1f);
        return hit;
    }

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

    public void RefreshHeldItem()
    {
        if (inventoryManager.selectedItem.GetItem() == null) //holding nothing
        {
            heldItemName.transform.GetComponent<TextMeshProUGUI>().text = "";
            heldTool.GetComponent<SpriteRenderer>().sprite = null;
            heldTile.GetComponent<SpriteRenderer>().sprite = null;
            heldMisc.GetComponent<SpriteRenderer>().sprite = null;
            if (heldMisc.transform.childCount > 0)
            {
                Component.Destroy(heldMisc.GetComponent<Animator>());
                GameObject.Destroy(heldMisc.transform.GetChild(0).gameObject);
            }

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
                    clickCounter = 0;
            }
        }
        else
        {
            if (inventoryManager.selectedItem.GetItem().itemName != "Torch" && heldMisc.transform.childCount > 0)
            {
                Component.Destroy(heldMisc.GetComponent<Animator>());
                GameObject.Destroy(heldMisc.transform.GetChild(0).gameObject);
            }

            heldItemName.transform.GetComponent<TextMeshProUGUI>().text = inventoryManager.selectedItem.GetItem().itemName;

            if (inventoryManager.selectedItem.GetItem().GetArmor() != null) //holding armor
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }

            else if (inventoryManager.selectedItem.GetItem().GetMisc() != null) //holding misc
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }

            else if (inventoryManager.selectedItem.GetItem().GetRawResource() != null) //holding raw resource
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }

            else if (inventoryManager.selectedItem.GetItem().GetConsumable() != null) //holding consumable
            {
                heldTool.GetComponent<SpriteRenderer>().sprite = null;
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
            }

            else if (inventoryManager.selectedItem.GetItem().GetTile() != null) //holding tile
            {
                if (inventoryManager.selectedItem.GetItem().GetTile().itemName == "Torch") //holding tile - torch
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
                else if (inventoryManager.selectedItem.GetItem().GetTile().itemName == "Crafting Table") //holding tile -  crafting table
                {
                    heldTool.transform.localScale = new Vector2(-0.5f, 0.5f);
                    heldTool.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
                    heldTile.GetComponent<SpriteRenderer>().sprite = null;
                    heldMisc.GetComponent<SpriteRenderer>().sprite = null;
                }
                else
                {
                    heldTool.transform.localScale = new Vector2(-1f, 1f);
                    heldTool.GetComponent<SpriteRenderer>().sprite = null;
                    heldTile.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
                    heldMisc.GetComponent<SpriteRenderer>().sprite = null;
                }

                if (inventoryManager.selectedItem.GetItem().GetTile().tileLocation == TileClass.TileLocation.Ground && //holding ground tile
                    Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange &&
                    Vector2.Distance(playerHeadPosition, mousePosition) > 0.8f &&
                    Vector2.Distance(playerFeetPosition, mousePosition) > 0.8f)
                {
                    if (rightClickContinuous)
                    {
                        inventoryManager.selectedItem.GetItem().Use(this); //place tile
                    }
                }
                else if (Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange) //can place surface tiles and wall tiles behind player
                {
                    if (rightClickContinuous)
                    {
                        inventoryManager.selectedItem.GetItem().Use(this); //place tile
                    }
                }
            }

            else if (inventoryManager.selectedItem.GetItem().GetTool() != null) //holding tool
            {
                heldTool.transform.localScale = new Vector2(1f, 1f);
                heldTool.GetComponent<SpriteRenderer>().sprite = inventoryManager.selectedItem.GetItem().itemSprites[0];
                heldTile.GetComponent<SpriteRenderer>().sprite = null;
                heldMisc.GetComponent<SpriteRenderer>().sprite = null;

                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.pickaxe && //holding tool - pickaxe
                    Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange)
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
                else if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.axe && //holding tool - axe
                    Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange)
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
                else if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.hammer && //holding tool - hammer
                    Vector2.Distance(playerHeadPosition, mousePosition) <= playerRange)
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
                        clickCounter = 0;
                }
                
            }
        }
    }

    public void RefreshEquippedArmor()
    {
        helmetSlot = inventoryManager.equippedArmor[0];
        chestSlot = inventoryManager.equippedArmor[1];
        legSlot = inventoryManager.equippedArmor[2];
        gloveSlot = inventoryManager.equippedArmor[3];
        bootSlot = inventoryManager.equippedArmor[4];

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

        this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = playerHead;
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = playerChest;
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = playerFrontArm;
        this.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = playerBackArm;
        this.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = playerFrontLeg;
        this.transform.GetChild(4).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = playerFrontFoot;
        this.transform.GetChild(5).GetComponent<SpriteRenderer>().sprite = playerBackLeg;
        this.transform.GetChild(5).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = playerBackFoot;
    }

    public void RefreshDamageDealer()
    {
        if (inventoryManager.selectedItem.GetItem() == null) //holding nothing
        {
            currentDamage = baseDamage; //weak attack

            if (rightClickSingle)
            {
                //do nothing
            }
        }
        else
        {
            currentDamage = baseDamage; //weak attack
            hitDetectionSize = 1.5f;

            if (inventoryManager.selectedItem.GetItem().GetArmor() != null)
            {

            }
            else if (inventoryManager.selectedItem.GetItem().GetConsumable() != null)
            {
                if (rightClickSingle)
                {
                    inventoryManager.selectedItem.GetItem().Use(this); //consume consumable
                    singleClickDelay = timeBetweenClick;
                }
            }
            else if (inventoryManager.selectedItem.GetItem().GetMisc() != null)
            {

            }
            else if (inventoryManager.selectedItem.GetItem().GetRawResource() != null)
            {

            }
            else if (inventoryManager.selectedItem.GetItem().GetTile() != null)
            {

            }
            else if (inventoryManager.selectedItem.GetItem().GetTool() != null)
            {
                currentDamage = inventoryManager.selectedItem.GetItem().GetTool().toolDamage;
                hitDetectionSize = 2.0f;

                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.pickaxe) //holding tool - pickaxe
                {
                    
                }
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.axe) //holding tool - pickaxe
                {

                }
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.hammer) //holding tool - pickaxe
                {

                }
                if (inventoryManager.selectedItem.GetItem().GetTool().toolType == ToolClass.ToolType.weapon) //holding tool - pickaxe
                {

                }
            }
        }
    }

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

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitDetectionNoWeapon.position, 1.5f);
        Gizmos.DrawWireSphere(hitDetectionWeapon.position, 2f);
    }

    public void TakeDamage(float damageTaken, Vector3 hitFromPosition)
    {
        animator.SetTrigger("Take Damage");
        FindObjectOfType<AudioManager>().Play("Player_TakeDamage");

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

        float reducedDamage = damageTaken;

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

        Debug.Log("Hit for " + reducedDamage);

        currentHealth -= reducedDamage;

        Vector2 knockbackDirection = (transform.position - hitFromPosition).normalized;
        Vector2 knockbackX = new Vector2(knockbackDirection.x, 0f);

        knockbackCounter = knockbackDelay;
        //rb.AddForce(knockbackDirection * 20, ForceMode2D.Impulse);
        rb.AddForce(knockbackX * 10, ForceMode2D.Impulse);

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
