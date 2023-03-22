using UnityEngine;

public static class WorldGenerationSettings
{
    // The following variables hold the settings for world generation
    // They are public, so they can be accessed from other scripts
    public static float worldWidthSetting;
    public static float hillHeightSetting;
    public static float caveRaritySetting;
    public static float caveDensitySetting;
    public static bool generateCavesSetting;
    public static bool spawnEnemiesSetting;

    ///////////////////

    // The following methods are used to set the values of the settings
    // Width needs to be a multiple of 100. Allows Player to spawn in middle of the world, and no errors in terrain generation
    // caveDensity and caveRarity contain two decimal digits, 
    // Values multiplied or divided are done so to enable 'Whole Numbers'
    // Enabling this allows the slider to 'click' to each to value when scrolled
    // Min Value and Max Value determine how many 'click' intervals the slider will have    
    public static void SetWorldWidthSetting(float value)
    {
        worldWidthSetting = value * 100;                      // Slider: Min Value = 2, Max Value = 18, Value = 10
    }
    public static void SetHillHeightSetting(float value)
    {
        hillHeightSetting = value;                            // Slider: Min Value = 0, Max Value = 30, Value = 15
    }
    public static void SetCaveRaritySetting(float value)
    {
        caveRaritySetting = value / 100f;                     // Slider: Min Value = 1, Max Value = 15, Value = 8
    }
    public static void SetCaveDensitySetting(float value)
    {
        caveDensitySetting = value / 100f;                    // Slider: Min Value = 20, Max Value = 60, Value = 40
    }
    public static void SetGenerateCavesSetting(bool toggle)
    {
        generateCavesSetting = toggle;                        // Button: Is On = TRUE
    }
    public static void SetSpawnEnemiesSetting(bool toggle)
    {
        spawnEnemiesSetting = toggle;                         // Button: Is On = TRUE
    }
}