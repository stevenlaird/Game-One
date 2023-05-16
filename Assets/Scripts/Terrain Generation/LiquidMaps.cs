using UnityEngine;

// The LiquidMaps class holds data for each type of liquid, defining its properties for procedural generation
[System.Serializable]
public class LiquidMaps
{
    // Name of the liquid type
    public string name;
    // Rarity of the liquid (0 to 1, lower value means rarer)
    [Range(0, 1)] public float rarity;
    // Density of the liquid within clusters (0 to 1)
    [Range(0, 1)] public float density;
    // Minimum distance from the surface where the liquid can spawn
    public int spawnXFromSurface;
    // Noise map used for generating the distribution of liquids in the world
    public Texture2D mapTexture;
}

// Water: rarity = 0.097, density = 0.73
// Lava: rarity = 0.095, density = 0.77