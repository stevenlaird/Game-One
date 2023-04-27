using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Parents and Prefabs")]
    private PlayerController player;                        // Reference to the player's script
    private CamController mainCamera;                       // Reference to the main camera's script
    private InventoryManager inventory;                     // Reference to the inventory manager script
    private GameObject surfaceSpawnerPrefab;                // Prefab for surface spawners (resources, enemies)
    private GameObject caveEnemySpawnerPrefab;              // Prefab for cave enemy spawners
    private GameObject tileDamagePrefab;                    // Prefab for the tile damage effect
    private GameObject tileDropSquare;                      // Prefab for square tile drop (items/resources)
    private GameObject tileDropCircle;                      // Prefab for circle tile drop (items/resources)
    private GameObject torchLight;                          // Prefab for the torch light object
    private RuntimeAnimatorController torchAnim;            // Animator controller for the torch object
    private RuntimeAnimatorController craftTableAnim;       // Animator controller for the crafting table object
    private RuntimeAnimatorController ovenAnim;             // Animator controller for the oven object

    [HideInInspector] public GameObject[] worldChunks;      // Array to store the world's chunks
    [HideInInspector] public TileDirectory tileDirectory;   // Reference to the tile directory script

    // Lists to store the ground, wall, and surface tile positions, objects, and classes
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
    public Material spriteLit;  // Material needed for generated tiles to be lit in the final build

    [Header("Settings Applied in Main Menu")]
    public int worldWidth;      // World width, set in Main Menu (default = 1000)
    public float hillRange;     // Hill range, set in Main Menu (default = 15f)
    public float caveRarity;    // Cave rarity, set in Main Menu (default = 0.08f)
    public float caveDensity;   // Cave density, set in Main Menu (default = 0.40f)
    public bool generateCaves;  // Toggle cave generation, set in Main Menu (default = true)
    public bool spawnEnemies;   // Toggle enemy spawning, set in Main Menu (default = true)

    [Header("Generation Settings")]
    public float seed;                  // Seed value for terrain generation
    public int chunkSize;               // Size of the chunks in the world
    public int renderDistance = 100;    // Render distance for terrain
    private int worldHeight = 100;      // Height of the world

    [Header("Cave Settings")]
    public Texture2D caveNoiseTexture;  // Noise texture for cave generation

    [Header("Biome Customization")]
    public BiomeClass[] biomes;         // Array of biomes for the world
    private BiomeClass currentBiome;    // Current biome during terrain generation

    /*    [Header("Biome Map")]
    public float biomeFrequency = 0.8f;
    public Gradient biomeGradient;      // Generates color gradient for random biomes
    public Texture2D biomeMap;*/        // Not Used, does not fit style I wanted for the game

    [Header("Ore Customization")]
    public OreMaps[] ores;                          // Array of ores for the world

    [Header("Cave Spawner Customization")]
    public EnemySpawnerMaps[] caveEnemySpawners;    // Array of cave enemy spawners for the world

    [Header("Liquid Customization")]
    public LiquidMaps[] liquids;                    // Array of liquids for the world

    ///////////////////

    // This method is called for when the script is enabled, and initializes the terrain to be randomly generated
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
        // Default chunk size would be 50. Set to /40 to change default chunk size to 25
        chunkSize = Mathf.RoundToInt(worldWidth / 20); 
        
        LoadPrefabs();          // Assign Prefabs & Parents
        GenerateTextures();     // Generate Cave, Ore, and Liquid Map Textures
        GenerateChunks();       // Generate Chunk Parents
        GenerateWorldBorder();  // Generate Invisible World Border
        GenerateTerrain();      // Generate Ground Tiles & Surface Tiles
        GenerateTerrainWalls(); // Generate Wall Tiles

        player.PlaceAtSpawn();  // Move Player to SpawnPosition
        // Spawn the main camera at the Player's position and set it's worldWidth variable
        mainCamera.Spawn(new Vector3(player.spawnPosition.x, player.spawnPosition.y, mainCamera.transform.position.z));
        mainCamera.worldWidth = worldWidth;
        
        LoadChunks();           // Load and unload chunks based on player's spawn position
    }

    /*private void OnValidate()
    {
        GenerateTextures();     // Generate textures during runtime to fine tune random Perlin textures
    }*/

    // This method is called once per frame. It is responsible for loading and unloading chunks based on the player's position.
    private void Update()
    {
        LoadChunks();
    }

    // Enables chunks inside renderDistance var, Disables chunks outside renderDistance var. Based off players location
    void LoadChunks()   
    {
        // Iterate through all worldChunks
        for (int i = 0; i < worldChunks.Length; i++)
        {
            // Calculate the distance between the current chunk's center and the player's position
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(player.transform.position.x, 0)) > renderDistance)
                // IF the distance is greater than renderDistance, set the chunk to inactive
                { worldChunks[i].SetActive(false); }
            else
                // IF the distance is within renderDistance, set the chunk to active
                { worldChunks[i].SetActive(true); }
        }
    }

    // Assign, load, and set prefabs & parents
    void LoadPrefabs()  
    {
        // Assign references to the GameObjects containing the PlayerController, CamController, and InventoryManager scripts
        player = PlayerController.FindObjectOfType<PlayerController>();
        mainCamera = CamController.FindObjectOfType<CamController>();
        inventory = InventoryManager.FindObjectOfType<InventoryManager>();

        // Load TileDirectory
        tileDirectory = Resources.Load<TileDirectory>("TileDirectory");

        // Load SurfaceSpawner and CaveEnemySpawner prefabs
        surfaceSpawnerPrefab = Resources.Load<GameObject>("SurfaceSpawner");
        caveEnemySpawnerPrefab = Resources.Load<GameObject>("OgreSpawner");

        // Load TileDamage, TileDropSquare, and TileDropCircle prefabs
        tileDamagePrefab = Resources.Load<GameObject>("TileDamage");
        tileDropSquare = Resources.Load<GameObject>("TileDropSquare");
        tileDropCircle = Resources.Load<GameObject>("TileDropCircle");

        // Load TorchLight, TorchAnim, CraftTableAnim, and OvenAnim prefabs
        torchLight = Resources.Load<GameObject>("TorchLight");
        torchAnim = Resources.Load<RuntimeAnimatorController>("Torch");
        craftTableAnim = Resources.Load<RuntimeAnimatorController>("Crafting Table");
        ovenAnim = Resources.Load<RuntimeAnimatorController>("Oven");
    }

    // Callback to determine the current biome based on the x coordinate and a random number.
    public BiomeClass GetCurrentBiome(int x, int randomNum) 
    {
        // Define biome regions based on the percentage of the world width
        if (x + randomNum < worldWidth * .05)
            currentBiome = biomes[4]; // Ice Biome
        else if (x + randomNum >= worldWidth * .05 && x + randomNum < worldWidth * .30)
            currentBiome = biomes[3]; // Snow Biome
        else if (x + randomNum >= worldWidth * .30 && x + randomNum <= worldWidth * .50)
            currentBiome = biomes[0]; // Grassland Biome
        else if (x + randomNum > worldWidth * .50 && x + randomNum <= worldWidth * .70)
            currentBiome = biomes[1]; // Forest Biome
        else if (x + randomNum > worldWidth * .70 && x + randomNum <= worldWidth * .95)
            currentBiome = biomes[2]; // Desert Biome
        else if (x + randomNum > worldWidth * .95)
            currentBiome = biomes[5]; // Lava Biome

        // Return the current biome
        return currentBiome;
    }

