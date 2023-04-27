using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    // The unique name of the biome
    public string biomeName;
    // The color associated with the biome
    public Color biomeColor;
    // The TileDirectory containing biome-specific tiles
    public TileDirectory tileDirectory;

    [Header("Top Layer Settings")]
    // The frequency of terrain generation in the biome
    public float terrainFreq = 0.008f;
    // The maximum depth of the dirt layer in the biome
    public int dirtLayerMax = 16;
    // The minimum depth of the dirt layer in the biome
    public int dirtLayerMin = 10;

    [Header("Trees")]
    // The chance of a tree spawning in the biome
    public int treeChance = 8;
    // The minimum height of trees in the biome
    public int treeMinHeight = 3;
    // The maximum height of trees in the biome
    public int treeMaxHeight = 10;

    [Header("Surface Add Ons")]
    // The chance of a cactus spawning in the biome
    public int cactusChance = 8;
    // The chance of tall grass spawning in the biome
    public int tallGrassChance = 2;
    // The chance of mushrooms spawning in the biome
    public int mushroomChance = 5;
}