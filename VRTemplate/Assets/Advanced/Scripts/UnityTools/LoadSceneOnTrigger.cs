using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneOnTrigger : MonoBehaviour
{
    // The name of the scene to load when the trigger is activated
    [SerializeField]
    [Tooltip("The name of the scene to load when the trigger is activated.")]
    private string _sceneName;

    /// <summary>
    /// Called when another collider enters the trigger collider attached to this GameObject.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Load the scene specified by the _sceneName variable
        LevelManager.Instance.LoadLevel(_sceneName);
    }
}
