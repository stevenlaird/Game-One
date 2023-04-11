using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    // References to the objects and parents required for the inventory system
    [SerializeField] public GameObject player;
    [SerializeField] public TerrainGeneration terrain;

    [SerializeField] private GameObject inventoryHotbarParent;
    [SerializeField] private GameObject inventoryBackpackParent;
    [SerializeField] private GameObject equippedArmorParent;
    [SerializeField] private GameObject inventorySlotSelectedSprite;
    [SerializeField] private GameObject itemGrabSprite;
    [SerializeField] private GameObject tileDropSquarePrefab;
    [SerializeField] private GameObject tileDropCirclePrefab;

    [SerializeField] private GameObject craftingResultSlotsParent;
    [SerializeField] private GameObject craftingRequiredItemOneSlotsParent;
    [SerializeField] private GameObject craftingRequiredItemTwoSlotsParent;
    [SerializeField] private GameObject craftingRequiredBackgroundParent;
    [SerializeField] private GameObject craftingResult_SlotPrefab;
    [SerializeField] private GameObject craftingRequired_SlotPrefab;

    [Header("Inventory")]
    // References to the items in the inventory
    [SerializeField] public InvSlotClass[] startingItems;
    public InvSlotClass[] inventoryItems;
    private GameObject[] inventorySlots;
    public InvSlotClass[] equippedArmor;
    private GameObject[] armorSlots;

    [Header("Inventory Controls")]
    // References to the inventory control variables
    public bool inventoryOpen = false;
    public bool usingInventory = false;

    [SerializeField] public int selectedSlot = 0;
    private int previouslySelectedSlot;
    public InvSlotClass selectedItem;

    public bool grabbingItem;
    public InvSlotClass grabItemMoving;
    private InvSlotClass grabItemOrigin;
    private InvSlotClass changeSlotTemp;

    [Header("Crafting")]
    // References to the crafting variables
    private Vector2 craftingResetPositionResult;
    private Vector2 craftingResetPositionRequiredOne;
    private Vector2 craftingResetPositionRequiredTwo;

    [SerializeField] private List<CraftingRecipeClass> craftingRecipeClasses = new List<CraftingRecipeClass>();
    
    private InvSlotClass[] craftingResultInvSlotClassCanCraft;
    private InvSlotClass[] craftingResultInvSlotClassCantCraft;
    private GameObject[] craftingResultSlots;

    private CraftingRecipeClass[] selectedCraftClass;
    private CraftingRecipeClass[] selectedCraftClass_Off;
    private InvSlotClass[] craftingRequiredItemOneInvSlotClass;
    private InvSlotClass[] craftingRequiredItemTwoInvSlotClass;
    private InvSlotClass[] craftingRequiredItemOneInvSlotClass_Off;
    private InvSlotClass[] craftingRequiredItemTwoInvSlotClass_Off;
    private GameObject[] craftingRequiredItemOneSlots;
    private GameObject[] craftingRequiredItemTwoSlots;

    [SerializeField] private int selectedCraftingResultSlot = 0;
    public CraftingRecipeClass selectedCraft;
    public InvSlotClass selectedResultItem;
    public InvSlotClass selectedRequiredItemOne;
    public InvSlotClass selectedRequiredItemTwo;

    ///////////////////

    // This method is called when the script is enabled, and initializes the inventory and crafting systems
    private void Start()
    {
        // Initializes the inventory slots and items
        inventorySlots = new GameObject[inventoryHotbarParent.transform.childCount + inventoryBackpackParent.transform.childCount];
        inventoryItems = new InvSlotClass[inventorySlots.Length];
        for (int i = 0; i < inventoryHotbarParent.transform.childCount + inventoryBackpackParent.transform.childCount; i++)
        {
            if (i < inventoryHotbarParent.transform.childCount)
                inventorySlots[i] = inventoryHotbarParent.transform.GetChild(i).gameObject;
            else
                inventorySlots[i] = inventoryBackpackParent.transform.GetChild(i - inventoryHotbarParent.transform.childCount).gameObject;
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventoryItems[i] = new InvSlotClass();
        }

        // Initializes the equipped armor slots and items
        armorSlots = new GameObject[equippedArmorParent.transform.childCount];
        equippedArmor = new InvSlotClass[armorSlots.Length];
        for (int i = 0; i < equippedArmorParent.transform.childCount; i++)
        {
            armorSlots[i] = equippedArmorParent.transform.GetChild(i).gameObject;
        }
        for (int i = 0; i < armorSlots.Length; i++)
        {
            equippedArmor[i] = new InvSlotClass();
        }

        // Initialize the crafting system variables
        craftingResetPositionResult = craftingResultSlotsParent.gameObject.transform.localPosition;
        craftingResetPositionRequiredOne = craftingRequiredItemOneSlotsParent.gameObject.transform.localPosition;
        craftingResetPositionRequiredTwo = craftingRequiredItemTwoSlotsParent.gameObject.transform.localPosition;

        craftingRecipeClasses = Resources.LoadAll("Crafting", typeof(CraftingRecipeClass)).Cast<CraftingRecipeClass>().ToList();
        craftingResultSlots = new GameObject[craftingRecipeClasses.Count];
        craftingRequiredItemOneSlots = new GameObject[craftingRecipeClasses.Count];
        craftingRequiredItemTwoSlots = new GameObject[craftingRecipeClasses.Count];
        craftingResultInvSlotClassCanCraft = new InvSlotClass[craftingRecipeClasses.Count];
        craftingResultInvSlotClassCantCraft = new InvSlotClass[craftingRecipeClasses.Count];
        craftingRequiredItemOneInvSlotClass = new InvSlotClass[craftingRecipeClasses.Count];
        craftingRequiredItemTwoInvSlotClass = new InvSlotClass[craftingRecipeClasses.Count];
        craftingRequiredItemOneInvSlotClass_Off = new InvSlotClass[craftingRecipeClasses.Count];
        craftingRequiredItemTwoInvSlotClass_Off = new InvSlotClass[craftingRecipeClasses.Count];
        selectedCraftClass = new CraftingRecipeClass[craftingRecipeClasses.Count];
        selectedCraftClass_Off = new CraftingRecipeClass[craftingRecipeClasses.Count];

        // Creates the crafting result and required item slots for each crafting recipe
        for (int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            craftingResultSlots[i] = Instantiate(craftingResult_SlotPrefab, craftingResultSlotsParent.transform);
            craftingResultSlots[i].name = craftingRecipeClasses[i].craftedItem.GetItem().itemName;
            craftingResultSlots[i].transform.localScale = Vector3.one;

            craftingRequiredItemOneSlots[i] = Instantiate(craftingRequired_SlotPrefab, craftingRequiredItemOneSlotsParent.transform);
            craftingRequiredItemOneSlots[i].name = craftingRecipeClasses[i].requiredItems[0].GetItem().itemName;
            craftingRequiredItemOneSlots[i].transform.localScale = Vector3.one;
            craftingRequiredItemTwoSlots[i] = Instantiate(craftingRequired_SlotPrefab, craftingRequiredItemTwoSlotsParent.transform);
            if (craftingRecipeClasses[i].requiredItems.Length == 2)
            {
                craftingRequiredItemTwoSlots[i].name = craftingRecipeClasses[i].requiredItems[1].GetItem().itemName;
            }
            else if (craftingRecipeClasses[i].requiredItems.Length == 1)
            {
                craftingRequiredItemTwoSlots[i].name = "Placeholder";
            }
            craftingRequiredItemTwoSlots[i].transform.localScale = Vector3.one;
        }

        // Add the starting items to the inventory
        for (int i = 0; i < startingItems.Length; i++)
        {
            AddToInventory(startingItems[i].GetItem(), startingItems[i].GetQuantity(), startingItems[i].GetStartingDurability(), startingItems[i].GetCurrentDurability());
        }

        // Refresh the UI to display the correct inventory and crafting systems
        RefreshUI();
    }

    private void Update()
    {
        // Call these Functions every frame
        UsingInventory();
        HighlightSlotSelected();
        HighlightHoveredSlot();
        RefreshItemGrab();
        RefreshCraftingList();
        ScrollCraftingList();
        RefreshUI();

        // IF 'E' on keyboard is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Enable/Open Inventory if Disabled/Closed, Disable/Close Inventory if Enabled/Opened
            transform.GetChild(3).gameObject.SetActive(!transform.GetChild(3).gameObject.activeSelf);
        }

        // IF the Inventory is Enabled/Opened
        if (transform.GetChild(3).gameObject.activeSelf == true)
        {
            // inventoryOpen BOOL set TRUE, used in various Functions
            inventoryOpen = true;
        }
        // ELSE IF the Inventory is Disabled/Closed
        else
        {
            // inventoryOpen BOOL set FALSE
            inventoryOpen = false;
        }

        // IF the selectedCraftingResultSlot + 1 is more than the amount of currently craftable items
            // The crafting list will shift because inventory has lost or gained required items
            // Or because inventory has lost or gained result items, this will happen when the player enters/exits range of Crafting Tables, Anvils, and Ovens
        if (selectedCraftingResultSlot + 1 > CountCurrentCraftableItems())
        {
            // Call ResetCraftingPosition Function. Reset the position of the Crafting List
            ResetCraftingPosition();
        }

        // IF the Player's mousePosition is not over the crafting area.
            // In this game, if the Player's cursor is grabbing an item that's the same as the selectedCrafitingItem,
            // and they press the crafting button, it will add to the quantity of the grabbed item until it hits a max stack.
            // Without this check, the Player would drop items when clicking the craft button if they are already holding an item.
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > 100 || Input.mousePosition.y > 650 || Input.mousePosition.y < 300)
        {
            // IF Player has Left clicked
            if (Input.GetMouseButtonDown(0))
            {
                // IF grabbing an Inventory item
                if (grabbingItem)
                {
                    // Call DropItemLeftClick
                    DropItemLeftClick();
                }
                // ELSE not grabbing an Inventory item
                else
                {
                    // Call GrabItemLeftClick
                    GrabItemLeftClick();
                }
            }
            // ELSE IF Player has Right clicked
            else if (Input.GetMouseButtonDown(1))
            {
                // IF grabbing an Inventory item
                if (grabbingItem)
                {
                    // Call DropItemRightClick
                    DropItemRightClick();
                }
                // ELSE not grabbing an Inventory item
                else
                {
                    // Call GrabItemRightClick
                    GrabItemRightClick();
                }
            }
        }
    }

    // Determine if Player is using their inventory by checking if their mousePosition is in the specified region where Inventory is based on Inventory being Opened/Closed
    // Used for Player's HitDetection function AND to disable swing animation when moving items in the Inventory
    public void UsingInventory()
    {

        if (inventoryOpen == false)// IF the Inventory is Disabled/Closed
        {
            if (Input.mousePosition.x >= 600 || Input.mousePosition.y <= 985)// IF mousePosition is not in the area of a Disabled/Closed Inventory
            {
                usingInventory = false;// Player is not using Inventory
            }
            else if (Input.mousePosition.x < 600 && Input.mousePosition.y > 985)// IF mousePosition is in the area of a Disabled/Closed Inventory
            {
                usingInventory = true;// Player is using Inventory
            }
        }
        else if (inventoryOpen == true)// IF the Inventory is Enabled/Opened
        {
            if (Input.mousePosition.x >= 600 || Input.mousePosition.y <= 688)// IF mousePosition is not in the area of a Enabled/Opened Inventory
            {
                usingInventory = false;// Player is not using Inventory
            }
            else if (Input.mousePosition.x < 600 && Input.mousePosition.y > 688)// IF mousePosition is in the area of a Enabled/Opened Inventory
            {
                usingInventory = true;// Player is using Inventory
            }
        }
    }

    // Shifts the red outline Sprite behind the selectedSlot (item the Player is currently holding)
    // Can't scroll the list if the mouse is hovering over the crafting area, so only the crafting list scrolls
    public void HighlightSlotSelected()
    {
        if (inventoryOpen == true)// IF the Inventory is Enabled/Opened
        {
            if (Input.mousePosition.x < 0 || Input.mousePosition.x > 200 || Input.mousePosition.y > 750)// IF the Player's mousePosition is not hovering over the crafting area
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0)// IF Player scrolls down on mouse wheel
                {
                    selectedSlot = Mathf.Clamp(selectedSlot + 1, 0, inventoryHotbarParent.transform.childCount - 1);// selectedSlot + 1, clamp between 0, and 9 (HotBarChildCount)
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)// IF Player scrolls up on mouse wheel
                {
                    selectedSlot = Mathf.Clamp(selectedSlot - 1, 0, inventoryHotbarParent.transform.childCount - 1);// selectedSlot - 1, clamp between 0, and 9 (HotBarChildCount)
                }
            }
        }
        else if (inventoryOpen == false)// IF the Inventory is Disabled/Closed
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0)// IF Player scrolls down on mouse wheel
            {
                selectedSlot = Mathf.Clamp(selectedSlot + 1, 0, inventoryHotbarParent.transform.childCount - 1);// selectedSlot + 1, clamp between 0, and 9 (HotBarChildCount)
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)// IF Player scrolls up on mouse wheel
            {
                selectedSlot = Mathf.Clamp(selectedSlot - 1, 0, inventoryHotbarParent.transform.childCount - 1);// selectedSlot - 1, clamp between 0, and 9 (HotBarChildCount)
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) { selectedSlot = 0; }// IF Player presses a number on the keyboard, set selected Slot to that number
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { selectedSlot = 1; }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { selectedSlot = 2; }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { selectedSlot = 3; }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) { selectedSlot = 4; }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) { selectedSlot = 5; }
        else if (Input.GetKeyDown(KeyCode.Alpha7)) { selectedSlot = 6; }
        else if (Input.GetKeyDown(KeyCode.Alpha8)) { selectedSlot = 7; }
        else if (Input.GetKeyDown(KeyCode.Alpha9)) { selectedSlot = 8; }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) { selectedSlot = 9; }

        if (previouslySelectedSlot != selectedSlot)// IF the previouslySelectedSlot is not the selectedSlot / Highlighted slot has changed
                                                   // Placed in this statement so the sound is not played on scrolls that are clamped
        {
            previouslySelectedSlot = selectedSlot;// Update previouslySelectedSlot to Play sound again on next trigger of event
            FindObjectOfType<AudioManager>().Play("Inventory_ScrollHighlighted");// Calls AudioManager to Play ScrollHighlighted sound
        }
        
        inventorySlotSelectedSprite.transform.position = inventorySlots[selectedSlot].transform.position;// Move the red outline Sprite to the location of selectedSlot in HotBar

        selectedItem = inventoryItems[selectedSlot];// Update selectedItem InvSlotClass
                                                    // selectedItem is used for updating Player's held item and making checks when placing an item into the Terrain
    }

    // If the Player's mousePosition is hovering over an Inventory Slot, changes the background Sprite of that Inventory Slot
    public void HighlightHoveredSlot()
    {
        for (int i = 0; i < inventorySlots.Length; i++)// FOR all Inventory Slots
        {
            if (Vector2.Distance(inventorySlots[i].transform.position, Input.mousePosition) <= 29)// IF Player's mouseposition is within range of Inventory Slot in loop
            {
                inventorySlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlotHover");// Change Sprite of Inventory Slot in loop to hovered
            }
            else// ELSE Player's mouseposition is not within range of Inventory Slot in loop
            {
                inventorySlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlot");// Change Sprite of Inventory Slot in loop back to default
            }
        }

        for (int i = 0; i < armorSlots.Length; i++)// FOR all Armor Slots
        {
            if (Vector2.Distance(armorSlots[i].transform.position, Input.mousePosition) <= 29)// IF Player's mouseposition is within range of Armor Slot in loop
            {
                armorSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlotHover");// Change Sprite of Armor Slot in loop to hovered
            }
            else// ELSE Player's mouseposition is not within range of Armor Slot in loop
            {
                armorSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlot");// Change Sprite of Armor Slot in loop back to default
            }
        }
    }

    // Refresh the Sprite of grabbingItem to the Sprite of the item the Player is grabbing from Inventory, including quantity and durability
    // AND places it where the Player's mousePosition is
    public void RefreshItemGrab()
    {
        itemGrabSprite.SetActive(grabbingItem);// Enable Image if grabbingItem BOOL is TRUE
        itemGrabSprite.transform.position = new Vector2(Input.mousePosition.x + 18f, Input.mousePosition.y - 18f);// Move GameObject containing Image above and to the left of Player's mousePosition

        if (grabbingItem)// IF grabbingItem BOOL is TRUE, Player is grabbing an Inventory item
        {
            itemGrabSprite.GetComponent<Image>().sprite = grabItemMoving.GetItem().itemSprites[0];// Set the Image to the first sprite of the InvSlot's ItemClass, this is the same Sprite used to for Inventory icons
            if (grabItemMoving.GetQuantity() > 1)// IF the quantity of the held item stack is greater than 0
            {
                itemGrabSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = grabItemMoving.GetQuantity() + "";// Update the TextMeshProUGUI child attached to the itemGrabSprite GameObject to held item stack quantity
            }
            else// ELSE the quantity is not greater than 1, usually 1 because if 0 or less the stack is cleared
            {
                itemGrabSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";// Update the TextMeshProUGUI child attached to the itemGrabSprite GameObject to be blank / Won't display the quantity when item stack is 1
            }

            if (grabItemMoving.GetItem().GetTool() || grabItemMoving.GetItem().GetArmor())// IF the grabbed item is a Tool or Armor
            {
                // Enable the item durability child attached to the itemGrabSprite GameObject
                itemGrabSprite.transform.GetChild(1).gameObject.SetActive(true);
                // Change the scale of the bar to a % of 1.0 by dividing the item's current durability by it's starting durability
                // Scale adjusts from the left side of the bar because of the pivot on the Image
                itemGrabSprite.transform.GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()), 1, 1);
                // IF durability is full, disable/hide the durability bar
                if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) == 1)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = false;
                }
                // ELSE IF durability % is between .50 and .99, enable the durability bar and set it's color to Green
                else if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) <= .99 && ((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) > .50)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.green;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = true;
                }
                // ELSE IF durability % is between .25 and .50, enable the durability bar and set it's color to Yellow
                else if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) <= .50 && ((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) > .25)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.yellow;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = true;
                }
                // ELSE IF durability % is less than .25, enable the durability bar and set it's color to Red
                else if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) <= .25)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.red;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = true;
                }
            }
            else// ELSE the grabbed item is not a Tool or Armor
            {
                itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = false;// Disable the item durability child attached to the itemGrabSprite GameObject
            }

            gameObject.transform.GetChild(4).gameObject.SetActive(true);// Enable the GrabItemEnlarged GameObject
            gameObject.transform.GetChild(4).transform.GetChild(0).GetComponent<Image>().sprite = grabItemMoving.GetItem().itemSprites[0];// Set the GameItemEnlarged child's Image component to the Sprite of the item grabbed
        }
        else if (!grabbingItem)// IF grabbingItem BOOL is FALSE / Player is not grabbing an item
        { gameObject.transform.GetChild(4).gameObject.SetActive(false); }// Disable the GrabItemEnlarged GameObject
    }

    // Displays the crafting list and allows it to be scrolled through when the Inventory is Opened AND transforms the Images of crafting list based on their position on Camera
    public void ScrollCraftingList()
    {
        if (inventoryOpen == true)// IF the Inventory is Enabled/Opened
        {
            if (Input.mousePosition.x > 0 && Input.mousePosition.x < 200 && Input.mousePosition.y < 750)// IF the Player's mousePosition is in the crafting area
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0)// IF Player scrolls down on mouse wheel
                {
                    ScrollDown();// Call ScrollDown Function
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)// IF Player scrolls up on mouse wheel
                {
                    ScrollUp();// Call ScrollUp Function
                }
            }
        }
        selectedCraft = selectedCraftClass[selectedCraftingResultSlot];// Update selectedCraft's CraftingRecipeClass to the selected CraftingRecipeClass in array of currently craftable items
        selectedResultItem = craftingResultInvSlotClassCanCraft[selectedCraftingResultSlot];// Update selectedResultItem's InvSlotClass to the selected InvSlotClass in array of currently craftable items
        selectedRequiredItemOne = craftingRequiredItemOneInvSlotClass[selectedCraftingResultSlot];// Update selectedRequiredItem's InvSlotClasses to the selected InvSlotClasses in array of currently craftable items
        selectedRequiredItemTwo = craftingRequiredItemTwoInvSlotClass[selectedCraftingResultSlot];

        for (int i = 0; i < craftingResultSlots.Length; i++)// FOR all craftingResultSlots / currently craftable items
        {
            Image ResultBox = craftingResultSlots[i].GetComponent<Image>();// ResultBox is assigned to the CraftingSlotPrefab background of Crafting Slot in loop
            Image ResultIcon = craftingResultSlots[i].transform.GetChild(0).GetComponent<Image>();// ResultIcon is assigned to the CraftingSlotPrefab item icon Image of Crafting Slot in loop

            // IF the Crafting Result Slot is in the Opened Inventory, Disable it's Images
            if (craftingResultSlots[i].transform.position.y > 782)
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = false;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(false);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            // ELSE IF the Crafting Result Slot is just under the Opened Inventory, Enable and make it's Images transparent
            else if (craftingResultSlots[i].transform.position.y >= 725 && craftingResultSlots[i].transform.position.y < 782)
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = true;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                ResultBox.color = new Color (ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.2f);
                ResultIcon.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.5f);
            }
            // ELSE IF the Crafting Result Slot is in the crafting area, and above the selectedCraftResultItem, Enable and set it's Images to default (some transparency)
            else if (craftingResultSlots[i].transform.position.y > 500 && craftingResultSlots[i].transform.position.y < 725)
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = true;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                ResultBox.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.4f);
                ResultIcon.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 1f);
                craftingResultSlots[i].GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            // ELSE IF the Crafting Result Slot is in the crafting area, and below the selectedCraftResultItem, Enable and set it's Images to default (some transparency)
            else if (craftingResultSlots[i].transform.position.y >= 96 && craftingResultSlots[i].transform.position.y < 495)
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = true;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                ResultBox.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.4f);
                ResultIcon.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 1f);
                craftingResultSlots[i].GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            // ELSE IF the Crafting Result Slot is just above the bottom of the Camera, Enable and make it's Images transparent
            else if (craftingResultSlots[i].transform.position.y >= 39 && craftingResultSlots[i].transform.position.y < 96)
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = true;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                ResultBox.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.2f);
                ResultIcon.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.5f);
            }
            // ELSE IF the Crafting Result Slot is below the Camera, Disable it's Images
            else if (craftingResultSlots[i].transform.position.y < 39)
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = false;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(false);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            // ELSE the Crafting Result Slot is in the crafting area, and is the selectedCraftResultItem, Enable, enlarge, and no transparency
            else
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = true;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                ResultBox.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.8f);
                ResultIcon.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 1f);
                craftingResultSlots[i].GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }
        }

        for (int i = 0; i < craftingRequiredItemOneSlots.Length; i++)// FOR all craftingRequiredItemOneSlots / currently craftable required items
        {
            // IF the Crafting Required Item One Slot is above the selectedCraftRequiredItemOne, Disable it's Images
            if (craftingRequiredItemOneSlots[i].transform.position.y > 497)
            {
                craftingRequiredItemOneSlots[i].transform.GetChild(0).gameObject.SetActive(false);
                craftingRequiredItemOneSlots[i].transform.GetChild(1).gameObject.SetActive(false);
                craftingRequiredItemTwoSlots[i].transform.GetChild(0).gameObject.SetActive(false);
                craftingRequiredItemTwoSlots[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            // ELSE IF the Crafting Required Item One Slot is below the selectedCraftRequiredItemOne, Disable it's Images
            else if (craftingRequiredItemOneSlots[i].transform.position.y < 496)
            {
                craftingRequiredItemOneSlots[i].transform.GetChild(0).gameObject.SetActive(false);
                craftingRequiredItemOneSlots[i].transform.GetChild(1).gameObject.SetActive(false);
                craftingRequiredItemTwoSlots[i].transform.GetChild(0).gameObject.SetActive(false);
                craftingRequiredItemTwoSlots[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            // ELSE the Crafting Required Item One Slot is the selectedCraftRequiredItemOne, Enable it's Images
            else
            {
                craftingRequiredItemOneSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingRequiredItemOneSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                craftingRequiredItemTwoSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingRequiredItemTwoSlots[i].transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    // ScrollUp crafting list if selectedCraftingResultSlot greater than 0 and Inventory is Open
    // Moves the crafting list down, subtract one from the value of selectedCraftingResultSlot within clamp, and calls AudioManager to Play ScrollHighlighted sound
    public void ScrollUp()
    {
        if (selectedCraftingResultSlot > 0 && inventoryOpen)
        {
            craftingResultSlotsParent.transform.localPosition = new Vector2(craftingResultSlotsParent.transform.localPosition.x, craftingResultSlotsParent.transform.localPosition.y - 27.5f);
            craftingRequiredItemOneSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemOneSlotsParent.transform.localPosition.x, craftingRequiredItemOneSlotsParent.transform.localPosition.y - 27.5f);
            craftingRequiredItemTwoSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemTwoSlotsParent.transform.localPosition.x, craftingRequiredItemTwoSlotsParent.transform.localPosition.y - 27.5f);
            selectedCraftingResultSlot = Mathf.Clamp(selectedCraftingResultSlot - 1, 0, craftingResultSlotsParent.transform.childCount);
            FindObjectOfType<AudioManager>().Play("Inventory_ScrollHighlighted");
        }
    }

    // ScrollDown crafting list if selectedCraftingResultSlot less than amount of currently craftable items and Inventory is Open
    // Moves the crafting list down, add one from the value of selectedCraftingResultSlot within clamp, and calls AudioManager to Play ScrollHighlighted sound
    public void ScrollDown()
    {
        if (selectedCraftingResultSlot < CountCurrentCraftableItems() - 1 && inventoryOpen)
        {
            craftingResultSlotsParent.transform.localPosition = new Vector2(craftingResultSlotsParent.transform.localPosition.x, craftingResultSlotsParent.transform.localPosition.y + 27.5f);
            craftingRequiredItemOneSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemOneSlotsParent.transform.localPosition.x, craftingRequiredItemOneSlotsParent.transform.localPosition.y + 27.5f);
            craftingRequiredItemTwoSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemTwoSlotsParent.transform.localPosition.x, craftingRequiredItemTwoSlotsParent.transform.localPosition.y + 27.5f);
            selectedCraftingResultSlot = Mathf.Clamp(selectedCraftingResultSlot + 1, 0, craftingResultSlotsParent.transform.childCount);
            FindObjectOfType<AudioManager>().Play("Inventory_ScrollHighlighted");
        }
    }

    // Update the Image, quantity, and durabilities of every Inventory Slot, Armor Slot, Crafting Result Slot, Crafting Required One Slot, Crafting Required Two Slot
    // Image, quantity, and durability are Enabled if the Slot contains an item OR Disabled if the Slot does not contain an item
    // For the Image of each, Enable and set Image from InvSlotClass OR Disable it
    // For the quantity of each, display value if the Slot contains a stack with a quantity greater than 1 OR Disable it
    // For the durability of each, if the item is a tool or armor Enable and update the durability bar's size and color based on it's current durability divided by it's starting durability OR Disable it
    public void RefreshUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            try
            {
                inventorySlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                inventorySlots[i].transform.GetChild(0).GetComponent<Image>().sprite = inventoryItems[i].GetItem().itemSprites[0];

                if (inventoryItems[i].GetQuantity() == 1)
                    inventorySlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                else
                    inventorySlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = inventoryItems[i].GetQuantity() + "";
                if (inventoryItems[i].GetItem().GetTool() || inventoryItems[i].GetItem().GetArmor())
                {
                    inventorySlots[i].transform.GetChild(2).gameObject.SetActive(true);
                    inventorySlots[i].transform.GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(((float)inventoryItems[i].GetCurrentDurability() / (float)inventoryItems[i].GetStartingDurability()), 1, 1);
                    if (((float)inventoryItems[i].GetCurrentDurability() / (float)inventoryItems[i].GetStartingDurability()) == 1)
                    {
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.white;
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().enabled = false;
                    }
                    else if (((float)inventoryItems[i].GetCurrentDurability() / (float)inventoryItems[i].GetStartingDurability()) <= .99 && ((float)inventoryItems[i].GetCurrentDurability() / (float)inventoryItems[i].GetStartingDurability()) > .50)
                    {
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.green;
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().enabled = true;
                    }
                    else if (((float)inventoryItems[i].GetCurrentDurability() / (float)inventoryItems[i].GetStartingDurability()) <= .50 && ((float)inventoryItems[i].GetCurrentDurability() / (float)inventoryItems[i].GetStartingDurability()) > .25)
                    {
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.yellow;
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().enabled = true;
                    }
                    else if (((float)inventoryItems[i].GetCurrentDurability() / (float)inventoryItems[i].GetStartingDurability()) <= .25)
                    {
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.red;
                        inventorySlots[i].transform.GetChild(2).GetComponent<Image>().enabled = true;
                    }
                }
                else
                    inventorySlots[i].transform.GetChild(2).GetComponent<Image>().enabled = false;

            }
            catch
            {
                inventorySlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                inventorySlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                inventorySlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                inventorySlots[i].transform.GetChild(2).GetComponent<Image>().enabled = false;
            }
        }
        for (int i = 0; i < armorSlots.Length; i++)
        {
            try
            {
                armorSlots[i].transform.GetChild(1).GetComponent<Image>().enabled = true;
                armorSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = equippedArmor[i].GetItem().itemSprites[0];
                armorSlots[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                if (equippedArmor[i].GetItem().GetArmor())
                {
                    armorSlots[i].transform.GetChild(3).gameObject.SetActive(true);
                    armorSlots[i].transform.GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(((float)equippedArmor[i].GetCurrentDurability() / (float)equippedArmor[i].GetStartingDurability()), 1, 1);
                    if (((float)equippedArmor[i].GetCurrentDurability() / (float)equippedArmor[i].GetStartingDurability()) == 1)
                    {
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().color = Color.white;
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().enabled = false;
                    }
                    else if (((float)equippedArmor[i].GetCurrentDurability() / (float)equippedArmor[i].GetStartingDurability()) <= .99 && ((float)equippedArmor[i].GetCurrentDurability() / (float)equippedArmor[i].GetStartingDurability()) > .50)
                    {
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().color = Color.green;
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().enabled = true;
                    }
                    if (((float)equippedArmor[i].GetCurrentDurability() / (float)equippedArmor[i].GetStartingDurability()) <= .50 && ((float)equippedArmor[i].GetCurrentDurability() / (float)equippedArmor[i].GetStartingDurability()) > .25)
                    {
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().color = Color.yellow;
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().enabled = true;
                    }
                    if (((float)equippedArmor[i].GetCurrentDurability() / (float)equippedArmor[i].GetStartingDurability()) <= .25)
                    {
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().color = Color.red;
                        armorSlots[i].transform.GetChild(3).GetComponent<Image>().enabled = true;
                    }
                }
                else
                    armorSlots[i].transform.GetChild(3).GetComponent<Image>().enabled = false;
            }
            catch
            {
                armorSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = null;
                armorSlots[i].transform.GetChild(1).GetComponent<Image>().enabled = false;
                armorSlots[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                armorSlots[i].transform.GetChild(3).GetComponent<Image>().enabled = false;
            }
        }

        for (int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            try
            {
                craftingResultSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                craftingResultSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = craftingRecipeClasses[i].craftedItem.GetItem().itemSprites[0];
                if (craftingRecipeClasses[i].craftedItem.GetQuantity() == 1)
                    craftingResultSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                else
                    craftingResultSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = craftingRecipeClasses[i].craftedItem.GetQuantity() + "";
            }
            catch
            {
                craftingResultSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                craftingResultSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                craftingResultSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
        for(int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            try
            {
                craftingRequiredItemOneSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                craftingRequiredItemOneSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = craftingRecipeClasses[i].requiredItems[0].GetItem().itemSprites[0];
                if (craftingRecipeClasses[i].requiredItems[0].GetQuantity() == 1)
                    craftingRequiredItemOneSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                else
                    craftingRequiredItemOneSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = craftingRecipeClasses[i].requiredItems[0].GetQuantity() + "";
            }
            catch
            {
                craftingRequiredItemOneSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                craftingRequiredItemOneSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                craftingRequiredItemOneSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
        for (int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            try
            {
                craftingRequiredItemTwoSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                craftingRequiredItemTwoSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = craftingRecipeClasses[i].requiredItems[1].GetItem().itemSprites[0];
                if (craftingRecipeClasses[i].requiredItems[1].GetQuantity() == 1)
                    craftingRequiredItemTwoSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                else
                    craftingRequiredItemTwoSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = craftingRecipeClasses[i].requiredItems[1].GetQuantity() + "";
            }
            catch
            {
                craftingRequiredItemTwoSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                craftingRequiredItemTwoSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                craftingRequiredItemTwoSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }

    // Enable the GameObjects for ResultSlots, RequiredSlotOnes, and RequiredSlotTwos that are currently craftable
    // For each add to the lists of selectedCraftClass, craftingResultInvSlotClassCanCraft, craftingRequiredItemOneInvSlotClass, craftingRequiredItemTwoInvSlotClass
    // Disable the GameObjects for ResultSlots, RequiredSlotOnes, and RequiredSlotTwos that are not currently craftable
    // For each add to the lists of selectedCraftClass_Off, craftingResultInvSlotClassCantCraft, craftingRequiredItemOneInvSlotClass_Off, and craftingRequiredItemTwoInvSlotClass_Off
    // Disables the craftingRequiredBackgroundParent if there are no currently craftable items or Inventory_StorageChest or Inventory_StorageBarrel are Enabled. Otherwise, Enables the craftingRequiredBackgroundParent
    public void RefreshCraftingList()
    {
        int x = 0;
        int y = 0;
        for (int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            if (!craftingRecipeClasses[i].CanCraft(this))
            {
                craftingResultSlots[i].gameObject.SetActive(false);
                craftingRequiredItemOneSlots[i].gameObject.SetActive(false);
                craftingRequiredItemTwoSlots[i].gameObject.SetActive(false);
                selectedCraftClass_Off[x] = craftingRecipeClasses[i];
                craftingResultInvSlotClassCantCraft[x] = craftingRecipeClasses[i].craftedItem;
                craftingRequiredItemOneInvSlotClass_Off[x] = craftingRecipeClasses[i].requiredItems[0];
                if (craftingRecipeClasses[i].requiredItems.Length == 2)
                {
                    craftingRequiredItemTwoInvSlotClass_Off[x] = craftingRecipeClasses[i].requiredItems[1];
                }
                else if (craftingRecipeClasses[i].requiredItems.Length == 1)
                {
                    craftingRequiredItemTwoInvSlotClass_Off[x] = new InvSlotClass();
                }
                x++;
            }
            else
            {
                craftingResultSlots[i].gameObject.SetActive(true);
                craftingRequiredItemOneSlots[i].gameObject.SetActive(true);
                craftingRequiredItemTwoSlots[i].gameObject.SetActive(true);
                selectedCraftClass[y] = craftingRecipeClasses[i];
                craftingResultInvSlotClassCanCraft[y] = craftingRecipeClasses[i].craftedItem;
                craftingRequiredItemOneInvSlotClass[y] = craftingRecipeClasses[i].requiredItems[0];
                if (craftingRecipeClasses[i].requiredItems.Length == 2)
                {
                    craftingRequiredItemTwoInvSlotClass[y] = craftingRecipeClasses[i].requiredItems[1];
                }
                else if (craftingRecipeClasses[i].requiredItems.Length == 1)
                {
                    craftingRequiredItemTwoInvSlotClass[y] = new InvSlotClass();
                }
                y++;
            }
        }

        if (CountCurrentCraftableItems() == 0 
            || this.transform.GetChild(3).transform.GetChild(6).gameObject.activeSelf == true 
            || this.transform.GetChild(3).transform.GetChild(7).gameObject.activeSelf == true)
        { 
            craftingRequiredBackgroundParent.SetActive(false);
        }
        else
        { 
            craftingRequiredBackgroundParent.SetActive(true);
        }
    }

    // Reset the position of the GameObject's containing the Result and Required Slots and the selectedCraftingResultSlot value
    public void ResetCraftingPosition()
    {
        craftingResultSlotsParent.transform.localPosition = craftingResetPositionResult;
        craftingRequiredItemOneSlotsParent.transform.localPosition = craftingResetPositionRequiredOne;
        craftingRequiredItemTwoSlotsParent.transform.localPosition = craftingResetPositionRequiredTwo;
        selectedCraftingResultSlot = 0;
    }

    // Counts the amount of items the Player can currently craft
    // Used for calling the ResetCraftingPosition function,
    // to clamp and not scroll down the crafting list too far,
    // Enable/Disable the background of required items one and two,
    // and allow the CraftItem button to be pressed
    public int CountCurrentCraftableItems()
    {
        int currentCraftableItems = 0;
        for (int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            if (craftingResultSlots[i].gameObject.activeSelf == true)
            { currentCraftableItems++; }
        }
        return currentCraftableItems;
    }

    // IF there are craftable items AND Inventory is Open, call the Craft Function from CraftingRecipeClass Script
    // Called by GameObject Button in the Canvas UI under Inventory
    public void CraftItem()
    {
        if (CountCurrentCraftableItems() > 0 && inventoryOpen)
        {
            selectedCraft.Craft(this);
        }
    }

    // IF there is a InvSlotClass Slot in the Inventory containing the same item Input into the function, it's stackable, and is not at max stack/max quantity, return that InvSlotClass Slot
    // Used when Player picks up tiledrops in the world, adds them to the slots already containing the tiledrop until its at max stack/max quantity
    public InvSlotClass ContainsStackable(ItemClass item)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i].GetItem() == item && inventoryItems[i].GetItem().stack && inventoryItems[i].GetQuantity() < inventoryItems[i].GetItem().maxStack)
                return inventoryItems[i];
        }

        return null;
    }

    // Checks all Inventory and Armor Slots to see if any contain the Input item. If a Slot does return the item. Otherwise, return null
    // Called by RemoveRequiredItems to check if the Inventory contains the items needed for crafting the Result Item
    public InvSlotClass Contains(ItemClass item)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i].GetItem() == item)
                return inventoryItems[i];
        }
        for (int i = 0; i < equippedArmor.Length; i++)
        {
            if (equippedArmor[i].GetItem() == item)
                return equippedArmor[i];
        }

        return null;
    }

    // Checks all Inventory Slots to see if any contain the Input item, and that the Input quantity is greater than or equal to Slots quantity. If a Slot does, return TRUE. Otherwise, return FALSE
    // Called by CraftingRecipeClass CanCraft Function to check if the Inventory contains the item and proper quantity needed for crafting the Result Item
    public bool Contains(ItemClass item, int quantity)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i].GetItem() == item && inventoryItems[i].GetQuantity() >= quantity)
                return true;
        }

        return false;
    }

    // Adds an item to the Invetory using an Input item, quantity, starting durability, and current durability
    // Calls ContainsStackable Function to add the items to item stack containing the same item as Input until the stack is full (max quantity)
    // If a stack becomes full, starts adding to the next slot containing that item that isn't at a full stack (max quantity)
    // If the inventory does not contain the item already place it in the next empty slot
    // Calls itself within the Function to find the next available stack when stack is full
    // Calls RefreshUI to update Inventory if it adds an item to the Inventory
    // Called in Start Function to add the starting items to the Player's inventory
    // Called when the Player dies to add the starting items to the Player's inventory again
    // Called by the TileDropController Script to pickup tiledrops in the world
    public bool AddToInventory(ItemClass item, int quantity,int startingDurability, int currerntDurability)
    {
        InvSlotClass inventorySlot = ContainsStackable(item);

        if (inventorySlot != null && inventorySlot.GetQuantity() < inventorySlot.GetItem().maxStack)
        {
            int availableSpace = inventorySlot.GetItem().maxStack - inventorySlot.GetQuantity();
            int quantityToAdd = Mathf.Clamp(quantity, 0, availableSpace);
            int remainder = quantity - availableSpace;
            inventorySlot.AddQuantity(quantityToAdd); //add quantity to item stack of same type
            if (remainder > 0) { AddToInventory(item, remainder, startingDurability, currerntDurability); }
            RefreshUI();
            return true;
        }
        else
        {
            for (int i = 0; i < inventoryItems.Length; i++)
            {
                if (inventoryItems[i].GetItem() == null) //empty slot
                {
                    int availableSpace = item.GetItem().maxStack - inventoryItems[i].GetQuantity();
                    int quantityToAdd = Mathf.Clamp(quantity, 0, availableSpace);
                    int remainder = quantity - availableSpace;

                    inventoryItems[i].AddItem(item, quantityToAdd, startingDurability, currerntDurability);
                    if (remainder > 0) { AddToInventory(item, remainder, startingDurability, currerntDurability); }
                    RefreshUI();
                    return true;
                }
            }
        }
        return false;
    }

    // Instantiate a TileDrop using an Input item, quantity, starting durability, and current durability
    // Instantiates it at the Player's position so it can be 'Thrown' (Have a force added to it) by the TileDropController
    // Assigns the TileDrop's TileDropController playerDroppedX BOOLs to be 'Thrown' in that direction by the TileDropController
    // Sets the TileDrop as a child of the chunk that the Player is currently in
    // Set it's Sprite to the first Sprite of the Input item's ItemClass
    // Give it a black background/outline using that first Sprite
    // Assign ItemClass of the TileDrop to the ItemClass of the Input Item
    // Then calls RefreshUI to update Inventory
    // Called by DropItemLeftClick, DropItemRightClick, and DropAllItems
    public void CreateTileDrop(ItemClass item, int quantity, int startingDurability, int currentDurability)
    {
        float chunkCoord = (Mathf.Round(player.transform.position.x / terrain.chunkSize) * terrain.chunkSize);
        chunkCoord /= terrain.chunkSize;

        GameObject newTileDrop;
        Vector2 spawnPosition = new Vector2(player.transform.position.x, player.transform.position.y + 0.5f);
        
        newTileDrop = Instantiate(tileDropCirclePrefab, spawnPosition, Quaternion.identity);
        if (player.GetComponent<PlayerController>().facingDirection == PlayerController.FacingDirection.Right)
        {
            newTileDrop.GetComponent<TileDropController>().playerDroppedRight = true;
        }
        else
        {
            newTileDrop.GetComponent<TileDropController>().playerDroppedLeft = true;
        }
        newTileDrop.transform.parent = terrain.worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
        newTileDrop.GetComponent<SpriteRenderer>().sprite = item.itemSprites[0];
        newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = item.itemSprites[0];
        InvSlotClass tiledrop = new InvSlotClass(item, quantity, startingDurability, currentDurability);
        newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop;

        RefreshUI();
    }

    // Subtracts Input quantity from selectedItem (Player's held item) InvSlotClass by Calling SubtractQuantity from InvSlotClass then Calls RefreshUI to update the Inventory display
    // Called by ConsumableClass to remove 1 when Player eats a consumable
    // Called by TileClass to remove 1 when Player places tiles
    public void SubtractSelectedItem(int quantity)
    {
        selectedItem.SubtractQuantity(quantity);
        RefreshUI();
    }

    // Calls the Contains Function to see if the Inventory contains the Input item
    // IF so, removes the Input quantity from Input item's quantity in Inventory. Input quantity will be the quantity needed for craftingResultItem
    // IF not, display the proper log message
    // Then calls RefreshUI to update the Inventory
    // Called by the Craft Function in the CraftingRecipeClass Script
    public bool RemoveRequiredItems(ItemClass item, int quantity)
    {
        InvSlotClass temp = Contains(item);
        if (temp != null)
        {
            if (temp.GetQuantity() >= quantity)
            {
                temp.SubtractQuantity(quantity);
            }
            else
            { Debug.Log("Can't Craft. Not enough"); return false; }
        }
        else { Debug.Log("Can't Craft"); return false; }
        RefreshUI();
        return true;
    }

    // For all Inventory Slots, just the HotBar if Inventory is Disabled/Closed and Armor Slots
    // If the Player's mousePosition is within 30f of the Slot, return that Slot. Otherwise, return null
    // Called by GrabItemLeftClick, GrabItemRightClick, DropItemLeftClick, and DropItemRightClick Functions
    private InvSlotClass GetClosestSlot()
    {
        if (transform.GetChild(3).gameObject.activeInHierarchy)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (Vector2.Distance(inventorySlots[i].transform.position, Input.mousePosition) <= 30)
                {
                    return inventoryItems[i];
                }
            }
            for (int i = 0; i < armorSlots.Length; i++)
            {
                if (Vector2.Distance(armorSlots[i].transform.position, Input.mousePosition) <= 30)
                {
                    return equippedArmor[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < inventoryHotbarParent.transform.childCount; i++)
            {
                if (Vector2.Distance(inventorySlots[i].transform.position, Input.mousePosition) <= 30)
                {
                    return inventoryItems[i];
                }
            }
        }
        return null;
    }

    // On Player's mousePosition, 'grab' item and take all of it's quantity from the item stack in that location of the Inventory
    // Calls GetClosestSlot and assigns it to grabItemOrigin
    // IF grabItemOrigin is null (empty), return (end function)
    // IF grabItemOrigin is an Armor Slot, calls AudioManager to Play removeArmor Sound
    // Assign grabItemMoving's InvSlotClass to grabItemOrigin's InvSlotClass, and Clear grabItemOrigin
    // (Clear is a function defined within InvSlotClass)
    // Assign grabbingItem to TRUE for use in Update and RefreshItemGrab
    // Calls RefreshUI to update the Inventory
    // Called by Update function
    private void GrabItemLeftClick()
    {
        grabItemOrigin = GetClosestSlot();
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return;
        }

        if (grabItemOrigin == equippedArmor[0])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[1])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[2])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[3])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[4])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else
            FindObjectOfType<AudioManager>().Play("Inventory_GrabItem");

        grabItemMoving = new InvSlotClass(grabItemOrigin);
        grabItemOrigin.Clear();
        grabbingItem = true;
        RefreshUI();
    }

    // On Player's mousePosition, 'grab' item and 'take' half of it's quantity (rounded up) from the item stack in that location of the Inventory
    // Calls GetClosestSlot and assigns it to grabItemOrigin
    // IF grabItemOrigin is null (empty), return (end function)
    // IF grabItemOrigin is an Armor Slot, calls AudioManager to Play removeArmor Sound
    // IF the quantity of the grabItemOrigin is 1, assign grabItemOrigin's InvSlotClass to grabItemMoving's InvSlotClass, and Clear grabItemOrigin
    // (Clear is a function defined within InvSlotClass)
    // ELSE assign grabItemMoving's InvSlotClass to grabItemOrigin's InvSlotClass with half of it's quantity (rounded up), and remove that quantity from grabItemOrigin's quantity
    // Assign grabbingItem to TRUE for use in Update and RefreshItemGrab
    // Calls RefreshUI to update the Inventory
    // Called by Update function
    private void GrabItemRightClick()
    {
        grabItemOrigin = GetClosestSlot();
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return;
        }

        if (grabItemOrigin == equippedArmor[0])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[1])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[2])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[3])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else if (grabItemOrigin == equippedArmor[4])
            FindObjectOfType<AudioManager>().Play("Inventory_RemoveArmor");
        else
            FindObjectOfType<AudioManager>().Play("Inventory_GrabItem");

        if (grabItemOrigin.GetQuantity() == 1)
        {
            grabItemMoving = new InvSlotClass(grabItemOrigin);
            grabItemOrigin.Clear();
        }
        else
        {
            grabItemMoving = new InvSlotClass(grabItemOrigin.GetItem(), Mathf.CeilToInt(grabItemOrigin.GetQuantity() / 2f), grabItemOrigin.GetStartingDurability(), grabItemOrigin.GetCurrentDurability());
            grabItemOrigin.SubtractQuantity(Mathf.CeilToInt(grabItemOrigin.GetQuantity() / 2f));
        }
        
        grabbingItem = true;
        RefreshUI();
    }

    // On Player's mousePosition, drop or swap the full item stack in Inventory or create tiledrop containing quantity of item stack

    // Calls GetClosestSlot and assigns it to grabItemOrigin
    // IF grabItemMoving (held item) is not an ArmorClass, and IF grabItemOrigin is an Armor Slot. Log message, and return (end function)
    // IF 'held item' is an ArmorClass and grabItemOrigin is the incorrect Armor Slot, return (end function)
    // IF grabItemOrigin is null (empty) AND Player's mouse is not in the Inventory area, calls CreateTileDrop function
    // ELSE IF grabItemOrigin is null (empty) AND Player's mouse is in the Inventory area, return (end function)
    // ELSE
    // IF grabItemOrigin is an ArmorSlot, calls AudioManager to Play EquipArmor sound
    // ELSE calls AudioManager to Play DropItem sound
    // IF grabItemOrigin's InvSlotClass contains an ItemClass
    // IF grabItemOrigin's ItemClass is the same as grabItemMoving's, AND the ItemClass is stackable, AND grabItemOrigin's InvSlotClass quantity is less than a max stack of that item
    // Determine how much can be added to quantity of grabItemOrigin
    // Add that quantity to grabItemOrigin and assign the remainder to grabItemMoving's quantity. IF remainder is 0, Clear grabItemMoving
    // (Clear is a function defined within InvSlotClass)
    // ELSE (grabItemOrigin and grabItemMoving are different items)
    // Assign a temporary InvSlotClass to grabItemOrigin's InvSlotClass and use it to 'swap' the InvSlotClass of grabItemOrigin and grabItemMoving
    // ELSE (grabItemOrigin is null (empty))
    // Assign grabItemOrigin's InvSlotClass to grabItemMoving's InvSlotClass, and Clear grabItemOrigin
    // Assign grabbingItem to FALSE for use in Update and RefreshItemGrab
    // Calls RefreshUI to update the Inventory
    private void DropItemLeftClick()
    {
        grabItemOrigin = GetClosestSlot();

        if (grabItemMoving.GetItem().GetArmor() == null)
            for (int i = 0; i < equippedArmor.Length; i++)
            {
                if (grabItemOrigin == equippedArmor[i])
                { Debug.Log("Can only place armor in these slots."); return; }
            }
        if (grabItemMoving.GetItem().GetArmor() != null)
        {
            if (grabItemOrigin == equippedArmor[0] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.helmet)
            { return; }
            else if (grabItemOrigin == equippedArmor[1] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.chestplate)
            { return; }
            else if (grabItemOrigin == equippedArmor[2] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.leggings)
            { return; }
            else if (grabItemOrigin == equippedArmor[3] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.gloves)
            { return; }
            else if (grabItemOrigin == equippedArmor[4] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.boots)
            { return; }
        }

        if (grabItemOrigin == null && (Input.mousePosition.x >= 600 || Input.mousePosition.y < 450)) //not dropped on a slot
        {
            CreateTileDrop(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
            grabItemMoving.Clear();
        }
        else if (grabItemOrigin == null && Input.mousePosition.x < 700 && Input.mousePosition.y >= 450)
        { return; }
        else
        {
            if (grabItemOrigin == equippedArmor[0])
                FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
            else if (grabItemOrigin == equippedArmor[1])
                FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
            else if (grabItemOrigin == equippedArmor[2])
                FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
            else if (grabItemOrigin == equippedArmor[3])
                FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
            else if (grabItemOrigin == equippedArmor[4])
                FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
            else
                FindObjectOfType<AudioManager>().Play("Inventory_DropItem");

            if (grabItemOrigin.GetItem() != null)
            {
                if (grabItemOrigin.GetItem() == grabItemMoving.GetItem()                 //grabbed item is same as item where dropped
                    && grabItemOrigin.GetItem().stack                                    //grabbed item is stackable
                    && grabItemOrigin.GetQuantity() < grabItemOrigin.GetItem().maxStack) //grabbed item quantity is less than a max stack of the item
                {
                    int availableSpace = grabItemOrigin.GetItem().maxStack - grabItemOrigin.GetQuantity();
                    int quantityToAdd = Mathf.Clamp(grabItemMoving.GetQuantity(), 0, availableSpace);
                    int remainder = grabItemMoving.GetQuantity() - availableSpace;
                    grabItemOrigin.AddQuantity(quantityToAdd);
                    if (remainder <= 0) { grabItemMoving.Clear(); }
                    else
                    {
                        grabItemMoving.SubtractQuantity(availableSpace);
                        RefreshUI();
                        return;
                    }
                }
                else
                {
                    changeSlotTemp = new InvSlotClass(grabItemOrigin);
                    grabItemOrigin.AddItem(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
                    grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
                    RefreshUI();
                    return;
                }
            }
            else //drop item in empty slot
            {

                grabItemOrigin.AddItem(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
                grabItemMoving.Clear();
            }
        }
        grabbingItem = false;
        RefreshUI();
    }

    // Calls GetClosestSlot,
    // IF not holding armor, and dropped in Armor Slot. Log message, and return FALSE
    // IF holding armor and not dropped in the correct Armor Slot, return FALSE
    // IF it returns null and Player's mouse is not in the Inventory area (not dropped on a Slot), calls CreateTileDrop Function
    // On Player's mousePosition, swap all or drop one of the item's stack in Inventory or create tiledrop containing one of item's stack
    private void DropItemRightClick()
    {
        grabItemOrigin = GetClosestSlot();

        if (grabItemMoving.GetItem().GetArmor() == null)
            for (int i = 0; i < equippedArmor.Length; i++)
            {
                if (grabItemOrigin == equippedArmor[i])
                { Debug.Log("Can only place armor in these slots."); return; }
            }
        if (grabItemMoving.GetItem().GetArmor() != null)
        {
            if (grabItemOrigin == equippedArmor[0] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.helmet)
            { return; }
            else if (grabItemOrigin == equippedArmor[1] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.chestplate)
            { return; }
            else if (grabItemOrigin == equippedArmor[2] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.leggings)
            { return; }
            else if (grabItemOrigin == equippedArmor[3] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.gloves)
            { return; }
            else if (grabItemOrigin == equippedArmor[4] && grabItemMoving.GetItem().GetArmor().armorType != ArmorClass.ArmorType.boots)
            { return; }
        }

        if (grabItemOrigin == null && (Input.mousePosition.x >= 600 || Input.mousePosition.y < 450)) //not dropped on a slot
        {
            CreateTileDrop(grabItemMoving.GetItem(), 1, grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
            grabItemMoving.SubtractQuantity(1);
            if (grabItemMoving.GetQuantity() < 1)
            {
                grabbingItem = false;
                grabItemMoving.Clear();
            }
            RefreshUI();
            return;
        }
        else if (grabItemOrigin == null && Input.mousePosition.x < 700 && Input.mousePosition.y >= 450)
        { return; }

        if (grabItemOrigin == equippedArmor[0])
            FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
        else if (grabItemOrigin == equippedArmor[1])
            FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
        else if (grabItemOrigin == equippedArmor[2])
            FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
        else if (grabItemOrigin == equippedArmor[3])
            FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
        else if (grabItemOrigin == equippedArmor[4])
            FindObjectOfType<AudioManager>().Play("Inventory_EquipArmor");
        else
            FindObjectOfType<AudioManager>().Play("Inventory_DropItem");

        if (grabItemOrigin.GetItem() != null //dropped on an available slot
            && (grabItemOrigin.GetItem() != grabItemMoving.GetItem() //not the same item OR location is at max stack
            || grabItemOrigin.GetQuantity() >= grabItemOrigin.GetItem().maxStack))
        {
            changeSlotTemp = new InvSlotClass(grabItemOrigin);
            grabItemOrigin.AddItem(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
            grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
            RefreshUI();
            return;
        }

        if (grabItemOrigin != null && grabItemOrigin.GetItem() == grabItemMoving.GetItem())
        { grabItemOrigin.AddQuantity(1); }
        else
        { grabItemOrigin.AddItem(grabItemMoving.GetItem(), 1, grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability()); }
        grabItemMoving.SubtractQuantity(1);

        if (grabItemMoving.GetQuantity() < 1)
        { grabbingItem = false; grabItemMoving.Clear(); }
        else
        { grabbingItem = true; }

        RefreshUI();
    }

    // Goes through all the Inventory Slots and Armor Slots
    // For each Slot containing an item, Call CreateTileDrop Function, and Clear the Slot
    // Each TileDrop will be of the same item, quantity, and durability that the Player had in their Inventory
    // Called by PlayerController in the TakeDamage Function when the Player's health is below 0 meaning the Player died
    public void DropAllItems()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventoryItems[i].GetItem() != null)
            {
                CreateTileDrop(inventoryItems[i].GetItem(), inventoryItems[i].GetQuantity(), inventoryItems[i].GetStartingDurability(), inventoryItems[i].GetCurrentDurability());
                inventoryItems[i].Clear();
            }
        }
        for (int i = 0; i < equippedArmor.Length; i++)
        {
            if (equippedArmor[i].GetItem() != null)
            {
                CreateTileDrop(equippedArmor[i].GetItem(), equippedArmor[i].GetQuantity(), equippedArmor[i].GetStartingDurability(), equippedArmor[i].GetCurrentDurability());
                equippedArmor[i].Clear();
            }
        }
    }
}