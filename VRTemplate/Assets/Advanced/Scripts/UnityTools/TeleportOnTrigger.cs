using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TeleportOnTrigger : MonoBehaviour
{
    // The target position to teleport the player to
    [Tooltip("Target position to teleport the player.")]
    [SerializeField]
    private Transform _teleportPosition;

    // Cooldown time in seconds before the player can be teleported again
    [Tooltip("Cooldown time before the player can be teleported again.")]
    [SerializeField]
    private float _teleportCooldown = 1f;

    // Optional tag to filter which objects can trigger the teleport
    [Tooltip("Optional tag to only work with that tag")]
    [SerializeField]
    private string _optionalTag;

    // Flag to manage whether teleportation is allowed or not
    private bool _canTeleport = true;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Checks if the teleport position is assigned. If not, it destroys the GameObject.
    /// </summary>
    private void Awake()
    {
        // Ensure the teleport position is assigned
        if (_teleportPosition == null)
        {
            Debug.LogWarning("Teleport position is not assigned. Destroying game object.");
            Destroy(gameObject); // Destroy this component's game object if teleport position is not set
        }
    }

    /// <summary>
    /// Called when another collider enters the trigger collider attached to this object.
    /// Teleports the player if conditions are met and starts the cooldown routine.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if teleportation is on cooldown
        if (!_canTeleport) return;

        // Determine if the collider's tag matches the optional tag (if specified)
        bool shouldTeleport = string.IsNullOrEmpty(_optionalTag) || other.CompareTag(_optionalTag);

        if (shouldTeleport)
        {
            // Teleport the player to the specified location
            TeleportPlayerToLocation(other.gameObject);
            // Start the cooldown routine
            StartCoroutine(TeleportCooldownRoutine());
        }
    }

    /// <summary>
    /// Teleports the target object to the teleport position.
    /// </summary>
    /// <param name="targetObject">The GameObject to be teleported.</param>
    private void TeleportPlayerToLocation(GameObject targetObject)
    {
        // Attempt to get the Rigidbody component for physics-based teleportation
        if (targetObject.TryGetComponent<Rigidbody>(out Rigidbody targetRB))
        {
            // Use Rigidbody to move the player to avoid physics issues
            targetRB.MovePosition(_teleportPosition.position);
        }
        else
        {
            // Fallback to directly setting the transform position if Rigidbody is not present
            targetObject.transform.position = _teleportPosition.position;
        }
    }

    /// <summary>
    /// Coroutine to handle the teleport cooldown.
    /// Disables teleportation, waits for the cooldown period, and then re-enables teleportation.
    /// </summary>
    private IEnumerator TeleportCooldownRoutine()
    {
        // Disable further teleportation
        _canTeleport = false;
        // Wait for the specified cooldown period
        yield return new WaitForSeconds(_teleportCooldown);
        // Re-enable teleportation
        _canTeleport = true;
    }
}

#if UNITY_EDITOR
// Custom property drawer to display a dropdown of all tags in the Unity editor
[CustomEditor(typeof(TeleportOnTrigger))]
public class TeleportOnTriggerEditor : Editor
{
    // Serialized properties to link with the editor fields
    SerializedProperty _teleportPositionProp;
    SerializedProperty _teleportCooldownProp;
    SerializedProperty _tagReplacementProp;

    private void OnEnable()
    {
        // Initialize the serialized properties
        _teleportPositionProp = serializedObject.FindProperty("_teleportPosition");
        _teleportCooldownProp = serializedObject.FindProperty("_teleportCooldown");
        _tagReplacementProp = serializedObject.FindProperty("_optionalTag");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw the properties in the inspector
        EditorGUILayout.PropertyField(_teleportPositionProp);
        EditorGUILayout.PropertyField(_teleportCooldownProp);
        _tagReplacementProp.stringValue = EditorGUILayout.TagField("Optional Tag", _tagReplacementProp.stringValue);

        // Apply modified properties to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
