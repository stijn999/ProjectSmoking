using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    // The audio clip to play when the trigger is activated
    [SerializeField]
    [Tooltip("The audio clip to play when the trigger is activated.")]
    private AudioClip _audioClip;

    /// <summary>
    /// Called when another collider enters the trigger collider attached to this GameObject.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Play the specified audio clip at the position of this GameObject
        SoundManager.Instance.PlaySoundAtLocation(_audioClip, transform.position);
    }
}
