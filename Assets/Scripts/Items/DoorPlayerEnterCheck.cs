using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPlayerEnterCheck : MonoBehaviour
{
    public bool playerEnteringDoor;

    ///////////////////

    void Start()
    {
        playerEnteringDoor = false;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerEnteringDoor = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerEnteringDoor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerEnteringDoor = false;
        }
    }
}
