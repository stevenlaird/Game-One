using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
    [SerializeField] 
    GameObject[] clouds;                // Array of cloud prefabs
    [SerializeField] 
    GameObject endPointObject;          // Object representing the end point of the cloud's movement

    public GameObject parentObject;     // Object to which the spawned clouds are parented

    private float spawnInterval;        // Random interval between cloud spawns
    public float spawnTime = 1.6f;      // Time between cloud spawns
    public float spawnRange = 0.5f;     // Random range for cloud spawn intervals
    public float speedBase = 4f;        // Base speed of cloud movement
    public float speedRange = 1f;       // Random range for cloud speeds
    public float heightRange = 45f;     // Random range for cloud spawn heights

    public bool preWarmScene = true;    // Option to pre-warm the scene by spawning clouds before gameplay begins

    Vector3 startPos;                   // Starting position for cloud spawns

    ///////////////////

    void Start()
    {
        // Set the starting position to the position of the CloudGenerator object
        startPos = transform.position;
        if (preWarmScene )
        {
            // Pre-warm the scene by spawning clouds
            PreWarmScene();
        }
        // Set the first random spawn interval
        spawnInterval = UnityEngine.Random.Range(spawnTime - spawnRange, spawnTime + spawnRange);
        // Start the cloud spawning loop
        Invoke("AttemptSpawn", spawnInterval);
    }

    void Update()
    {
        // Update the starting position
        startPos = transform.position;
    }

    void SpawnCloud(Vector3 startPos)
    {
        // Create a new cloud from the clouds array
        GameObject cloud = Instantiate(clouds[UnityEngine.Random.Range(0, clouds.Length)]);
        // Set the cloud's parent object
        cloud.transform.parent = parentObject.transform;

        // Set the cloud's random spawn height
        float startY = UnityEngine.Random.Range(startPos.y, startPos.y + heightRange);
        cloud.transform.position = new Vector3(startPos.x, startY, startPos.z);

        // Set the cloud's random scale
        float scale = UnityEngine.Random.Range(1f, 2f);
        cloud.transform.localScale = new Vector2 (scale, scale);

        // Set the cloud's movement speed
        float speed = UnityEngine.Random.Range(speedBase - speedRange, speedBase + speedRange);
        cloud.GetComponent<CloudMovement>().StartMoving(speed, endPointObject.transform.position.x);
    }

    void AttemptSpawn()
    {
        // Spawn a new cloud
        SpawnCloud(startPos);
        // Set the next random spawn interval
        spawnInterval = UnityEngine.Random.Range(spawnTime - spawnRange, spawnTime + spawnRange);
        // Loop the cloud spawning process every spawnInterval
        Invoke("AttemptSpawn", spawnInterval);
    }

    void PreWarmScene()
    {
        // Pre-warm 10 clouds
        for (int i = 0; i < 10; i++)
        {
            // Set the spawn position for the current cloud
            // Move the spawnPos 8f to the left each time the loop is ran
            Vector3 spawnPos = startPos + Vector3.left * (i * 8);
            // Spawn a cloud at the current position
            SpawnCloud(spawnPos);
        }
    }
}