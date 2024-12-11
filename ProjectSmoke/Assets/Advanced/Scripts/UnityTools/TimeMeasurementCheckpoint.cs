using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimeMeasurementCheckpoint : MonoBehaviour
{
    // Cooldown time in seconds to prevent immediate retriggering of the checkpoint
    [Tooltip("Cooldown time in seconds to prevent immediate retriggering.")]
    [SerializeField]
    [Range(0.1f, 60)]
    private float _checkpointCooldown = 1f;

    // Optional name for the checkpoint, can be set in the inspector, otherwise defaulting to the name of the gameobject
    [Tooltip("Optional name for the checkpoint, otherwise defaulting to the name of the gameobject.")]
    [SerializeField]
    private string _optionalName;

    // Optional tag to restrict which objects can trigger the checkpoint
    [Tooltip("Optional tag to only work with that tag.")]
    [SerializeField]
    private string _optionalTag;

    // Flag to indicate if the checkpoint is on cooldown
    private bool _onCooldown = false;

    // Method called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter(Collider other)
    {
        // Check if the checkpoint is not on cooldown and either no tag is specified or the tag matches
        if (!_onCooldown && (string.IsNullOrEmpty(_optionalTag) || other.CompareTag(_optionalTag)))
        {
            // Log the time and start the cooldown coroutine
            LogTime();
            Debug.Log("CheckpointTriggered");
            StartCoroutine(CooldownRoutine());
        }
    }

    // Coroutine to handle the cooldown period
    private IEnumerator CooldownRoutine()
    {
        // Set the cooldown flag to true
        _onCooldown = true;

        // Wait for the specified cooldown duration
        yield return new WaitForSeconds(_checkpointCooldown);

        // Reset the cooldown flag
        _onCooldown = false;
    }

    // Method to log the current time with an optional name for the checkpoint
    private void LogTime()
    {
        /*
        // Create a message with the checkpoint name and current time
        string message = $"Checkpoint {GetSubjectName()} got triggered at {GetCurrentTime()}";
        */

        // Log the message to the DataManager
        DataManager.Instance.AddSubject(GetSubjectName(), GetCurrentTime());
    }

    // Helper method to get the checkpoint name, using the optional name if provided
    private string GetSubjectName()
    {
        return string.IsNullOrEmpty(_optionalName) ? gameObject.name : _optionalName;
    }

    // Helper method to get the current time as a string in HH:mm:ss format
    private string GetCurrentTime()
    {
        DateTime now = DateTime.Now;
        return now.ToString("HH:mm:ss");
    }
}

#if UNITY_EDITOR
// Custom editor script for TimeMeasurementCheckpoint to enhance inspector functionality in Unity Editor
[CustomEditor(typeof(TimeMeasurementCheckpoint))]
public class TimeMeasurementCheckpointEditor : Editor
{
    SerializedProperty _checkpointCooldownProp;   // Serialized property for checkpoint cooldown time
    SerializedProperty _optionalNameProp;         // Serialized property for optional checkpoint name
    SerializedProperty _optionalTagProp;          // Serialized property for optional tag restriction

    private void OnEnable()
    {
        // Initialize serialized properties based on the target component
        _checkpointCooldownProp = serializedObject.FindProperty("_checkpointCooldown");
        _optionalNameProp = serializedObject.FindProperty("_optionalName");
        _optionalTagProp = serializedObject.FindProperty("_optionalTag");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object's representation in the Inspector
        serializedObject.Update();

        // Display property fields for cooldown time, optional name, and tag
        EditorGUILayout.PropertyField(_checkpointCooldownProp);
        EditorGUILayout.PropertyField(_optionalNameProp);

        // Display a tag field specifically for the optionalTagProp, showing a dropdown of Unity tags
        _optionalTagProp.stringValue = EditorGUILayout.TagField("Optional Tag", _optionalTagProp.stringValue);

        // Apply any modified properties back to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
