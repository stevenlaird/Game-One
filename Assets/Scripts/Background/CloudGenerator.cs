using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject[] clouds;
    [SerializeField]
    GameObject endPointObject;

    public GameObject parentObject;

    private float spawnInterval;
    public float spawnTime = 1.6f;
    public float spawnRange = 0.5f;
    public float speedBase = 4f;
    public float speedRange = 1f;
    public float heightRange = 45f;

    public bool preWarmScene = true;

    Vector3 startPos;

    ///////////////////

    void Start()
    {
        startPos = transform.position;
        if (preWarmScene )
        {
            PreWarmScene();
        }
        spawnInterval = UnityEngine.Random.Range(spawnTime - spawnRange, spawnTime + spawnRange); //declare random spawn time based on public spawn time and spawn range vars
        Invoke("AttemptSpawn", spawnInterval);
    }

    void SpawnCloud(Vector3 startPos)
    {
        GameObject cloud = Instantiate(clouds[UnityEngine.Random.Range(0, clouds.Length)]); //create a GameObject from public GameObject array "clouds" containing cloud prefabs
        cloud.transform.parent = parentObject.transform; //parent to selected public GameObject

        float startY = UnityEngine.Random.Range(startPos.y, startPos.y + heightRange);
        cloud.transform.position = new Vector3(startPos.x, startY, startPos.z); //spawn at random height within public height range var

        float scale = UnityEngine.Random.Range(1f, 2f);
        cloud.transform.localScale = new Vector2 (scale, scale); //spawn with random size within scale var

        float speed = UnityEngine.Random.Range(speedBase - speedRange, speedBase + speedRange);
        cloud.GetComponent<CloudMovement>().StartMoving(speed, endPointObject.transform.position.x); //spawn and start moving at random speed within public speed range var
    }

    void AttemptSpawn()
    {
        //initiate checks if needed
        SpawnCloud(startPos); //spawn a cloud 
        spawnInterval = UnityEngine.Random.Range(spawnTime - spawnRange, spawnTime + spawnRange); //declare random spawn time based on public spawn time and spawn range vars
        Invoke("AttemptSpawn", spawnInterval); //loop function every spawn interval
    }

    void PreWarmScene()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 spawnPos = startPos + Vector3.left * (i * 8); //move spawnPos 8f to the left each time loop is ran
            SpawnCloud(spawnPos); //spawn 10 clouds using spawnPos to pre warm scene
        }
    }

    void Update()
    {
        startPos = transform.position;
    }
}
