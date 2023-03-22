using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceSpawner : MonoBehaviour
{
    private PlayerController player;
    public DayNightCycle dayNightCycle;
    public GameObject[] ogrePrefabs;
    public GameObject walkingBarrelPrefab;
    public LayerMask ogres;
    public LayerMask walkingBarrel;

    public float checkForEnemyRadius = 10;
    public int spawnRateTimeMin = 10;
    public int spawnRateTimeMax = 40;

    public float distanceToPlayer;
    public bool canSpawnNewEnemy;
    public float spawnCounter;
    public int ogreCount;
    public int wBCount;

    ///////////////////

    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
        dayNightCycle = DayNightCycle.FindObjectOfType<DayNightCycle>();

        spawnCounter = UnityEngine.Random.Range(10, 60);
    }

    void Update()
    {
        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer >= 30)
        { SpawnCounter(); }

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
                Destroy(detectOgre[i].transform.parent.gameObject);
                //detectOgre[i].GetComponent<OgreController>().TakeDamage(20, detectOgre[i].transform.position); 
            }
        }
        return numberOfOgres;
    }

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

    public void SpawnOgre()
    {
        GameObject EnemyOgre;

        int selectedOgre = UnityEngine.Random.Range(0, ogrePrefabs.Length);
        EnemyOgre = Instantiate(ogrePrefabs[selectedOgre], new Vector2(this.transform.position.x, this.transform.position.y + 3), Quaternion.identity);
        EnemyOgre.transform.parent = this.gameObject.transform;
    }

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

    public void SpawnWB()
    {
        GameObject EnemyWB;

        int selectedOgre = UnityEngine.Random.Range(0, ogrePrefabs.Length);
        EnemyWB = Instantiate(walkingBarrelPrefab, new Vector2(this.transform.position.x, this.transform.position.y + 3), Quaternion.identity);
        EnemyWB.transform.parent = this.gameObject.transform;
    }

    public int RandomSpawnTime()
    {
        int randomSpawnTime = UnityEngine.Random.Range(spawnRateTimeMin, spawnRateTimeMax);
        return randomSpawnTime;
    }

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

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.gameObject.transform.position, checkForEnemyRadius);
    }
}
