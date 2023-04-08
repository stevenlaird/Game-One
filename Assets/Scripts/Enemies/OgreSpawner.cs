using UnityEngine;

public class OgreSpawner : MonoBehaviour
{
    private PlayerController player;        // Reference to the PlayerController
    public GameObject[] ogrePrefabs;        // Array of Ogre prefabs
    public LayerMask ogres;                 // Layermask to detect Ogres

    // Variables to determine if new enemy can spawn
    public float checkForOgreRadius = 20;
    public int spawnRateTimeMin = 10;
    public int spawnRateTimeMax = 40;
    public int maxOgresInRadius = 1;
    // Variables to track spawning logic
    public float distanceToPlayer;
    public bool canSpawnNewOgre;
    public float spawnCounter;
    public int ogreCount;

    ///////////////////

    void Start()
    {
        // Find the PlayerController component in the scene and assign it to the player variable
        player = PlayerController.FindObjectOfType<PlayerController>();
        // Set the spawnCounter to a random value between 10 and 60
        spawnCounter = UnityEngine.Random.Range(10,61);
    }

    void Update()
    {
        // Calculate the distance between the spawner and the Player
        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // If the player is more than 30 units away from the spawner, run the spawn counter
        if (distanceToPlayer >= 30)
        { SpawnCounter(); }

        // IF the spawner can spawn a new ogre,
        // check if there are too many ogres nearby and no torches,
        // then spawn an ogre and set the spawnCounter to a new random value
        if (canSpawnNewOgre == true)
        {
            ogreCount = CountOgresNearSpawner();
            if (ogreCount < maxOgresInRadius && CountTorchesNearSpawner() < 1)
            {
                SpawnOgre();
            }
            spawnCounter = RandomSpawnTime();
        }
    }

    // Count the number of ogres within checkForOgreRadius of the spawner
    public int CountOgresNearSpawner()
    {
        int numberOfOgres = 0;
        Collider2D[] detectOgre = Physics2D.OverlapCircleAll(this.gameObject.transform.position, checkForOgreRadius, ogres);
        for (int i = 0; i < detectOgre.Length; i++)
        { 
            if (detectOgre[i].tag == "Enemy")
            { numberOfOgres++; }
        }
        return numberOfOgres;
    }

    // Count the number of torches within checkForOgreRadius of the spawner
    public int CountTorchesNearSpawner()
    {
        int numberOfTorches = 0;
        Collider2D[] detectTorch = Physics2D.OverlapCircleAll(this.gameObject.transform.position, checkForOgreRadius);
        for (int i = 0; i < detectTorch.Length; i++)
        {
            if (detectTorch[i].tag == "Torch")
            { numberOfTorches++; }
        }
        return numberOfTorches;
    }

    // Choose a random Ogre from the array of prefabs and spawn it at the spawner's position
    public void SpawnOgre()
    {
        GameObject EnemyOgre;
        int selectedOgre = UnityEngine.Random.Range(0, ogrePrefabs.Length);
        EnemyOgre = Instantiate(ogrePrefabs[selectedOgre], this.transform.position, Quaternion.identity);
        EnemyOgre.transform.parent = this.gameObject.transform;
    }

    // Choose a random spawn time between spawnRateTimeMin and spawnRateTimeMax
    public int RandomSpawnTime()
    {
        int randomSpawnTime = UnityEngine.Random.Range(spawnRateTimeMin, spawnRateTimeMax);
        return randomSpawnTime;
    }

    // Decrease the spawnCounter over time until it reaches 0, then allow a new ogre to spawn
    public void SpawnCounter()
    {
        if (spawnCounter <= 0)
        {
            canSpawnNewOgre = true;
        }
        else
        {
            canSpawnNewOgre = false;
            spawnCounter -= Time.fixedDeltaTime;
        }
    }

    // Draw a blue wire sphere to show the spawner's detection radius in the Editor
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.gameObject.transform.position, checkForOgreRadius);
    }
}