using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTileClass", menuName = "Item/Tile")]
public class TileClass : ItemClass
{

    [Header("Tile Properties")]
    public TileLocation tileLocation;
    public enum TileLocation
    {
        Ground,
        Surface,
        Wall
    };

    public int tileHealth = 100;

    public bool isTree = false;
    public bool replaceable = false;
    public bool isUnbreakable = false;

    //public bool gravity = false;    
    //public bool liquid = false;

    [Header("Tile Drop Properties")]
    public bool dropTile = true;
    public bool alwaysDropTile = true;
    public int dropTileChance = 1;
    public ItemClass droppedItem;

    ///////////////////

    public override void Use(PlayerController playerCaller)
    {
        if (playerCaller.inventoryManager.selectedItem.GetItem().GetTile().tileLocation == TileLocation.Ground &&
                    Vector2.Distance(playerCaller.playerHeadPosition, playerCaller.mousePosition) > 0.8f &&
                    Vector2.Distance(playerCaller.playerFeetPosition, playerCaller.mousePosition) > 0.8f)
        {
            if (!playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y)))
            {
                if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y)))
                {
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y))].itemName == "Crafting Table")
                    { return; }
                    else if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y))].itemName == "Wood Door")
                    { return; }
                    else if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y))].isTree)
                    { return; }
                }
                else if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y)))
                {
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y))].itemName == "Crafting Table")
                    { return; }
                }
                else if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 1)))
                {
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 1))].itemName == "Crafting Table")
                    { return; }
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 1))].itemName == "Wood Door")
                    { return; }
                }
                else if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y - 1)))
                {
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y - 1))].itemName == "Crafting Table")
                    { return; }
                }
                
                if ((playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y + 1))
                    || playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 1))
                    || playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y))
                    || playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y)))
                    && playerCaller.inventoryManager.selectedItem.GetItem().itemName != "Wood Platform")
                {
                    base.Use(playerCaller);
                    Debug.Log("Placed Ground Tile: " + playerCaller.inventoryManager.selectedItem.GetItem().GetTile().itemName);
                    playerCaller.terrainGenerator.PlayerPlaceGroundTile(playerCaller.inventoryManager.selectedItem.GetItem().GetTile(), playerCaller.mousePosition.x, playerCaller.mousePosition.y);
                    playerCaller.inventoryManager.SubtractSelectedItem(1);
                }
                else if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 2))
                    && playerCaller.inventoryManager.selectedItem.GetItem().itemName != "Wood Platform")
                {
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 2))].itemName == "Wood Door")
                    {
                        base.Use(playerCaller);
                        Debug.Log("Placed Ground Tile: " + playerCaller.inventoryManager.selectedItem.GetItem().GetTile().itemName);
                        playerCaller.terrainGenerator.PlayerPlaceGroundTile(playerCaller.inventoryManager.selectedItem.GetItem().GetTile(), playerCaller.mousePosition.x, playerCaller.mousePosition.y);
                        playerCaller.inventoryManager.SubtractSelectedItem(1);
                    }
                }
                else if (playerCaller.terrainGenerator.wallTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y)))
                {
                    base.Use(playerCaller);
                    Debug.Log("Placed Ground Tile: " + playerCaller.inventoryManager.selectedItem.GetItem().GetTile().itemName);
                    playerCaller.terrainGenerator.PlayerPlaceGroundTile(playerCaller.inventoryManager.selectedItem.GetItem().GetTile(), playerCaller.mousePosition.x, playerCaller.mousePosition.y);
                    playerCaller.inventoryManager.SubtractSelectedItem(1);
                }
            }
        }
        else if (playerCaller.inventoryManager.selectedItem.GetItem().GetTile().tileLocation == TileLocation.Surface)
        {
            if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y)))
            {
                if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y))].itemName == "Crafting Table")
                { return; }
            }

            if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y)))
            {
                if (playerCaller.inventoryManager.selectedItem.GetItem().itemName == "Storage Barrel" || playerCaller.inventoryManager.selectedItem.GetItem().itemName == "Storage Chest")
                {
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y))].itemName == "Storage Barrel" ||
                        playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y))].itemName == "Storage Chest")
                    { return; }
                }
            }
            if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y)))
            {
                if (playerCaller.inventoryManager.selectedItem.GetItem().itemName == "Storage Barrel" || playerCaller.inventoryManager.selectedItem.GetItem().itemName == "Storage Chest")
                {
                    if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y))].itemName == "Storage Barrel" ||
                        playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y))].itemName == "Storage Chest")
                    { return; }
                }
            }

            if (!playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y)) &&
            playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 1)))
            {
                if (playerCaller.inventoryManager.selectedItem.GetItem().itemName == "Crafting Table")
                {
                    if (playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y)) ||
                        playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y + 1)) ||
                        playerCaller.terrainGenerator.groundTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y + 1)))
                    { return; }
                    if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y)))
                    {
                        if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y))].replaceable)
                        {
                            playerCaller.terrainGenerator.PlayerRemoveSurfaceTile(playerCaller.mousePosition.x, playerCaller.mousePosition.y);
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y)))
                    {
                        if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y))].replaceable)
                        {
                            playerCaller.terrainGenerator.PlayerRemoveSurfaceTile(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if (playerCaller.terrainGenerator.surfaceTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y)))
                {
                    /*if (playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y))].replaceable)
                    {
                        base.Use(playerCaller);
                        Debug.Log("Placed Surface Tile: " + playerCaller.inventoryManager.selectedItem.GetItem().GetTile());
                        Debug.Log("Replaced Surface Tile: " + playerCaller.terrainGenerator.surfaceTileClasses[playerCaller.terrainGenerator.surfaceTilePositions.IndexOf(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y))].GetItem().itemName);
                        playerCaller.terrainGenerator.PlayerPlaceSurfaceTile(playerCaller.inventoryManager.selectedItem.GetItem().GetTile(), playerCaller.mousePosition.x, playerCaller.mousePosition.y);
                        playerCaller.inventoryManager.SubtractSelectedItem(1);
                    }
                    else*/
                    {
                        return;
                    }
                }

                base.Use(playerCaller);
                Debug.Log("Placed Surface Tile: " + playerCaller.inventoryManager.selectedItem.GetItem().GetTile().itemName);
                playerCaller.terrainGenerator.PlayerPlaceSurfaceTile(playerCaller.inventoryManager.selectedItem.GetItem().GetTile(), playerCaller.mousePosition.x, playerCaller.mousePosition.y);
                playerCaller.inventoryManager.SubtractSelectedItem(1);
            }
        }
        else if (playerCaller.inventoryManager.selectedItem.GetItem().GetTile().tileLocation == TileLocation.Wall)
        {
            if (!playerCaller.terrainGenerator.wallTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y)))
            {
                if ((playerCaller.terrainGenerator.wallTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y + 1))
                    || playerCaller.terrainGenerator.wallTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x, playerCaller.mousePosition.y - 1))
                    || playerCaller.terrainGenerator.wallTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x + 1, playerCaller.mousePosition.y))
                    || playerCaller.terrainGenerator.wallTilePositions.Contains(new Vector2Int(playerCaller.mousePosition.x - 1, playerCaller.mousePosition.y))))
                {
                    base.Use(playerCaller);
                    Debug.Log("Placed Wall Tile: " + playerCaller.inventoryManager.selectedItem.GetItem().GetTile().itemName);
                    playerCaller.terrainGenerator.PlayerPlaceWallTile(playerCaller.inventoryManager.selectedItem.GetItem().GetTile(), playerCaller.mousePosition.x, playerCaller.mousePosition.y);
                    playerCaller.inventoryManager.SubtractSelectedItem(1);
                }
            }
        }
    }

    public override TileClass GetTile() { return this; }
}
