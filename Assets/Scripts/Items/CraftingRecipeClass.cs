using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "newCraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeClass : ScriptableObject
{
    [Header("Crafting Recipe")]
    public InvSlotClass[] requiredItems;
    public InvSlotClass craftedItem;
    public bool requireCraftingTable = true;
    public bool requireAnvil = true;
    public bool requireOven = true;
    ///////////////////


    public bool CanCraft(InventoryManager inventory)
    {
        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (!inventory.Contains(requiredItems[i].GetItem(), requiredItems[i].GetQuantity()))
            { return false; }
            if (requireCraftingTable == true && inventory.player.GetComponent<PlayerController>().inRangeOfCraftingTable == false)
            { return false; }
            if (requireAnvil == true && inventory.player.GetComponent<PlayerController>().inRangeOfAnvil == false)
            { return false; }
            if (requireOven == true && inventory.player.GetComponent<PlayerController>().inRangeOfOven == false)
            { return false; }
        }

        return true;
    }

    public void Craft(InventoryManager inventory)
    {
        if (inventory.grabItemMoving.GetItem() == null)
        {
            for (int i = 0; i < requiredItems.Length; i++)
            {
                inventory.RemoveRequiredItems(requiredItems[i].GetItem(), requiredItems[i].GetQuantity());
            }
            inventory.grabItemMoving = new InvSlotClass(craftedItem);
            inventory.grabbingItem = true;
            inventory.RefreshUI();
            Debug.Log("New Craft.");
        }
        else if (inventory.grabItemMoving.GetItem() == craftedItem.GetItem() && craftedItem.GetItem().stack &&
                inventory.grabItemMoving.GetQuantity() + craftedItem.GetQuantity() < craftedItem.GetItem().maxStack)
        {
            for (int i = 0; i < requiredItems.Length; i++)
            {
                inventory.RemoveRequiredItems(requiredItems[i].GetItem(), requiredItems[i].GetQuantity());
            }
            inventory.grabItemMoving.AddQuantity(craftedItem.GetQuantity());
            Debug.Log("Craft added to held stack.");
        }
        else
        {
            Debug.Log("Can't craft, Already holding a different or nonstackable item.");
        }
    }
}
