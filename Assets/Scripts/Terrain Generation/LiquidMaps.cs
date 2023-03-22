using System.Collections;
using UnityEngine;

[System.Serializable]
public class LiquidMaps
{
    public string name;
    [Range(0, 1)]
    public float rarity;
    [Range(0, 1)]
    public float density;
    public int spawnXFromSurface;
    public Texture2D mapTexture;
}

//Water 0.097 / 0.73
//Lava 0.095 / 0.77