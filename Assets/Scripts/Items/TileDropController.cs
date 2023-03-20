using System.Collections;
using UnityEngine;

public class TileDropController : MonoBehaviour
{
    [HideInInspector] public bool playerDroppedLeft;
    [HideInInspector] public bool playerDroppedRight;
    public float dropCounter = 0;
    public InvSlotClass invSlotClass;

    private InventoryManager inventoryManager;
    private GameObject player;
    private GameObject torchLight;

    ///////////////////

    private void Start()
    {
        inventoryManager = GameObject.FindObjectOfType<InventoryManager>();
        player = GameObject.Find("Player");
        torchLight = Resources.Load<GameObject>("TorchLight");
        this.name = invSlotClass.GetItem().itemName;
        if (this.name == "Torch")
        {
            GameObject tileTorchLight = Instantiate(torchLight, this.transform.position, Quaternion.identity);
            tileTorchLight.transform.parent = this.transform;
        }
        if (this.name == "Crafting Table")
        {
            Debug.Log("Crafting Table Tile Drop");
            this.transform.localScale = new Vector2(0.25f, 0.25f);
        }
    }

    private void Update()
    {
        dropCounter += Time.deltaTime;

        if (dropCounter > 300) //tiles only last 5 minutes
        {
            Destroy(this.gameObject); //destroy tile
        }

        CircleCollider2D[] colliders = this.GetComponents<CircleCollider2D>();
        if (dropCounter < 0.2 && (playerDroppedLeft || playerDroppedRight))
        {
            colliders[0].enabled = false;
            if (playerDroppedLeft)
                GetComponent<Rigidbody2D>().velocity = new Vector2(-7f, 3f);
            if (playerDroppedRight)
                GetComponent<Rigidbody2D>().velocity = new Vector2(7f, 3f);
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        CircleCollider2D[] colliders = this.GetComponents<CircleCollider2D>();

        if (collider.gameObject.CompareTag("Ground")) //if tile touches ground
        {
            this.GetComponent<CircleCollider2D>().enabled = true;
            colliders[0].enabled = true;
            if (dropCounter > 1)
            {
                Rigidbody2D freezeX = this.GetComponent<Rigidbody2D>();
                freezeX.constraints = RigidbodyConstraints2D.FreezePositionX;
            }
        }

        if (collider.gameObject.CompareTag("Player")) //if tile touches player
        {
            if (playerDroppedLeft == true || playerDroppedRight == true)
            {
                if (dropCounter > 3)
                {
                    if (inventoryManager.AddToInventory(invSlotClass.GetItem(), invSlotClass.GetQuantity(), invSlotClass.GetStartingDurability(), invSlotClass.GetCurrentDurability())) //add to inventory
                    {
                        Destroy(this.gameObject); //destroy tile
                        FindObjectOfType<AudioManager>().Play("Inventory_PickupItem");
                    }
                }
                else
                {
                    Physics2D.IgnoreCollision(player.GetComponent<CapsuleCollider2D>(), GetComponent<CircleCollider2D>());
                }
            }
            if (playerDroppedLeft == false && playerDroppedRight == false)
            {
                if (inventoryManager.AddToInventory(invSlotClass.GetItem(), invSlotClass.GetQuantity(), invSlotClass.GetStartingDurability(), invSlotClass.GetCurrentDurability())) //add to inventory
                {
                    Destroy(this.gameObject); //destroy tile
                    FindObjectOfType<AudioManager>().Play("Inventory_PickupItem");
                }
                else
                    Physics2D.IgnoreCollision (player.GetComponent<CapsuleCollider2D>(), GetComponent<CircleCollider2D>());
            }
        }
    }
}
