using UnityEngine;

// Ensure the GameObject has a CharacterController component attached
[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Tooltip("Movement speed of the player.")]
    [SerializeField]
    private float _speed = 5.0f; // Movement speed of the player

    [Tooltip("Speed of the mouse look rotation.")]
    [SerializeField]
    private float _lookSpeed = 2.0f; // Speed at which the camera rotates based on mouse input

    [Tooltip("Initial upward speed when jumping.")]
    [SerializeField]
    private float _jumpSpeed = 8.0f; // Speed applied when the player jumps

    [Tooltip("The force of gravity applied to the player.")]
    [SerializeField]
    private float _gravity = 20.0f; // Gravity strength affecting the player

    private CharacterController _characterController; // Reference to the CharacterController component
    private Camera _targetCamera; // Reference to the camera used for viewing
    private Vector3 _moveDirection = Vector3.zero; // Direction the player is currently moving in
    private float _rotationX = 0.0f; // X rotation for the camera (up/down)
    private float _rotationY = 0.0f; // Y rotation for the player (left/right)

    void Start()
    {
        // Get the CharacterController component attached to this GameObject
        _characterController = GetComponent<CharacterController>();

        // Lock the cursor to the center of the screen for a better FPS experience
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set the camera for the player
        SetCamera();
    }

    /// <summary>
    /// Finds and assigns the first camera in the scene to the targetCamera variable.
    /// Logs an error if no camera is found.
    /// </summary>
    private void SetCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>(); // Find all cameras in the scene
        if (cameras.Length > 0)
        {
            _targetCamera = cameras[0]; // Assign the first camera to targetCamera
        }
        else
        {
            Debug.LogError("No camera found in the scene."); // Log an error if no camera is found
        }
    }

    void Update()
    {
        // Check if the target camera is set
        if (_targetCamera != null)
        {
            // Handle player rotation based on mouse input
            _rotationX += Input.GetAxis("Mouse X") * _lookSpeed; // Rotate player left/right based on mouse input
            _rotationY -= Input.GetAxis("Mouse Y") * _lookSpeed; // Rotate camera up/down based on mouse input
            _rotationY = Mathf.Clamp(_rotationY, -90, 90); // Clamp the vertical rotation to avoid flipping

            // Apply the rotations to the player and camera
            transform.localRotation = Quaternion.Euler(0, _rotationX, 0); // Rotate the player
            _targetCamera.transform.localRotation = Quaternion.Euler(_rotationY, 0, 0); // Rotate the camera

            // Handle player movement
            if (_characterController.isGrounded) // Check if the player is on the ground
            {
                // Get movement input from the user
                _moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                _moveDirection = transform.TransformDirection(_moveDirection); // Convert local movement direction to world space
                _moveDirection *= _speed; // Apply speed to the movement vector

                // Handle jumping
                if (Input.GetButton("Jump"))
                {
                    _moveDirection.y = _jumpSpeed; // Apply jump speed to the vertical movement
                }
            }

            // Apply gravity to the player
            _moveDirection.y -= _gravity * Time.deltaTime; // Update vertical movement with gravity
            _characterController.Move(_moveDirection * Time.deltaTime); // Move the player based on the calculated direction
        }
    }
}
