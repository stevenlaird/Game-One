using UnityEngine;

public class ItemClass : ScriptableObject
{
    [Header("Item Properties")]
    public string itemName;         // Name of the item
    public Sprite[] itemSprites;    // Array of item sprites
    public bool stack = true;       // Toggle for if the item is stackable or not
    public int maxStack = 99;       // Maximum stack size of the item

    // Use the item (default implementation logs "Used Item")
    public virtual void Use(PlayerController playerCaller)
    {
        Debug.Log("Used Item");
    }

    // Returns the current item
    public virtual ItemClass GetItem() { return this; }
    // Returns the item as an ArmorClass object if it is an armor, otherwise returns null
    public virtual ArmorClass GetArmor() { return null; }
    // Returns the item as a ConsumableClass object if it is a consumable, otherwise returns null
    public virtual ConsumableClass GetConsumable() { return null; }
    // Returns the item as a MiscClass object if it is a miscellaneous item, otherwise returns null
    public virtual MiscClass GetMisc() { return null; }
    // Returns the item as a RawResourceClass object if it is a raw resource, otherwise returns null
    public virtual RawResourceClass GetRawResource() { return null; }
    // Returns the item as a TileClass object if it is a tile, otherwise returns null
    public virtual TileClass GetTile() { return null; }
    // Returns the item as a ToolClass object if it is a tool, otherwise returns null
    public virtual ToolClass GetTool() { return null; }
}