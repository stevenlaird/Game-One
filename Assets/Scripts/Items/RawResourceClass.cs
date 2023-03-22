using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRawResourceClass", menuName = "Item/Raw Resource")]
public class RawResourceClass : ItemClass
{
    public override void Use(PlayerController playerCaller) { }
    public override RawResourceClass GetRawResource() { return this; }
}
