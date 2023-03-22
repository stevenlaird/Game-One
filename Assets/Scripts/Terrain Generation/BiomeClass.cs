using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeColor;
    public TileDirectory tileDirectory;

    [Header("Top Layer Settings")]
    public float terrainFreq = 0.008f;
    public int dirtLayerMax = 16;
    public int dirtLayerMin = 10;

    [Header("Trees")]
    public int treeChance = 8;
    public int treeMinHeight = 3;
    public int treeMaxHeight = 10;

    [Header("Surface Add Ons")]
    public int cactusChance = 8;
    public int tallGrassChance = 2;
    public int mushroomChance = 5;
}