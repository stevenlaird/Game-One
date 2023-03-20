using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class OgreSpawner : MonoBehaviour
{
    private PlayerController player;
    public GameObject[] ogrePrefabs;
    public LayerMask ogres;

    public float checkForOgreRadius = 20;
    public int spawnRateTimeMin = 10;
    public int spawnRateTimeMax = 40;
    public int maxOgresInRadius = 1;

    public float distanceToPlayer;
    public bool canSpawnNewOgre;
    public float spawnCounter;
    public int ogreCount;

    ///////////////////

    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();

        spawnCounter = UnityEngine.Random.Range(10,60);
    }

    void Update()
    {
        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer >= 30)
        { SpawnCounter(); }

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

    public void SpawnOgre()
    {
        GameObject EnemyOgre;

        int selectedOgre = UnityEngine.Random.Range(0, ogrePrefabs.Length);
        //Vector2 randomSpawn = new Vector2((UnityEngine.Random.Range(this.transform.position.x - 4, this.transform.position.x + 4)), (UnityEngine.Random.Range(this.transform.position.y - 4, this.transform.position.y + 4)));
        //EnemyOgre = Instantiate(ogrePrefabs[selectedOgre], randomSpawn, Quaternion.identity);
        EnemyOgre = Instantiate(ogrePrefabs[selectedOgre], this.transform.position, Quaternion.identity);
        EnemyOgre.transform.parent = this.gameObject.transform;
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
            canSpawnNewOgre = true;
        }
        else
        {
            canSpawnNewOgre = false;
            spawnCounter -= Time.fixedDeltaTime;
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.gameObject.transform.position, checkForOgreRadius);
    }
}
