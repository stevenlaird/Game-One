using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    private float cloudSpeed;       // Speed at which the cloud moves
    private float cloudEndPosX;     // X position at which the cloud should be destroyed

    ///////////////////

    void Update()
    {
        // Move the cloud to the left based on the cloud speed
        transform.Translate(Vector3.left * (Time.deltaTime * cloudSpeed));

        // If the cloud has reached its end position
        if (transform.position.x < cloudEndPosX) 
        {
            // Destroy the cloud
            Destroy(gameObject);
        }
    }

    public void StartMoving(float speed, float endPosX)
    {
        cloudSpeed = speed;
        cloudEndPosX = endPosX;
    }
}