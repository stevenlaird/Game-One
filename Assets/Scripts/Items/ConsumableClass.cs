using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableClass", menuName = "Item/Consumable")]
public class ConsumableClass : ItemClass
{
    [Header("Consumable")]
    // The amount of health added to the player when the consumable item is used
    public int healthAdded;

    ///////////////////

    // Overrides the Use method from the ItemClass to define consumable-specific behavior
    public override void Use(PlayerController playerCaller)
    {
        // Checks if the player's health is already full, and logs a message if it is
        if (playerCaller.currentHealth == playerCaller.maxHealth)
        {
            Debug.Log("Health is full, can't use.");
            return;
        }

        // Calls the base Use method from the ItemClass
        base.Use(playerCaller);
        // Logs a message indicating that the consumable was used
        Debug.Log("Used Consumable");
        // Plays the "Player_EatConsumable" audio effect
        FindObjectOfType<AudioManager>().Play("Player_EatConsumable");
        // Removes one of the consumed item from the player's inventory
        playerCaller.inventoryManager.SubtractSelectedItem(1);

        // Checks if the player's health after using the consumable will not exceed the maximum health
        if (playerCaller.currentHealth + healthAdded <= playerCaller.maxHealth)
        {
            // Adds the health from the consumable to the player's current health
            playerCaller.currentHealth = playerCaller.currentHealth + healthAdded;
        }
        // ELSE IF the player's health after using the consumable would exceed the maximum health
        else if (playerCaller.currentHealth + healthAdded > playerCaller.maxHealth)
        {
            // Sets the player's health to the maximum health value
            playerCaller.currentHealth = playerCaller.maxHealth;
        }
    }

    // Overrides the GetConsumable method from the ItemClass to return the current consumable as a ConsumableClass object
    public override ConsumableClass GetConsumable() { return this; }
}