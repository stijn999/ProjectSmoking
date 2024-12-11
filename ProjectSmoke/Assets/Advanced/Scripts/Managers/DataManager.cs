using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The name of the file to write data to. If empty, a default name will be generated.")]
    private string _fileName; // The name of the current file to write data to

    private static DataManager instance; // Singleton instance of DataManager

    [SerializeField]
    [Tooltip("Title for the CSV file. This will be the header of the CSV file.")]
    private string _title = "PlaytestData"; // Title for the CSV file

    [SerializeField]
    [Tooltip("Folder name where CSV files will be stored.")]
    private string _folderName = "PlaytestData"; // Folder name to store the CSV files

    private Dictionary<string, List<string>> subjectsDictionary = new Dictionary<string, List<string>>(); // Dictionary to hold subjects and their sentences

    private void Awake()
    {
        // Ensure there's only one instance of this class
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject); // Prevent the object from being destroyed on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    /// <summary>
    /// Public static instance property to access the singleton instance.
    /// </summary>
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DataManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("DataManager");
                    instance = singletonObject.AddComponent<DataManager>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Sorts the subjects dictionary alphabetically by keys.
    /// </summary>
    private void SortSubjectsDictionary()
    {
        subjectsDictionary = subjectsDictionary.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Tries to create a new file with the specified or default file name.
    /// </summary>
    private void TryCreateNewFile()
    {
        try
        {
            // Check if _fileName is null or empty, and generate a new file name if needed
            if (string.IsNullOrEmpty(_fileName))
            {
                string dateTimeString = DateTime.Now.ToString("dd-MM-yyyy_HHmm");
                _fileName = $"{dateTimeString}.csv";
                Debug.LogWarning("No filename for data logging has been given, defaulting to Date + time");
            }

            // Ensure the file name ends with .csv
            if (!_fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                _fileName += ".csv";
            }

            // Combine the paths to create the full directory path
            string directoryPath = Path.Combine(Application.dataPath, _folderName);

            // Check if the directory exists; if not, create it
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"Folder '{_folderName}' not found, creating a new folder at: {directoryPath}");
            }

            // Combine the full file path including the folder
            string filePath = Path.Combine(directoryPath, _fileName);

            // If the file already exists, do not change the _fileName
            if (File.Exists(filePath))
            {
                Debug.Log($"File '{_fileName}' already exists. Using existing file at: {filePath}");
            }
            else
            {
                // Create the file and write title and subjects
                WriteTitleAndSubjects(filePath);
                Debug.Log($"Created new file: {_fileName} at: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create or access file: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes the title and subjects to the specified file path.
    /// </summary>
    /// <param name="filePath">The full path of the file to write to.</param>
    private void WriteTitleAndSubjects(string filePath)
    {
        try
        {
            // Prepare the content to write to CSV
            string content = $"\"{_title}\"\n\n"; // Enclose title in quotes for CSV

            // Write the subjects horizontally
            List<string> subjects = new List<string>(subjectsDictionary.Keys);
            foreach (string subject in subjects)
            {
                content += $"\"{subject}\",";
            }
            content = content.TrimEnd(',') + "\n"; // Remove the last comma and add a new line

            // Write content to file
            File.WriteAllText(filePath, content);
            Debug.Log("Title and subjects written to file.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to write title and subjects to file: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a new sentence to the specified subject in the dictionary.
    /// </summary>
    /// <param name="subjectName">The name of the subject.</param>
    /// <param name="sentence">The sentence to add.</param>
    public void AddSubject(string subjectName, string sentence)
    {
        if (!subjectsDictionary.ContainsKey(subjectName))
        {
            subjectsDictionary.Add(subjectName, new List<string>());
        }
        subjectsDictionary[subjectName].Add(sentence);
    }

    /// <summary>
    /// Replaces the sentences for a specified subject with a new sentence.
    /// </summary>
    /// <param name="subjectName">The name of the subject.</param>
    /// <param name="sentence">The new sentence to set.</param>
    public void ReplaceSubject(string subjectName, string sentence)
    {
        if (subjectsDictionary.ContainsKey(subjectName))
        {
            subjectsDictionary[subjectName] = new List<string> { sentence };
        }
        else
        {
            Debug.LogWarning("Subject info can't be replaced since subject does not exist");
        }
    }

    /// <summary>
    /// Writes all subjects and their sentences to the file.
    /// </summary>
    public void WriteSubjectsToFile()
    {
        if (subjectsDictionary.Count == 0)
        {
            Debug.LogWarning("No subjects to write.");
            return;
        }

        // Sort the subjects alphabetically
        SortSubjectsDictionary();

        // Try creating a new file or use an existing one
        TryCreateNewFile();

        try
        {
            // Prepare the content to append to CSV
            string content = "";

            // Find the maximum number of sentences for any subject
            int maxSentences = 0;
            foreach (var kvp in subjectsDictionary)
            {
                if (kvp.Value.Count > maxSentences)
                {
                    maxSentences = kvp.Value.Count;
                }
            }

            // Write the sentences vertically under each subject
            for (int i = 0; i < maxSentences; i++)
            {
                foreach (string subject in subjectsDictionary.Keys)
                {
                    if (subjectsDictionary[subject].Count > i)
                    {
                        string sentence = subjectsDictionary[subject][i];
                        content += $"\"{sentence}\",";
                    }
                    else
                    {
                        content += "\"\","; // Add empty column if no sentence for this subject
                    }
                }
                content = content.TrimEnd(',') + "\n"; // Remove the last comma and add a new line
            }

            // Append content to file
            string directoryPath = Path.Combine(Application.dataPath, _folderName);
            string filePath = Path.Combine(directoryPath, _fileName);

            File.AppendAllText(filePath, content);
            Debug.Log("Subjects appended to file.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to append subjects to file: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when the application quits to ensure all subjects are written to the file.
    /// </summary>
    private void OnApplicationQuit()
    {
        WriteSubjectsToFile();
    }
}
