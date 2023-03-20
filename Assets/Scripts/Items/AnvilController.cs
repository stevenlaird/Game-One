using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnvilController : MonoBehaviour
{
    private PlayerController player;

    ///////////////////

    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(this.gameObject.transform.position, player.transform.position) <= 3f)
        {
            player.inRangeOfAnvil = true;
        }
    }
}
