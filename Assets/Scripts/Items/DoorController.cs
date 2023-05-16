using UnityEngine;

public class DoorController : MonoBehaviour
{
    // Reference to the PlayerController script
    private PlayerController player;

    // Sprites for the different door states
    public Sprite doorClosedSprite;
    public Sprite doorOpenLeftSprite;
    public Sprite doorOpenRightSprite;

    // GameObjects for the door player and blocked checks
    private GameObject doorPlayerCheck;
    private GameObject doorBlockedCheck;

    // BoxColliders for the door and clicker
    private BoxCollider2D doorCollider;
    private BoxCollider2D doorClicker;

    // GameObjects for the door's left and right player and blocked checks
    private GameObject doorPlayerCheckLeft;
    private GameObject doorPlayerCheckRight;
    private GameObject doorBlockedCheckLeft;
    private GameObject doorBlockedCheckRight;

    // BOOLS to track whether there are ground tiles blocking the door on the left or right side, 
    [SerializeField] private bool groundTilesBlockingRight;
    [SerializeField] private bool groundTilesBlockingLeft;
    // whether the player is in range of the door, 
    [SerializeField] private bool playerEnteringDoorLeft;
    [SerializeField] private bool playerEnteringDoorRight;
    // and whether the player is to the left or right of the door
    [SerializeField] private bool playerInRangeOfDoor;
    [SerializeField] private bool playerLeftOfDoor;

    // Enum to track the door's state
    [SerializeField] private DoorState doorState;
    [SerializeField] private enum DoorState
    {
        OpenLeft,
        OpenRight,
        Closed
    }

    ///////////////////

