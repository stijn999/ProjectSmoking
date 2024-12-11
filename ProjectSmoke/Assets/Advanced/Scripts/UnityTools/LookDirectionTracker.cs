using System.Collections.Generic;
using UnityEngine;

public class LookDirectionTracker : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Maximum distance the ray can check for objects.")]
    private float maxRayDistance = 10f;

    [SerializeField]
    [Tooltip("The camera to use for the raycast. Defaults to the first camera found if not assigned.")]
    private Camera playerCamera;

    [SerializeField]
    [Tooltip("Optional layer to only work with that layer.")]
    private LayerMask _optionalLayer = 1; // Set the -1 as default to include all layers

    // Dictionary to store the total look times for each object
    private Dictionary<string, float> lookTimes = new Dictionary<string, float>();

    // The currently looked-at object
    private GameObject currentLookObject = null;

    // The time when the current look began
    private float lookStartTime;

    private void Start()
    {
        // Attempt to find the first camera in the scene if none is assigned
        if (playerCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                playerCamera = cameras[0];
            }
            else
            {
                Debug.LogError("No camera found in the scene.");
            }
        }
    }

    private void Update()
    {
        // Check if the playerCamera is assigned before tracking look time
        if (playerCamera != null)
        {
            TrackLookTime();

            // Visualize the ray in the scene view for debugging purposes
#if UNITY_EDITOR
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * maxRayDistance, Color.red);
#endif
        }
    }

    private void TrackLookTime()
    {
        // Perform the raycast and check if it hits an object
        RaycastHit hit;
        bool hitDetected = PerformRaycast(out hit);

        if (hitDetected)
        {
            // Handle the object that the raycast hit
            HandleHitObject(hit.collider.gameObject);
        }
        else
        {
            // Handle the case where no object is hit
            HandleNoHit();
        }
    }

    private bool PerformRaycast(out RaycastHit hit)
    {
        // Perform the raycast based on whether a layer mask is specified
        return _optionalLayer == 0
            ? Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxRayDistance)
            : Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxRayDistance, _optionalLayer);
    }

    private void HandleHitObject(GameObject hitObject)
    {
        // Check if the object that is currently looked at has changed
        if (hitObject != currentLookObject)
        {
            // Update the look time for the previously looked-at object
            UpdateLookTimeForCurrentObject();

            // Set the new looked-at object and update the start time
            currentLookObject = hitObject;
            lookStartTime = Time.time;
        }
    }

    private void HandleNoHit()
    {
        // Update the look time for the last looked-at object if no object is currently being looked at
        UpdateLookTimeForCurrentObject();
        currentLookObject = null;
    }

    private void UpdateLookTimeForCurrentObject()
    {
        // Update the total look time for the current looked-at object
        if (currentLookObject != null)
        {
            float lookDuration = Time.time - lookStartTime;
            string objectName = currentLookObject.name;

            if (lookTimes.ContainsKey(objectName))
            {
                // Add the new look duration to the existing time
                lookTimes[objectName] += lookDuration;
            }
            else
            {
                // Start tracking the look duration for the new object
                lookTimes[objectName] = lookDuration;
            }
        }
    }

    // Method to retrieve the total look time for a given object
    public float GetLookTime(string objectName)
    {
        // Return the look time for the object if it exists, otherwise return 0
        return lookTimes.TryGetValue(objectName, out float lookDuration) ? lookDuration : 0f;
    }

    // Logs the look times for all objects when the application is quitting
    private void LogTime()
    {
        foreach (var entry in lookTimes)
        {
            string objectName = entry.Key;
            float lookDuration = entry.Value;

            // Log the look duration using the DataManager
            DataManager.Instance.AddSubject(objectName, $"{objectName} for {lookDuration:F2} seconds.");
        }
    }

    private void OnApplicationQuit()
    {
        // Log the look times when the application is closing
        LogTime();
    }
}
