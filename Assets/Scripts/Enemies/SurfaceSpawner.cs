using UnityEngine;

public class SurfaceSpawner : MonoBehaviour
{
    private PlayerController player;        // Reference to the PlayerController
    public DayNightCycle dayNightCycle;     // Reference to the DayNightCycle
    public GameObject[] ogrePrefabs;        // Array of Ogre prefabs
    public GameObject walkingBarrelPrefab;  // Walking Barrel prefab
    public LayerMask ogres;                 // Layermask to detect Ogres
    public LayerMask walkingBarrel;         // Layermask to detect Walking Barrels

    // Variables to determine if new enemy can spawn
    public float checkForEnemyRadius = 10;
    public int spawnRateTimeMin = 10;
    public int spawnRateTimeMax = 40;
    // Variables to track spawning logic
    public float distanceToPlayer;
    public bool canSpawnNewEnemy;
    public float spawnCounter;
    public int ogreCount;
    public int wBCount;

    ///////////////////

    void Start()
    {
        // Find the PlayerController component in the scene and assign it to the player variable
        player = PlayerController.FindObjectOfType<PlayerController>();
        // Find the DayNightCycle component in the scene and assign it to the dayNightCycle variable
        dayNightCycle = DayNightCycle.FindObjectOfType<DayNightCycle>();

        // Set the spawnCounter to a random value between 10 and 60
        spawnCounter = UnityEngine.Random.Range(10, 61);
    }

    void Update()
    {
        // Calculate the distance between the spawner and the Player
        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // If the player is more than 30 units away from the spawner, run the spawn counter
        if (distanceToPlayer >= 30)
        { SpawnCounter(); }

        // IF the spawner can spawn a new enemy,
        // check if it is day time and there are no walking barrels nearby,
        // then spawn a walking barrel;
        // otherwise, if it is night time and there are no ogres nearby and no torches,
        // spawn an ogre.
        // Then set the spawnCounter to a new random value.
        if (canSpawnNewEnemy == true)
        {
            if (dayNightCycle.dayTime == true)
            {
                CountOgresNearSpawner();
                wBCount = CountWBNearSpawner();
                if (wBCount < 1)
                {
                    SpawnWB();
                }
            }
            else if (dayNightCycle.dayTime == false)
            {
                ogreCount = CountOgresNearSpawner();
                if (ogreCount < 1 && CountTorchesNearSpawner() < 1)
                {
                    SpawnOgre();
                }
            }
            spawnCounter = RandomSpawnTime();
        }
    }

    // Count the number of ogres within checkForEnemyRadius of the spawner
    public int CountOgresNearSpawner()
    {
        int numberOfOgres = 0;
        Collider2D[] detectOgre = Physics2D.OverlapCircleAll(this.gameObject.transform.position, checkForEnemyRadius, ogres);
        for (int i = 0; i < detectOgre.Length; i++)
        {
            if (detectOgre[i].tag == "Enemy")
            { numberOfOgres++; }
            if (dayNightCycle.dayTime == true)
            {
                Destroy(detectOgre[i].transform.parent.gameObject); // If it's daytime, destroy the ogres
            }
        }
        return numberOfOgres;
    }

    // Count the number of torches within checkForEnemyRadius of the spawner
    public int CountTorchesNearSpawner()
    {
        int numberOfTorches = 0;
        Collider2D[] detectTorch = Physics2D.OverlapCircleAll(this.gameObject.transform.position, checkForEnemyRadius);
        for (int i = 0; i < detectTorch.Length; i++)
        {
            if (detectTorch[i].tag == "Torch")
            { numberOfTorches++; }
        }
        return numberOfTorches;
    }

    // Spawn an Ogre at the spawner's position, with a slight offset above
    public void SpawnOgre()
    {
        GameObject EnemyOgre;

        int selectedOgre = UnityEngine.Random.Range(0, ogrePrefabs.Length);
        EnemyOgre = Instantiate(ogrePrefabs[selectedOgre], new Vector2(this.transform.position.x, this.transform.position.y + 3), Quaternion.identity);
        EnemyOgre.transform.parent = this.gameObject.transform;
    }

    // Count the number of walking barrels within checkForEnemyRadius of the spawner
    public int CountWBNearSpawner()
    {
        int numberOfWB = 0;
        Collider2D[] detectWB = Physics2D.OverlapCircleAll(this.gameObject.transform.position, checkForEnemyRadius, walkingBarrel);
        for (int i = 0; i < detectWB.Length; i++)
        {
            if (detectWB[i].tag == "Enemy")
            { numberOfWB++; }
        }
        return numberOfWB;
    }

    // Spawn a walking barrel at the spawner's position, with a slight offset above
    public void SpawnWB()
    {
        GameObject EnemyWB;

        int selectedOgre = UnityEngine.Random.Range(0, ogrePrefabs.Length);
        EnemyWB = Instantiate(walkingBarrelPrefab, new Vector2(this.transform.position.x, this.transform.position.y + 3), Quaternion.identity);
        EnemyWB.transform.parent = this.gameObject.transform;
    }

    // Choose a random time between spawnRateTimeMin and spawnRateTimeMax for the next spawn
    public int RandomSpawnTime()
    {
        int randomSpawnTime = UnityEngine.Random.Range(spawnRateTimeMin, spawnRateTimeMax);
        return randomSpawnTime;
    }

    // Decrease the spawnCounter by Time.fixedDeltaTime until it reaches 0, at which point a new enemy can spawn
    public void SpawnCounter()
    {
        if (spawnCounter <= 0)
        {
            canSpawnNewEnemy = true;
        }
        else
        {
            canSpawnNewEnemy = false;
            spawnCounter -= Time.fixedDeltaTime;
        }
    }

    // Draw a wire sphere around the spawner to visualize the area where enemies can be detected and spawned
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.gameObject.transform.position, checkForEnemyRadius);
    }
}