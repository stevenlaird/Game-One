using UnityEngine;

public class LeafController : MonoBehaviour
{
    // Initialize variables
    public bool inRangeOfTreeTop = false; // Boolean to check if the leaf is within range of a tree top
    public float destroyCounter = 0; // Counter to track how long the leaf has been out of range
    private int destoryCheck; // Randomly generated value to determine whether to destroy the leaf or not
    private TerrainGeneration terrainGenerator; // Reference to the TerrainGeneration script

    ///////////////////

    void Start()
    {
        // Find the TerrainGeneration script in the scene
        terrainGenerator = GameObject.FindObjectOfType<TerrainGeneration>();
    }

    void Update()
    {
        // IF the leaf is out of range of a tree top, start counting
        if (inRangeOfTreeTop == false)
        {
            // Countdown destroyCounter
            destroyCounter += Time.deltaTime;

            // IF the leaf has been out of range for more than 5 seconds
            if (destroyCounter >= 5)
            {
                // Declare a random value between and including 0 and 9
                destoryCheck = UnityEngine.Random.Range(0, 10);

                // IF the random value is 1, call TerrainGenerator to destroy the leaf by removing a surface tile where it is located
                if (destoryCheck == 1)
                {
                    terrainGenerator.PlayerRemoveSurfaceTile(Mathf.RoundToInt(this.transform.position.x - 0.5f), Mathf.RoundToInt(this.transform.position.y - 0.5f));
                }
                // ELSE reset the counter if the leaf is not destroyed
                else
                {
                    destroyCounter = 0;
                }
            }
        }
    }

    // When the leaf enters the range of the tree top, set the inRangeOfTreeTop variable to true
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("TreeTop"))
        { inRangeOfTreeTop = true; }
    }

    // When the leaf stays within the range of the tree top, keep the inRangeOfTreeTop variable set to true
    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("TreeTop"))
        { inRangeOfTreeTop = true; }
    }

    // When the leaf exits the range of the tree top, set the inRangeOfTreeTop variable to false
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("TreeTop"))
        { inRangeOfTreeTop = false; }
    }
}