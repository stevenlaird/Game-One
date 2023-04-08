using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Parents and Prefabs")]
    private PlayerController player;
    private CamController mainCamera;
    private InventoryManager inventory;
    private GameObject surfaceSpawnerPrefab;
    private GameObject caveEnemySpawnerPrefab;
    private GameObject tileDamagePrefab;
    private GameObject tileDropSquare;
    private GameObject tileDropCircle;
    private GameObject torchLight;
    private RuntimeAnimatorController torchAnim;
    private RuntimeAnimatorController craftTableAnim;
    private RuntimeAnimatorController ovenAnim;

    [HideInInspector] public GameObject[] worldChunks;

    [HideInInspector] public TileDirectory tileDirectory;
    [HideInInspector] public List<Vector2> groundTilePositions = new List<Vector2>();
    [HideInInspector] public List<GameObject> groundTileObjects = new List<GameObject>();
    [HideInInspector] public List<TileClass> groundTileClasses = new List<TileClass>();

    [HideInInspector] public List<Vector2> wallTilePositions = new List<Vector2>();
    [HideInInspector] public List<GameObject> wallTileObjects = new List<GameObject>();
    [HideInInspector] public List<TileClass> wallTileClasses = new List<TileClass>();

    [HideInInspector] public List<Vector2> surfaceTilePositions = new List<Vector2>();
    [HideInInspector] public List<GameObject> surfaceTileObjects = new List<GameObject>();
    [HideInInspector] public List<TileClass> surfaceTileClasses = new List<TileClass>();

    [Header("Sprite Lit Material")]
    public Material spriteLit;          // Needed so generated tiles are lit in final build

    [Header("Settings Applied in Main Menu")]
    public int worldWidth;              // Main Menu default = 1000
    public float hillRange;             // Main Menu default = 15f
    public float caveRarity;            // Main Menu default = 0.08f
    public float caveDensity;           // Main Menu default = 0.40f
    public bool generateCaves;          // Main Menu default = true
    public bool spawnEnemies;           // Main Menu default = true

    [Header("Generation Settings")]
    public float seed;
    public int chunkSize;
    public int renderDistance = 100;
    private int worldHeight = 100;

    [Header("Cave Settings")]
    public Texture2D caveNoiseTexture;

    [Header("Biome Customization")]
    public BiomeClass[] biomes;
    private BiomeClass currentBiome;

    /*    [Header("Biome Map")]
    public float biomeFrequency = 0.8f;
    public Gradient biomeGradient;  //generates gradient for random biomes.
    public Texture2D biomeMap;*/    //unused, does not fit map style I wanted for game.

    [Header("Ore Customization")]
    public OreMaps[] ores;

    [Header("Cave Spawner Customization")]
    public EnemySpawnerMaps[] caveEnemySpawners;

    [Header("Liquid Customization")]
    public LiquidMaps[] liquids;

    ///////////////////

    private void Start()
    {
        // Apply Generation Settings Used in Main Menu
        worldWidth = (int)WorldGenerationSettings.worldWidthSetting;
        hillRange = WorldGenerationSettings.hillHeightSetting;
        caveRarity = WorldGenerationSettings.caveRaritySetting;
        caveDensity = WorldGenerationSettings.caveDensitySetting;
        generateCaves = WorldGenerationSettings.generateCavesSetting;
        spawnEnemies = WorldGenerationSettings.spawnEnemiesSetting;

        // Seed value used to randomize world
        seed = UnityEngine.Random.Range(-10000, 10000);
        // Default chunk size would be 50. Set to /40 to change default chunk size to 25.
        chunkSize = Mathf.RoundToInt(worldWidth / 20); 
        
        LoadPrefabs();          // Assign Prefabs & Parents
        GenerateTextures();     // Generate Cave, Ore, and Liquid Map Textures
        GenerateChunks();       // Generate Chunk Parents
        GenerateWorldBorder();  // Generate Invisible World Border
        GenerateTerrain();      // Generate Ground Tiles & Surface Tiles
        GenerateTerrainWalls(); // Generate Wall Tiles

        player.PlaceAtSpawn();  // Move Player to SpawnPosition
        mainCamera.Spawn(new Vector3(player.spawnPosition.x, player.spawnPosition.y, mainCamera.transform.position.z));
        mainCamera.worldWidth = worldWidth;

        LoadChunks();
    }

    /*private void OnValidate()
    {
        GenerateTextures();     // Generate textures in runtime to fine tune random Perlin textures
    }*/

    private void Update()
    {
        LoadChunks();
    }

    void LoadChunks()   // Enables chunks inside renderDistance var, Disables chunks outside renderDistance var. Based off players location
    {
        for (int i = 0; i < worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(player.transform.position.x, 0)) > renderDistance)
                worldChunks[i].SetActive(false);
            else
                worldChunks[i].SetActive(true);
        }
    }

    void LoadPrefabs()  // Assign Prefabs & Parents
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
        mainCamera = CamController.FindObjectOfType<CamController>();
        inventory = InventoryManager.FindObjectOfType<InventoryManager>();
        tileDirectory = Resources.Load<TileDirectory>("TileDirectory");
        surfaceSpawnerPrefab = Resources.Load<GameObject>("SurfaceSpawner");
        caveEnemySpawnerPrefab = Resources.Load<GameObject>("OgreSpawner");
        tileDamagePrefab = Resources.Load<GameObject>("TileDamage");
        tileDropSquare = Resources.Load<GameObject>("TileDropSquare");
        tileDropCircle = Resources.Load<GameObject>("TileDropCircle");
        torchLight = Resources.Load<GameObject>("TorchLight");
        torchAnim = Resources.Load<RuntimeAnimatorController>("Torch");
        craftTableAnim = Resources.Load<RuntimeAnimatorController>("Crafting Table");
        ovenAnim = Resources.Load<RuntimeAnimatorController>("Oven");
    }

    // Callback to place tiles in the proper assigned biome.
    public BiomeClass GetCurrentBiome(int x, int randomNum) 
    {
        if (x + randomNum < worldWidth * .05)
            currentBiome = biomes[4]; //Ice Biome
        else if (x + randomNum >= worldWidth * .05 && x + randomNum < worldWidth * .30)
            currentBiome = biomes[3]; //Snow Biome
        else if (x + randomNum >= worldWidth * .30 && x + randomNum <= worldWidth * .50)
            currentBiome = biomes[0]; //Grassland Biome
        else if (x + randomNum > worldWidth * .50 && x + randomNum <= worldWidth * .70)
            currentBiome = biomes[1]; //Forest Biome
        else if (x + randomNum > worldWidth * .70 && x + randomNum <= worldWidth * .95)
            currentBiome = biomes[2]; // Desert Biome
        else if (x + randomNum > worldWidth * .95)
            currentBiome = biomes[5]; // Lava Biome

        return currentBiome;
    }

