using UnityEngine;

[CreateAssetMenu(fileName = "NewTileClass", menuName = "Item/Tile")]
public class TileClass : ItemClass
{

    [Header("Tile Properties")]
    // The location where the tile can be placed
    public TileLocation tileLocation;
    public enum TileLocation
    {
        Ground,
        Surface,
        Wall
    };

    // The tile's health, defining how durable it is
    public int tileHealth = 100;

    // Additional properties for tile types and behavior
    public bool isTree = false;
    public bool replaceable = false;
    public bool isUnbreakable = false;

    //public bool gravity = false;    
    //public bool liquid = false;

    [Header("Tile Drop Properties")]
    // Properties related to dropping the tile when it is destroyed
    public bool dropTile = true;
    public bool alwaysDropTile = true;
    public int dropTileChance = 1;
    public ItemClass droppedItem;

    ///////////////////

    // Overrides the Use method from the ItemClass to define tile-specific behavior
    public override void Use(PlayerController playerCaller)
    {
        // This method is quite long and consists of several cases where the tile is placed depending on the conditions.
        // I'm not sure how to solve this yet, but these checks must be placed within the place methods of the TerrainGeneration script also

        // Check if the current tile is a ground tile
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
        // Check if the current tile is a surface tile
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
        // Check if the current tile is a wall tile
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

    // Overrides the GetTile method from the ItemClass to return the current tile as a TileClass object
    public override TileClass GetTile() { return this; }
}