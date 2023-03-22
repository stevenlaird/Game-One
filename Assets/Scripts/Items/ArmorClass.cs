using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArmorClass", menuName = "Item/Armor")]
public class ArmorClass : ItemClass
{
    [Header("Armor")]
    public ArmorType armorType;
    public enum ArmorType
    {
        helmet,
        chestplate,
        leggings,
        gloves,
        boots
    }

    public int armorDurability = 100;

    public override ArmorClass GetArmor() { return this; }
}