/*    public void GenerateBiomes()
    {
        biomeMap = new Texture2D(worldWidth, worldWidth);

        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * biomeFrequency, seed * biomeFrequency);

                Color col = biomeGradient.Evaluate(v);
                biomeMap.SetPixel(x, y, col);
            }
        }
        biomeMap.Apply();
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            if (biomes[i].biomeColor == biomeMap.GetPixel(x, y))
            {
                return biomes[i];
            }
        }

        return currentBiome;
    }*/

    // Generates a random Perlin noise texture using world seed along with two input variables and a texture2D
    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if (v > limit)
                {
                    noiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x, y, Color.black);
                }
            }
        }
        noiseTexture.Apply();
    }

    // Generate random pixel map using set inspector values
    // GenerateTerrain calls for these generated maps
    public void GenerateTextures()  
    {
        // FOR all biomes in the array of biomes defined in the Inspector
        for (int i = 0; i < biomes.Length; i++)
        {
            // Generate base texture using worldWidth (World Size)
            caveNoiseTexture = new Texture2D(worldWidth, worldWidth);
            // Calls GenerateNoiseTexture using the base texture, the caveRarity and the caveDensity from the Input selected on main menu
            GenerateNoiseTexture(caveRarity, caveDensity, caveNoiseTexture);
        }
        // FOR all oreMaps in the array of ores defined in the Inspector
        for (int x = 0; x < ores.Length; x++)
        {
            // Generate base texture using worldWidth (World Size)
            ores[x].mapTexture = new Texture2D(worldWidth, worldWidth);
            // Calls GenerateNoiseTexture using the base texture, the oreRarity, and the oreDensity defined in the Inspector
            GenerateNoiseTexture(ores[x].rarity, ores[x].density, ores[x].mapTexture);
        }
        // FOR all enemySpawnerMaps in the array of enemySpawners defined in the Inspector
        for (int x = 0; x < caveEnemySpawners.Length; x++)
        {
            // Generate base texture using worldWidth (World Size)
            caveEnemySpawners[x].mapTexture = new Texture2D(worldWidth, worldWidth);
            // Calls GenerateNoiseTexture using the base texture, the spawnerRarity, and the spawnerDensity defined in the Inspector
            GenerateNoiseTexture(caveEnemySpawners[x].rarity, caveEnemySpawners[x].density, caveEnemySpawners[x].mapTexture);
        }
        // FOR all liquidMaps in the array of liquids definied in the Inspector
        // Not using liquids yet. Want to add water and lava
        for (int l = 0; l < liquids.Length; l++)
        {
            // Generate base texture using worldWidth (World Size)
            liquids[l].mapTexture = new Texture2D(worldWidth, worldWidth);
            // Calls GenerateNoiseTexture using the base texture, the liquidRarity, and the liquidDensity defined in the Inspector
            GenerateNoiseTexture(liquids[l].rarity, liquids[l].density, liquids[l].mapTexture);
        }
    }

    //Generate chunk GameObjects with children for tiles, tiledrops, and spawners
    // Used for loading everything in chunks and organizing hierarchy
    public void GenerateChunks()    
    {                                               
        int numChunks = worldWidth / chunkSize;
        worldChunks = new GameObject[numChunks];
        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = ("Chunk " + (i + 1));
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;

            GameObject walls = new GameObject("Walls");
            walls.transform.parent = worldChunks[i].transform;
            GameObject groundTiles = new GameObject("Ground Tiles");
            groundTiles.transform.parent = worldChunks[i].transform;
            GameObject surfaceObjects = new GameObject("Surface Objects");
            surfaceObjects.transform.parent = worldChunks[i].transform;
            GameObject tileDrops = new GameObject("Tile Drops");
            tileDrops.transform.parent = worldChunks[i].transform;
            GameObject surfaceSpawners = new GameObject("Surface Spawners");
            surfaceSpawners.transform.parent = worldChunks[i].transform;
            GameObject caveEnemySpawners = new GameObject("Cave Enemy Spawners");
            caveEnemySpawners.transform.parent = worldChunks[i].transform;
        }
    }

    public void GenerateWorldBorder()
    {
        GameObject worldBorder = new GameObject("World Barrier"); // Generate GameObjects for hierarchy organization
        worldBorder.transform.parent = this.transform;
        GameObject left = new GameObject("Left");
        left.transform.parent = worldBorder.transform;
        GameObject right = new GameObject("Right");
        right.transform.parent = worldBorder.transform;
        GameObject top = new GameObject("Top");
        top.transform.parent = worldBorder.transform;

        for (int x = -1; x <= worldWidth; x++)
        {
            for (int y = -11; y < worldHeight + 57 + 1; y++)
            {
                if (x == -1 || x == worldWidth || y == worldHeight + 57) // Border is placed 58 tiles above the surface
                {
                    GameObject barrier = new GameObject();

                    barrier.name = "barrier"; // Name GameObject
                    barrier.transform.position = new Vector2(x + 0.5f, y + 0.5f); // Move tile to x,y in loop

                    if (x == -1)
                        barrier.transform.parent = left.transform; // Parent each wall for hierarchy organization
                    else if (x == worldWidth)
                        barrier.transform.parent = right.transform;
                    else if (y == worldHeight + 57)
                        barrier.transform.parent = top.transform;

                    barrier.AddComponent<SpriteRenderer>();
                    barrier.GetComponent<SpriteRenderer>().sprite = tileDirectory.barrier.itemSprites[0]; // Assign sprite
                    barrier.GetComponent<SpriteRenderer>().enabled = false; // Make barriers invisible
                    barrier.GetComponent<SpriteRenderer>().sortingOrder = 0;

                    barrier.AddComponent<BoxCollider2D>();
                    barrier.GetComponent<BoxCollider2D>().size = Vector2.one; // Player collides with barriers
                }
            }
        }
    }

    public void GenerateTerrain()
    {
        TileClass tileClass;
        for (int x = 0; x < worldWidth; x++)
        {
            float height;
            int bedrockHeight = UnityEngine.Random.Range(0, 3);

            for (int y = -10; y < worldHeight + 57; y++)
            {
                // Calls GetCurrentBiome using Input location x, and a random number between 1 and 10
                // The random number allows for a gradient effect in tiles between biomes
                currentBiome = GetCurrentBiome(x, UnityEngine.Random.Range(0, 10));
                // Height is defined by Perlin noise using seed, terrainFreq of the currentBiome, and hillRange
                height = Mathf.PerlinNoise((x + seed) * currentBiome.terrainFreq, seed * currentBiome.terrainFreq) * hillRange + worldHeight;
                // Dirt Layer height changes every time X changes for a varying height in terrain
                int dirtLayerHeight = UnityEngine.Random.Range(currentBiome.dirtLayerMin, currentBiome.dirtLayerMax);

                // Define Player's spawn position when generating terrain
                // Player's spawn will be in the middle of the world 2 tiles above surface
                if (x == worldWidth / 2)
                    player.spawnPosition = new Vector2(x + 0.5f, height + 2);

                // Break function IF y >= height to define hills of the terrain
                if (y >= height)
                    break;

                // IF white pixel in gravel oreMap AND up to 80 below surface, tile to place = gravel
                if (ores[0].mapTexture.GetPixel(x, y).r > 0.5f && height - y < 80) 
                    tileClass = tileDirectory.gravel;

                // ELSE IF white pixel in sand oreMap AND 40 below surface AND not in the snow biome, tile to place = sand
                else if (ores[1].mapTexture.GetPixel(x, y).r > 0.5f && height - y < 40 && x > worldWidth * .30) 
                    tileClass = tileDirectory.sand;

                // ELSE IF y less than height - dirt layer height
                // This allows the next statements after to fill the remaining area with dirt and grass
                // Or sand/sandstone in desert, Or snow/ice in the snow
                else if (y < height - dirtLayerHeight)
                {
                    // IF y less than height - 25 - dirt layer height, tile to place =  stone of biome type
                    // Adds a second "dirt" layer of the biomes stone type for another layer of terrain
                    if (y > height - 25 - dirtLayerHeight)
                        tileClass = currentBiome.tileDirectory.stone; 
                    // ELSE, tile to place =  normal stone
                    else
                        tileClass = tileDirectory.stone;

                    // The next checks do not use 'else' so the ores can be overwritten, this allows the more rare ores to not be extremely rare
                    // IF white pixel in coal map, tile to place = coal
                    if (ores[2].mapTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].spawnXFromSurface)           
                        tileClass = tileDirectory.coal;                
                    // IF white pixel in iron map, tile to place = iron
                    if (ores[3].mapTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].spawnXFromSurface)
                        tileClass = tileDirectory.iron;                
                    // IF white pixel in gold map, tile to place = gold
                    if (ores[4].mapTexture.GetPixel(x, y).r > 0.5f && height - y > ores[4].spawnXFromSurface)
                        tileClass = tileDirectory.gold;                
                    // IF white pixel in diamond map, tile to place = diamond
                    // (spawn from surface changed to from base in IF statement)
                    if (ores[5].mapTexture.GetPixel(x, y).r > 0.5f && y - dirtLayerHeight < ores[5].spawnXFromSurface)
                        tileClass = tileDirectory.diamond;                                                              
                }
                // ELSE IF y less than height - 1. This leaves space for the grass layer
                else if (y < height - 1)
                {
                    // Tile to place =  dirt of biome type
                    tileClass = currentBiome.tileDirectory.dirt; 
                }
                // ELSE remaining space is filled in with grass and surface spawners that will be invisible and contain a script
                else
                {
                    // Tile to place = grass of biome type on top layer of terrain
                    tileClass = currentBiome.tileDirectory.grass; 
                    // These tiles have a SurfaceSpawner Prefab placed under them / If selected true in menu settings
                    if (spawnEnemies == true)
                    { PlaceTile(tileDirectory.surfaceSpawner, x, y); } 
                }

                // IF y less than bedrockHeight and more than -10, tile to place = bedrock world border on the bottom of world
                // bedRockHeight defined at begging of each 'X' loop as random between 0-2
                if (y < bedrockHeight && y >= -10)
                    tileClass = tileDirectory.bedrock; 

                // IF generateCaves was enabled (selected from main menu), AND within the defined world space
                if (generateCaves && x >= 0 && x <= worldWidth && y >= bedrockHeight)
                {
                    // Will only place previously selected tiles if there is not a cave here
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    { PlaceTile(tileClass, x, y); }

                    // Place OgreSpawner Prefabs in caves
                    else if (caveEnemySpawners[0].mapTexture.GetPixel(x, y).r > 0.5f 
                        && height - y > caveEnemySpawners[0].spawnXFromSurface && spawnEnemies == true)
                    { PlaceTile(tileDirectory.caveEnemySpawner, x, y); }

                    /*// Place water in caves up to x below surface and not in snow or desert (Currently not implemented)
                    else if (liquids[0].mapTexture.GetPixel(x, y).r > 0.5f && height - y < liquids[0].spawnXFromSurface
                        && y < height - 1 && x > worldWidth * .30 && x < worldWidth * .70)
                    { PlaceTile(tileDirectory.water, x, y); }
                    // Place lava in caves up to x above bedrock (Currently not implemented)
                    else if (liquids[1].mapTexture.GetPixel(x, y).r > 0.5f && y < liquids[1].spawnXFromSurface + 4 && y > 4)
                    { PlaceTile(tileDirectory.lava, x, y); }*/
                }
                // ELSE generateCaves was not enabled, there is no statement looking for a caveMap to place tiles
                // therefore, there will be no cave gaps in the terrain and won't place spawners underground
                // can add a check in the future that allows spawners to be placed underground at defined positions
                // as spawners already won't spawn enemies ontop of ground tiles
                else
                {
                    PlaceTile(tileClass, x, y);
                }


                // ELSE y greater than or equal to height - 1. Places tiles above grass layer (World Surface)
                if (y >= height - 1)                                                                
                {
                    // IF there is a ground tile here
                    if (groundTilePositions.Contains(new Vector2(x, y)))                                
                    {
                        // Calculates random values from biomeClass'
                        int treeRandom = UnityEngine.Random.Range(0, currentBiome.treeChance);              
                        int cactusRandom = UnityEngine.Random.Range(0, currentBiome.cactusChance);
                        int grassRandom = UnityEngine.Random.Range(0, currentBiome.tallGrassChance);
                        int mushroomRandom = UnityEngine.Random.Range(0, currentBiome.mushroomChance);

                        // IF treeRandom is 1, randomizer is TRUE
                        if (treeRandom == 1)                                                                
                        {
                            // IF no surface tiles 3 above the ground and 1 to the left or right
                            // (in this case its usually logs of other trees)
                            if (!surfaceTilePositions.Contains(new Vector2(x - 1, y + 3)) &&                    
                                !surfaceTilePositions.Contains(new Vector2(x + 1, y + 3)))                      
                            {
                                // IF currentBiome is Grassland or Forest
                                if (currentBiome.biomeName == "Grassland" || currentBiome.biomeName == "Forest")
                                {
                                    // Calls GenerateTree 1 tile above the biomes grass layer
                                    GenerateTree(x, y + 1); 
                                }
                                // IF currentBiome is Desert
                                else if (currentBiome.biomeName == "Desert")
                                {
                                    // Calls GenerateAcaciaTree 1 tile above the biomes "grass" layer (Sand)
                                    GenerateAcaciaTree(x, y + 1); 
                                }
                                // IF currentBiome is Snow
                                else if (currentBiome.biomeName == "Snow")
                                {
                                    // Calls GenerateSpruceTree 1 tile above the biomes "grass" layer (Snow)
                                    GenerateSpruceTree(x, y + 1);
                                }
                            }
                        }
                        // ELSE IF cactusRandom is 1, randomizer is TRUE, AND a tree wasn't generated
                        else if (cactusRandom == 1)                                                         
                        {
                            // IF no surface tiles 2 above the ground to the left or right
                            // (in this case its other logs or cacti)
                            if (!surfaceTilePositions.Contains(new Vector2(x - 1, y + 2)) &&                    
                                !surfaceTilePositions.Contains(new Vector2(x + 1, y + 2)))                      
                            {
                                // Call GenerateCactus 1 tile above the biomes "grass" layer (Sand)
                                GenerateCactus(x, y + 1);
                            }
                        }
                        // ELSE IF grassRandom is 1, randomizer is TRUE, AND a tree or cacti wasn't generated
                        else if (grassRandom == 1)                                                          
                        {
                            // Places tall grass or dead grass
                            PlaceTile(currentBiome.tileDirectory.tallGrass, x, y + 1);
                        }
                        // ELSE IF mushroomRandom is 1, randomizer is TRUE, AND a tree, cacti, or grass wasn't generated/placed
                        else if (mushroomRandom == 1)
                        {
                            // Calculate mushroomChoice as 0, 1, or 2
                            int mushroomChoice = UnityEngine.Random.Range(0, 3);
                            if (mushroomChoice == 0)                                                            
                                PlaceTile(tileDirectory.mushroomRed, x, y + 1);// Places red mushroom
                            if (mushroomChoice == 1)                                                        
                                PlaceTile(tileDirectory.mushroomBrown, x, y + 1);// Places brown mushroom
                            if (mushroomChoice == 2)
                                PlaceTile(tileDirectory.mushroomTan, x, y + 1);// Places tan mushroom
                        }
                    }
                }
            }
        }
    }

    public void GenerateTerrainWalls()
    {
        TileClass tileClass;
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y <= worldHeight + 57; y++)
            {
                currentBiome = GetCurrentBiome(x, UnityEngine.Random.Range(0, 10));
                float height;
                height = Mathf.PerlinNoise((x + seed) * currentBiome.terrainFreq, seed * currentBiome.terrainFreq) * hillRange + worldHeight;
                int dirtLayerHeight = UnityEngine.Random.Range(currentBiome.dirtLayerMin, currentBiome.dirtLayerMax);

                if (y < height)
                {
                    if (ores[0].mapTexture.GetPixel(x, y).r > 0.5f && height - y < 80)
                    {
                        tileClass = tileDirectory.gravelWall;//place gravel up to 80 below surface
                    }
                    else if (ores[1].mapTexture.GetPixel(x, y).r > 0.5f && height - y < 40 && x > worldWidth * .30)
                    {
                        tileClass = tileDirectory.sandWall;//place sand up to 40 below surface and not in snow
                    }
                    else if (y < height - dirtLayerHeight)
                    {
                        tileClass = currentBiome.tileDirectory.stoneWall; //place biome stone
                    }
                    else
                    {
                        tileClass = currentBiome.tileDirectory.dirtWall; //place grass on top layer of terrain
                    }

                    PlaceTile(tileClass, x, y);
                }
            }
        }
    }

    public void GenerateTree(int x, int y)
    {
        //Place Tree Base
        PlaceTile(tileDirectory.treeBase, x, y);

        //Place Logs based on random range
        int treeHeight = UnityEngine.Random.Range(currentBiome.treeMinHeight, currentBiome.treeMaxHeight);
        for (int i = 1; i < treeHeight - 1; i++)
        {
            PlaceTile(tileDirectory.treeLog, x, y + i);
        }

        PlaceTile(tileDirectory.treeLogTop, x, y + treeHeight - 1);

        //Place Leaves on Tree
        for (int a = -1; a <=1; a++)
        {
            for (int b = 0; b <=2; b++)
            {
                PlaceTile(tileDirectory.treeLeaf, x + a, y + treeHeight + b);
            }
        }
    }

    public void GenerateAcaciaTree(int x, int y)
    {
        //Place Tree Base
        PlaceTile(tileDirectory.acaciaTreeBase, x, y);

        //Place Logs based on random range
        int treeHeight = UnityEngine.Random.Range(currentBiome.treeMinHeight, currentBiome.treeMaxHeight);
        for (int i = 1; i < treeHeight; i++)
        {
            PlaceTile(tileDirectory.acaciaTreeLog, x, y + i);
        }
        //Acacia Log Style
        PlaceTile(tileDirectory.acaciaTreeLog, x + 1, y + treeHeight - 1);
        PlaceTile(tileDirectory.acaciaTreeLogTop, x + 2, y + treeHeight);
        PlaceTile(tileDirectory.acaciaTreeLogTop, x - 1, y + treeHeight);
        //Place Leaves on Tree
        for (int a = -3; a <= 5; a++)
        {
            PlaceTile(tileDirectory.acaciaTreeLeaf, x + a, y + treeHeight + 1);
        }
        for (int b = -2; b <= 3; b++)
        {
            PlaceTile(tileDirectory.acaciaTreeLeaf, x + b, y + treeHeight + 2);
        }
    }

    public void GenerateSpruceTree(int x, int y)
    {
        //Place Tree Base
        PlaceTile(tileDirectory.spruceTreeBase, x, y);

        //Place Logs based on random range
        int treeHeight = UnityEngine.Random.Range(currentBiome.treeMinHeight, currentBiome.treeMaxHeight);
        for (int i = 1; i < treeHeight - 1; i++)
        {
            PlaceTile(tileDirectory.spruceTreeLog, x, y + i);
        }

        PlaceTile(tileDirectory.spruceTreeLogTop, x, y + treeHeight - 1);

        //Place Leaves on Tree
        for (int a = 0; a <= 6; a++)
        {
            PlaceTile(tileDirectory.spruceTreeLeaf, x, y + treeHeight + a);
        }
        for (int b = 0; b <= 4; b++)
        {
            PlaceTile(tileDirectory.spruceTreeLeaf, x - 1, y + treeHeight + b);
        }
        for (int c = 0; c <= 4; c++)
        {
            PlaceTile(tileDirectory.spruceTreeLeaf, x + 1, y + treeHeight + c);
        }
        for (int d = 0; d <= 2; d++)
        {
            PlaceTile(tileDirectory.spruceTreeLeaf, x + 2, y + treeHeight + d);
        }
        for (int e = 0; e <= 2; e++)
        {
            PlaceTile(tileDirectory.spruceTreeLeaf, x - 2, y + treeHeight + e);
        }
        PlaceTile(tileDirectory.spruceTreeLeaf, x + 3, y + treeHeight);
        PlaceTile(tileDirectory.spruceTreeLeaf, x - 3, y + treeHeight);
    }

    public void GenerateCactus(int x, int y)
    {
        int cactusHeight = UnityEngine.Random.Range(1, 5);
        for (int i = 0; i < cactusHeight; i++)
        {
            PlaceTile(tileDirectory.cactus, x, y + i);
        }
    }

    public void PlaceTile (TileClass tile, int x, int y)
    {        
        GameObject newTile = new GameObject();
            
        newTile.name = tile.itemName; // Name the GameObject to Item's Name
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f); // Place at (X, Y) location. add 0.5f to each to be in line with editor grid

        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize); // Determine which chunk the block is in using X
        chunkCoord /= chunkSize;

        if (newTile.name == "Surface Spawner")
        {
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(4).transform;
            newTile.layer = 9;
            GameObject spawner;
            spawner = Instantiate(surfaceSpawnerPrefab, newTile.transform.position, Quaternion.identity);
            spawner.transform.parent = newTile.transform;
            spawner.layer = 9;
            return;
        }
        if (newTile.name == "Cave Enemy Spawner")
        {
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(5).transform;
            newTile.layer = 9;
            GameObject spawner;
            spawner = Instantiate(caveEnemySpawnerPrefab, newTile.transform.position, Quaternion.identity);
            spawner.transform.parent = newTile.transform;
            spawner.layer = 9;
            return;
        }

        newTile.AddComponent<TileHealth>();
        newTile.GetComponent<TileHealth>().tileHealth = tile.tileHealth;

        newTile.AddComponent<SpriteRenderer>(); //add sprite renderer component
        newTile.GetComponent<SpriteRenderer>().material = spriteLit;
        int spriteIndex = UnityEngine.Random.Range(0, tile.itemSprites.Length); //random sprite selected
        newTile.GetComponent<SpriteRenderer>().sprite = tile.itemSprites[spriteIndex]; //random sprite applied


        if (tile.tileLocation == TileClass.TileLocation.Wall)
        {
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(0).transform; //parent the block to walls of chunk
            newTile.GetComponent<SpriteRenderer>().sortingLayerName = "Terrain_WallTiles";
            newTile.layer = 11;

            wallTilePositions.Add(new Vector2(Mathf.RoundToInt(newTile.transform.position.x - 0.5f), Mathf.RoundToInt(newTile.transform.position.y - 0.5f))); //add location of blocks to world tiles position list
            wallTileObjects.Add(newTile); //add tile to object list
            wallTileClasses.Add(tile); //add tile to class list
        }
        else if (tile.tileLocation == TileClass.TileLocation.Surface)
        {
            newTile.layer = 13;
            if (newTile.name == "Torch")
            {
                newTile.AddComponent<Animator>().runtimeAnimatorController = torchAnim;
                newTile.AddComponent<CircleCollider2D>().isTrigger = true;
                newTile.GetComponent<CircleCollider2D>().radius = 3f;
                newTile.tag = "Torch";
                GameObject tileTorchLight = Instantiate(torchLight, new Vector2(x + 0.5f , y + 0.5f), Quaternion.identity);
                tileTorchLight.transform.parent = newTile.transform;
            }
            if (newTile.name == "Crafting Table")
            {
                newTile.AddComponent<Animator>().runtimeAnimatorController = craftTableAnim;
                newTile.AddComponent<CraftingTableController>();

                /*newTile.AddComponent<CircleCollider2D>().isTrigger = true;
                newTile.GetComponent<CircleCollider2D>().radius = 3f;
                newTile.tag = "CraftingTable";*/
            }
            if (newTile.name == "Storage Barrel")
            {
                newTile.AddComponent<StorageBarrelController>();
                newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }
            if (newTile.name == "Storage Chest")
            {
                newTile.AddComponent<StorageChestController>();
                newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }
            if (newTile.name == "Wood Door")
            {
                newTile.AddComponent<DoorController>();
            }
            if (newTile.name == "Tree Log Top")
            {
                newTile.AddComponent<BoxCollider2D>().isTrigger = true;
                newTile.GetComponent<BoxCollider2D>().size = new Vector2(3, 3);
                newTile.GetComponent<BoxCollider2D>().offset = new Vector2(0, 2);
                newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                newTile.tag = "TreeTop";
            }
            if (newTile.name == "Acacia Tree Log Top")
            {
                newTile.AddComponent<BoxCollider2D>().isTrigger = true;
                newTile.GetComponent<BoxCollider2D>().size = new Vector2(7, 2);
                newTile.GetComponent<BoxCollider2D>().offset = new Vector2(0, 1.5f);
                newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                newTile.tag = "TreeTop";
            }
            if (newTile.name == "Spruce Tree Log Top")
            {
                newTile.AddComponent<BoxCollider2D>().isTrigger = true;
                newTile.GetComponent<BoxCollider2D>().size = new Vector2(7,7);
                newTile.GetComponent<BoxCollider2D>().offset = new Vector2(0, 4);
                newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                newTile.tag = "TreeTop";
            }
            if (newTile.name.Contains("Leaf"))
            {
                newTile.AddComponent<LeafController>();
                newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                newTile.AddComponent<BoxCollider2D>().size = Vector2.one;
                newTile.layer = 8;
            }

            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(2).transform; //parent tile to current chunk
            newTile.GetComponent<SpriteRenderer>().sortingLayerName = "Terrain_SurfaceTiles";

            surfaceTilePositions.Add(new Vector2(Mathf.RoundToInt(newTile.transform.position.x - 0.5f), Mathf.RoundToInt(newTile.transform.position.y - 0.5f))); //add location of blocks to world tiles position list
            surfaceTileObjects.Add(newTile); //add tile to object list
            surfaceTileClasses.Add(tile); //add tile to class list

        }
        else if (tile.tileLocation == TileClass.TileLocation.Ground)
        {
            if (newTile.name == "Anvil")
            {
                newTile.AddComponent<AnvilController>();
                /*newTile.AddComponent<CircleCollider2D>().isTrigger = true;
                newTile.GetComponent<CircleCollider2D>().radius = 3f;
                newTile.tag = "Anvil";*/
            }
            else if (newTile.name == "Oven")
            {
                newTile.AddComponent<Animator>().runtimeAnimatorController = ovenAnim;
                newTile.AddComponent<OvenController>();
                GameObject tileTorchLight = Instantiate(torchLight, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                tileTorchLight.transform.parent = newTile.transform;
                tileTorchLight.GetComponent<Light2D>().pointLightInnerRadius = 0.5f;
                tileTorchLight.GetComponent<Light2D>().pointLightOuterRadius = 4f;
            }
            else if (newTile.name == "Wood Platform")
            {
                newTile.AddComponent<PlatformController>();
            }
            else
            {
                newTile.AddComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }

            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(1).transform;
            newTile.GetComponent<SpriteRenderer>().sortingLayerName = "Terrain_GroundTiles";
            newTile.layer = 12;

            groundTilePositions.Add(new Vector2(Mathf.RoundToInt(newTile.transform.position.x - 0.5f), Mathf.RoundToInt(newTile.transform.position.y - 0.5f))); //add location of blocks to world tiles position list
            groundTileObjects.Add(newTile); //add tile to object list
            groundTileClasses.Add(tile); //add tile to class list
        }

        /*if (tile.gravity)
        {
            newTile.GetComponent<BoxCollider2D>().size = new Vector2(0.95f, 0.95f);
            newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }*/
    }

    public void PlayerPlaceGroundTile(TileClass tile, int x, int y)
    {
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize); //determine which chunk the block is in
        chunkCoord /= chunkSize;

        if (!groundTilePositions.Contains(new Vector2Int(x, y)))
        {
            if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Crafting Table")
                { return; }
                else if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Wood Door")
                { return; }
                else if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].isTree)
                { return; }

                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].replaceable)
                {
                    Destroy(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2(x, y))]); //destroy object
                    if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].dropTile)
                    {
                        GameObject newTileDrop;
                        InvSlotClass tileDrop;
                        if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].alwaysDropTile == false)
                        {
                            int dropChance = UnityEngine.Random.Range(1, surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].dropTileChance);
                            if (dropChance == 1)
                            {
                                newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 1.5f), Quaternion.identity);
                                newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                                newTileDrop.GetComponent<CircleCollider2D>().enabled = true;

                                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)
                                {
                                    newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                                    newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0]; //
                                    tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem(), 1, 0, 999);
                                }
                                else
                                {
                                    newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                    newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0]; //
                                    tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetItem(), 1, 0, 999);
                                }
                                newTileDrop.GetComponent<TileDropController>().invSlotClass = tileDrop;

                                surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from objects list
                                surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); // remove from class list
                                surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from position list (has to be done last)
                            }
                            else
                            {
                                surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from objects list
                                surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); // remove from class list
                                surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from position list (has to be done last)
                            }
                        }
                        else
                        {
                            newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 1.5f), Quaternion.identity);
                            newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                            newTileDrop.GetComponent<CircleCollider2D>().enabled = true;

                            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)
                            {
                                newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                                newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0]; //
                                tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem(), 1, 0, 999);
                            }
                            else
                            {
                                newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0]; //
                                tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetItem(), 1, 0, 999);
                            }
                            newTileDrop.GetComponent<TileDropController>().invSlotClass = tileDrop;

                            surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from objects list
                            surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); // remove from class list
                            surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from position list (has to be done last)
                        }
                    }
                }
            }

            if (surfaceTilePositions.Contains(new Vector2Int(x - 1, y)))
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y))].itemName == "Crafting Table")
                { return; }
            }
            if (surfaceTilePositions.Contains(new Vector2Int(x, y - 1)))
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y - 1))].itemName == "Crafting Table")
                { return; }

                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y - 1))].itemName == "Wood Door")
                { return; }
            }
            if (surfaceTilePositions.Contains(new Vector2Int(x - 1, y - 1)))
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y - 1))].itemName == "Crafting Table")
                { return; }
            }



            if ((groundTilePositions.Contains(new Vector2Int(x, y + 1))
            || groundTilePositions.Contains(new Vector2Int(x, y - 1))
            || groundTilePositions.Contains(new Vector2Int(x + 1, y))
            || groundTilePositions.Contains(new Vector2Int(x - 1, y)))
            && tile.name != "Wood Platform")
            {
                //place tile if location is empty and beside another tile
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
            else if (surfaceTilePositions.Contains(new Vector2Int(x, y - 2))
                && tile.name != "Wood Platform")
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y - 2))].itemName == "Wood Door")
                {
                    PlaceTile(tile, x, y);
                    FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
                }
            }
            else if (wallTilePositions.Contains(new Vector2Int(x, y)))
            {
                //or if location is empty and a wall is behind
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
        }
    }

    public void PlayerPlaceSurfaceTile(TileClass tile, int x, int y)
    {
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize); //determine which chunk the block is in
        chunkCoord /= chunkSize;

        if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Crafting Table")
            { return; }
        }
        if (surfaceTilePositions.Contains(new Vector2Int(x - 1, y)))
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y))].itemName == "Crafting Table")
            { return; }
        }
        if (surfaceTilePositions.Contains(new Vector2Int(x, y - 1)))
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y - 1))].itemName == "Crafting Table")
            { return; }

            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y - 1))].itemName == "Wood Door")
            { return; }
        }
        if (surfaceTilePositions.Contains(new Vector2Int(x - 1, y - 1)))
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y - 1))].itemName == "Crafting Table")
            { return; }
        }

        if (tile.name == "Storage Barrel" || tile.name == "Storage Chest")
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y))].itemName == "Storage Barrel" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x + 1, y))].itemName == "Storage Barrel" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y))].itemName == "Storage Chest" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x + 1, y))].itemName == "Storage Chest")
            { return; }
        }

        if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Dead Grass" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Tall Grass")
            {
                Destroy(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2(x, y))]); //destroy object
            }
        }

        if (!groundTilePositions.Contains(new Vector2Int(x, y)) &&
            !surfaceTilePositions.Contains(new Vector2Int(x, y)) &&
            groundTilePositions.Contains(new Vector2Int(x, y - 1))) //if location is empty and has a tile under it
        {
            if (tile.name == "Tree Sapling")
            {
                return;
            }

            if (tile.name == "Crafting Table" && 
            !groundTilePositions.Contains(new Vector2Int(x + 1, y)) &&
            //!surfaceTilePositions.Contains(new Vector2Int(x + 1, y)) &&
            !groundTilePositions.Contains(new Vector2Int(x, y + 1)) &&
            !surfaceTilePositions.Contains(new Vector2Int(x, y + 1)) &&
            !groundTilePositions.Contains(new Vector2Int(x + 1, y + 1)) &&
            !surfaceTilePositions.Contains(new Vector2Int(x + 1, y + 1)))
            {
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
            else if (tile.name != "Crafting Table")
            {
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
        }
    }

    public void PlayerPlaceWallTile(TileClass tile, int x, int y)
    {
        if (!wallTilePositions.Contains(new Vector2Int(x, y)))
        {
            if ((wallTilePositions.Contains(new Vector2Int(x, y + 1))
                || wallTilePositions.Contains(new Vector2Int(x, y - 1))
                || wallTilePositions.Contains(new Vector2Int(x + 1, y))
                || wallTilePositions.Contains(new Vector2Int(x - 1, y))))
            {
                //place tile if there is no wall tile, and there is a wall next to place location
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
        }
    }

    public void PlayerRemoveGroundTile (int x, int y)
    {
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);// Calculate chunkCoord using the Input x location / This is the chunk of the Ground Tile at Input location
        chunkCoord /= chunkSize;

        if (groundTilePositions.Contains(new Vector2Int(x, y))// IF there a Ground Tile at Input location
            && !groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].isUnbreakable)// AND it's not unbreakable
        {
            if (surfaceTilePositions.Contains(new Vector2Int(x, y + 1)))// IF there is a Surface Tile above that Ground Tile at Input location
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].replaceable)// IF that Surface Tile is replaceable
                {
                    if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].dropTile)// IF that replaceable Surface Tile drops a tile
                    {
                        PlayerRemoveSurfaceTile(x, y + 1);// Call the RemoveSurfaceTile Function to remove it
                    }
                }
                else if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].isTree)// IF that Surface Tile is a tree
                {
                    return;// Return, Can't remove the Ground Tile
                }
            }

            Destroy(groundTileObjects[groundTilePositions.IndexOf(new Vector2(x, y))]);// Destroy the Ground Tile's GameObject at Input location

            if (inventory.inventoryItems[inventory.selectedSlot].GetItem() != null)// IF the Player is holding something
                                                                                   // Player can break most tiles if they aren't holding an item, but the tile won't create a tiledrop because of this check
            {
                if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].dropTile)// IF the Ground Tile at Input location's item dropTile BOOL is TRUE
                {
                    GameObject newTileDrop;
                    ItemClass tile;

                    if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].alwaysDropTile == false)// IF the Ground Tile at Input location's item alwaysDropTile BOOL is FALSE
                    {
                        int dropChance = UnityEngine.Random.Range(0, groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].dropTileChance);// Calculate dropChance random between 0 and Ground Tile's ItemClass's dropTileChance
                        if (dropChance == 0)// IF dropChance is 0 / Randomizer is TRUE
                        {
                            newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                            newTileDrop.GetComponent<CircleCollider2D>().enabled = true;
                            newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                            if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)
                            {
                                newTileDrop.GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                                newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                                tile = groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem();
                            }
                            else
                            {
                                newTileDrop.GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                tile = groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetItem();
                            }
                            InvSlotClass tiledrop2 = new InvSlotClass(tile, 1, 0, 999);// Create a new InvSlotClass using the ItemClass from proper statement above
                            newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop2;// Assign that InvSlotClass to the TileDrop
                            groundTileObjects.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Object from the list of groundTileObjects at Input location
                            groundTileClasses.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Class from the list of groundTileClasses at Input location
                            groundTilePositions.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Position from the list of groundTilePositions at Input location (has to be done last)
                            return;// END Function
                        }
                        else// ELSE dropChance is not 0 / Randomizer is FALSE
                        {
                            groundTileObjects.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Object from the list of groundTileObjects at Input location
                            groundTileClasses.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Class from the list of groundTileClasses at Input location
                            groundTilePositions.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Position from the list of groundTilePositions at Input location (has to be done last)
                            return;// END Function
                        }
                    }

                    newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);// Instantiate TileDrop at the Ground Tile at Input location
                    newTileDrop.GetComponent<CircleCollider2D>().enabled = true;// Enable it's CircleCollider so Player can pick it up
                    newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;// Parent the TileDrop to the same chunk as the Ground Tile at Input location using calculation at beginning of Function
                    if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)// IF the Ground Tile at Input location's dropped item contains an ItemClass
                    {
                        // Set it's Sprite to the first Sprite of the ItemClass in the Ground Tile's droppedItem ItemClass
                        newTileDrop.GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                        // Give it a black background/outline using that first Sprite
                        newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                        // Assign ItemClass of the TileDrop to the ItemClass in the Ground Tile's droppedItem ItemClass
                        tile = groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem();
                    }
                    else// ELSE the Ground Tile at Input location's dropped item does not contain an ItemClass
                    {
                        // Set it's Sprite to the first Sprite of the Ground Tile's ItemClass
                        newTileDrop.GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                        // Give it a black background/outline using that first Sprite
                        newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = groundTileClasses[groundTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                        // Assign ItemClass of the TileDrop to the ItemClass of the Ground Tile at Input location
                        tile = groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetItem();
                    }
                    InvSlotClass tiledrop = new InvSlotClass(tile, 1, 0, 999);// Create a new InvSlotClass using the ItemClass from proper statement above
                    newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop;// Assign that InvSlotClass to the TileDrop
                }
            }

            groundTileObjects.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Object from the list of groundTileObjects at Input location
            groundTileClasses.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Class from the list of groundTileClasses at Input location
            groundTilePositions.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));// Remove the Ground Tile's Position from the list of groundTilePositions at Input location (has to be done last)

            if (inventory.inventoryItems[inventory.selectedSlot].GetItem() != null)// IF Player is holding an item. Will always either be a tool or barehands
            {
                if (!inventory.inventoryItems[inventory.selectedSlot].GetItem().itemName.Contains("Wood"))// IF the held tool is not made of Wood
                {
                    inventory.inventoryItems[inventory.selectedSlot].SubtractDurability(1);// Lower the held tool's current durability by 1
                }
            }
        }
    }

    public void PlayerRemoveSurfaceTile(int x, int y)
    {
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);// Calculate chunkCoord using the Input x location / This is the chunk of the Surface Tile at Input location
        chunkCoord /= chunkSize;

        if (surfaceTilePositions.Contains(new Vector2Int(x, y)))// IF there a Surface Tile at Input location
        {
            Destroy(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2(x, y))]);// Destroy the Surface Tile's GameObject at Input location

            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].dropTile)// IF the Surface Tile at Input location's item dropTile BOOL is TRUE
            {
                GameObject newTileDrop;
                ItemClass tile;
                
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].alwaysDropTile == false)
                {
                    int dropChance = UnityEngine.Random.Range(1, surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].dropTileChance);
                    if (dropChance == 1)
                    {
                        newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                        if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)
                        {
                            newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                            newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                            tile = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem();
                        }
                        else
                        {
                            newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                            newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                            tile = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetItem();
                        }
                        newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                        newTileDrop.GetComponent<CircleCollider2D>().enabled = true;
                        
                        InvSlotClass tiledrop2 = new InvSlotClass(tile, 1, 0, 999);
                        newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop2;
                        surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));// Remove from position list (has to be done last)
                        return;
                    }
                    else
                    {
                        surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));// Remove from position list (has to be done last)
                        return;
                    }
                }

                newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)
                {
                    newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                    newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                    tile = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem();
                }
                else
                {
                    newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                    newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                    tile = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetItem();
                }
                newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                newTileDrop.GetComponent<CircleCollider2D>().enabled = true;

                InvSlotClass tiledrop = new InvSlotClass(tile, 1, 0, 999);
                newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop;
            }

            surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from objects list
            surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); // remove from class list
            surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); //remove from position list (has to be done last)

            if (surfaceTilePositions.Contains(new Vector2Int(x, y + 1)))
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].isTree)
                    PlayerRemoveSurfaceTile(x, y + 1);
                else
                {
                    if (!inventory.inventoryItems[inventory.selectedSlot].GetItem().itemName.Contains("Wood"))
                    {
                        inventory.inventoryItems[inventory.selectedSlot].SubtractDurability(1);
                    }
                    return;
                }
            }

        }
    }

    public void PlayerRemoveWallTile(int x, int y)
    {
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize); //determine which chunk the block is in
        chunkCoord /= chunkSize;

        if (wallTilePositions.Contains(new Vector2Int(x, y))) //if there is a surface tile
        {
            if ((!wallTilePositions.Contains(new Vector2Int(x, y + 1))
                || !wallTilePositions.Contains(new Vector2Int(x, y - 1))
                || !wallTilePositions.Contains(new Vector2Int(x + 1, y))
                || !wallTilePositions.Contains(new Vector2Int(x - 1, y))))
            {
                Destroy(wallTileObjects[wallTilePositions.IndexOf(new Vector2(x, y))]); //destroy object
                GameObject newTileDrop;
                ItemClass tile;

                newTileDrop = Instantiate(tileDropSquare, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                newTileDrop.GetComponent<CircleCollider2D>().enabled = true;
                newTileDrop.GetComponent<SpriteRenderer>().sprite = wallTileClasses[wallTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                tile = wallTileClasses[wallTilePositions.IndexOf(new Vector2Int(x, y))].GetItem();

                InvSlotClass tiledrop = new InvSlotClass(tile, 1, 0, 999);
                newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop;


                wallTileObjects.RemoveAt(wallTilePositions.IndexOf(new Vector2(x, y))); //remove from objects list
                wallTileClasses.RemoveAt(wallTilePositions.IndexOf(new Vector2(x, y))); // remove from class list
                wallTilePositions.RemoveAt(wallTilePositions.IndexOf(new Vector2(x, y))); //remove from position list (has to be done last)

                if (!inventory.inventoryItems[inventory.selectedSlot].GetItem().itemName.Contains("Wood"))
                {
                    inventory.inventoryItems[inventory.selectedSlot].SubtractDurability(1);
                }
            }
        }
    }

    public void PlayerRemoveGroundTileHealth(int x, int y, int toolDamage, string toolName)
    {
        if (groundTilePositions.Contains(new Vector2Int(x, y)) && //contains ground tile
            !groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].isUnbreakable) //not unbreakable
        {
            if (surfaceTilePositions.Contains(new Vector2Int(x, y + 1))) //if there is a surface tile above it
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].isTree)
                {
                    Debug.Log("Tree Blocking");
                    return;
                }
            }
            if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Diamond") && 
                (toolName.Contains("Wood") || toolName.Contains("Stone") || toolName.Contains("Iron")))
            {
                Debug.Log("Can't Mine, Need Stronger Pickaxe");
                return;
            }
            if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Gold") &&
                (toolName.Contains("Wood") || toolName.Contains("Stone")))
            {
                Debug.Log("Can't Mine, Need Stronger Pickaxe");
                return;
            }
            if (groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount < 1)
            {
                GameObject tileDamage;
                tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                tileDamage.transform.SetParent(groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].transform);
            }
            else if (groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount == 1)
            {
                if (groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].transform.GetChild(0).gameObject.name != "TileDamage(Clone)")
                {
                    GameObject tileDamage;
                    tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                    tileDamage.transform.SetParent(groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].transform);
                }
            }

            groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth -= toolDamage;
            //Debug.Log("Tile Health Now: " + groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth);

            FindObjectOfType<AudioManager>().Play("Player_HitTile");

            if (groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth <= 0)
            {
                PlayerRemoveGroundTile(x, y);
                FindObjectOfType<AudioManager>().Play("Player_BreakTile");
            }
        }
    }

    public void PlayerRemoveSurfaceTileHealth(int x, int y, int toolDamage)
    {
        if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Storage Barrel"))
            {
                if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<StorageBarrelController>().storageEmpty == false)
                { return; }
            }

            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Storage Chest"))
            {
                if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<StorageChestController>().storageEmpty == false)
                { return; }
            }

            if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount < 1)
            {
                GameObject tileDamage;
                tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                tileDamage.transform.SetParent(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].transform);
            }
            else if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount == 1)
            { 
                if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].transform.GetChild(0).gameObject.name != "TileDamage(Clone)")
                {
                    GameObject tileDamage;
                    tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                    tileDamage.transform.SetParent(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].transform);
                }
            }

            surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth -= toolDamage;
            //Debug.Log("Tile Health Now: " + surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth);

            FindObjectOfType<AudioManager>().Play("Player_HitTile");

            if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth <= 0 ||
                surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].name.Contains("Torch") ||
                surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].name.Contains("Grass") ||
                surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].name.Contains("Mushroom"))
            {
                PlayerRemoveSurfaceTile(x, y);
                FindObjectOfType<AudioManager>().Play("Player_BreakTile");
            }
        }
        else
        {
            return;
        }
    }

    public void PlayerRemoveWallTileHealth(int x, int y, int toolDamage)
    {
        if (wallTilePositions.Contains(new Vector2Int(x, y)) &&
           (!wallTilePositions.Contains(new Vector2Int(x, y + 1)) ||
            !wallTilePositions.Contains(new Vector2Int(x, y - 1)) ||
            !wallTilePositions.Contains(new Vector2Int(x + 1, y)) ||
            !wallTilePositions.Contains(new Vector2Int(x - 1, y))) &&
            !groundTilePositions.Contains(new Vector2Int(x, y)))
        {
            if (wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount < 1)
            {
                GameObject tileDamage;
                tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                tileDamage.transform.SetParent(wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform);
            }
            else if (wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount == 1)
            {
                if (wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform.GetChild(0).gameObject.name != "TileDamage(Clone)")
                {
                    GameObject tileDamage;
                    tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                    tileDamage.transform.SetParent(wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform);
                }
            }

            wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth -= toolDamage;
            //Debug.Log("Tile Health Now: " + wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth);

            FindObjectOfType<AudioManager>().Play("Player_HitTile");

            if (wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth <= 0)
            {
                PlayerRemoveWallTile(x, y);
                FindObjectOfType<AudioManager>().Play("Player_BreakTile");
            }
        }
        else
        {
            return;
        }
    }

}