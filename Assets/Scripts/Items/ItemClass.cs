using System.Collections;
using UnityEngine;

public class ItemClass : ScriptableObject
{
    [Header("Item Properties")]
    public string itemName;
    public Sprite[] itemSprites;
    public bool stack = true;
    public int maxStack = 99;

    public virtual void Use(PlayerController playerCaller)
    {
        Debug.Log("Used Item");
    }
    public virtual ItemClass GetItem() { return this; }
    public virtual ArmorClass GetArmor() { return null; }
    public virtual ConsumableClass GetConsumable() { return null; }
    public virtual MiscClass GetMisc() { return null; }
    public virtual RawResourceClass GetRawResource() { return null; }
    public virtual TileClass GetTile() { return null; }
    public virtual ToolClass GetTool() { return null; }
}
