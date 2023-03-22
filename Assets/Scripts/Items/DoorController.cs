using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private PlayerController player;

    public Sprite doorClosedSprite;
    public Sprite doorOpenLeftSprite;
    public Sprite doorOpenRightSprite;
    private GameObject doorPlayerCheck;
    private GameObject doorBlockedCheck;

    private BoxCollider2D doorCollider;
    private BoxCollider2D doorClicker;

    private GameObject doorPlayerCheckLeft;
    private GameObject doorPlayerCheckRight;
    private GameObject doorBlockedCheckLeft;
    private GameObject doorBlockedCheckRight;

    [SerializeField] private bool groundTilesBlockingRight;
    [SerializeField] private bool groundTilesBlockingLeft;
    [SerializeField] private bool playerEnteringDoorLeft;
    [SerializeField] private bool playerEnteringDoorRight;

    [SerializeField] private bool playerInRangeOfDoor;
    [SerializeField] private bool playerLeftOfDoor;
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
        player = PlayerController.FindObjectOfType<PlayerController>();
        doorClosedSprite = Resources.Load<Sprite>("WoodDoor_Closed");
        doorOpenLeftSprite = Resources.Load<Sprite>("WoodDoor_OpenLeft");
        doorOpenRightSprite = Resources.Load<Sprite>("WoodDoor_OpenRight");
        doorPlayerCheck = Resources.Load<GameObject>("DoorPlayerEnterCheck");
        doorBlockedCheck = Resources.Load<GameObject>("DoorBlockedCheck");

        doorCollider = this.gameObject.AddComponent<BoxCollider2D>();// Add BoxCollider2D that will prevent Player & Enemy movement when enabled
        doorCollider.size = new Vector2(0.25f, 2.0f);// Set the Size of doorCollider's BoxCollider2D
        doorClicker = this.gameObject.AddComponent<BoxCollider2D>();// Add BoxCollider2D for the Player to click on
        doorClicker.isTrigger = true;// Allows doorClicker's BoxCollider2D to be clicked

        doorPlayerCheckLeft = Instantiate(doorPlayerCheck, new Vector2(this.transform.position.x - 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);// Instantiates check to the Left of this Door
        doorPlayerCheckRight = Instantiate(doorPlayerCheck, new Vector2(this.transform.position.x + 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);// Instantiates check to the Right of this Door
        doorPlayerCheckLeft.transform.parent = this.gameObject.transform;// Parent it to this Door
        doorPlayerCheckRight.transform.parent = this.gameObject.transform;// Parent it to this Door

        doorBlockedCheckLeft = Instantiate(doorBlockedCheck, new Vector2(this.transform.position.x - 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);// Instantiates check to the Left of this Door
        doorBlockedCheckRight = Instantiate(doorBlockedCheck, new Vector2(this.transform.position.x + 1.0f, this.transform.position.y + 0.5f), Quaternion.identity);// Instantiates check to the Right of this Door
        doorBlockedCheckLeft.transform.parent = this.gameObject.transform;// Parent it to this Door
        doorBlockedCheckRight.transform.parent = this.gameObject.transform;// Parent it to this Door

        CloseDoor();// Sets this Door Closed when placed by Player
    }

    void Update()
    {
        if (player.transform.position.x < this.gameObject.transform.position.x)// IF the Player's X position in world is less than this Door's position in world
        { playerLeftOfDoor = true; }// playerLeftOfDoor BOOL set to TRUE. Player is to the Left of the Door
        else// IF the Player's X position in world is more than or equal to this Door's position in world
        { playerLeftOfDoor = false; }// playerLeftOfDoor BOOL set to FALSE. Player is to the Right of the Door

        if (Vector2.Distance(this.gameObject.transform.position, player.transform.position) <= 5f)// IF the distance between this Door and Player is less than or equal to 5f
        { playerInRangeOfDoor = true; }// playerInRangeOfDoor BOOL set to TRUE. Player is in range and will be able to click on this Door to Open/Close it
        else// ELSE the distance between this Door and Player is more than 5f
        { playerInRangeOfDoor = false; }// playerInRangeOfDoor BOOL set to FALSE. Player is not in range and won't be able to click on this Door to Open/Close it

        groundTilesBlockingLeft = doorBlockedCheckLeft.GetComponent<DoorBlockedCheck>().blockingDoor;// Update BOOLs from Children's Component's
        groundTilesBlockingRight = doorBlockedCheckRight.GetComponent<DoorBlockedCheck>().blockingDoor;
        playerEnteringDoorLeft = doorPlayerCheckLeft.GetComponent<DoorPlayerEnterCheck>().playerEnteringDoor;
        playerEnteringDoorRight = doorPlayerCheckRight.GetComponent<DoorPlayerEnterCheck>().playerEnteringDoor;


        if (groundTilesBlockingLeft && doorState == DoorState.OpenLeft)// IF there is a ground tile blocking the Left of this Door AND Door is Open to Left
        {
            CloseDoor();// Close this Door
        }
        if (groundTilesBlockingRight && doorState == DoorState.OpenRight)// IF there is a ground tile blocking the Right of this Door AND Door is Open to Right
        {
            CloseDoor();// Close this Door
        }

        if (doorState == DoorState.Closed)// IF this Door is Closed
        {
            if (playerEnteringDoorLeft && player.horizontal > 0.01f)// IF playerEnteringDoorLeft detects Player AND Player is inputting movement to the Right
            {
                if (!groundTilesBlockingLeft && groundTilesBlockingRight)// IF the Door is blocked by tiles on the right, AND not the left, AND is closed
                {
                    OpenDoorLeft();// Open Door to the Left / Towards Player
                }
                else if (!groundTilesBlockingRight && doorState == DoorState.Closed)// ELSE IF the Door is not blocked by ground tiles, and is closed
                {
                    OpenDoorRight();// Open Door to the Right / Away from Player
                }
            }
            if (playerEnteringDoorRight && player.horizontal < -0.01f)// IF playerEnteringDoorRight detects Player AND Player is inputting movement to the Left
            {
                if (groundTilesBlockingLeft && !groundTilesBlockingRight)// IF the Door is blocked by tiles on the left, AND not the right, AND is closed
                {
                    OpenDoorRight();// Open Door to the Right / Towards Player
                }
                else if (!groundTilesBlockingLeft && doorState == DoorState.Closed)// ELSE IF the Door is not blocked by ground tiles on the Left, and is closed
                {
                    OpenDoorLeft();// Open Door to the Left / Away from Player
                }
            }

        }

        if (doorState == DoorState.OpenLeft)// IF this Door is Open to the Left
        {
            if (playerEnteringDoorLeft && player.horizontal < -0.01f)// IF playerEnteringDoorLeft detects Player AND Player is inputting movement to the Left
            {
                CloseDoor();// Close this Door
            }
        }
        if (doorState == DoorState.OpenRight)// IF this Door is Open to the Right
        {
            if (playerEnteringDoorRight && player.horizontal > 0.01f)// IF playerEnteringDoorRight detects Player AND Player is inputting movement to the Right
            {
                CloseDoor();// Close this Door
            }
        }
    }

    private void OpenDoorLeft()
    {
        doorCollider.enabled = false;// Disable this Door's box collider to enable Player & Enemy movement through Door
        doorClicker.size = new Vector2(1.0f, 2.0f);// Resize doorClicker to fill doorOpenLeftSprite
        doorClicker.offset = new Vector2(-0.4f, 0.5f);// Offset doorClicker to fill doorOpenLeftSprite
        this.gameObject.GetComponent<SpriteRenderer>().sprite = doorOpenLeftSprite;// Change the current sprite so the Door is Opened to the Left
        doorState = DoorState.OpenLeft;// Update doorState to OpenLeft
    }

    private void OpenDoorRight()
    {
        doorCollider.enabled = false;// Disable this Door's box collider to enable Player & Enemy movement through Door
        doorClicker.size = new Vector2(1.0f, 2.0f);// Resize doorClicker to fill doorOpenRightSprite
        doorClicker.offset = new Vector2(0.4f, 0.5f);// Offset doorClicker to fill doorOpenRightSprite
        this.gameObject.GetComponent<SpriteRenderer>().sprite = doorOpenRightSprite;// Change the current sprite so the Door is Opened to the Right
        doorState = DoorState.OpenRight;// Update doorState to OpenRight
    }

    private void CloseDoor()
    {
        doorCollider.enabled = true;// Enable this Door's box collider to block Player & Enemy movement through Door
        doorClicker.size = new Vector2(0.5f, 2.0f);// Resize doorClicker to fill doorClosedSprite
        doorClicker.offset = new Vector2(0.0f, 0.5f);// Offset doorClicker to fill doorClosedSprite
        this.gameObject.GetComponent<SpriteRenderer>().sprite = doorClosedSprite;// Change the current sprite so the Door is Closed
        doorState = DoorState.Closed;// Update doorState to Closed
    }

    private void OnMouseUpAsButton()// When a click AND release are detected on this GameObject's collider
    {
        if (playerInRangeOfDoor)// IF playerInRangeOfDoor BOOL is TRUE. Player is within range of this Door
        {
            if (doorState == DoorState.OpenLeft || doorState == DoorState.OpenRight)// IF this Door is Open either Left or Right
            {
                CloseDoor();// Close this Door
            }
            else if (doorState == DoorState.Closed)// IF this Door is Closed
            {
                if (groundTilesBlockingLeft && groundTilesBlockingRight)// IF there are ground tiles blocking the Left AND Right of this Door
                {
                    return;// Can't Open this Door, do nothing
                }

                if (playerLeftOfDoor == true)// IF Player is to the Left of this Door
                {
                    if (!groundTilesBlockingRight)// IF there are no ground tiles to the Right of this Door
                    {
                        OpenDoorRight();// Open this Door to the Right / Away from Player
                    }
                    else if (!groundTilesBlockingLeft && groundTilesBlockingRight)// IF there are ground tiles blocking the Right of this Door AND not on the Left
                    {
                        OpenDoorLeft();// Open this Door to the Left / Towards Player
                    }
                }
                else if (playerLeftOfDoor == false)// IF Player is to the Right of this Door
                {
                    if (!groundTilesBlockingLeft)// IF there are no ground tiles to the Left of this Door
                    {
                        OpenDoorLeft();// Open this Door to the Left / Away from Player
                    }
                    else if (groundTilesBlockingLeft && !groundTilesBlockingRight)// IF there are ground tiles blocking the Left of this Door AND not on the Right
                    {
                        OpenDoorRight();// Open this Door to the Right / Towards Player
                    }
                }
            }
        }
    }
}
