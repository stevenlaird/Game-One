using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StorageChestController : MonoBehaviour
{
    // InventoryManager reference and load barrel sprites
    private InventoryManager inventory;
    private Sprite storageSpriteClosed;
    private Sprite storageSpriteOpened;

    // Storage variables
    public bool storageInRange;
    public bool storageEmpty;

    // Storage item variables
    private GameObject storageParent;
    public InvSlotClass[] storageItems;
    private GameObject[] storageSlots;

    // Variables used when grabbing or changing items
    private InvSlotClass grabItemOrigin;
    private InvSlotClass changeSlotTemp;

    ///////////////////

    void Start()
    {
        // Assign InventoryManager instance to the inventory variable
        inventory = InventoryManager.FindObjectOfType<InventoryManager>();

        // Load closed and opened barrel sprites from Resources
        storageSpriteClosed = Resources.Load<Sprite>("StorageChest");
        storageSpriteOpened = Resources.Load<Sprite>("StorageChestOpen");

        // Initialize storageInRange and storageEmpty variables
        storageInRange = false;
        storageEmpty = true;

        // Set up the StorageChest tag and BoxCollider2D component
        this.tag = "StorageChest";
        this.AddComponent<BoxCollider2D>().isTrigger = true;
        this.GetComponent<BoxCollider2D>().size = new Vector2(2, 1);

        // Assign the storageParent GameObject
        storageParent = inventory.transform.GetChild(3).gameObject.transform.GetChild(7).gameObject;

        // Initialize storageSlots and storageItems arrays
        storageSlots = new GameObject[storageParent.transform.childCount];
        storageItems = new InvSlotClass[storageSlots.Length];
        for (int i = 0; i < storageParent.transform.childCount; i++)
        {
            storageSlots[i] = storageParent.transform.GetChild(i).gameObject;
        }
        for (int i = 0; i < storageItems.Length; i++)
        {
            storageItems[i] = new InvSlotClass();
        }

    }

    void Update()
    {
        // Call HighlightHoveredSlot method to update the appearance of storage slots
        HighlightHoveredSlot();

        // IF storage is in range and the storage UI is active, call LoadStorageItems and CheckIfEmpty methods
        if (storageInRange && inventory.transform.GetChild(3).transform.GetChild(7).gameObject.activeSelf == true)
        {
            LoadStorageItems();
            CheckIfEmpty();

            // Process left and right mouse button clicks for grabbing and dropping items
            if (Input.GetMouseButtonDown(0))        // Left click
            {
                if (inventory.grabbingItem)
                {
                    DropItemLeftClick();
                }
                else
                {
                    GrabItemLeftClick();
                }
            }
            else if (Input.GetMouseButtonDown(1))   // Right click
            {
                if (inventory.grabbingItem)
                {
                    DropItemRightClick();
                }
                else
                {
                    GrabItemRightClick();
                }
            }
        }
    }

    private void LoadStorageItems()
    {
        // Iterate through storageSlots and storageItems
        // Set up the appearance of the storage slot based on the item and its quantity
        // Display item durability if the item is a tool or armor
        // Hide durability bar if the item is not a tool or armor or has full durability
        for (int i = 0; i < storageSlots.Length; i++)
        {
            try
            {
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = storageItems[i].GetItem().itemSprites[0];

                if (storageItems[i].GetQuantity() == 1)
                    storageSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                else
                    storageSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = storageItems[i].GetQuantity() + "";
                if (storageItems[i].GetItem().GetTool() || storageItems[i].GetItem().GetArmor())
                {
                    storageSlots[i].transform.GetChild(2).gameObject.SetActive(true);
                    storageSlots[i].transform.GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()), 1, 1);
                    if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) == 1)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.white;
                        storageSlots[i].transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) <= .99 && ((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) > .50)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.green;
                    }
                    if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) <= .50 && ((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) > .25)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.yellow;
                    }
                    if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) <= .25)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.red;
                    }
                }
                else
                    storageSlots[i].transform.GetChild(2).gameObject.SetActive(false);
            }
            catch
            {
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                storageSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                storageSlots[i].transform.GetChild(2).gameObject.SetActive(false);
            }
        }
    }

    private void CheckIfEmpty()
    {
        // Set storageEmpty to true by default
        storageEmpty = true;

        // Iterate through storageItems to check if any slot has an item
        for (int i = 0; i < storageItems.Length; i++)
        {
            // If any slot contains an item, set storageEmpty to false
            if (storageItems[i].GetItem() != null)
            {
                storageEmpty = false;
            }
        }
    }

    public void HighlightHoveredSlot()
    {
        // Iterate through storageSlots
        for (int i = 0; i < storageSlots.Length; i++)
        {
            // Check the distance between the mouse cursor and the storage slot
            // Change the storage slot's appearance based on whether it is being hovered over
            if (Vector2.Distance(storageSlots[i].transform.position, Input.mousePosition) <= 29)
            {
                storageSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlotHover");
            }
            else
            {
                storageSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlot");
            }
        }
    }

    public InvSlotClass Contains(ItemClass item)
    {
        // Iterate through storageItems
        for (int i = 0; i < storageItems.Length; i++)
        {
            // Check if the current storage slot contains the item
            if (storageItems[i].GetItem() == item)
            {
                // If the item is found, return the storage slot
                return storageItems[i];
            }
        }
        // If the item is not found, return null
        return null;
    }

    private InvSlotClass GetClosestSlot()
    {
        // Iterate through storageSlots
        for (int i = 0; i < storageSlots.Length; i++)
        {
            // Calculate the distance between the storage slot and the mouse cursor
            if (Vector2.Distance(storageSlots[i].transform.position, Input.mousePosition) <= 30)
            {
                // If the distance is within a certain range, return the corresponding storage slot
                return storageItems[i];
            }
        }
        // If no storage slot is within range, return null
        return null;
    }

    private bool GrabItemLeftClick()
    {
        // Get the closest storage slot to the mouse cursor using the GetClosestSlot method
        grabItemOrigin = GetClosestSlot();
        // IF the closest storage slot is empty, return false (indicating that no item was grabbed)
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return false;
        }
        // Grab the entire stack of items in the slot and store it in the inventory's grabbed item
        inventory.grabItemMoving = new InvSlotClass(grabItemOrigin);
        // Clear the slot by setting the slot's item and count to null and 0 respectively
        grabItemOrigin.Clear();
        // Indicate that the player is holding an item by setting the inventory's grabbingItem variable to true
        inventory.grabbingItem = true;
        // Return true to indicate that the item was successfully grabbed
        return true;
    }

    private bool GrabItemRightClick()
    {
        // Get the closest storage slot to the mouse cursor using the GetClosestSlot method
        grabItemOrigin = GetClosestSlot();
        // Check if the closest slot is empty, and return false if it is
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return false;
        }
        // Check if the slot contains an item with a quantity of 1, grab it, and clear the slot
        if (grabItemOrigin.GetQuantity() == 1)
        {
            inventory.grabItemMoving = new InvSlotClass(grabItemOrigin);
            grabItemOrigin.Clear();
        }
        // Otherwise, grab half of the stack and set inventory.grabbingItem to true
        else
        {
            inventory.grabItemMoving = new InvSlotClass(grabItemOrigin.GetItem(), Mathf.CeilToInt(grabItemOrigin.GetQuantity() / 2f), grabItemOrigin.GetStartingDurability(), grabItemOrigin.GetCurrentDurability());
            grabItemOrigin.SubtractQuantity(Mathf.CeilToInt(grabItemOrigin.GetQuantity() / 2f));
        }
        // Update grabbingItem variable, refresh inventory UI, and return true to indicate that the item was successfully grabbed
        inventory.grabbingItem = true;
        inventory.RefreshUI();
        return true;
    }

    private bool DropItemLeftClick()
    {
        // Get the closest storage slot to the mouse cursor using the GetClosestSlot method
        grabItemOrigin = GetClosestSlot();

        // Check if the closest slot is null and return false if it is (indicating that the item could not be dropped)
        if (grabItemOrigin == null)
        { return false; }

        // Handle different cases for dropping items based on the contents of the closest slot
        if (grabItemOrigin.GetItem() != null)
        {
            // Check if the closest slot contains the same item type and the item is stackable and not at max stack size
            if (grabItemOrigin.GetItem() == inventory.grabItemMoving.GetItem()          // Grabbed item is same as item where dropped
                && grabItemOrigin.GetItem().stack                                       // Grabbed item is stackable
                && grabItemOrigin.GetQuantity() < grabItemOrigin.GetItem().maxStack)    // Grabbed item quantity is less than a max stack of the item
            {
                // Add the grabbed stack size to the existing stack, considering the item's max stack size
                // Update the inventory's grabbed item count accordingly
                // If the entire grabbed stack was added, set the inventory's grabbed item and count to null and 0 respectively
                int availableSpace = grabItemOrigin.GetItem().maxStack - grabItemOrigin.GetQuantity();
                int quantityToAdd = Mathf.Clamp(inventory.grabItemMoving.GetQuantity(), 0, availableSpace);
                int remainder = inventory.grabItemMoving.GetQuantity() - availableSpace;
                grabItemOrigin.AddQuantity(quantityToAdd);
                if (remainder <= 0) { inventory.grabItemMoving.Clear(); }
                else
                {
                    inventory.grabItemMoving.SubtractQuantity(availableSpace);
                    inventory.RefreshUI();
                    return false;
                }
            }
            // ELSE IF the closest slot contains a different item, swap the grabbed item with the slot's item
            else
            {
                changeSlotTemp = new InvSlotClass(grabItemOrigin);
                grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), inventory.grabItemMoving.GetQuantity(), inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability());
                inventory.grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
                inventory.RefreshUI();
                return true;
            }
        }
        else // Drop item in empty slot
        {
            grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), inventory.grabItemMoving.GetQuantity(), inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability());
            inventory.grabItemMoving.Clear();
        }

        // Update grabbingItem variable, Refresh inventory UI, and return TRUE to indicate that the item was successfully dropped
        inventory.grabbingItem = false;
        inventory.RefreshUI();
        return true;
    }

    private bool DropItemRightClick()
    {
        // Get the closest storage slot to the mouse cursor using the GetClosestSlot method
        grabItemOrigin = GetClosestSlot();

        // Check if the closest slot is null, return false to indicate that the item could not be dropped
        if (grabItemOrigin == null)
        { return false; }

        // Check if the closest slot is available and not the same item or at max quantity
        if (grabItemOrigin.GetItem() != null                                        // Dropped on an available slot
            && (grabItemOrigin.GetItem() != inventory.grabItemMoving.GetItem()      // Not the same item OR location is at max stack
            || grabItemOrigin.GetQuantity() >= grabItemOrigin.GetItem().maxStack))
        {
            // Place one item from the grabbed stack into the slot
            changeSlotTemp = new InvSlotClass(grabItemOrigin);
            grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), inventory.grabItemMoving.GetQuantity(), inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability());
            inventory.grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
            inventory.RefreshUI();
            return true;
        }

        // Check if the closest slot contains the same item, add one item from the grabbed stack to the slot
        if (grabItemOrigin != null && grabItemOrigin.GetItem() == inventory.grabItemMoving.GetItem())
        { grabItemOrigin.AddQuantity(1); }
        // Otherwise, add one item from the grabbed stack to the empty slot
        else
        { grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), 1, inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability()); }

        // Subtract one item from the grabbed stack
        inventory.grabItemMoving.SubtractQuantity(1);

        // Set the inventory's grabbingItem variable to false if the player is no longer holding an item
        if (inventory.grabItemMoving.GetQuantity() < 1)
        { inventory.grabbingItem = false; inventory.grabItemMoving.Clear(); }
        else
        { inventory.grabbingItem = true; }

        // Refresh inventory UI and return TRUE to indicate that the item was successfully dropped
        inventory.RefreshUI();
        return true;
    }

    private void OnMouseUpAsButton()
    {
        // Check if the storage is within range of the player
        if (storageInRange)
        {
            // Change the storage sprite to the opened version
            this.GetComponent<SpriteRenderer>().sprite = storageSpriteOpened;

            // Update the inventory UI to display the appropriate elements for the opened storage
            inventory.transform.GetChild(3).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(0).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(3).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(4).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(5).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(6).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(7).gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Check if the collider is tagged as the "Player"
        if (collider.gameObject.CompareTag("Player"))
        {
            // Set storageInRange to true when the player enters the trigger area
            storageInRange = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        // Check if the collider is tagged as the "Player"
        if (collider.gameObject.CompareTag("Player"))
        {
            // Keep storageInRange as true while the player stays within the trigger area
            storageInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        // Check if the collider is tagged as the "Player"
        if (collider.gameObject.CompareTag("Player"))
        {
            // Set storageInRange to false when the player exits the trigger area
            storageInRange = false;
            // Change the storage sprite to the closed version
            this.GetComponent<SpriteRenderer>().sprite = storageSpriteClosed;
            // Update the inventory UI to display the appropriate elements for closing the storage
            inventory.transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(3).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(4).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(5).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(7).gameObject.SetActive(false);
        }
    }
}