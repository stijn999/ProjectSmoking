using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class AssignPlayerControls : MonoBehaviour
{
    [Tooltip("The time it takes before the FPS controls get assigned if no XR device is detected.")]
    [SerializeField]
    private float timeOutTime = 2f; // Duration to wait before spawning the FPS controller if no XR device is found

    [Tooltip("Prefab that represents the FPS controller to be instantiated.")]
    [SerializeField]
    private GameObject fpsControllerPrefab; // Prefab for the First-Person Shooter (FPS) controller

    void Start()
    {
        // Start the coroutine to check for XR devices and potentially spawn the FPS controller
        StartCoroutine(CheckAndSpawnController());
    }

    private IEnumerator CheckAndSpawnController()
    {
        // Immediately check if an XR device is active
        if (XRSettings.isDeviceActive)
        {
            yield break; // Exit the coroutine if an XR device is already active
        }

        // Wait for the specified timeout duration if no XR device is detected
        yield return new WaitForSeconds(timeOutTime);

        // Check again if an XR device is now active after the wait
        if (XRSettings.isDeviceActive)
        {
            yield break; // Exit if an XR device is detected after the wait
        }
        else
        {
            // Spawn the FPS controller if no XR device is detected
            SpawnFPSController();
        }
    }

    private void SpawnFPSController()
    {
        // Check if the FPS controller prefab has been assigned
        if (fpsControllerPrefab != null)
        {
            // Calculate the target position for the FPS controller, slightly above this object's position
            Vector3 targetPosition = transform.position + new Vector3(0, 1, 0);
            // Instantiate the FPS controller prefab at the target position with no rotation
            Instantiate(fpsControllerPrefab, targetPosition, Quaternion.identity);
            // Destroy this GameObject to prevent multiple controller assignments
            Destroy(gameObject);
        }
        else
        {
            // Log an error if the FPS controller prefab is not assigned
            Debug.LogError("FPS Controller Prefab is not assigned.");
        }
    }
}
