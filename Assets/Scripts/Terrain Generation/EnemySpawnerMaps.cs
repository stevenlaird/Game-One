using System.Collections;
using UnityEngine;

[System.Serializable]
public class EnemySpawnerMaps
{
    public string name;
    [Range(0, 1)]
    public float rarity;
    [Range(0, 1)]
    public float density;
    public int spawnXFromSurface;
    public Texture2D mapTexture;
}

//Cave Spawner 0.16 / 0.85
