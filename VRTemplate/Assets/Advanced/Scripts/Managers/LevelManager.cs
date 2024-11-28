using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Static reference to the singleton instance of LevelManager
    private static LevelManager _instance;

    /// <summary>
    /// Singleton instance of the LevelManager.
    /// Ensures there is only one instance of LevelManager in the scene.
    /// </summary>
    public static LevelManager Instance
    {
        get
        {
            // Check if the instance already exists
            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = FindObjectOfType<LevelManager>();

                // If no instance is found, create a new GameObject and attach LevelManager
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(LevelManager).Name);
                    _instance = singletonObject.AddComponent<LevelManager>();
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure there is only one instance of LevelManager
        if (_instance != null && _instance != this)
        {
            // If another instance exists, destroy this one
            Destroy(this.gameObject);
            return;
        }

        // Set this instance as the singleton instance
        _instance = this;

        // Prevent this GameObject from being destroyed on scene load
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Loads a scene by its name.
    /// </summary>
    /// <param name="levelName">The name of the scene to load.</param>
    public void LoadLevel(string levelName)
    {
        // Check if the specified scene exists in the build settings
        if (SceneExists(levelName))
        {
            // Load the scene
            SceneManager.LoadScene(levelName);
        }
        else
        {
            // Log a warning if the scene does not exist
            Debug.LogWarning("Scene '" + levelName + "' does not exist in the build settings.");
        }
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }

    /// <summary>
    /// Checks if a scene exists in the build settings.
    /// </summary>
    /// <param name="sceneName">The name of the scene to check for.</param>
    /// <returns>True if the scene exists, otherwise false.</returns>
    private bool SceneExists(string sceneName)
    {
        // Iterate over all the scenes in the build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            // Get the path of the scene by build index
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            // Extract the scene file name from the path
            string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            // Check if the file name matches the requested scene name
            if (sceneFileName == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}
