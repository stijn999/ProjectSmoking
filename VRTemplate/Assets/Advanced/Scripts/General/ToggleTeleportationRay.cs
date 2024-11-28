using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ToggleTeleportationRay : MonoBehaviour
{
    [Tooltip("The GameObject representing the left teleportation ray.")]
    [SerializeField]
    private GameObject leftTeleportationRay;

    [Tooltip("The GameObject representing the right teleportation ray.")]
    [SerializeField]
    private GameObject rightTeleportationRay;

    [Tooltip("The input action property for the left ray activation.")]
    [SerializeField]
    private InputActionProperty leftActivation;

    [Tooltip("The input action property for the right ray activation.")]
    [SerializeField]
    private InputActionProperty rightActivation;

    [Tooltip("The XRRayInteractor associated with the left teleportation ray.")]
    [SerializeField]
    private XRRayInteractor leftRay;

    [Tooltip("The XRRayInteractor associated with the right teleportation ray.")]
    [SerializeField]
    private XRRayInteractor rightRay;

    // Update is called once per frame
    private void Update()
    {
        // Check if the left ray is currently hovering over an object
        bool isLeftRayHovering = leftRay.TryGetHitInfo(out Vector3 leftPos, out Vector3 leftNormal, out int leftNumber, out bool leftValid);

        // Check if the right ray is currently hovering over an object
        bool isRightRayHovering = rightRay.TryGetHitInfo(out Vector3 rightPos, out Vector3 rightNormal, out int rightNumber, out bool rightValid);

        // Toggle the visibility of the left teleportation ray based on hover status and input action
        leftTeleportationRay.SetActive(!isLeftRayHovering && leftActivation.action.ReadValue<float>() > 0.1f);

        // Toggle the visibility of the right teleportation ray based on hover status and input action
        rightTeleportationRay.SetActive(!isRightRayHovering && rightActivation.action.ReadValue<float>() > 0.1f);
    }
}
