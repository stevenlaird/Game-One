using UnityEngine;

[CreateAssetMenu(fileName = "NewMiscClass", menuName = "Item/Misc")]
public class MiscClass : ItemClass
{
    // Overrides the Use method from the ItemClass for misc items (no specific behavior yet)
    public override void Use(PlayerController playerCaller) { }
    // Overrides the GetMisc method from the ItemClass to return the current misc item as a MiscClass object
    public override MiscClass GetMisc() { return this; }
}