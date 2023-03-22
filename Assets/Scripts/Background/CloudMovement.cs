using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    private float cloudSpeed;
    private float cloudEndPosX;

    ///////////////////

    public void StartMoving(float speed, float endPosX)
    {
        cloudSpeed = speed;
        cloudEndPosX = endPosX;
    }

    void Update()
    {
        transform.Translate(Vector3.left * (Time.deltaTime * cloudSpeed));

        if (transform.position.x < cloudEndPosX)
        {
            Destroy(gameObject);
        }
    }
}
