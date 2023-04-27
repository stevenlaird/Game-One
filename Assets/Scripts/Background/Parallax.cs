using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;           // Length of the background element sprite
    public GameObject cam;          // Camera object
    public float startPosX;         // Starting X position of the background element sprite
    public bool startPosXEnable;    // Enable to use the starting X position of the background element sprite
    public float yHeight;           // Y position of the background element sprite
    public bool yFollowCam;         // Enable to follow the camera's Y position
    
    [SerializeField]
    private float parallaxEffect;   // Parallax effect factor, controls the speed of the movement

    ///////////////////

    void Start()
    {
        // If startPosXEnable is true, use the current position of the background element sprite as the starting X position
        if (startPosXEnable)
        { 
            startPosX = transform.position.x; 
        }

        // Get the length of the background element sprite
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Calculate the distance based on the camera's X position and the parallax effect factor
        float distance = (cam.transform.position.x * parallaxEffect);
        // Calculate the temporary position based on the camera's X position and the inverse of the parallax effect factor
        float temporary = (cam.transform.position.x * (1 - parallaxEffect));

        // If yFollowCam is true, set the position of the background element sprite to follow the camera's Y position
        if (yFollowCam == true)
        { 
            transform.position = new Vector3(startPosX + distance, cam.transform.position.y, transform.position.z); 
        }
        else
        // Otherwise, set the Y position of the background element sprite to the fixed yHeight
        { 
            transform.position = new Vector3(startPosX + distance, yHeight, transform.position.z); 
        }

        // If temporary is greater than the sum of the starting X position and the length of the background element sprite, 
        // update the starting X position to the sum of the starting X position and the length of the background element sprite
        if (temporary > startPosX + length)
        { 
            startPosX += length; 
        }
        // If temporary is less than the difference between the starting X position and the length of the background element sprite, 
        // update the starting X position to the difference between the starting X position and the length of the background element sprite
        else if (temporary < startPosX - length)
        {
            startPosX -= length;
        }
    }
}