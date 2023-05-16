using UnityEngine;

[CreateAssetMenu(fileName = "NewRawResourceClass", menuName = "Item/Raw Resource")]
public class RawResourceClass : ItemClass
{
    // Overrides the Use method from the ItemClass for raw resource items (no specific behavior yet)
    public override void Use(PlayerController playerCaller) { }
    // Overrides the GetRawResource method from the ItemClass to return the current raw resource item as a RawResourceClass object
    public override RawResourceClass GetRawResource() { return this; }
}