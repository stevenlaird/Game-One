using UnityEngine;

public class CamController : MonoBehaviour
{
    // Public variable for Smooth Time with a slider ranging from 0 to 1
    [Range(0, 1)]
    public float smoothTime;

    // Public variable assigned in Inspector referencing the PlayerController script to get the Player's position
    public Transform playerTransform;

    // Hidden public variable for storing the width of the generated world
    [HideInInspector]
    public int worldWidth;

    // Private variable for Camera Projection Size
    private float orthoSize = 10;

    ///////////////////

    // Public method called Spawn that takes in a Vector3 object called pos as a parameter
    public void Spawn(Vector3 pos)
    {
        // Setting the position of the Camera's transform to the Vector3 object passed in
        GetComponent<Transform>().position = pos;

        // Setting the orthographic size of the camera to the value stored in orthoSize
        orthoSize = GetComponent<Camera>().orthographicSize;

        // Setting the private worldWidth variable by referencing the TerrainGeneration script
        worldWidth = TerrainGeneration.FindObjectOfType<TerrainGeneration>().worldWidth;
    }

    // Public method called every fixed frame
    public void FixedUpdate()
    {
        // Storing the position of the Camera's transform in a Vector3 variable called pos
        Vector3 pos = GetComponent<Transform>().position;

        // Smooth the x and y position of the Camera towards the Player's position over time
        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y + 0.017f, playerTransform.position.y, smoothTime);

        // Clamping the x position of the Camera so that it does not go beyond the bounds of the game world
        pos.x = Mathf.Clamp(pos.x, 0 + (orthoSize * 1.778f), worldWidth - (orthoSize * 1.778f));

        // Setting the position of the Camera's transform to the new position
        GetComponent<Transform>().position = pos;
    }
}