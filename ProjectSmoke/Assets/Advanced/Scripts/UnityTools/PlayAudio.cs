using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioSimplified : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The audio clip to be played. Assign this in the Unity editor.")]
    private AudioClip audioClip; // The audio clip that will be played

    /// <summary>
    /// Plays the assigned audio clip at the object's current position.
    /// </summary>
    public void PlayAudio()
    {
        // Check if the audio clip is assigned
        if (audioClip != null)
        {
            // Use SoundManager to play the sound at this object's position
            SoundManager.Instance.PlaySoundAtLocation(audioClip, transform.position);
        }
        else
        {
            // Log a warning if the audio clip is not assigned
            Debug.LogWarning("Audio clip not assigned in PlayAudioSimplified script on object: " + gameObject.name);
        }
    }
}
