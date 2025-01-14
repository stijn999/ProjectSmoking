using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportsoundontrigger : MonoBehaviour
{
    // The audio clip to play when the trigger is activated
    [SerializeField]
    [Tooltip("The audio clip to play when the trigger is activated.")]
    private GameObject Teleportobject;
    public Vector3 Teleportcoordinates;

    /// <summary>
    /// Called when another collider enters the trigger collider attached to this GameObject.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // teleport object to set position
        Teleportobject.transform.position = Teleportcoordinates;
    }
}
