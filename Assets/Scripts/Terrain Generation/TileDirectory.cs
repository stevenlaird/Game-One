using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileDirectory", menuName = "Tile Directory")]
public class TileDirectory : ScriptableObject
{
    [Header("Biome Tiles")]
    public TileClass grass;
    public TileClass dirt;
    public TileClass stone;
    public TileClass gravel;
    public TileClass sand;
    public TileClass sandstone;
    public TileClass snow;
    public TileClass ice;
    public TileClass greystone;
    [Header("Biome Walls")]
    public TileClass dirtWall;
    public TileClass stoneWall;
    public TileClass gravelWall;
    public TileClass sandWall;
    public TileClass sandstoneWall;
    public TileClass snowWall;
    public TileClass iceWall;
    public TileClass greystoneWall;
    [Header("Ores")]
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
    public TileClass diamond;
    [Header("Trees")]
    public TileClass treeSapling;
    public TileClass treeBase;
    public TileClass treeLog;
    public TileClass treeLogTop;
    public TileClass treeLeaf;
    public TileClass spruceTreeBase;
    public TileClass spruceTreeLog;
    public TileClass spruceTreeLogTop;
    public TileClass spruceTreeLeaf;
    public TileClass acaciaTreeBase;
    public TileClass acaciaTreeLog;
    public TileClass acaciaTreeLogTop;
    public TileClass acaciaTreeLeaf;
    [Header("Surface Add Ons")]
    public TileClass cactus;
    public TileClass tallGrass;
    public TileClass mushroomRed;
    public TileClass mushroomBrown;
    public TileClass mushroomTan;
    public TileClass torch;
    [Header("Liquids")]
    public TileClass water;
    public TileClass lava;
    [Header("Spawners")]
    public TileClass surfaceSpawner;
    public TileClass caveEnemySpawner;
    [Header("World Border")]
    public TileClass bedrock;
    public TileClass barrier;
}
