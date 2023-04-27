using UnityEngine;

[System.Serializable]
public class InvSlotClass
{
    [SerializeField] private ItemClass item;
    [SerializeField] private int quantity;
    [SerializeField] private int startingDurability;
    [SerializeField] private int currentDurability;

    ///////////////////

    // Initializes a new inventory slot with the specified item, quantity, starting durability, and current durability.
    public InvSlotClass(ItemClass _item, int _quantity, int _startingDurability, int _currentDurability)
    {
        item = _item;
        quantity = _quantity;
        startingDurability = _startingDurability;
        currentDurability = _currentDurability;
    }

    // Initializes a new inventory slot based on an existing inventory slot.
    public InvSlotClass(InvSlotClass invSlot)
    {
        item = invSlot.GetItem();
        quantity = invSlot.GetQuantity();
        currentDurability = invSlot.GetCurrentDurability();
        startingDurability = invSlot.GetStartingDurability();
    }

    // Initializes an empty inventory slot.
    public InvSlotClass()
    {
        item = null;
        quantity = 0;
        currentDurability = 0;
    }

    // Clears the inventory slot by setting all properties to their default values.
    public void Clear()
    {
        this.item = null;
        this.quantity = 0;
        this.startingDurability = 0;
        this.currentDurability = 0;
    }

    // Getters and setters for properties
    public ItemClass GetItem() 
    { return item; }
    public int GetQuantity() 
    { return quantity; }
    public int GetCurrentDurability() 
    { return currentDurability; }
    public int GetStartingDurability() 
    { return startingDurability; }
    public void AddQuantity(int _quantity) 
    { quantity += _quantity; }
    public void SubtractQuantity(int _quantity) 
    { quantity -= _quantity; if (quantity <= 0) { Clear(); } }
    public void SubtractDurability(int _quantity) 
    { currentDurability -= _quantity; if (currentDurability <= 0) { Clear(); } }
    public void AddItem(ItemClass _item, int _quantity, int _startingDurability, int _currentDurability) 
    { item = _item; quantity = _quantity; startingDurability = _startingDurability; currentDurability = _currentDurability; }
}