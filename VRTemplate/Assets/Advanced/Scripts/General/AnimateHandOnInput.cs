using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class AnimateHand : MonoBehaviour
{
    [Tooltip("Reference to the InputAction for grip input.")]
    public InputActionReference gripInputActionReference; // Reference to the InputAction for grip input

    [Tooltip("Reference to the InputAction for trigger input.")]
    public InputActionReference triggerInputActionReference; // Reference to the InputAction for trigger input

    private Animator _handAnimator; // Reference to the Animator component responsible for hand animations
    private float _gripValue; // Current value of the grip input
    private float _triggerValue; // Current value of the trigger input

    private void Start()
    {
        // Initialize the Animator component from the attached GameObject
        _handAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Update grip and trigger animations based on input values every frame
        AnimateGrip();
        AnimateTrigger();
    }

    /// <summary>
    /// Updates the grip animation based on the current grip input value.
    /// </summary>
    private void AnimateGrip()
    {
        // Read the current value of the grip input
        _gripValue = gripInputActionReference.action.ReadValue<float>();
        // Set the Animator parameter for grip animation to the current grip value
        _handAnimator.SetFloat("Grip", _gripValue);
    }

    /// <summary>
    /// Updates the trigger animation based on the current trigger input value.
    /// </summary>
    private void AnimateTrigger()
    {
        // Read the current value of the trigger input
        _triggerValue = triggerInputActionReference.action.ReadValue<float>();
        // Set the Animator parameter for trigger animation to the current trigger value
        _handAnimator.SetFloat("Trigger", _triggerValue);
    }
}
