using UnityEngine;

[CreateAssetMenu(fileName = "NewArmorClass", menuName = "Item/Armor")]
public class ArmorClass : ItemClass
{
    [Header("Armor")]
    // Type of armor (helmet, chestplate, leggings, gloves, boots)
    public ArmorType armorType;
    public enum ArmorType
    {
        helmet,
        chestplate,
        leggings,
        gloves,
        boots
    }
    // Durability of the armor
    public int armorDurability = 100;

    ///////////////////

    // Overrides the GetArmor method from the ItemClass to return the current armor as an ArmorClass object
    public override ArmorClass GetArmor() { return this; }
}