using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MultipleChoiceQuestion : MonoBehaviour
{
    [SerializeField, Tooltip("The TMP_Text component that displays the question text.")]
    private TMP_Text _questionText; // TMP_Text component for displaying the question

    private string _subjectName; // Stores the subject name of the question
    private string _answer = "Unanswered"; // Tracks the selected answer

    private void Start()
    {
        // Initialize the subject name and add it to the DataManager with a placeholder answer
        SetSubjectName();
        DataManager.Instance.AddSubject(_subjectName, "X");
    }

    private void SetSubjectName()
    {
        // Set the subject name from the question text if available, otherwise use the GameObject's name
        _subjectName = _questionText != null ? _questionText.text : gameObject.name;
    }

    // Sets the answer based on the text of the provided TMP_Text component
    public void SetAnswer(TMP_Text targetText)
    {
        // Check if the provided TMP_Text component is null
        if (targetText == null)
        {
            Debug.LogWarning($"Trying to assign the answer to the text, but the TMP_Text component is null on script '{GetType().Name}' on object '{gameObject.name}'");
            return;
        }

        // Assign the answer to the text of the provided TMP_Text component
        _answer = targetText.text; // Set the answer to the text of the provided TMP_Text component
    }

    // Finalizes the question by submitting the answer and then destroying the GameObject
    public void FinishQuestion()
    {
        // Check if an answer has been selected before finalizing
        if (_answer == "Unanswered")
        {
            Debug.LogWarning($"Trying to finish the question, but the question has not been answered yet on script '{GetType().Name}' on object '{gameObject.name}'");
            return;
        }

        // Replace the subject's placeholder answer with the final answer in DataManager
        DataManager.Instance.ReplaceSubject(_subjectName, _answer);

        // Destroy the GameObject after processing to clean up
        Destroy(gameObject);
    }
}
