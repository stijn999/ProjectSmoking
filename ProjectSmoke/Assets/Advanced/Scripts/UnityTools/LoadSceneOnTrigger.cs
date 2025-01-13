using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnTrigger : MonoBehaviour
{
    // The name of the scene to load when the trigger is activated
    [SerializeField]
    [Tooltip("The name of the scene to load when the trigger is activated.")]
    private string _sceneName;
    public float SceneDelay = 3f;

    /// <summary>
    /// Called when another collider enters the trigger collider attached to this GameObject.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(SceneDelay); // Wait for the delay
        LevelManager.Instance.LoadLevel(_sceneName);    // Load the target scene
    }
}
