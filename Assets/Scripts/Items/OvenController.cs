using UnityEngine;

public class OvenController : MonoBehaviour
{
    private PlayerController player;    // Reference to the GameObject containing the PlayerController script

    ///////////////////

    // Initialize the player reference at the start of the game
    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player is within range of the oven
        if (Vector2.Distance(this.gameObject.transform.position, player.transform.position) <= 3f)
        {
            // Set the player's inRangeOfOven flag to TRUE
            player.inRangeOfOven = true;
        }
    }
}