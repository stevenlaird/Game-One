using UnityEngine;

public class PlayQuitButtons : MonoBehaviour
{
    // This method is called when the "Play Game" button is clicked
    public void PlayGame()
    {
        Loader.Load(Loader.Scene.Game); // Load the Game scene using the Loader script
    }

    // This method is called when the "Quit Game" button is clicked
    public void QuitGame()
    {
        Debug.Log("Quit Game");         // Log a message to the console to indicate that the game is quitting
        Application.Quit();             // Quit the application
    }
}