using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    // Enum to define possible sliding directions for the door
    public enum Direction
    {
        Forward,   // Slide in the forward direction
        Backward,  // Slide in the backward direction
        Left,      // Slide to the left
        Right,     // Slide to the right
        Up,        // Slide upwards
        Down       // Slide downwards
    }

    [Tooltip("The maximum distance the door can slide.")]
    public float maxSlideDistance = 5f; // Set the maximum slide distance in the Inspector

    [Tooltip("The direction in which the door will slide.")]
    public Direction slideDirection; // Set the slide direction in the Inspector

    [Tooltip("The speed at which the door slides.")]
    public float slidingSpeed = 0.5f; // Adjust the default speed of door movement

    private Vector3 initialPosition; // The starting position of the door
    private Vector3 targetPosition;  // The target position for the door to slide to
    private bool isOpen = false;     // Track whether the door is currently open

    private void Start()
    {
        // Record the initial position of the door when the script starts
        initialPosition = transform.position;
        targetPosition = initialPosition; // Initialize target position to be the closed position
    }

    /// <summary>
    /// Toggles the door between open and closed states.
    /// </summary>
    public void ToggleDoor()
    {
        // Switch the door's state between open and closed
        isOpen = !isOpen;

        if (isOpen)
            targetPosition = CalculateTargetPosition(maxSlideDistance); // Set the target position to open
        else
            targetPosition = initialPosition; // Return the target position to the closed position
    }

    /// <summary>
    /// Calculates the target position for the door based on the slide direction and distance.
    /// </summary>
    /// <param name="distance">The distance to slide the door.</param>
    /// <returns>The calculated target position.</returns>
    private Vector3 CalculateTargetPosition(float distance)
    {
        Vector3 directionVector = Vector3.zero;

        // Determine the direction vector based on the specified slide direction
        switch (slideDirection)
        {
            case Direction.Forward:
                directionVector = transform.forward;
                break;
            case Direction.Backward:
                directionVector = -transform.forward;
                break;
            case Direction.Left:
                directionVector = -transform.right;
                break;
            case Direction.Right:
                directionVector = transform.right;
                break;
            case Direction.Up:
                directionVector = transform.up;
                break;
            case Direction.Down:
                directionVector = -transform.up;
                break;
        }

        // Clamp the distance to ensure it does not exceed maxSlideDistance
        float clampedDistance = Mathf.Clamp(distance, 0f, maxSlideDistance);
        return initialPosition + directionVector * clampedDistance; // Calculate the target position
    }

    private void Update()
    {
        // Move the door towards the target position
        float step = slidingSpeed * Time.deltaTime; // Calculate step size based on sliding speed and delta time
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }
}
