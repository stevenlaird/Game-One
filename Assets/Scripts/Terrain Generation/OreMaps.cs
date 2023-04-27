using UnityEngine;

// The OreMaps class holds data for each type of ore, defining its properties for procedural generation
[System.Serializable]
public class OreMaps
{
    // The name of the ore type
    public string name;
    // The rarity of the ore, defined by a float value between 0 and 1 (lower value = more rare)
    [Range(0, 1)] public float rarity;
    // The density of the ore, defined by a float value between 0 and 1 (higher value = denser ore clusters)
    [Range(0, 1)] public float density;
    // The minimum distance from the surface for this ore type to spawn
    public int spawnXFromSurface;
    // A Texture2D that represents the generated noise map for this ore type
    public Texture2D mapTexture;
}

// Gravel: rarity = 0.095, density = 0.8, spawnXFromSurface = 0
// Sand: rarity = 0.1, density = 0.77, spawnXFromSurface = 0
// Coal: rarity = 0.092, density = 0.825, spawnXFromSurface = 2
// Iron: rarity = 0.088, density = 0.763, spawnXFromSurface = 5
// Gold: rarity = 0.075, density = 0.82, spawnXFromSurface = 40
// Diamond: rarity = 0.19, density = 0.752, spawnXFromSurface = 15