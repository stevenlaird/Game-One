using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuGenerationSettings : MonoBehaviour
{
    public TextMeshProUGUI worldWidth;
    public TextMeshProUGUI hillHeight;
    public TextMeshProUGUI caveRarity;
    public TextMeshProUGUI caveDensity;

    ///////////////////

    void Start()
    {
        // Apply Default Values to World Generation Settings and subsequently sliders and toggles
        WorldGenerationSettings.SetWorldWidthSetting(10f);
        WorldGenerationSettings.SetHillHeightSetting(15f);
        WorldGenerationSettings.SetCaveRaritySetting(8f);
        WorldGenerationSettings.SetCaveDensitySetting(40f);
        WorldGenerationSettings.SetGenerateCavesSetting(true);
        WorldGenerationSettings.SetSpawnEnemiesSetting(true);
    }

    void Update()
    {
        worldWidth.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.worldWidthSetting.ToString();
        hillHeight.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.hillHeightSetting.ToString();
        caveRarity.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.caveRaritySetting.ToString();
        caveDensity.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.caveDensitySetting.ToString();

        // If World Generation Settings are open
        if (this.gameObject.transform.GetChild(1).gameObject.activeSelf)        
        {
            // Disable/Close the Audio Settings menu
            this.gameObject.transform.GetChild(2).gameObject.SetActive(false);  
        }
    }

    // Apply Main Menu generation options to WorldGenerationSettings
    public void SetWorldWidthSetting(float value)
    {
        WorldGenerationSettings.SetWorldWidthSetting(value);
    }
    public static void SetHillHeightSetting(float value)
    {
        WorldGenerationSettings.SetHillHeightSetting(value);
    }
    public static void SetCaveRaritySetting(float value)
    {
        WorldGenerationSettings.SetCaveRaritySetting(value);
    }
    public static void SetCaveDensitySetting(float value)
    {
        WorldGenerationSettings.SetCaveDensitySetting(value);
    }
    public static void SetGenerateCavesSetting(bool toggle)
    {
        WorldGenerationSettings.SetGenerateCavesSetting(toggle);
    }
    public static void SetSpawnEnemiesSetting(bool toggle)
    {
        WorldGenerationSettings.SetSpawnEnemiesSetting(toggle);
    }
}