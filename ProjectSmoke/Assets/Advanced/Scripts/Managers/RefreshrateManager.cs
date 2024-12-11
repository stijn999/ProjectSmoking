using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class RefreshrateManager : MonoBehaviour
{
    private void Start()
    {
        // Attempt to adjust the fixedDeltaTime based on the XR device's refresh rate when the script starts
        AttemptChangeDeltaTime();
    }

    /// <summary>
    /// Checks if an XR device is active and attempts to adjust the fixedDeltaTime accordingly.
    /// If no XR device is detected, retries after a short delay.
    /// </summary>
    private void AttemptChangeDeltaTime()
    {
        // Check if an XR device is currently active
        if (XRSettings.isDeviceActive)
        {
            // Adjust fixedDeltaTime based on the XR device's refresh rate
            ChangeDeltaTime();
        }
        else
        {
            // Retry the check after a short delay
            Invoke(nameof(AttemptChangeDeltaTime), 0.25f);
        }
    }

    /// <summary>
    /// Adjusts the fixedDeltaTime to match the XR device's refresh rate.
    /// If the refresh rate is invalid, retries after a short delay.
    /// </summary>
    private void ChangeDeltaTime()
    {
        // Retrieve the current refresh rate of the XR device
        float refreshRate = XRDevice.refreshRate;

        // Validate the refresh rate to ensure it's a positive value
        if (refreshRate > 0)
        {
            // Set the fixedDeltaTime to the reciprocal of the refresh rate
            Time.fixedDeltaTime = 1.0f / refreshRate;
        }
        else
        {
            // Log a warning if the refresh rate is invalid (for debugging purposes)
            Debug.LogWarning("Invalid XR device refresh rate: " + refreshRate);
            // Retry the adjustment after a short delay
            Invoke(nameof(ChangeDeltaTime), 0.25f);
        }
    }
}
