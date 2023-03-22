using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LeafController : MonoBehaviour
{
    public bool inRangeOfTrunk = false;
    public float destroyCounter = 0;
    private int destoryCheck;
    private TerrainGeneration terrainGenerator;

    ///////////////////

    void Start()
    {
        terrainGenerator = GameObject.FindObjectOfType<TerrainGeneration>();
    }
    
    void Update()
    {
        if (inRangeOfTrunk == false)
        {
            destroyCounter += Time.deltaTime;

            if (destroyCounter >= 5)
            {
                destoryCheck = UnityEngine.Random.Range(1,10);
                if (destoryCheck == 1) 
                { 
                    terrainGenerator.PlayerRemoveSurfaceTile(Mathf.RoundToInt(this.transform.position.x - 0.5f), Mathf.RoundToInt(this.transform.position.y - 0.5f));
                }
                else 
                { 
                    destroyCounter = 0;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("TreeTop"))
        {
            inRangeOfTrunk = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("TreeTop"))
        {
            inRangeOfTrunk = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("TreeTop"))
        {
            inRangeOfTrunk = false;
        }
    }
}
