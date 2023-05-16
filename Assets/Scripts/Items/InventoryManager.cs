using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    // References to the objects and parents required for the inventory system
    [SerializeField] public GameObject player; // The player game object
    [SerializeField] public TerrainGeneration terrain; // The terrain generation script

    [SerializeField] private GameObject inventoryHotbarParent; // The parent of the inventory hotbar
    [SerializeField] private GameObject inventoryBackpackParent; // The parent of the inventory backpack
    [SerializeField] private GameObject equippedArmorParent; // The parent of the equipped armor slots
    [SerializeField] private GameObject inventorySlotSelectedSprite; // The sprite indicating the selected inventory slot
    [SerializeField] private GameObject itemGrabSprite; // The sprite indicating a grabbed item
    [SerializeField] private GameObject tileDropSquarePrefab; // The tile drop square prefab
    [SerializeField] private GameObject tileDropCirclePrefab; // The tile drop circle prefab

    [SerializeField] private GameObject craftingResultSlotsParent; // The parent of the crafting result slots
    [SerializeField] private GameObject craftingRequiredItemOneSlotsParent; // The parent of the crafting required item one slots
    [SerializeField] private GameObject craftingRequiredItemTwoSlotsParent; // The parent of the crafting required item two slots
    [SerializeField] private GameObject craftingRequiredBackgroundParent; // The parent of the crafting required background
    [SerializeField] private GameObject craftingResult_SlotPrefab; // The prefab for crafting result slots
    [SerializeField] private GameObject craftingRequired_SlotPrefab; // The prefab for crafting required slots

    [Header("Inventory")]
    // References to the items in the inventory
    [SerializeField] public InvSlotClass[] startingItems; // The starting items in the inventory
    public InvSlotClass[] inventoryItems; // The items in the inventory
    private GameObject[] inventorySlots; // The inventory slots
    public InvSlotClass[] equippedArmor; // The equipped armor items
    private GameObject[] armorSlots; // The armor slots

    [Header("Inventory Controls")]
    // References to the inventory control variables
    public bool inventoryOpen = false; // Whether the inventory is open
    public bool usingInventory = false; // Whether the inventory is being used

    [SerializeField] public int selectedSlot = 0; // The index of the selected inventory slot
    private int previouslySelectedSlot; // The index of the previously selected inventory slot
    public InvSlotClass selectedItem; // The currently selected inventory item

    public bool grabbingItem; // Whether an item is being grabbed
    public InvSlotClass grabItemMoving; // The item being moved
    private InvSlotClass grabItemOrigin; // The origin of the grabbed item
    private InvSlotClass changeSlotTemp; // Temporary storage for an item being moved

    [Header("Crafting")]
    // References to the crafting variables
    private Vector2 craftingResetPositionResult; // The reset position for the crafting result slot
    private Vector2 craftingResetPositionRequiredOne; // The reset position for crafting required item one slot
    private Vector2 craftingResetPositionRequiredTwo; // The reset position for crafting required item two slot

    [SerializeField] private List<CraftingRecipeClass> craftingRecipeClasses = new List<CraftingRecipeClass>(); // The crafting recipes

    private InvSlotClass[] craftingResultInvSlotClassCanCraft; // The crafting result slots for valid recipes
    private InvSlotClass[] craftingResultInvSlotClassCantCraft; // The crafting result slots for invalid recipes
    private GameObject[] craftingResultSlots; // The crafting result slots

    private CraftingRecipeClass[] selectedCraftClass; // The currently selected crafting recipe
    private CraftingRecipeClass[] selectedCraftClass_Off; // The currently selected crafting recipe when not focused
    private InvSlotClass[] craftingRequiredItemOneInvSlotClass; // The required item one slots for the selected crafting recipe
    private InvSlotClass[] craftingRequiredItemTwoInvSlotClass; // The required item two slots for the selected crafting recipe
    private InvSlotClass[] craftingRequiredItemOneInvSlotClass_Off; // The required item one slots for the selected crafting recipe when not focused
    private InvSlotClass[] craftingRequiredItemTwoInvSlotClass_Off; // The required item two slots for the selected crafting recipe when not focused
    private GameObject[] craftingRequiredItemOneSlots; // The required item one slots
    private GameObject[] craftingRequiredItemTwoSlots; // The required item two slots

    [SerializeField] private int selectedCraftingResultSlot = 0; // The index of the selected crafting result slot
    public CraftingRecipeClass selectedCraft; // The currently selected crafting recipe
    public InvSlotClass selectedResultItem; // The selected crafting result item
    public InvSlotClass selectedRequiredItemOne; // The selected required item one
    public InvSlotClass selectedRequiredItemTwo; // The selected required item two

    ///////////////////

    // This method is called when the script is enabled, and initializes the inventory and crafting systems
    private void Start()
    {
        // Initialize inventory system
        inventorySlots = new GameObject[inventoryHotbarParent.transform.childCount + inventoryBackpackParent.transform.childCount];
        inventoryItems = new InvSlotClass[inventorySlots.Length];
        for (int i = 0; i < inventoryHotbarParent.transform.childCount + inventoryBackpackParent.transform.childCount; i++)
        {
            // Assign inventory slots to corresponding parents
            if (i < inventoryHotbarParent.transform.childCount)
                inventorySlots[i] = inventoryHotbarParent.transform.GetChild(i).gameObject;
            else
                inventorySlots[i] = inventoryBackpackParent.transform.GetChild(i - inventoryHotbarParent.transform.childCount).gameObject;
        }
        // Initialize inventory slots and items
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventoryItems[i] = new InvSlotClass();
        }

        // Initialize equipped armor system
        armorSlots = new GameObject[equippedArmorParent.transform.childCount];
        equippedArmor = new InvSlotClass[armorSlots.Length];
        for (int i = 0; i < equippedArmorParent.transform.childCount; i++)
        {
            armorSlots[i] = equippedArmorParent.transform.GetChild(i).gameObject;
        }
        // Initialize equipped armor slots and items
        for (int i = 0; i < armorSlots.Length; i++)
        {
            equippedArmor[i] = new InvSlotClass();
        }

        // Load crafting recipes
        craftingRecipeClasses = Resources.LoadAll("Crafting", typeof(CraftingRecipeClass)).Cast<CraftingRecipeClass>().ToList();

        // Initialize the crafting system variables
        craftingResetPositionResult = craftingResultSlotsParent.gameObject.transform.localPosition;
        craftingResetPositionRequiredOne = craftingRequiredItemOneSlotsParent.gameObject.transform.localPosition;
        craftingResetPositionRequiredTwo = craftingRequiredItemTwoSlotsParent.gameObject.transform.localPosition;
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

    // This method is called every frame and handles various updates and inputs related to the inventory and crafting systems
    private void Update()
    {
        // Call methods to update the inventory and crafting systems
        UsingInventory();               // Determine if Player's mouse is in the inventory area
        HighlightSlotSelected();        // Highlights the currently selected inventory slot, the Player's held item
        HighlightHoveredSlot();         // Highlights the inventory or armor slot currently being hovered over by the mouse
        RefreshItemGrab();              // Updates the visual representation of the grabbed item
        RefreshCraftingList();          // Updates the visual representation of the crafting recipes
        ScrollCraftingList();           // Handles scrolling through the crafting recipes
        RefreshUI();                    // Updates the inventory and crafting UI elements

        // IF 'E' on keyboard is pressed / Handle opening and closing the inventory
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Enable/Open Inventory if Disabled/Closed, Disable/Close Inventory if Enabled/Opened
            transform.GetChild(3).gameObject.SetActive(!transform.GetChild(3).gameObject.activeSelf);
        }

        // Set inventoryOpen boolean based on the inventory window being open or closed
        inventoryOpen = transform.GetChild(3).gameObject.activeSelf;

        // IF the selectedCraftingResultSlot + 1 is more than the amount of currently craftable items
        // The crafting list will shift because inventory has lost or gained required items
        // Or because inventory has lost or gained result items, this will happen when the player enters/exits range of Crafting Tables, Anvils, and Ovens
        if (selectedCraftingResultSlot + 1 > CountCurrentCraftableItems())
        {
            // Call ResetCraftingPosition method. Reset the position of the crafting list
            ResetCraftingPosition();
        }

        // IF the Player's mousePosition is not over the crafting area.
        // Handle left and right mouse clicks while the cursor is outside of the inventory or crafting area
            // In this game, if the Player's cursor is grabbing an item that's the same as the selectedCrafitingItem,
            // and they press the crafting button, it will add to the quantity of the grabbed item until it hits a max stack.
            // Without this check, the Player would drop items when clicking the craft button if they are already holding an item.
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > 100 || Input.mousePosition.y > 650 || Input.mousePosition.y < 300)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (grabbingItem)
                {
                    // Drops the grabbed item if the left mouse button is pressed while an item is being held
                    DropItemLeftClick();
                }
                else
                {
                    // Attempts to grab an item if the left mouse button is pressed and no item is being held
                    GrabItemLeftClick();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (grabbingItem)
                {
                    // Drops the grabbed item if the right mouse button is pressed while an item is being held
                    DropItemRightClick();
                }
                else
                {
                    // Attempts to grab an item if the right mouse button is pressed and no item is being held
                    GrabItemRightClick();
                }
            }
        }
    }

    // Determines if the Player is currently interacting with the inventory by checking if their mouse is within the inventory's bounds
    // Used for the Player's hit detection and to disable swing animation when moving items in the inventory
    public void UsingInventory()
    {
        // Check if the inventory is closed
        if (!inventoryOpen)
        {
            // Check if the mouse is not within the bounds of a closed inventory
            if (Input.mousePosition.x >= 600 || Input.mousePosition.y <= 985)
            {
                usingInventory = false; // Player is not using inventory
            }
            // Check if the mouse is within the bounds of a closed inventory
            else if (Input.mousePosition.x < 600 && Input.mousePosition.y > 985)
            {
                usingInventory = true; // Player is using inventory
            }
        }
        // Check if the inventory is open
        else if (inventoryOpen)
        {
            // Check if the mouse is not within the bounds of an open inventory
            if (Input.mousePosition.x >= 600 || Input.mousePosition.y <= 688)
            {
                usingInventory = false; // Player is not using inventory
            }
            // Check if the mouse is within the bounds of an open inventory
            else if (Input.mousePosition.x < 600 && Input.mousePosition.y > 688)
            {
                usingInventory = true; // Player is using inventory
            }
        }
    }

    // Shifts the red outline sprite behind the currently selected inventory slot (the item the Player is currently holding)
    // Can't scroll the list if the mouse is hovering over the crafting area, so only the crafting list scrolls
    // Plays a sound effect when the highlighted slot changes
    public void HighlightSlotSelected()
    {
        // Check if the inventory is open
        if (inventoryOpen == true)
        {
            // IF the mouse is not hovering over the crafting area
            if (Input.mousePosition.x < 0 || Input.mousePosition.x > 200 || Input.mousePosition.y > 750)
            {
                // IF the Player scrolls down on the mouse wheel
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    // Increment the selected slot, clamped between 0 and the number of hotbar slots
                    selectedSlot = Mathf.Clamp(selectedSlot + 1, 0, inventoryHotbarParent.transform.childCount - 1);
                }
                // IF the Player scrolls up on the mouse wheel
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    // Decrement the selected slot, clamped between 0 and the number of hotbar slots
                    selectedSlot = Mathf.Clamp(selectedSlot - 1, 0, inventoryHotbarParent.transform.childCount - 1);
                }
            }
        }
        // Check if the inventory is closed
        else if (inventoryOpen == false)
        {
            // IF the Player scrolls down on the mouse wheel
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                // Increment the selected slot, clamped between 0 and the number of hotbar slots
                selectedSlot = Mathf.Clamp(selectedSlot + 1, 0, inventoryHotbarParent.transform.childCount - 1);
            }
            // IF the Player scrolls up on the mouse wheel
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                // Decrement the selected slot, clamped between 0 and the number of hotbar slots
                selectedSlot = Mathf.Clamp(selectedSlot - 1, 0, inventoryHotbarParent.transform.childCount - 1);
            }
        }

        // Allow the player to select hotbar slots by pressing number keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) { selectedSlot = 0; }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { selectedSlot = 1; }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { selectedSlot = 2; }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { selectedSlot = 3; }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) { selectedSlot = 4; }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) { selectedSlot = 5; }
        else if (Input.GetKeyDown(KeyCode.Alpha7)) { selectedSlot = 6; }
        else if (Input.GetKeyDown(KeyCode.Alpha8)) { selectedSlot = 7; }
        else if (Input.GetKeyDown(KeyCode.Alpha9)) { selectedSlot = 8; }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) { selectedSlot = 9; }

        // Play a sound effect if the highlighted slot has changed
        // Placed in it's own check so the sound is not played on scrolls that are clamped
        if (previouslySelectedSlot != selectedSlot)                                                   
        {
            // Update previouslySelectedSlot to Play sound again on next trigger of event
            previouslySelectedSlot = selectedSlot;
            // Calls AudioManager to Play ScrollHighlighted sound
            FindObjectOfType<AudioManager>().Play("Inventory_ScrollHighlighted");
        }

        // Move the red outline sprite to the location of the selected slot in the hotbar
        inventorySlotSelectedSprite.transform.position = inventorySlots[selectedSlot].transform.position;

        // Update selectedItem InvSlotClass.
        // selectedItem is used for updating Player's held item and making checks when placing an item into the Terrain
        selectedItem = inventoryItems[selectedSlot];
    }

    // Changes the background Sprite of inventory and armor slots if the Player's mousePosition is hovering over them
    public void HighlightHoveredSlot()
    {
        // Loop through all Inventory Slots
        for (int i = 0; i < inventorySlots.Length; i++)// FOR all Inventory Slots
        {
            // Check if the Player's mouse position is within range of the current Inventory Slot
            if (Vector2.Distance(inventorySlots[i].transform.position, Input.mousePosition) <= 29)
            {
                // Change the Sprite of the current Inventory Slot to the hovered version
                inventorySlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlotHover");
            }
            else
            {
                // Change the Sprite of the current Inventory Slot back to the default version
                inventorySlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlot");
            }
        }

        // Loop through all Armor Slots
        for (int i = 0; i < armorSlots.Length; i++)// FOR all Armor Slots
        {
            // Check if the Player's mouse position is within range of the current Armor Slot
            if (Vector2.Distance(armorSlots[i].transform.position, Input.mousePosition) <= 29)
            {
                // Change the Sprite of the current Armor Slot to the hovered version
                armorSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlotHover");
            }
            else
            {
                // Change the Sprite of the current Armor Slot back to the default version
                armorSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlot");
            }
        }
    }

    // Refreshes the Sprite of grabbingItem to the Sprite of the item the Player is grabbing from Inventory, including quantity and durability
    // AND places it where the Player's mousePosition is
    public void RefreshItemGrab()
    {
        // Enable Image if grabbingItem BOOL is TRUE
        itemGrabSprite.SetActive(grabbingItem);
        // Move GameObject containing Image above and to the left of Player's mousePosition
        itemGrabSprite.transform.position = new Vector2(Input.mousePosition.x + 18f, Input.mousePosition.y - 18f);

        // IF the Player is grabbing an Inventory item
        if (grabbingItem)
        {
            // Set the Image to the first sprite of the InvSlot's ItemClass, this is the same Sprite used for Inventory icons
            itemGrabSprite.GetComponent<Image>().sprite = grabItemMoving.GetItem().itemSprites[0];
            // Update the quantity TextMeshProUGUI child attached to the itemGrabSprite GameObject if the quantity of the held item stack is greater than 1
            if (grabItemMoving.GetQuantity() > 1)
            {
                itemGrabSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = grabItemMoving.GetQuantity() + "";
            }
            // Else, make the quantity TextMeshProUGUI child attached to the itemGrabSprite GameObject blank,
            // won't display a quantity of 1
            else
            {
                itemGrabSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            }

            // IF the grabbed item is a Tool or Armor
            if (grabItemMoving.GetItem().GetTool() || grabItemMoving.GetItem().GetArmor())
            {
                // Enable the item durability child attached to the itemGrabSprite GameObject
                itemGrabSprite.transform.GetChild(1).gameObject.SetActive(true);
                // Change the scale of the bar to a % of 1.0 by dividing the item's current durability by it's starting durability
                // Scale adjusts from the left side of the bar because of the pivot on the Image
                itemGrabSprite.transform.GetChild(1).GetComponent<RectTransform>().localScale = 
                    new Vector3(((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()), 1, 1);
                // IF the durability is full, disable/hide the durability bar
                if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) == 1)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = false;
                }
                // ELSE IF the durability % is between .50 and .99, enable the durability bar and set it's color to Green
                else if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) <= .99 && 
                    ((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) > .50)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.green;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = true;
                }
                // ELSE IF the durability % is between .25 and .50, enable the durability bar and set it's color to Yellow
                else if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) <= .50 && 
                    ((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) > .25)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.yellow;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = true;
                }
                // ELSE IF the durability % is less than .25, enable the durability bar and set it's color to Red
                else if (((float)grabItemMoving.GetCurrentDurability() / (float)grabItemMoving.GetStartingDurability()) <= .25)
                {
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().color = Color.red;
                    itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = true;
                }
            }
            // ELSE the grabbed item is not a Tool or Armor
            else
            {
                // Disable the item durability child attached to the itemGrabSprite GameObject
                itemGrabSprite.transform.GetChild(1).GetComponent<Image>().enabled = false;
            }

            // Enable the GrabItemEnlarged GameObject
            gameObject.transform.GetChild(4).gameObject.SetActive(true);
            // Set the GameItemEnlarged child's Image component to the Sprite of the item grabbed
            gameObject.transform.GetChild(4).transform.GetChild(0).GetComponent<Image>().sprite = grabItemMoving.GetItem().itemSprites[0];
        }
        // IF grabbingItem BOOL is FALSE / Player is not grabbing an item
        else if (!grabbingItem)
        // Disable the GrabItemEnlarged GameObject
        { gameObject.transform.GetChild(4).gameObject.SetActive(false); }
    }

    // Displays the crafting list and allows it to be scrolled through when the Inventory is Opened
    // AND transforms the Images of crafting list based on their position on Camera
    public void ScrollCraftingList()
    {
        // Check if the inventory is open
        if (inventoryOpen == true)
        {
            // Check if the mouse is in the crafting area
            if (Input.mousePosition.x > 0 && Input.mousePosition.x < 200 && Input.mousePosition.y < 750)
            {
                // Check if the player scrolls down on the mouse wheel
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    ScrollDown();   // Call ScrollDown Function
                }
                // Check if the player scrolls up on the mouse wheel
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    ScrollUp();     // Call ScrollUp Function
                }
            }
        }

        // Update selectedCraft's CraftingRecipeClass to the selected CraftingRecipeClass in array of currently craftable items
        selectedCraft = selectedCraftClass[selectedCraftingResultSlot];
        // Update selectedResultItem's InvSlotClass to the selected InvSlotClass in array of currently craftable items
        selectedResultItem = craftingResultInvSlotClassCanCraft[selectedCraftingResultSlot];
        // Update selectedRequiredItem's InvSlotClasses to the selected InvSlotClasses in array of currently craftable items
        selectedRequiredItemOne = craftingRequiredItemOneInvSlotClass[selectedCraftingResultSlot];
        selectedRequiredItemTwo = craftingRequiredItemTwoInvSlotClass[selectedCraftingResultSlot];

        // Loop through all craftingResultSlots / currently craftable items
        for (int i = 0; i < craftingResultSlots.Length; i++)
        {
            // ResultBox is assigned to the CraftingSlotPrefab background of Crafting Slot in loop
            Image ResultBox = craftingResultSlots[i].GetComponent<Image>();
            // ResultIcon is assigned to the CraftingSlotPrefab item icon Image of Crafting Slot in loop
            Image ResultIcon = craftingResultSlots[i].transform.GetChild(0).GetComponent<Image>();

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
            // ELSE IF the Crafting Result Slot is in the crafting area, and above the selectedCraftResultItem,
            // Enable and set it's Images to default (some transparency)
            else if (craftingResultSlots[i].transform.position.y > 500 && craftingResultSlots[i].transform.position.y < 725)
            {
                craftingResultSlots[i].GetComponent<Image>().enabled = true;
                craftingResultSlots[i].transform.GetChild(0).gameObject.SetActive(true);
                craftingResultSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                ResultBox.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 0.4f);
                ResultIcon.color = new Color(ResultBox.color.r, ResultBox.color.g, ResultBox.color.b, 1f);
                craftingResultSlots[i].GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            // ELSE IF the Crafting Result Slot is in the crafting area, and below the selectedCraftResultItem,
            // Enable and set it's Images to default (some transparency)
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
        // Loop through all craftingRequiredItemOneSlots, which are the slots for the required items of currently craftable items
        for (int i = 0; i < craftingRequiredItemOneSlots.Length; i++)
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

    // Scrolls up the crafting list if selectedCraftingResultSlot is greater than 0 and the inventory is open.
    // Moves the crafting list down, subtracts one from the value of selectedCraftingResultSlot within clamp,
    // AND plays the "Inventory_ScrollHighlighted" sound
    public void ScrollUp()
    {
        if (selectedCraftingResultSlot > 0 && inventoryOpen)
        {
            // Move the crafting list up and update the selectedCraftingResultSlot value
            craftingResultSlotsParent.transform.localPosition = new Vector2(craftingResultSlotsParent.transform.localPosition.x, craftingResultSlotsParent.transform.localPosition.y - 27.5f);
            craftingRequiredItemOneSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemOneSlotsParent.transform.localPosition.x, craftingRequiredItemOneSlotsParent.transform.localPosition.y - 27.5f);
            craftingRequiredItemTwoSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemTwoSlotsParent.transform.localPosition.x, craftingRequiredItemTwoSlotsParent.transform.localPosition.y - 27.5f);
            selectedCraftingResultSlot = Mathf.Clamp(selectedCraftingResultSlot - 1, 0, craftingResultSlotsParent.transform.childCount);
            // Play the "Inventory_ScrollHighlighted" sound
            FindObjectOfType<AudioManager>().Play("Inventory_ScrollHighlighted");
        }
    }

    // Scrolls down the crafting list if selectedCraftingResultSlot is less than the number of currently craftable items and the inventory is open.
    // Moves the crafting list down, adds one to the value of selectedCraftingResultSlot within clamp,
    // AND plays the "Inventory_ScrollHighlighted" sound.
    public void ScrollDown()
    {
        if (selectedCraftingResultSlot < CountCurrentCraftableItems() - 1 && inventoryOpen)
        {
            // Move the crafting list down and update the selectedCraftingResultSlot value
            craftingResultSlotsParent.transform.localPosition = new Vector2(craftingResultSlotsParent.transform.localPosition.x, craftingResultSlotsParent.transform.localPosition.y + 27.5f);
            craftingRequiredItemOneSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemOneSlotsParent.transform.localPosition.x, craftingRequiredItemOneSlotsParent.transform.localPosition.y + 27.5f);
            craftingRequiredItemTwoSlotsParent.transform.localPosition = new Vector2(craftingRequiredItemTwoSlotsParent.transform.localPosition.x, craftingRequiredItemTwoSlotsParent.transform.localPosition.y + 27.5f);
            selectedCraftingResultSlot = Mathf.Clamp(selectedCraftingResultSlot + 1, 0, craftingResultSlotsParent.transform.childCount);
            // Play the "Inventory_ScrollHighlighted" sound
            FindObjectOfType<AudioManager>().Play("Inventory_ScrollHighlighted");
        }
    }

    // Update the Image, quantity, and durabilities of every Inventory Slot, Armor Slot, and Crafting Slots
    // Image, quantity, and durability are Enabled if the Slot contains an item OR Disabled if the Slot does not contain an item
    // For the Image of each, Enable and set Image from InvSlotClass OR Disable it
    // For the quantity of each, display value if the Slot contains a stack with a quantity greater than 1 OR Disable it
    // For the durability of each, if the item is a tool or armor Enable and update the durability bar's size and color OR Disable it
    public void RefreshUI()
    {
        // Update the Image, quantity, and durabilities of each Inventory Slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            try
            {
                // Enable the Image and set it from the InvSlotClass if the slot contains an item
                inventorySlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                inventorySlots[i].transform.GetChild(0).GetComponent<Image>().sprite = inventoryItems[i].GetItem().itemSprites[0];

                // Enable the quantity if the slot contains a stack with a quantity greater than 1, otherwise disable it
                if (inventoryItems[i].GetQuantity() == 1)
                    inventorySlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                else
                    inventorySlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = inventoryItems[i].GetQuantity() + "";

                // Check if item is a tool or armor and update durability bar accordingly
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
        // Update the Image and durability of each Armor Slot
        for (int i = 0; i < armorSlots.Length; i++)
        {
            try
            {
                armorSlots[i].transform.GetChild(1).GetComponent<Image>().enabled = true;
                armorSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = equippedArmor[i].GetItem().itemSprites[0];
                armorSlots[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                // Check if item is armor and update durability bar accordingly
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

        // Update the Image and quantity of each Crafting Result Slot
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
        // Update the Image and quantity of each Crafting Required One Slot
        for (int i = 0; i < craftingRecipeClasses.Count; i++)
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
        // Update the Image and quantity of each Crafting Required Two Slot
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

    // Refreshes the crafting list by enabling or disabling result slots and required item slots based on whether the crafting recipe can be crafted.
    // Updates the lists of selected crafting classes and inventory slot classes accordingly.
    // Enables or disables the crafting required background parent based on the number of currently craftable items and the state of Inventory_StorageChest and Inventory_StorageBarrel.
    public void RefreshCraftingList()
    {
        int x = 0;
        int y = 0;
        // Loop through all craftingRecipeClasses
        for (int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            // If the crafting recipe cannot be crafted
            if (!craftingRecipeClasses[i].CanCraft(this))
            {
                // Disable the corresponding crafting result slot, required item one slot, and required item two slot GameObjects
                craftingResultSlots[i].gameObject.SetActive(false);
                craftingRequiredItemOneSlots[i].gameObject.SetActive(false);
                craftingRequiredItemTwoSlots[i].gameObject.SetActive(false);
                // Add the current crafting recipe class and inventory slot classes to the respective "Off" lists
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
            else // ELSE the crafting recipe can be crafted
            {
                // Enable the corresponding crafting result slot, required item one slot, and required item two slot GameObjects
                craftingResultSlots[i].gameObject.SetActive(true);
                craftingRequiredItemOneSlots[i].gameObject.SetActive(true);
                craftingRequiredItemTwoSlots[i].gameObject.SetActive(true);
                // Add the current crafting recipe class and inventory slot classes to the respective lists
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

        // Check the number of current craftable items and the state of Inventory_StorageChest and Inventory_StorageBarrel
        if (CountCurrentCraftableItems() == 0 
            || this.transform.GetChild(3).transform.GetChild(6).gameObject.activeSelf == true 
            || this.transform.GetChild(3).transform.GetChild(7).gameObject.activeSelf == true)
        {
            // Disable the craftingRequiredBackgroundParent GameObject
            craftingRequiredBackgroundParent.SetActive(false);
        }
        else
        {
            // Enable the craftingRequiredBackgroundParent GameObject
            craftingRequiredBackgroundParent.SetActive(true);
        }
    }

    // Resets the positions of the GameObjects containing the crafting result and required item slots,
    // AND resets the selectedCraftingResultSlot value
    public void ResetCraftingPosition()
    {
        // Reset the local position of the craftingResultSlotsParent GameObject to the initial position
        craftingResultSlotsParent.transform.localPosition = craftingResetPositionResult;
        // Reset the local position of the craftingRequiredItemOneSlotsParent GameObject to the initial position
        craftingRequiredItemOneSlotsParent.transform.localPosition = craftingResetPositionRequiredOne;
        // Reset the local position of the craftingRequiredItemTwoSlotsParent GameObject to the initial position
        craftingRequiredItemTwoSlotsParent.transform.localPosition = craftingResetPositionRequiredTwo;
        // Reset the selectedCraftingResultSlot value to 0
        selectedCraftingResultSlot = 0;
    }

    // Counts the number of items the Player can currently craft.
    // Used for resetting the crafting position, clamping the crafting list scroll,
    // enabling/disabling required item backgrounds, and allowing the CraftItem button to be pressed.
    public int CountCurrentCraftableItems()
    {
        int currentCraftableItems = 0;
        for (int i = 0; i < craftingRecipeClasses.Count; i++)
        {
            // Check if the crafting result slot is active and increment the craftable items count
            if (craftingResultSlots[i].gameObject.activeSelf == true)
            { currentCraftableItems++; }
        }
        return currentCraftableItems;
    }

    // Crafts an item if there are craftable items and the inventory is open.
    // Called by the CraftItem button in the Canvas UI under the Inventory.
    public void CraftItem()
    {
        if (CountCurrentCraftableItems() > 0 && inventoryOpen)
        {
            selectedCraft.Craft(this);
        }
    }

    // If there is an InvSlotClass slot in the inventory containing the same item input into the function,
    // it's stackable, and is not at max stack/max quantity, return that InvSlotClass slot.
    // Used when the player picks up tile drops in the world and adds them to slots already containing the tile drop
    // until it reaches the max stack/max quantity.
    public InvSlotClass ContainsStackable(ItemClass item)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i].GetItem() == item && inventoryItems[i].GetItem().stack && inventoryItems[i].GetQuantity() < inventoryItems[i].GetItem().maxStack)
                return inventoryItems[i];
        }

        return null;
    }

    // Checks all inventory and armor slots to see if any contain the input item.
    // If a slot does, return the item. Otherwise, return null.
    // Called by RemoveRequiredItems to check if the inventory contains the items needed for crafting the result item.
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

    // Checks all inventory slots to see if any contain the input item,
    // and that the input quantity is greater than or equal to the slot's quantity.
    // If a slot does, return TRUE. Otherwise, return FALSE.
    // Called by CraftingRecipeClass CanCraft method to check if the inventory contains
    // the item and the proper quantity needed for crafting the result item.
    public bool Contains(ItemClass item, int quantity)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i].GetItem() == item && inventoryItems[i].GetQuantity() >= quantity)
                return true;
        }

        return false;
    }

    // Adds an item to the inventory. Handles stacking, placing items in empty slots, and refreshing the UI.
    // Called in Start Function to add the starting items to the Player's inventory
    // Called when the Player dies to add the starting items to the Player's inventory again
    // Called by the TileDropController Script to pickup tiledrops in the world
    public bool AddToInventory(ItemClass item, int quantity,int startingDurability, int currerntDurability)
    {
        InvSlotClass inventorySlot = ContainsStackable(item);

        // Check if the item can be added to an existing stack.
        if (inventorySlot != null && inventorySlot.GetQuantity() < inventorySlot.GetItem().maxStack)
        {
            int availableSpace = inventorySlot.GetItem().maxStack - inventorySlot.GetQuantity();
            int quantityToAdd = Mathf.Clamp(quantity, 0, availableSpace);
            int remainder = quantity - availableSpace;
            // Add quantity to the item stack of the same type
            inventorySlot.AddQuantity(quantityToAdd);
            // If there is a remainder, call this method again using remainder as quantity
            if (remainder > 0) { AddToInventory(item, remainder, startingDurability, currerntDurability); }
            // Update the inventory UI and return TRUE
            RefreshUI();
            return true;
        }
        // The item can't be added to an existing stack, find an empty slot
        else
        {
            for (int i = 0; i < inventoryItems.Length; i++)
            {
                if (inventoryItems[i].GetItem() == null) // Empty slot
                {
                    int availableSpace = item.GetItem().maxStack - inventoryItems[i].GetQuantity();
                    int quantityToAdd = Mathf.Clamp(quantity, 0, availableSpace);
                    int remainder = quantity - availableSpace;

                    // Add the item to the empty slot
                    inventoryItems[i].AddItem(item, quantityToAdd, startingDurability, currerntDurability);
                    // If there is a remainder, call this method again using remainder as quantity
                    if (remainder > 0) { AddToInventory(item, remainder, startingDurability, currerntDurability); }
                    RefreshUI();
                    // Update the inventory UI and return TRUE
                    return true;
                }
            }
        }
        return false;
    }

    // Creates a tile drop with the specified item, quantity, and durability.
    // Handles instantiation, setting the sprite, and updating the UI.
    // Called by DropItemLeftClick, DropItemRightClick, and DropAllItems
    public void CreateTileDrop(ItemClass item, int quantity, int startingDurability, int currentDurability)
    {
        // Get the chunk that the Player is currently in
        float chunkCoord = (Mathf.Round(player.transform.position.x / terrain.chunkSize) * terrain.chunkSize);
        chunkCoord /= terrain.chunkSize;

        // Instantiates it at the Player's position so it can be 'Thrown' (Have a force added to it) by the TileDropController
        GameObject newTileDrop;
        Vector2 spawnPosition = new Vector2(player.transform.position.x, player.transform.position.y + 0.5f);        
        newTileDrop = Instantiate(tileDropCirclePrefab, spawnPosition, Quaternion.identity);

        // Set the tile drop direction for the TileDropController to 'Throw' the item in that direction
        if (player.GetComponent<PlayerController>().facingDirection == PlayerController.FacingDirection.Right)
        {
            newTileDrop.GetComponent<TileDropController>().playerDroppedRight = true;
        }
        else
        {
            newTileDrop.GetComponent<TileDropController>().playerDroppedLeft = true;
        }

        // Sets the TileDrop as a child of the chunk that the Player is currently in
        newTileDrop.transform.parent = terrain.worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
        // Set it's Sprite to the first Sprite of the Input item's ItemClass
        newTileDrop.GetComponent<SpriteRenderer>().sprite = item.itemSprites[0];
        // Give it a black background/outline using that first Sprite
        newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = item.itemSprites[0];
        // Assign InvSlotClass of the TileDrop to the InvSlotClass of the Input Item
        InvSlotClass tiledrop = new InvSlotClass(item, quantity, startingDurability, currentDurability);
        newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop;

        // Update the inventory UI
        RefreshUI();
    }

    // Subtracts the input quantity from the selectedItem (Player's held item) InvSlotClass by calling the SubtractQuantity method from InvSlotClass
    // and then calls RefreshUI to update the inventory display.
    // Called by ConsumableClass to remove one unit when the player consumes an item.
    // Called by TileClass to remove one unit when the player places tiles.
    public void SubtractSelectedItem(int quantity)
    {
        selectedItem.SubtractQuantity(quantity);
        RefreshUI();
    }

    // Checks if the inventory contains the input item and removes the input quantity from the item's quantity in the inventory.
    // Returns TRUE if successful, otherwise returns FALSE and displays a log message.
    // Called by the Craft function in the CraftingRecipeClass script.
    public bool RemoveRequiredItems(ItemClass item, int quantity)
    {
        // Calls the Contains() method to check if the inventory contains the input item
        InvSlotClass temp = Contains(item);
        if (temp != null)
        {
            // Check if the item's quantity in the inventory is greater than or equal to the input quantity
            if (temp.GetQuantity() >= quantity)
            {
                // Subtract the input quantity from the item's quantity in the inventory
                temp.SubtractQuantity(quantity);
            }
            else
            // Log message for insufficient item quantity and return false
            { Debug.Log("Can't Craft. Not enough"); return false; }
        }
        else
        // Log message for item not present in the inventory and return false
        { Debug.Log("Can't Craft"); return false; }

        // Update the inventory UI and return TRUE
        RefreshUI();
        return true;
    }

    // Finds the inventory slot closest to the player's mouse position, considering all inventory slots, armor slots,
    // or just the hotbar if the inventory is disabled/closed.
    // Returns the closest slot if the mouse position is within 30f of the slot, otherwise returns null.
    // Called by GrabItemLeftClick, GrabItemRightClick, DropItemLeftClick, and DropItemRightClick methods
    private InvSlotClass GetClosestSlot()
    {
        // Check if the inventory is open
        if (transform.GetChild(3).gameObject.activeInHierarchy)
        {
            // Iterate through all inventory slots
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                // Calculate the distance between the slot's transform position and the player's mouse position
                if (Vector2.Distance(inventorySlots[i].transform.position, Input.mousePosition) <= 30)
                {
                    // Return the corresponding InvSlotClass object if the distance is less than or equal to 30
                    return inventoryItems[i];
                }
            }
            // Iterate through all armor slots
            for (int i = 0; i < armorSlots.Length; i++)
            {
                // Calculate the distance between the slot's transform position and the player's mouse position
                if (Vector2.Distance(armorSlots[i].transform.position, Input.mousePosition) <= 30)
                {
                    // Return the corresponding InvSlotClass object if the distance is less than or equal to 30
                    return equippedArmor[i];
                }
            }
        }
        else
        {
            // Iterate through the hotbar slots if the inventory is closed
            for (int i = 0; i < inventoryHotbarParent.transform.childCount; i++)
            {
                // Calculate the distance between the slot's transform position and the player's mouse position
                if (Vector2.Distance(inventorySlots[i].transform.position, Input.mousePosition) <= 30)
                {
                    // Return the corresponding InvSlotClass object if the distance is less than or equal to 30
                    return inventoryItems[i];
                }
            }
        }
        // Return null if no slot meets the distance criteria
        return null;
    }

    // Grabs an item at the player's mouse position, taking all of its quantity from the item stack in the inventory.
    // Called by the Update() method when the player left clicks and is not already grabbing an item from the inventory.
    private void GrabItemLeftClick()
    {
        // Calls the GetClosestSlot() method to get the closest slot based on the player's mouse position
        grabItemOrigin = GetClosestSlot();

        // Return if the grabItemOrigin slot is empty or contains a null item
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return;
        }

        // Play the appropriate sound for removing armor or grabbing an item
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

        // Create a new InvSlotClass with the contents of grabItemOrigin and clear grabItemOrigin
        grabItemMoving = new InvSlotClass(grabItemOrigin);
        grabItemOrigin.Clear();
        // Set grabbingItem to true for use in Update and RefreshItemGrab methods
        grabbingItem = true;
        // Update the inventory UI
        RefreshUI();
    }

    // Grabs an item at the player's mouse position, taking half of its quantity (rounded up) from the item stack in the inventory.
    // Called by the Update() method when the player right clicks and is not already grabbing an item from the inventory.
    private void GrabItemRightClick()
    {
        // Get the closest slot based on the player's mouse position
        grabItemOrigin = GetClosestSlot();

        // Return if the grabItemOrigin slot is empty or contains a null item
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return;
        }

        // Play the appropriate sound for removing armor or grabbing an item
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

        // If the item has only 1 quantity, create a new InvSlotClass with the contents of grabItemOrigin and clear grabItemOrigin
        // Otherwise, create a new InvSlotClass with half of the quantity (rounded up) and remove that quantity from grabItemOrigin
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

        // Set grabbingItem to true for use in Update and RefreshItemGrab
        grabbingItem = true;
        // Update the inventory UI
        RefreshUI();
    }

    // At the player's mouse position, drops or swaps the full item stack in the inventory,
    // or creates a tile drop containing the quantity of the item stack.
    // Called by the Update() method when the player left clicks and is already grabbing an item from the inventory.
    private void DropItemLeftClick()
    {
        // Get the closest inventory slot based on the player's mouse position
        grabItemOrigin = GetClosestSlot();

        // IF the held item is not armor, and the closest slot is an armor slot, log a message and return
        if (grabItemMoving.GetItem().GetArmor() == null)
            for (int i = 0; i < equippedArmor.Length; i++)
            {
                if (grabItemOrigin == equippedArmor[i])
                { Debug.Log("Can only place armor in these slots."); return; }
            }

        // IF the held item is armor and the closest slot is an incorrect armor slot, return
        if (grabItemMoving.GetItem().GetArmor() != null)
        {
            // Check for each type of armor slot
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

        // IF the closest slot is null (empty) and the player's mouse is not in the inventory area, create a tile drop
        if (grabItemOrigin == null && (Input.mousePosition.x >= 600 || Input.mousePosition.y < 450)) // Not dropped on a slot
        {
            CreateTileDrop(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
            grabItemMoving.Clear();
        }
        // IF the closest slot is null (empty) and the player's mouse is in the inventory area, return
        else if (grabItemOrigin == null && Input.mousePosition.x < 700 && Input.mousePosition.y >= 450)
        { return; }
        else
        {
            // Play the appropriate sound based on the slot type
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

            // IF the closest slot contains an item
            if (grabItemOrigin.GetItem() != null)
            {
                // IF the held item is the same as the item in the slot, the item is stackable, and the slot quantity is less than the max stack
                if (grabItemOrigin.GetItem() == grabItemMoving.GetItem()
                    && grabItemOrigin.GetItem().stack
                    && grabItemOrigin.GetQuantity() < grabItemOrigin.GetItem().maxStack)
                {
                    // Determine the available space, quantity to add, and remainder if any
                    int availableSpace = grabItemOrigin.GetItem().maxStack - grabItemOrigin.GetQuantity();
                    int quantityToAdd = Mathf.Clamp(grabItemMoving.GetQuantity(), 0, availableSpace);
                    int remainder = grabItemMoving.GetQuantity() - availableSpace;
                    // Add the quantity to the closest slot and update the held item's quantity
                    grabItemOrigin.AddQuantity(quantityToAdd);
                    if (remainder <= 0) { grabItemMoving.Clear(); }
                    else
                    {
                        grabItemMoving.SubtractQuantity(availableSpace);
                        RefreshUI();
                        return;
                    }
                }
                // ELSE the held item and the item in the slot are different, swap the items
                else
                {
                    changeSlotTemp = new InvSlotClass(grabItemOrigin);
                    grabItemOrigin.AddItem(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
                    grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
                    RefreshUI();
                    return;
                }
            }
            // ELSE the closest slot is empty
            else
            {
                // Add the held item to the empty slot and clear the held item
                grabItemOrigin.AddItem(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
                grabItemMoving.Clear();
            }
        }
        // Set grabbingItem to FALSE and refresh the inventory UI
        grabbingItem = false;
        RefreshUI();
    }

    // At the player's mouse position, swaps all or drops one item from the item stack in the inventory,
    // or creates a tile drop containing one item from the item stack.
    // Called by the Update() method when the player right clicks and is already grabbing an item from the inventory.
    private void DropItemRightClick()
    {
        // Get the closest inventory slot based on the player's mouse position
        grabItemOrigin = GetClosestSlot();

        // IF the held item is not armor and if the closest slot is an armor slot, log a message and return
        if (grabItemMoving.GetItem().GetArmor() == null)
            for (int i = 0; i < equippedArmor.Length; i++)
            {
                if (grabItemOrigin == equippedArmor[i])
                { Debug.Log("Can only place armor in these slots."); return; }
            }

        // IF the held item is armor and if the closest slot is an incorrect armor slot, return
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

        // IF the closest slot is null (empty) and the player's mouse is not in the inventory area,
        // create a tile drop with one item and update held item's quantity
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
        // ELSE IF the closest slot is null (empty) and the player's mouse is in the inventory area, return
        else if (grabItemOrigin == null && Input.mousePosition.x < 700 && Input.mousePosition.y >= 450)
        { return; }

        // Play the appropriate sound based on the slot type using AudioManager
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

        // IF the held item is dropped on an available slot,
        // and the item is not the same as the current item in the slot or the slot is at max stack, swap the items
        if (grabItemOrigin.GetItem() != null // Dropped on an available slot
            && (grabItemOrigin.GetItem() != grabItemMoving.GetItem() // AND Not the same item OR slot is at max stack
            || grabItemOrigin.GetQuantity() >= grabItemOrigin.GetItem().maxStack))
        {
            changeSlotTemp = new InvSlotClass(grabItemOrigin);
            grabItemOrigin.AddItem(grabItemMoving.GetItem(), grabItemMoving.GetQuantity(), grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability());
            grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
            RefreshUI();
            return;
        }

        // IF the held item is the same as the item in the closest slot, add one item to the slot's stack
        if (grabItemOrigin != null && grabItemOrigin.GetItem() == grabItemMoving.GetItem())
        { grabItemOrigin.AddQuantity(1); }
        // ELSE the held item is not the same as the item in the closest slot, add one item from the held stack to the closest slot
        else
        { grabItemOrigin.AddItem(grabItemMoving.GetItem(), 1, grabItemMoving.GetStartingDurability(), grabItemMoving.GetCurrentDurability()); }

        // Subtract one item from the held item's stack
        grabItemMoving.SubtractQuantity(1);

        // IF its quantity is less than 1, clear the held item
        if (grabItemMoving.GetQuantity() < 1)
        { grabbingItem = false; grabItemMoving.Clear(); }
        // ELSE the held item's quantity is greater than or equal to 1, set grabbingItem to TRUE
        else
        { grabbingItem = true; }

        // Refresh the inventory UI
        RefreshUI();
    }

    // Drops all items in the inventory and armor slots as tile drops when the player dies.
    // Called by the PlayerController script in the TakeDamage() method when the player's health is below 0.
    public void DropAllItems()
    {
        // Iterate through all inventory slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // IF the slot contains an item, create a tile drop with the same item, quantity, and durability, then clear the slot
            if (inventoryItems[i].GetItem() != null)
            {
                CreateTileDrop(inventoryItems[i].GetItem(), inventoryItems[i].GetQuantity(), inventoryItems[i].GetStartingDurability(), inventoryItems[i].GetCurrentDurability());
                inventoryItems[i].Clear();
            }
        }

        // Iterate through all equipped armor slots
        for (int i = 0; i < equippedArmor.Length; i++)
        {
            // IF the armor slot contains an item, create a tile drop with the same item, quantity, and durability, then clear the slot
            if (equippedArmor[i].GetItem() != null)
            {
                CreateTileDrop(equippedArmor[i].GetItem(), equippedArmor[i].GetQuantity(), equippedArmor[i].GetStartingDurability(), equippedArmor[i].GetCurrentDurability());
                equippedArmor[i].Clear();
            }
        }
    }
}