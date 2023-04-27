using TMPro;
using UnityEngine;

public class MainMenuGenerationSettings : MonoBehaviour
{
    // Public fields to store references to UI Text components
    public TextMeshProUGUI worldWidth;
    public TextMeshProUGUI hillHeight;
    public TextMeshProUGUI caveRarity;
    public TextMeshProUGUI caveDensity;

    ///////////////////

    void Start()
    {
        // Set default values for world generation settings
        WorldGenerationSettings.SetWorldWidthSetting(10f);
        WorldGenerationSettings.SetHillHeightSetting(15f);
        WorldGenerationSettings.SetCaveRaritySetting(8f);
        WorldGenerationSettings.SetCaveDensitySetting(40f);
        WorldGenerationSettings.SetGenerateCavesSetting(true);
        WorldGenerationSettings.SetSpawnEnemiesSetting(true);
    }

    void Update()
    {
        // Update UI Text components with current world generation settings
        worldWidth.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.worldWidthSetting.ToString();
        hillHeight.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.hillHeightSetting.ToString();
        caveRarity.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.caveRaritySetting.ToString();
        caveDensity.GetComponent<TextMeshProUGUI>().text = WorldGenerationSettings.caveDensitySetting.ToString();

        // IF World Generation Settings are open
        if (this.gameObject.transform.GetChild(1).gameObject.activeSelf)        
        {
            // Disable/Close the Audio Settings menu
            this.gameObject.transform.GetChild(2).gameObject.SetActive(false);  
        }
    }

    // Public methods to update world generation settings
    public void SetWorldWidthSetting(float value)
    { WorldGenerationSettings.SetWorldWidthSetting(value); }
    public static void SetHillHeightSetting(float value)
    { WorldGenerationSettings.SetHillHeightSetting(value); }
    public static void SetCaveRaritySetting(float value)
    { WorldGenerationSettings.SetCaveRaritySetting(value); }
    public static void SetCaveDensitySetting(float value)
    { WorldGenerationSettings.SetCaveDensitySetting(value); }
    public static void SetGenerateCavesSetting(bool toggle)
    { WorldGenerationSettings.SetGenerateCavesSetting(toggle); }
    public static void SetSpawnEnemiesSetting(bool toggle)
    { WorldGenerationSettings.SetSpawnEnemiesSetting(toggle); }
}