/*    
    // Generate biomes based on Perlin noise and a color gradient.
    public void GenerateBiomes()
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

    // Determine the current biome based on the x and y coordinates, using the color gradient.
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
    }
*/

    // Generates a random Perlin noise texture using world seed along with two input variables and a texture2D
    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        // Loop through the width and height of the noiseTexture
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                // Calculate Perlin noise value using the world seed and input frequency
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);

                // Set the pixel color based on the Perlin noise value and input limit
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
        // Apply changes to the noiseTexture
        noiseTexture.Apply();
    }

    // Generates a random Perlin noise texture using a world seed, two input variables (frequency and limit),
    // and a Texture2D for the GenerateTerrain method
    public void GenerateTextures()  
    {
        // Loop through all biomes defined in the Inspector
        for (int i = 0; i < biomes.Length; i++)
        {
            // Create a new caveNoiseTexture using worldWidth (World Size)
            caveNoiseTexture = new Texture2D(worldWidth, worldWidth);
            // Call GenerateNoiseTexture using the caveNoiseTexture, caveRarity, and caveDensity
            GenerateNoiseTexture(caveRarity, caveDensity, caveNoiseTexture);
        }
        // Loop through all ores defined in the Inspector
        for (int x = 0; x < ores.Length; x++)
        {
            // Create a new mapTexture for each ore using worldWidth (World Size)
            ores[x].mapTexture = new Texture2D(worldWidth, worldWidth);
            // Call GenerateNoiseTexture using the ore's mapTexture, rarity, and density
            GenerateNoiseTexture(ores[x].rarity, ores[x].density, ores[x].mapTexture);
        }
        // Loop through all enemySpawners defined in the Inspector
        for (int x = 0; x < caveEnemySpawners.Length; x++)
        {
            // Create a new mapTexture for each enemySpawner using worldWidth (World Size)
            caveEnemySpawners[x].mapTexture = new Texture2D(worldWidth, worldWidth);
            // Call GenerateNoiseTexture using the enemySpawner's mapTexture, rarity, and density
            GenerateNoiseTexture(caveEnemySpawners[x].rarity, caveEnemySpawners[x].density, caveEnemySpawners[x].mapTexture);
        }
        // Loop through all liquids defined in the Inspector (not used yet, intended for water and lava)
        for (int l = 0; l < liquids.Length; l++)
        {
            // Create a new mapTexture for each liquid using worldWidth (World Size)
            liquids[l].mapTexture = new Texture2D(worldWidth, worldWidth);
            // Call GenerateNoiseTexture using the liquid's mapTexture, rarity, and density
            GenerateNoiseTexture(liquids[l].rarity, liquids[l].density, liquids[l].mapTexture);
        }
    }

    // Generates chunk GameObjects with children for tiles, tile drops, and spawners
    // Organizes hierarchy and creates children for various elements within each chunk
    public void GenerateChunks()    
    {
        // Calculate the number of chunks based on world width and chunk size
        int numChunks = worldWidth / chunkSize;
        worldChunks = new GameObject[numChunks];

        // Iterate through the number of chunks, creating a new GameObject for each chunk
        for (int i = 0; i < numChunks; i++)
        {
            // Create and set up a new chunk GameObject
            GameObject newChunk = new GameObject();
            newChunk.name = ("Chunk " + (i + 1));
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;

            // Create GameObjects for various elements within each chunk and set their parents
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

    // Creates a world border consisting of left, right, and top barriers
    public void GenerateWorldBorder()
    {
        // Create GameObjects for hierarchy organization and set their parents
        GameObject worldBorder = new GameObject("World Barrier");
        worldBorder.transform.parent = this.transform;
        GameObject left = new GameObject("Left");
        left.transform.parent = worldBorder.transform;
        GameObject right = new GameObject("Right");
        right.transform.parent = worldBorder.transform;
        GameObject top = new GameObject("Top");
        top.transform.parent = worldBorder.transform;

        // Iterate through the X and Y coordinates within the world dimensions, including a buffer above the surface.
        for (int x = -1; x <= worldWidth; x++)
        {
            for (int y = -11; y < worldHeight + 57 + 1; y++)
            {
                // Create a new GameObject for each barrier at the border positions (left, right, or top).
                if (x == -1 || x == worldWidth || y == worldHeight + 57) // Border is placed 58 tiles above the surface
                {
                    // Set up the barrier GameObject with name, position, parent, SpriteRenderer, and BoxCollider2D components.
                    GameObject barrier = new GameObject();

                    barrier.name = "barrier";                                       // Name GameObject
                    barrier.transform.position = new Vector2(x + 0.5f, y + 0.5f);   // Move tile to x,y in loop

                    if (x == -1)
                        barrier.transform.parent = left.transform;                  // Parent each wall for hierarchy organization
                    else if (x == worldWidth)
                        barrier.transform.parent = right.transform;
                    else if (y == worldHeight + 57)
                        barrier.transform.parent = top.transform;

                    barrier.AddComponent<SpriteRenderer>();
                    barrier.GetComponent<SpriteRenderer>().sprite = tileDirectory.barrier.itemSprites[0]; // Assign sprite
                    barrier.GetComponent<SpriteRenderer>().enabled = false;         // Make barriers invisible
                    barrier.GetComponent<SpriteRenderer>().sortingOrder = 0;

                    barrier.AddComponent<BoxCollider2D>();
                    barrier.GetComponent<BoxCollider2D>().size = Vector2.one;       // Player collides with barriers
                }
            }
        }
    }

    // Generates the terrain using Perlin noise, biome settings, and various conditions.
    public void GenerateTerrain()
    {
        // Define the TileClass object that will be used to determine the tile type to place.
        TileClass tileClass;

        // Loop through each X coordinate in the world width.
        for (int x = 0; x < worldWidth; x++)
        {
            // Define the height variable that will be used to calculate the surface height.
            float height;
            // Determine the bedrock height randomly between 0 and 2 (inclusive).
            int bedrockHeight = UnityEngine.Random.Range(0, 3);

            // Loop through each Y coordinate from -10 to worldHeight + 57.
            for (int y = -10; y < worldHeight + 57; y++)
            {
                // Determine the current biome using Input location x, and a random number between 1 and 10
                // The random number allows for a gradient effect in tiles between biomes
                currentBiome = GetCurrentBiome(x, UnityEngine.Random.Range(0, 10));
                // Calculate the height based on Perlin noise, current biome terrain frequency, and hill range.
                height = Mathf.PerlinNoise((x + seed) * currentBiome.terrainFreq, seed * currentBiome.terrainFreq) * hillRange + worldHeight;
                // Determine the dirt layer height, which varies for each X coordinate.
                int dirtLayerHeight = UnityEngine.Random.Range(currentBiome.dirtLayerMin, currentBiome.dirtLayerMax);

                // Define Player's spawn position when generating terrain
                // Set the player's spawn position in the middle of the world and two tiles above the surface.
                if (x == worldWidth / 2)
                    player.spawnPosition = new Vector2(x + 0.5f, height + 2);

                // Break the loop if the current Y position is equal to or above the calculated height.
                if (y >= height)
                    break;

                // Define the tile type to place based on ore maps and height conditions.
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
                // Generate trees, cacti, tall grass, or mushrooms based on random values and biome conditions
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
                            // Call the appropriate tree generation method based on the current biome
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
                                // Call GenerateCactus() to generate a cactus one tile above the surface.
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
                                PlaceTile(tileDirectory.mushroomRed, x, y + 1);     // Places red mushroom
                            if (mushroomChoice == 1)                                                        
                                PlaceTile(tileDirectory.mushroomBrown, x, y + 1);   // Places brown mushroom
                            if (mushroomChoice == 2)
                                PlaceTile(tileDirectory.mushroomTan, x, y + 1);     // Places tan mushroom
                        }
                    }
                }
            }
        }
    }

    // Generates the terrain walls using Perlin noise, biome settings, and various conditions
    public void GenerateTerrainWalls()
    {
        // Define the TileClass object that will be used to determine the tile type to place
        TileClass tileClass;

        // Loop through each X coordinate in the world width.
        for (int x = 0; x < worldWidth; x++)
        {
            // Loop through each Y coordinate from 0 to worldHeight + 57.
            for (int y = 0; y <= worldHeight + 57; y++)
            {
                // Determine the current biome using Input location x, and a random number between 1 and 10
                // The random number allows for a gradient effect in tiles between biomes
                currentBiome = GetCurrentBiome(x, UnityEngine.Random.Range(0, 10));
                // Calculate the height based on Perlin noise, current biome terrain frequency, and hill range
                float height;
                height = Mathf.PerlinNoise((x + seed) * currentBiome.terrainFreq, seed * currentBiome.terrainFreq) * hillRange + worldHeight;
                // Determine the dirt layer height, which varies for each X coordinate
                int dirtLayerHeight = UnityEngine.Random.Range(currentBiome.dirtLayerMin, currentBiome.dirtLayerMax);

                // If the current Y position is below the calculated height, place the background tiles.
                if (y < height)
                {
                    // Define the tile type to place based on ore maps and height conditions.
                    if (ores[0].mapTexture.GetPixel(x, y).r > 0.5f && height - y < 80)
                    {
                        tileClass = tileDirectory.gravelWall;   // Place gravel up to 80 below surface
                    }
                    else if (ores[1].mapTexture.GetPixel(x, y).r > 0.5f && height - y < 40 && x > worldWidth * .30)
                    {
                        tileClass = tileDirectory.sandWall;     // Place sand up to 40 below surface and not in snow biome
                    }
                    else if (y < height - dirtLayerHeight)
                    {
                        tileClass = currentBiome.tileDirectory.stoneWall;   // Place stone type of the proper biome
                    }
                    else
                    {
                        tileClass = currentBiome.tileDirectory.dirtWall;    // Place dirt type of the proper biome
                    }

                    // Place the selected tile at the current (x, y) position
                    PlaceTile(tileClass, x, y);
                }
            }
        }
    }

    // Generates a standard tree at the given X and Y coordinates
    public void GenerateTree(int x, int y)
    {
        //Place Tree Base
        PlaceTile(tileDirectory.treeBase, x, y);

        // Generate a random tree height based on the current biome's min and max tree height
        int treeHeight = UnityEngine.Random.Range(currentBiome.treeMinHeight, currentBiome.treeMaxHeight);

        // Place tree logs based on the random tree height
        for (int i = 1; i < treeHeight - 1; i++)
        {
            PlaceTile(tileDirectory.treeLog, x, y + i);
        }

        // Place the tree log top at the top of the tree
        PlaceTile(tileDirectory.treeLogTop, x, y + treeHeight - 1);

        // Place tree leaves in a rectangular pattern around the top of the tree
        for (int a = -1; a <=1; a++)
        {
            for (int b = 0; b <=2; b++)
            {
                PlaceTile(tileDirectory.treeLeaf, x + a, y + treeHeight + b);
            }
        }
    }

    // Generates an acacia tree at the given X and Y coordinates
    public void GenerateAcaciaTree(int x, int y)
    {
        // Place the acacia tree base
        PlaceTile(tileDirectory.acaciaTreeBase, x, y);

        // Generate a random tree height based on the current biome's min and max tree height
        int treeHeight = UnityEngine.Random.Range(currentBiome.treeMinHeight, currentBiome.treeMaxHeight);

        // Place acacia tree logs based on the random tree height
        for (int i = 1; i < treeHeight; i++)
        {
            PlaceTile(tileDirectory.acaciaTreeLog, x, y + i);
        }

        // Create the unique acacia tree log structure at the top
        PlaceTile(tileDirectory.acaciaTreeLog, x + 1, y + treeHeight - 1);
        PlaceTile(tileDirectory.acaciaTreeLogTop, x + 2, y + treeHeight);
        PlaceTile(tileDirectory.acaciaTreeLogTop, x - 1, y + treeHeight);

        // Place acacia tree leaves in a rectangular pattern around the top of the tree
        for (int a = -3; a <= 5; a++)
        {
            PlaceTile(tileDirectory.acaciaTreeLeaf, x + a, y + treeHeight + 1);
        }
        for (int b = -2; b <= 3; b++)
        {
            PlaceTile(tileDirectory.acaciaTreeLeaf, x + b, y + treeHeight + 2);
        }
    }

    // Generates a spruce tree at the given X and Y coordinates
    public void GenerateSpruceTree(int x, int y)
    {
        // Place the spruce tree base
        PlaceTile(tileDirectory.spruceTreeBase, x, y);

        // Generate a random tree height based on the current biome's min and max tree height
        int treeHeight = UnityEngine.Random.Range(currentBiome.treeMinHeight, currentBiome.treeMaxHeight);

        // Place spruce tree logs based on the random tree height
        for (int i = 1; i < treeHeight - 1; i++)
        {
            PlaceTile(tileDirectory.spruceTreeLog, x, y + i);
        }

        // Place the spruce tree log top at the top of the tree
        PlaceTile(tileDirectory.spruceTreeLogTop, x, y + treeHeight - 1);

        // Place spruce tree leaves in a triangular pattern around the tree
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

    // Generates a cactus at the given X and Y coordinates
    public void GenerateCactus(int x, int y)
    {
        // Generate a random cactus height between 1 and 4 (inclusive)
        int cactusHeight = UnityEngine.Random.Range(1, 5);

        // Place cactus tiles in a vertical column based on the random cactus height
        for (int i = 0; i < cactusHeight; i++)
        {
            PlaceTile(tileDirectory.cactus, x, y + i);
        }
    }

    // Places a tile in the world based on the given TileClass object and X and Y coordinates, configures it
    // according to its location type (Wall, Surface, or Ground), and sets its properties and components as needed
    public void PlaceTile (TileClass tile, int x, int y)
    {
        // Create a new GameObject called newTile
        GameObject newTile = new GameObject();

        // Set the name of newTile
        newTile.name = tile.itemName;
        // Place at (X, Y) location. Add 0.5f to each to be in line with editor grid
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        // Calculate the chunk coordinate using the input x variable
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        // Check if the tile is a "Surface Spawner" or "Cave Enemy Spawner", and set up the respective spawner
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

        // Add a TileHealth component and set its health value
        newTile.AddComponent<TileHealth>();
        newTile.GetComponent<TileHealth>().tileHealth = tile.tileHealth;

        // Add a SpriteRenderer component, set the material and assign a random sprite
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().material = spriteLit;
        int spriteIndex = UnityEngine.Random.Range(0, tile.itemSprites.Length);         // Random sprite selected
        newTile.GetComponent<SpriteRenderer>().sprite = tile.itemSprites[spriteIndex];  // Random sprite applied

        // Determine the tile's location type (Wall, Surface, or Ground) and set its properties accordingly
        if (tile.tileLocation == TileClass.TileLocation.Wall)
        {
            // Set the parent, sorting layer, and layer
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(0).transform;
            newTile.GetComponent<SpriteRenderer>().sortingLayerName = "Terrain_WallTiles";
            newTile.layer = 11;

            // Add the tile's position, GameObject, and TileClass to their respective lists
            wallTilePositions.Add(new Vector2(Mathf.RoundToInt(newTile.transform.position.x - 0.5f), Mathf.RoundToInt(newTile.transform.position.y - 0.5f)));
            wallTileObjects.Add(newTile);
            wallTileClasses.Add(tile);
        }
        else if (tile.tileLocation == TileClass.TileLocation.Surface)
        {
            // Set the base layer
            newTile.layer = 13;

            // Configure the GameObject based on its name for special tiles
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

            // Set the parent and sorting layer
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(2).transform; //parent tile to current chunk
            newTile.GetComponent<SpriteRenderer>().sortingLayerName = "Terrain_SurfaceTiles";

            // Add the tile's position, GameObject, and TileClass to their respective lists
            surfaceTilePositions.Add(new Vector2(Mathf.RoundToInt(newTile.transform.position.x - 0.5f), Mathf.RoundToInt(newTile.transform.position.y - 0.5f)));
            surfaceTileObjects.Add(newTile);
            surfaceTileClasses.Add(tile);

        }
        else if (tile.tileLocation == TileClass.TileLocation.Ground)
        {
            // Configure the GameObject based on its name for special tiles
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

            // Set the parent, sorting layer, and layer
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(1).transform;
            newTile.GetComponent<SpriteRenderer>().sortingLayerName = "Terrain_GroundTiles";
            newTile.layer = 12;

            // Add the tile's position, GameObject, and TileClass to their respective lists
            groundTilePositions.Add(new Vector2(Mathf.RoundToInt(newTile.transform.position.x - 0.5f), Mathf.RoundToInt(newTile.transform.position.y - 0.5f)));
            groundTileObjects.Add(newTile);
            groundTileClasses.Add(tile);
        }

        // If the tile had gravity, adjust the BoxCollider2D size and add a Rigidbody2D component with constraints
        /*if (tile.gravity)
        {
            newTile.GetComponent<BoxCollider2D>().size = new Vector2(0.95f, 0.95f);
            newTile.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }*/
    }

    // Places a ground tile in the world based on the given TileClass object and X and Y coordinates when the player
    // initiates the action, ensuring valid placement conditions are met and handling tile drops if necessary
    public void PlayerPlaceGroundTile(TileClass tile, int x, int y)
    {
        // Calculate the chunk coordinate using the input x variable
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        // Check if the ground tile position is not already occupied
        if (!groundTilePositions.Contains(new Vector2Int(x, y)))
        {
            // Check if there is a surface tile at the target position
            if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
            {
                // Handle special cases where the tile cannot be placed (e.g., near a Crafting Table or Wood Door)
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Crafting Table")
                { return; }
                else if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Wood Door")
                { return; }
                else if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].isTree)
                { return; }

                // Handle replaceable surface tiles and manage tile drops if necessary
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].replaceable)
                {
                    // Instantiate the dropped tile GameObject and set its properties based on the surface tile being replaced
                    // Remove the replaced tile from the surfaceTileObjects, surfaceTileClasses, and surfaceTilePositions lists
                    Destroy(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2(x, y))]);
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
                                    newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                                    tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem(), 1, 0, 999);
                                }
                                else
                                {
                                    newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                    newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                    tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetItem(), 1, 0, 999);
                                }
                                newTileDrop.GetComponent<TileDropController>().invSlotClass = tileDrop;
                                
                                surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));   // Remove from objects list
                                surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));   // Remove from class list
                                surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); // Remove from position list (has to be done last)
                            }
                            else
                            {
                                surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));   // Remove from objects list
                                surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));   // Remove from class list
                                surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); // Remove from position list (has to be done last)
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
                                newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                                tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem(), 1, 0, 999);
                            }
                            else
                            {
                                newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                                tileDrop = new InvSlotClass(surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetItem(), 1, 0, 999);
                            }
                            newTileDrop.GetComponent<TileDropController>().invSlotClass = tileDrop;

                            surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));   // Remove from objects list
                            surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));   // Remove from class list
                            surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y))); // Remove from position list (has to be done last)
                        }
                    }
                }
            }

            // Check for additional special cases where the tile cannot be placed (e.g., near a Crafting Table or Wood Door)
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

            // Check if there is a valid neighboring tile (e.g., another ground tile or a wall) and place the tile
            if ((groundTilePositions.Contains(new Vector2Int(x, y + 1))
            || groundTilePositions.Contains(new Vector2Int(x, y - 1))
            || groundTilePositions.Contains(new Vector2Int(x + 1, y))
            || groundTilePositions.Contains(new Vector2Int(x - 1, y)))
            && tile.name != "Wood Platform")
            {
                // Place tile if location is empty and beside another tile
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
            else if (surfaceTilePositions.Contains(new Vector2Int(x, y - 2))
                && tile.name != "Wood Platform")
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y - 2))].itemName == "Wood Door")
                {
                    // Or if there is a wood door below which is 2 tiles high
                    PlaceTile(tile, x, y);
                    FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
                }
            }
            else if (wallTilePositions.Contains(new Vector2Int(x, y)))
            {
                // Or if location is empty and a wall is behind
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
        }
    }

    // Places a surface tile in the world based on the given TileClass object and X and Y coordinates when the player
    // initiates the action, ensuring valid placement conditions are met and handling special cases
    public void PlayerPlaceSurfaceTile(TileClass tile, int x, int y)
    {
        // Calculate the chunk coordinate using the input x variable
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        // Check for special cases where the tile cannot be placed (e.g., near a Crafting Table or Wood Door)
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

        // Check for additional special cases where Storage Barrels and Storage Chests cannot be placed
        if (tile.name == "Storage Barrel" || tile.name == "Storage Chest")
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y))].itemName == "Storage Barrel" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x + 1, y))].itemName == "Storage Barrel" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x - 1, y))].itemName == "Storage Chest" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x + 1, y))].itemName == "Storage Chest")
            { return; }
        }

        // If Dead Grass or Tall Grass is present at the target position, destroy the object
        if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
        {
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Dead Grass" ||
                surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName == "Tall Grass")
            {
                Destroy(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2(x, y))]);
            }
        }

        // Place the tile and play a sound if the location is empty, has a ground tile under it, and doesn't conflict with other tiles
        if (!groundTilePositions.Contains(new Vector2Int(x, y)) &&
            !surfaceTilePositions.Contains(new Vector2Int(x, y)) &&
            groundTilePositions.Contains(new Vector2Int(x, y - 1)))
        {
            /*if (tile.name == "Tree Sapling")
            { return; }*/

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

    // Places a wall tile in the world based on the given TileClass object and X and Y coordinates when the player
    // initiates the action, ensuring valid placement conditions are met
    public void PlayerPlaceWallTile(TileClass tile, int x, int y)
    {
        // Check if there is no wall tile at the target position
        if (!wallTilePositions.Contains(new Vector2Int(x, y)))
        {
            // Check if there is a neighboring wall tile and place the tile if conditions are met
            if ((wallTilePositions.Contains(new Vector2Int(x, y + 1))
                || wallTilePositions.Contains(new Vector2Int(x, y - 1))
                || wallTilePositions.Contains(new Vector2Int(x + 1, y))
                || wallTilePositions.Contains(new Vector2Int(x - 1, y))))
            {
                // Place tile and play sound
                PlaceTile(tile, x, y);
                FindObjectOfType<AudioManager>().Play("Player_PlaceTile");
            }
        }
    }

    // Remove a ground tile at a specified (x, y) position
    public void PlayerRemoveGroundTile (int x, int y)
    {
        // Calculate the chunk coordinate using the input x variable
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        // Check if there is a ground tile at the input location and if it is not unbreakable
        if (groundTilePositions.Contains(new Vector2Int(x, y))
            && !groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].isUnbreakable)
        {
            // Check if there is a surface tile above the ground tile at the input location
            if (surfaceTilePositions.Contains(new Vector2Int(x, y + 1)))
            {
                // Check if the surface tile above the ground tile is replaceable and has a dropTile property
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].replaceable)
                {
                    if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].dropTile)
                    {
                        PlayerRemoveSurfaceTile(x, y + 1);
                    }
                }
                // ELSE IF the surface tile above the ground tile is a tree, return and don't remove the ground tile
                else if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].isTree)// IF that Surface Tile is a tree
                {
                    return;
                }
            }

            // Destroy the ground tile's GameObject at the input location
            Destroy(groundTileObjects[groundTilePositions.IndexOf(new Vector2(x, y))]);

            // If the player is holding something, create a tile drop based on the ground tile's properties
            // Player can break most tiles if they aren't holding an item, but the tile won't create a tiledrop because of this check
            if (inventory.inventoryItems[inventory.selectedSlot].GetItem() != null)                                                                                   
            {
                if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].dropTile)
                {
                    GameObject newTileDrop;
                    ItemClass tile;

                    // Check if the tile should always drop or if it's based on chance
                    if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].alwaysDropTile == false)
                    {
                        int dropChance = UnityEngine.Random.Range(0, groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].dropTileChance);
                        if (dropChance == 0)
                        {
                            // Create the tile drop with properties based on the ground tile's dropped item, if available
                            newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                            newTileDrop.GetComponent<CircleCollider2D>().enabled = true;
                            newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;

                            // Set the tile drop's sprite and assign the corresponding ItemClass
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
                            // Create a new InvSlotClass using the ItemClass and assign it to the tile drop
                            InvSlotClass tiledrop2 = new InvSlotClass(tile, 1, 0, 999);
                            newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop2;
                            groundTileObjects.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));
                            groundTileClasses.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));
                            groundTilePositions.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));   // (Has to be done last)
                            return;
                        }
                        else
                        {
                            groundTileObjects.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));
                            groundTileClasses.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));
                            groundTilePositions.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));   // (Has to be done last)
                            return;
                        }
                    }

                    newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                    // Enable it's CircleCollider so Player can pick it up
                    newTileDrop.GetComponent<CircleCollider2D>().enabled = true;
                    newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                    // Check if the Ground Tile at Input location's dropped item contains an ItemClass
                    if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)
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
                    InvSlotClass tiledrop = new InvSlotClass(tile, 1, 0, 999);
                    newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop;
                }
            }

            // Remove the ground tile's data from the corresponding lists
            groundTileObjects.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));
            groundTileClasses.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));
            groundTilePositions.RemoveAt(groundTilePositions.IndexOf(new Vector2(x, y)));

            // Check if the player is holding an item not made of wood, subtract the tool's durability
            if (inventory.inventoryItems[inventory.selectedSlot].GetItem() != null)
            {
                if (!inventory.inventoryItems[inventory.selectedSlot].GetItem().itemName.Contains("Wood"))
                {
                    inventory.inventoryItems[inventory.selectedSlot].SubtractDurability(1);
                }
            }
        }
    }

    // Remove a surface tile at the specified (x, y) position
    public void PlayerRemoveSurfaceTile(int x, int y)
    {
        // Calculate the chunk coordinate using the input x variable
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        // Check if there is a surface tile at the target position and if it is not unbreakable
        if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
        {
            // Destroy the tile object
            Destroy(surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2(x, y))]);

            // Check if the tile should drop an item when destroyed
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].dropTile)
            {
                GameObject newTileDrop;
                ItemClass tile;

                // Check if the tile should always drop an item when destroyed or if it should drop based on random chance
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].alwaysDropTile == false)
                {
                    // Calculate the drop chance for the tile
                    int dropChance = UnityEngine.Random.Range(1, surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].dropTileChance);

                    // Check if the tile should drop an item based on the calculated drop chance
                    if (dropChance == 1)
                    {
                        // Instantiate a new item drop object and set its properties
                        newTileDrop = Instantiate(tileDropCircle, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);

                        // Check if the tile has a specific item to drop, and set the sprite accordingly
                        if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem != null)
                        {
                            newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                            newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].droppedItem.itemSprites[0];
                            tile = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].droppedItem.GetItem();
                        }
                        else
                        {
                            // If no specific item is set to be dropped, use the tile's own sprite and item data
                            newTileDrop.GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                            newTileDrop.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                            tile = surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetItem();
                        }

                        // Set the parent transform and enable the collider for the new item drop
                        newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                        newTileDrop.GetComponent<CircleCollider2D>().enabled = true;

                        // Create an inventory slot for the dropped item and set it as the item drop's inventory slot
                        InvSlotClass tiledrop2 = new InvSlotClass(tile, 1, 0, 999);
                        newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop2;
                        
                        // Remove the tile data from the lists
                        surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));     // (Has to be done last)
                        return;
                    }
                    else
                    {
                        // Remove the tile data from the lists
                        surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
                        surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));     // (Has to be done last)
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

            // Remove the tile data from the lists
            surfaceTileObjects.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
            surfaceTileClasses.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));
            surfaceTilePositions.RemoveAt(surfaceTilePositions.IndexOf(new Vector2(x, y)));     // (Has to be done last)

            // Check if there is a tree tile above the broken tile, and if so call the method to run again
            if (surfaceTilePositions.Contains(new Vector2Int(x, y + 1)))
            {
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].isTree)
                    PlayerRemoveSurfaceTile(x, y + 1);
                else
                {
                    // Check if the player is holding an item not made of wood, subtract the tool's durability
                    if (!inventory.inventoryItems[inventory.selectedSlot].GetItem().itemName.Contains("Wood"))
                    {
                        inventory.inventoryItems[inventory.selectedSlot].SubtractDurability(1);
                    }
                    return;
                }
            }
        }
    }

    // Method for removing a wall tile at the specified (x, y)
    public void PlayerRemoveWallTile(int x, int y)
    {
        // Calculate the chunk coordinate using the input x variable
        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        // Check if there is a wall tile at the target position
        if (wallTilePositions.Contains(new Vector2Int(x, y)))
        {
            // Check if the wall tile is not connected to any neighboring wall tiles
            if ((!wallTilePositions.Contains(new Vector2Int(x, y + 1))
                || !wallTilePositions.Contains(new Vector2Int(x, y - 1))
                || !wallTilePositions.Contains(new Vector2Int(x + 1, y))
                || !wallTilePositions.Contains(new Vector2Int(x - 1, y))))
            {
                // Destroy the wall tile object
                Destroy(wallTileObjects[wallTilePositions.IndexOf(new Vector2(x, y))]);

                // Instantiate a new item drop object for the wall tile and set its properties
                GameObject newTileDrop;
                ItemClass tile;

                newTileDrop = Instantiate(tileDropSquare, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                newTileDrop.transform.parent = worldChunks[(int)chunkCoord].transform.GetChild(3).transform;
                newTileDrop.GetComponent<CircleCollider2D>().enabled = true;
                newTileDrop.GetComponent<SpriteRenderer>().sprite = wallTileClasses[wallTilePositions.IndexOf(new Vector2(x, y))].itemSprites[0];
                tile = wallTileClasses[wallTilePositions.IndexOf(new Vector2Int(x, y))].GetItem();

                InvSlotClass tiledrop = new InvSlotClass(tile, 1, 0, 999);
                newTileDrop.GetComponent<TileDropController>().invSlotClass = tiledrop;

                // Remove the tile data from the lists
                wallTileObjects.RemoveAt(wallTilePositions.IndexOf(new Vector2(x, y)));
                wallTileClasses.RemoveAt(wallTilePositions.IndexOf(new Vector2(x, y)));
                wallTilePositions.RemoveAt(wallTilePositions.IndexOf(new Vector2(x, y)));   // (Has to be done last)

                // Check if the player is holding an item not made of wood, subtract the tool's durability
                if (!inventory.inventoryItems[inventory.selectedSlot].GetItem().itemName.Contains("Wood"))
                {
                    inventory.inventoryItems[inventory.selectedSlot].SubtractDurability(1);
                }
            }
        }
    }

    // Damages and removes ground tiles based on the tool used, considering factors such as tool strength and tile type
    public void PlayerRemoveGroundTileHealth(int x, int y, int toolDamage, string toolName)
    {
        // Check if there is a ground tile at the given position and if it is not unbreakable
        if (groundTilePositions.Contains(new Vector2Int(x, y)) &&
            !groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].isUnbreakable)
        {
            // Check if there is a surface tile (like a tree) above the ground tile
            if (surfaceTilePositions.Contains(new Vector2Int(x, y + 1)))
            {
                // If there is a tree above the ground tile, do not remove the tile
                if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y + 1))].isTree)
                {
                    Debug.Log("Tree Blocking");
                    return;
                }
            }
            // Prevent certain tools from mining Diamond or Gold tiles
            if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Diamond") && 
                (toolName.Contains("Wood") || toolName.Contains("Stone") || toolName.Contains("Iron")))
            {
                Debug.Log("Can't Mine, Need Stronger Pickaxe");
                return;
            }

            // Instantiate a tileDamage object if there isn't one already
            if (groundTileClasses[groundTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Gold") &&
                (toolName.Contains("Wood") || toolName.Contains("Stone")))
            {
                Debug.Log("Can't Mine, Need Stronger Pickaxe");
                return;
            }

            // Check if a TileDamage object is already present and update it
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

            // Reduce the tile's health based on the tool's damage
            groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth -= toolDamage;
            //Debug.Log("Tile Health Now: " + groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth);

            // Play a sound effect when the player hits a tile
            FindObjectOfType<AudioManager>().Play("Player_HitTile");

            // Check if the tile's health is depleted, if so remove it and play a sound
            if (groundTileObjects[groundTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth <= 0)
            {
                PlayerRemoveGroundTile(x, y);
                FindObjectOfType<AudioManager>().Play("Player_BreakTile");
            }
        }
    }

    // Damages and removes surface tiles based on the tool used
    public void PlayerRemoveSurfaceTileHealth(int x, int y, int toolDamage)
    {
        // Check if the target position has a surface tile
        if (surfaceTilePositions.Contains(new Vector2Int(x, y)))
        {
            // Check if the surface tile is a Storage Barrel or Storage Chest that is not empty
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Storage Barrel"))
            {
                if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<StorageBarrelController>().storageEmpty == false)
                { return; }
            }

            // Instantiate a tileDamage object if there isn't one already
            if (surfaceTileClasses[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].itemName.Contains("Storage Chest"))
            {
                if (surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<StorageChestController>().storageEmpty == false)
                { return; }
            }

            // Check if a TileDamage object is already present and update it
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

            // Reduce the tile's health based on the tool's damage
            surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth -= toolDamage;
            //Debug.Log("Tile Health Now: " + surfaceTileObjects[surfaceTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth);

            // Play a sound effect when the player hits a tile
            FindObjectOfType<AudioManager>().Play("Player_HitTile");

            // Check if the tile's health is depleted or if it's a specific tile that should be removed, if so remove it and play a sound
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

    // Damages and removes wall tiles based on the tool used
    public void PlayerRemoveWallTileHealth(int x, int y, int toolDamage)
    {
        // Check if the target position has a wall tile and is accessible (not completely surrounded by other wall tiles)
        if (wallTilePositions.Contains(new Vector2Int(x, y)) &&
           (!wallTilePositions.Contains(new Vector2Int(x, y + 1)) ||
            !wallTilePositions.Contains(new Vector2Int(x, y - 1)) ||
            !wallTilePositions.Contains(new Vector2Int(x + 1, y)) ||
            !wallTilePositions.Contains(new Vector2Int(x - 1, y))) &&
            !groundTilePositions.Contains(new Vector2Int(x, y)))
        {
            // Instantiate a tileDamage object if there isn't one already
            if (wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount < 1)
            {
                GameObject tileDamage;
                tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                tileDamage.transform.SetParent(wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform);
            }
            else if (wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform.childCount == 1)
            {
                // Check if a TileDamage object is already present and update it
                if (wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform.GetChild(0).gameObject.name != "TileDamage(Clone)")
                {
                    GameObject tileDamage;
                    tileDamage = Instantiate(tileDamagePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                    tileDamage.transform.SetParent(wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].transform);
                }
            }

            // Reduce the tile's health based on the tool's damage
            wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth -= toolDamage;
            //Debug.Log("Tile Health Now: " + wallTileObjects[wallTilePositions.IndexOf(new Vector2Int(x, y))].GetComponent<TileHealth>().tileHealth);

            // Play a sound effect when the player hits a tile
            FindObjectOfType<AudioManager>().Play("Player_HitTile");

            // Check if the tile's health is depleted or if it's a specific tile that should be removed, if so remove it and play a sound
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