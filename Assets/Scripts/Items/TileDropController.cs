using UnityEngine;

public class TileDropController : MonoBehaviour
{
    [HideInInspector] public bool playerDroppedLeft;    // Indicates if the player dropped the tile to the left
    [HideInInspector] public bool playerDroppedRight;   // Indicates if the player dropped the tile to the right
    public float dropCounter = 0;                       // Counter for how long the tile has been dropped

    public InvSlotClass invSlotClass;                   // Inventory slot information for the dropped tile

    private InventoryManager inventoryManager;          // Reference to the InventoryManager
    private GameObject player;                          // Reference to the player GameObject
    private GameObject torchLight;                      // Reference to the torch light prefab

    ///////////////////

    private void Start()
    {
        // Find the game object containing the InventoryManager script
        inventoryManager = GameObject.FindObjectOfType<InventoryManager>();
        // Find the game object named Player
        player = GameObject.Find("Player");
        // Load the torch light prefab
        torchLight = Resources.Load<GameObject>("TorchLight");
        // Set the name of the tile GameObject
        this.name = invSlotClass.GetItem().itemName;
        // Check if the tile is a torch
        if (this.name == "Torch")
        {
            // Instantiate the torch light and set it as a child of the tile
            GameObject tileTorchLight = Instantiate(torchLight, this.transform.position, Quaternion.identity);
            tileTorchLight.transform.parent = this.transform;
        }
        // Check if the tile is a crafting table
        if (this.name == "Crafting Table")
        {
            // Scale the tile GameObject
            this.transform.localScale = new Vector2(0.25f, 0.25f);
        }
    }

    private void Update()
    {
        // Increment the drop counter
        dropCounter += Time.deltaTime;

        // Destroy the tile if it has been dropped for more than 5 minutes
        if (dropCounter > 300)
        {
            Destroy(this.gameObject);
        }

        // Get the CircleCollider2D components
        CircleCollider2D[] colliders = this.GetComponents<CircleCollider2D>();
        // Check if the tile has been dropped recently and if by the player
        if (dropCounter < 0.2 && (playerDroppedLeft || playerDroppedRight))
        {
            // Disable the first CircleCollider2D
            colliders[0].enabled = false;
            // Set the velocity based on the direction the player dropped the tile
            if (playerDroppedLeft)
                GetComponent<Rigidbody2D>().velocity = new Vector2(-7f, 3f);
            if (playerDroppedRight)
                GetComponent<Rigidbody2D>().velocity = new Vector2(7f, 3f);
        }
    }

    // OnTriggerEnterStay2D is called when a Collider2D is continuously touching another Collider2D
    private void OnTriggerStay2D(Collider2D collider)
    {
        // Get the CircleCollider2D components
        CircleCollider2D[] colliders = this.GetComponents<CircleCollider2D>();

        // Check if the tile is touching the ground
        if (collider.gameObject.CompareTag("Ground"))
        {
            // Enable the CircleCollider2D components
            this.GetComponent<CircleCollider2D>().enabled = true;
            colliders[0].enabled = true;
            // Freeze the X position of the tile if the drop counter is greater than 1
            if (dropCounter > 1)
            {
                Rigidbody2D freezeX = this.GetComponent<Rigidbody2D>();
                freezeX.constraints = RigidbodyConstraints2D.FreezePositionX;
            }
        }

        // Check if the tile is touching the Player
        if (collider.gameObject.CompareTag("Player"))
        {
            // Check if the tile was dropped by the player
            if (playerDroppedLeft == true || playerDroppedRight == true)
            {
                // Check if the drop counter is greater than 3
                if (dropCounter > 3)
                {
                    // Attempt to add the tile to the player's inventory
                    if (inventoryManager.AddToInventory(invSlotClass.GetItem(), invSlotClass.GetQuantity(), invSlotClass.GetStartingDurability(), invSlotClass.GetCurrentDurability()))
                    {
                        // Destroy the tile and play the inventory pickup sound
                        Destroy(this.gameObject);
                        FindObjectOfType<AudioManager>().Play("Inventory_PickupItem");
                    }
                }
                else
                {
                    // Ignore collisions between the player and tile
                    Physics2D.IgnoreCollision(player.GetComponent<CapsuleCollider2D>(), GetComponent<CircleCollider2D>());
                }
            }
            // If the tile was not dropped by the player
            if (playerDroppedLeft == false && playerDroppedRight == false)
            {
                // Attempt to add the tile to the player's inventory
                if (inventoryManager.AddToInventory(invSlotClass.GetItem(), invSlotClass.GetQuantity(), invSlotClass.GetStartingDurability(), invSlotClass.GetCurrentDurability()))
                {
                    // Destroy the tile and play the inventory pickup sound
                    Destroy(this.gameObject);
                    FindObjectOfType<AudioManager>().Play("Inventory_PickupItem");
                }
                else
                {
                    // Ignore collisions between the player and tile
                    Physics2D.IgnoreCollision (player.GetComponent<CapsuleCollider2D>(), GetComponent<CircleCollider2D>());
                }
            }
        }
    }
}