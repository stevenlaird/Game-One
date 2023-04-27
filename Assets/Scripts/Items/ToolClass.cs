using UnityEngine;

[CreateAssetMenu(fileName = "NewToolClass", menuName = "Item/Tool")]
public class ToolClass : ItemClass
{
    [Header("Tool")]
    // The type of tool (pickaxe, axe, hammer, weapon)
    public ToolType toolType;
    public enum ToolType
    {
        pickaxe,
        axe,
        hammer,
        weapon
    }
    // The strength of the tool, affecting its efficiency
    public int toolStrength = 10;
    // The durability of the tool, affecting how long it lasts before breaking
    public int toolDurability = 100;
    // The damage dealt by the tool when used as a weapon
    public int toolDamage = 10;

    ///////////////////

    // Overrides the Use method from the ItemClass for tool items, applying the effects of the tool when used by the player
    public override void Use(PlayerController playerCaller) 
    {
        base.Use(playerCaller);
    }
    // Overrides the GetTool method from the ItemClass to return the current tool as a ToolClass object
    public override ToolClass GetTool() { return this; }
}