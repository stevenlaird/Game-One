using UnityEngine;

[CreateAssetMenu(fileName = "newCraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeClass : ScriptableObject
{
    [Header("Crafting Recipe")]
    // Define the required items for the crafting recipe
    public InvSlotClass[] requiredItems;
    // Define the crafted item as an inventory slot object
    public InvSlotClass craftedItem;
    // Define the crafting station requirements for the recipe
    public bool requireCraftingTable = true;
    public bool requireAnvil = true;
    public bool requireOven = true;

    ///////////////////

    // Check if the player has the necessary items in their inventory and is within range of the required crafting stations
    public bool CanCraft(InventoryManager inventory)
    {
        // Loop through all required items.
        for (int i = 0; i < requiredItems.Length; i++)
        {
            // Check if the inventory contains the required item and quantity
            if (!inventory.Contains(requiredItems[i].GetItem(), requiredItems[i].GetQuantity()))
            { return false; }
            // Check if the player is in range of a crafting table
            if (requireCraftingTable == true && inventory.player.GetComponent<PlayerController>().inRangeOfCraftingTable == false)
            { return false; }
            // Check if the player is in range of an anvil
            if (requireAnvil == true && inventory.player.GetComponent<PlayerController>().inRangeOfAnvil == false)
            { return false; }
            // Check if the player is in range of an oven
            if (requireOven == true && inventory.player.GetComponent<PlayerController>().inRangeOfOven == false)
            { return false; }
        }
        // IF all conditions are met, return true
        return true;
    }

    // Craft the item by removing the required items from the player's inventory and adding the crafted item
    public void Craft(InventoryManager inventory)
    {
        if (inventory.grabItemMoving.GetItem() == null)
        // Check if the player is not holding an item
        {
            // Loop through all required items and remove them from the inventory
            for (int i = 0; i < requiredItems.Length; i++)
            {
                inventory.RemoveRequiredItems(requiredItems[i].GetItem(), requiredItems[i].GetQuantity());
            }
            // Set the crafted item as the grabbed item
            inventory.grabItemMoving = new InvSlotClass(craftedItem);
            inventory.grabbingItem = true;
            // Refresh the inventory UI and log a message to the console
            inventory.RefreshUI();
            Debug.Log("New Craft.");
        }
        else if (inventory.grabItemMoving.GetItem() == craftedItem.GetItem() && craftedItem.GetItem().stack &&
                inventory.grabItemMoving.GetQuantity() + craftedItem.GetQuantity() < craftedItem.GetItem().maxStack)
        // Check if the player is holding the same item and it is stackable
        {
            // Loop through all required items and remove them from the inventory
            for (int i = 0; i < requiredItems.Length; i++)
            {
                inventory.RemoveRequiredItems(requiredItems[i].GetItem(), requiredItems[i].GetQuantity());
            }
            // Add the crafted item's quantity to the held item stack
            inventory.grabItemMoving.AddQuantity(craftedItem.GetQuantity());
            Debug.Log("Craft added to held stack.");
        }
        else
        // If the player is holding a different or non-stackable item, display a message to the console
        {
            Debug.Log("Can't craft, Already holding a different or nonstackable item.");
        }
    }
}