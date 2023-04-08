using UnityEngine;

public class DoorBlockedCheck : MonoBehaviour
{
    public bool blockingDoor;

    ///////////////////

    void Start()
    {
        blockingDoor = false;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Ground"))
        {
            blockingDoor = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Ground"))
        {
            blockingDoor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Ground"))
        {
            blockingDoor = false;
        }
    }
}