    void Start()
    {
        // Find the PlayerController script in the scene
        player = PlayerController.FindObjectOfType<PlayerController>();

        // Load the Door Sprites from 'Sprites/Resources'
        doorClosedSprite = Resources.Load<Sprite>("WoodDoor_Closed");
        doorOpenLeftSprite = Resources.Load<Sprite>("WoodDoor_OpenLeft");
        doorOpenRightSprite = Resources.Load<Sprite>("WoodDoor_OpenRight");
        // Load the Door player and blocked check GameObjects from 'Prefabs/Resources'
        doorPlayerCheck = Resources.Load<GameObject>("DoorPlayerEnterCheck");
        doorBlockedCheck = Resources.Load<GameObject>("DoorBlockedCheck");

        // Add BoxCollider2D that will prevent Player & Enemy movement when enabled
        doorCollider = this.gameObject.AddComponent<BoxCollider2D>();
        // Set the Size of doorCollider's BoxCollider2D
        doorCollider.size = new Vector2(0.25f, 2.0f);
        // Add BoxCollider2D for the Player to click on
        doorClicker = this.gameObject.AddComponent<BoxCollider2D>();
        // Enable isTrigger of doorClicker's BoxCollider2D so it can be clicked
        doorClicker.isTrigger = true;

        // Instantiate Left and Right checks for the Player and Tiles
        // Parent them to this door
        doorPlayerCheckLeft = Instantiate(doorPlayerCheck, new Vector2(this.transform.position.x - 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);
        doorPlayerCheckRight = Instantiate(doorPlayerCheck, new Vector2(this.transform.position.x + 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);
        doorPlayerCheckLeft.transform.parent = this.gameObject.transform;
        doorPlayerCheckRight.transform.parent = this.gameObject.transform;

        doorBlockedCheckLeft = Instantiate(doorBlockedCheck, new Vector2(this.transform.position.x - 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);
        doorBlockedCheckRight = Instantiate(doorBlockedCheck, new Vector2(this.transform.position.x + 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);
        doorBlockedCheckLeft.transform.parent = this.gameObject.transform;
        doorBlockedCheckRight.transform.parent = this.gameObject.transform;

        // Spawn the Door Closed
        CloseDoor();
    }

    void Update()
    {
        // IF the Player's X position in world is less than this Door's position in world
        if (player.transform.position.x < this.gameObject.transform.position.x)
            // playerLeftOfDoor BOOL set to TRUE. Player is to the Left of the Door
            { playerLeftOfDoor = true; }
        // ELSE the Player's X position in world is more than or equal to this Door's position in world
        else
            // playerLeftOfDoor BOOL set to FALSE. Player is to the Right of the Door
            { playerLeftOfDoor = false; }

        // IF the distance between this Door and Player is less than or equal to 5f
        if (Vector2.Distance(this.gameObject.transform.position, player.transform.position) <= 5f)
            // playerInRangeOfDoor BOOL set to TRUE. Player is in range and will be able to click on this Door to Open/Close it
            { playerInRangeOfDoor = true; }
        // ELSE the distance between this Door and Player is more than 5f
        else
            // playerInRangeOfDoor BOOL set to FALSE. Player is not in range and won't be able to click on this Door to Open/Close it
            { playerInRangeOfDoor = false; }

        // Update BOOLs from Children's Component's
        groundTilesBlockingLeft = doorBlockedCheckLeft.GetComponent<DoorBlockedCheck>().blockingDoor;
        groundTilesBlockingRight = doorBlockedCheckRight.GetComponent<DoorBlockedCheck>().blockingDoor;
        playerEnteringDoorLeft = doorPlayerCheckLeft.GetComponent<DoorPlayerEnterCheck>().playerEnteringDoor;
        playerEnteringDoorRight = doorPlayerCheckRight.GetComponent<DoorPlayerEnterCheck>().playerEnteringDoor;

        // IF there is a ground tile blocking the Left of this Door AND Door is Open to Left
        if (groundTilesBlockingLeft && doorState == DoorState.OpenLeft)
        {
            CloseDoor();// Close this Door
        }
        // IF there is a ground tile blocking the Right of this Door AND Door is Open to Right
        if (groundTilesBlockingRight && doorState == DoorState.OpenRight)
        {
            CloseDoor();// Close this Door
        }

        // IF this Door is Closed
        if (doorState == DoorState.Closed)
        {
            // IF playerEnteringDoorLeft detects Player AND Player is inputting movement to the Right
            if (playerEnteringDoorLeft && player.horizontal > 0.01f)
            {
                // IF the Door is blocked by tiles on the right, AND not the left, AND is closed
                if (!groundTilesBlockingLeft && groundTilesBlockingRight)
                {
                    OpenDoorLeft();// Open Door to the Left / Towards Player
                }
                // ELSE IF the Door is not blocked by ground tiles, and is closed
                else if (!groundTilesBlockingRight && doorState == DoorState.Closed)
                {
                    OpenDoorRight();// Open Door to the Right / Away from Player
                }
            }
            // IF playerEnteringDoorRight detects Player AND Player is inputting movement to the Left
            if (playerEnteringDoorRight && player.horizontal < -0.01f)
            {
                // IF the Door is blocked by tiles on the left, AND not the right, AND is closed
                if (groundTilesBlockingLeft && !groundTilesBlockingRight)
                {
                    OpenDoorRight();// Open Door to the Right / Towards Player
                }
                // ELSE IF the Door is not blocked by ground tiles on the Left, and is closed
                else if (!groundTilesBlockingLeft && doorState == DoorState.Closed)
                {
                    OpenDoorLeft();// Open Door to the Left / Away from Player
                }
            }

        }

        // IF this Door is Open to the Left
        if (doorState == DoorState.OpenLeft)
        {
            // IF playerEnteringDoorLeft detects Player AND Player is inputting movement to the Left
            if (playerEnteringDoorLeft && player.horizontal < -0.01f)
            {
                CloseDoor();// Close this Door
            }
        }
        // IF this Door is Open to the Right
        if (doorState == DoorState.OpenRight)
        {
            // IF playerEnteringDoorRight detects Player AND Player is inputting movement to the Right
            if (playerEnteringDoorRight && player.horizontal > 0.01f)
            {
                CloseDoor();// Close this Door
            }
        }
    }

    private void OpenDoorLeft()
    {
        doorCollider.enabled = false;                                               // Disable this Door's box collider to enable Player & Enemy movement through Door
        doorClicker.size = new Vector2(1.0f, 2.0f);                                 // Resize doorClicker to fill doorOpenLeftSprite
        doorClicker.offset = new Vector2(-0.4f, 0.5f);                              // Offset doorClicker to fill doorOpenLeftSprite
        this.gameObject.GetComponent<SpriteRenderer>().sprite = doorOpenLeftSprite; // Change the current sprite so the Door is Opened to the Left
        doorState = DoorState.OpenLeft;                                             // Update doorState to OpenLeft
    }

    private void OpenDoorRight()
    {
        doorCollider.enabled = false;                                               // Disable this Door's box collider to enable Player & Enemy movement through Door
        doorClicker.size = new Vector2(1.0f, 2.0f);                                 // Resize doorClicker to fill doorOpenRightSprite
        doorClicker.offset = new Vector2(0.4f, 0.5f);                               // Offset doorClicker to fill doorOpenRightSprite
        this.gameObject.GetComponent<SpriteRenderer>().sprite = doorOpenRightSprite;// Change the current sprite so the Door is Opened to the Right
        doorState = DoorState.OpenRight;                                            // Update doorState to OpenRight
    }

    private void CloseDoor()
    {
        doorCollider.enabled = true;                                                // Enable this Door's box collider to block Player & Enemy movement through Door
        doorClicker.size = new Vector2(0.5f, 2.0f);                                 // Resize doorClicker to fill doorClosedSprite
        doorClicker.offset = new Vector2(0.0f, 0.5f);                               // Offset doorClicker to fill doorClosedSprite
        this.gameObject.GetComponent<SpriteRenderer>().sprite = doorClosedSprite;   // Change the current sprite so the Door is Closed
        doorState = DoorState.Closed;                                               // Update doorState to Closed
    }

    private void OnMouseUpAsButton()// When a click AND release are detected on this GameObject's collider
    {
        // IF playerInRangeOfDoor BOOL is TRUE. Player is within range of this Door
        if (playerInRangeOfDoor)
        {
            // IF this Door is Open either Left or Right
            if (doorState == DoorState.OpenLeft || doorState == DoorState.OpenRight)
            {
                CloseDoor();// Close this Door
            }
            // ELSE IF this Door is Closed
            else if (doorState == DoorState.Closed)
            {
                // IF there are ground tiles blocking the Left AND Right of this Door
                if (groundTilesBlockingLeft && groundTilesBlockingRight)
                {
                    return;// Can't Open this Door, do nothing
                }

                // IF Player is to the Left of this Door
                if (playerLeftOfDoor == true)
                {
                    // IF there are no ground tiles to the Right of this Door
                    if (!groundTilesBlockingRight)
                    {
                        OpenDoorRight();// Open this Door to the Right / Away from Player
                    }
                    // ELSE IF there are ground tiles blocking the Right of this Door AND not on the Left
                    else if (!groundTilesBlockingLeft && groundTilesBlockingRight)
                    {
                        OpenDoorLeft();// Open this Door to the Left / Towards Player
                    }
                }
                // ELSE IF Player is to the Right of this Door
                else if (playerLeftOfDoor == false)
                {
                    // IF there are no ground tiles to the Left of this Door
                    if (!groundTilesBlockingLeft)
                    {
                        OpenDoorLeft();// Open this Door to the Left / Away from Player
                    }
                    // ELSE IF there are ground tiles blocking the Left of this Door AND not on the Right
                    else if (groundTilesBlockingLeft && !groundTilesBlockingRight)
                    {
                        OpenDoorRight();// Open this Door to the Right / Towards Player
                    }
                }
            }
        }
    }
}