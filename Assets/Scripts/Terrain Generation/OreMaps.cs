using System.Collections;
using UnityEngine;

[System.Serializable]
public class OreMaps
{
    public string name;
    [Range(0,1)]
    public float rarity;
    [Range(0, 1)]
    public float density;
    public int spawnXFromSurface;
    public Texture2D mapTexture;
}

//gravel 0.095 / 0.8 / 0
//sand 0.1 / 0.77 / 0
//coal 0.092 / 0.825 / 2
//iron 0.088 / 0.763 / 5
//gold 0.075 / 0.82 / 40
//diamond 0.19 / 0.752 / 15