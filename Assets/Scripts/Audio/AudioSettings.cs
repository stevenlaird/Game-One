using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
    public GameObject settings;             // Reference to the settings UI panel in the game
    public static bool gamePaused;          // Static boolean variable to keep track of the game's paused state
    public AudioMixer audioMixer;           // Reference to the AudioMixer component in the game

    ///////////////////

    void Start()
    {
        ResumeGame();                       // Ensures that the game is not paused when the script is first executed
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
            { ResumeGame(); }               // If the game is already paused, this function will resume the game
            else
            { PauseGame(); }                // If the game is not paused, this function will pause the game
        }
    }

    public void ResumeGame()
    {
        Debug.Log("Game Resumed");          // Outputs a message to the console when the game is resumed
        Time.timeScale = 1.0f;              // Restores normal time scale to the game
        settings.SetActive(false);          // Disables the settings UI panel in the game
        gamePaused = false;                 // Updates the game's paused state to "false"
    }

    public void PauseGame()
    {
        Debug.Log("Game Paused");           // Outputs a message to the console when the game is paused
        Time.timeScale = 0;                 // Pauses the game by setting the time scale to zero
        settings.SetActive(true);           // Enables the settings UI panel in the game
        gamePaused = true;                  // Updates the game's paused state to "true"
    }

    public void SetMusicVolume(float musicVolume)
    { audioMixer.SetFloat("MusicVolume", musicVolume); }            // Sets the volume level for background music in the game

    public void SetAmbientVolume(float ambientVolume)
    { audioMixer.SetFloat("AmbientSoundsVolume", ambientVolume); }  // Sets the volume level for ambient sounds in the game

    public void SetSoundVolume(float soundVolume)
    { audioMixer.SetFloat("GameSoundsVolume", soundVolume); }       // Sets the volume level for sound effects in the game

    public void SetUIVolume(float UIVolume)
    { audioMixer.SetFloat("UISoundsVolume", UIVolume); }            // Sets the volume level for UI sounds in the game
}