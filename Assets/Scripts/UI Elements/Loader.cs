using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene           // The available scenes in the game
    {
        MainMenu,
        Game,
        Loading,
    }   
    private static Scene targetScene;   // The target scene to load

    ///////////////////

    // This method is used to load the target scene and set the scene to Loading scene
    public static void Load(Scene targetSceneName)
    {
        Loader.targetScene = targetSceneName;               // Set the target scene to the scene passed as a parameter
        SceneManager.LoadScene(Scene.Loading.ToString());   // Load the Loading scene
    }

    // This method is used as a callback method for when the loading scene is finished loading
    public static void LoaderCallBack()
    {
        SceneManager.LoadScene(targetScene.ToString());     // Load the target scene
    }
}