using UnityEngine;

// The EnemySpawnerMaps class holds data for each type of enemy spawner, defining its properties for procedural generation
[System.Serializable]
public class EnemySpawnerMaps
{
    // Name of the enemy spawner type
    public string name;
    // Rarity of the enemy spawner (0 to 1, lower value means rarer)
    [Range(0, 1)] public float rarity;
    // Density of the enemy spawner within clusters (0 to 1)
    [Range(0, 1)] public float density;
    // Minimum distance from the surface where the enemy spawner can spawn
    public int spawnXFromSurface;
    // Noise map used for generating the distribution of enemy spawners in the world
    public Texture2D mapTexture;
}

// Cave Spawner: rarity = 0.16, density = 0.85