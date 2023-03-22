using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewToolClass", menuName = "Item/Tool")]
public class ToolClass : ItemClass
{
    [Header("Tool")]
    public ToolType toolType;
    public enum ToolType
    {
        pickaxe,
        axe,
        hammer,
        weapon
    }
    public int toolStrength = 10;
    public int toolDurability = 100;

    public int toolDamage = 10;

    public override void Use(PlayerController playerCaller) 
    {
        base.Use(playerCaller);
    }
    public override ToolClass GetTool() { return this; }
}
