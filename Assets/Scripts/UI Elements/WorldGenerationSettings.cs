using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldGenerationSettings
{
    public static float worldWidthSetting;
    public static float hillHeightSetting;
    public static float caveRaritySetting;
    public static float caveDensitySetting;

    public static bool generateCavesSetting;
    public static bool spawnEnemiesSetting;

    ///////////////////

    public static void SetWorldWidthSetting(float value)
    { // Number needs to be a multiple of 100. Allows player to spawn in middle of the world, and for no errors with generating chunks and placing tiles
        worldWidthSetting = value * 100;
    }
    public static void SetHillHeightSetting(float value)
    {
        hillHeightSetting = value;
    }
    public static void SetCaveRaritySetting(float value)
    { // Higher whole value used by slider to be more user friendly and allow slider to click to whole values
        caveRaritySetting = value / 100f;
    }
    public static void SetCaveDensitySetting(float value)
    { // Higher whole value used by slider to be more user friendly and allow slider to click to whole values
        caveDensitySetting = value / 100f;
    }
    public static void SetGenerateCavesSetting(bool toggle)
    {
        generateCavesSetting = toggle;
    }
    public static void SetSpawnEnemiesSetting(bool toggle)
    {
        spawnEnemiesSetting = toggle;
    }
}
