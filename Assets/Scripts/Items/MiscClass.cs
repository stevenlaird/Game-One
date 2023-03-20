using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMiscClass", menuName = "Item/Misc")]
public class MiscClass : ItemClass
{
    public override void Use(PlayerController playerCaller) { }
    public override MiscClass GetMisc() { return this; }
}