using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FeedbackQuestion : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The slider used to rate the feedback question.")]
    private Slider _questionSlider; // Slider component to capture user rating

    [SerializeField]
    [Tooltip("The text component displaying the question or subject name.")]
    private TMP_Text _questionText; // Text component showing the question or subject

    private string _subjectName; // The name of the subject being rated

    private void Start()
    {
        // Initialize the subject name and add it to the DataManager
        SetSubjectName();
        DataManager.Instance.AddSubject(_subjectName, "X");
    }

    /// <summary>
    /// Sets the subject name based on the text component or the game object's name if text is not available.
    /// </summary>
    private void SetSubjectName()
    {
        _subjectName = _questionText != null ? _questionText.text : gameObject.name;
    }

    /// <summary>
    /// Finalizes the question by storing the slider value in the DataManager and destroying the game object.
    /// </summary>
    public void FinishQuestion()
    {
        if (_questionSlider == null) return; // Early exit if the slider is not set

        // Determine the subject name based on the text if available, otherwise use the gameObject name
        string subjectName = _questionText != null ? _questionText.text : gameObject.name;

        // Get the slider value as a float
        float rating = _questionSlider.value;

        // Add or replace the subject and rating in the DataManager
        DataManager.Instance.ReplaceSubject(subjectName, rating.ToString());

        // Destroy the game object
        Destroy(gameObject);
    }
}
