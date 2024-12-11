using UnityEngine;

public class ExampleWriter : MonoBehaviour
{
    [Tooltip("The first number used in feedback messages.")]
    [SerializeField] private int numberOne = 10; // The first number to be included in feedback

    [Tooltip("The second number used in feedback messages.")]
    [SerializeField] private int numberTwo = 2; // The second number to be included in feedback

    void Start()
    {
        // This method runs when the script is first initialized

        // Add subjects and associated sentences to DataManager
        // Assuming DataManager.Instance already exists and is properly set up in the scene

        // Add physics-related subjects and their descriptions
        DataManager.Instance.AddSubject("Physics", "Newton's Laws of Motion");
        DataManager.Instance.AddSubject("Physics", "Quantum Mechanics");

        // Add feedback-related subjects with dynamic content
        DataManager.Instance.AddSubject("Feedback", "Question one is marked with a " + numberOne.ToString());
        DataManager.Instance.AddSubject("Feedback", "Question two is marked with a " + numberTwo.ToString());

        // Write all added subjects and their descriptions to the file
        DataManager.Instance.WriteSubjectsToFile();
    }
}
