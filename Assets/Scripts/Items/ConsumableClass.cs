using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableClass", menuName = "Item/Consumable")]
public class ConsumableClass : ItemClass
{
    [Header("Consumable")]
    public int healthAdded;
    public override void Use(PlayerController playerCaller)
    {
        if (playerCaller.currentHealth == playerCaller.maxHealth)
        {
            Debug.Log("Health is full, can't use.");
            return;
        }

        base.Use(playerCaller);
        Debug.Log("Used Consumable");
        FindObjectOfType<AudioManager>().Play("Player_EatConsumable");
        playerCaller.inventoryManager.SubtractSelectedItem(1);

        if (playerCaller.currentHealth + healthAdded <= playerCaller.maxHealth)
        {
            playerCaller.currentHealth = playerCaller.currentHealth + healthAdded;
        }
        else if (playerCaller.currentHealth + healthAdded > playerCaller.maxHealth)
        {
            playerCaller.currentHealth = playerCaller.maxHealth;
        }
    }
    public override ConsumableClass GetConsumable() { return this; }
}
