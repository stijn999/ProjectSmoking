using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Singleton instance of the SoundManager
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find an existing instance of SoundManager in the scene
                _instance = FindObjectOfType<SoundManager>();
                if (_instance == null)
                {
                    // If none exists, create a new GameObject and attach the SoundManager component
                    GameObject soundManagerObject = new GameObject("SoundManager");
                    _instance = soundManagerObject.AddComponent<SoundManager>();
                    Debug.LogWarning($"Instance of SoundManager created on script '{nameof(SoundManager)}' on object '{soundManagerObject.name}'");
                }
            }
            return _instance;
        }
    }
    private static SoundManager _instance; // Singleton instance reference

    [SerializeField]
    [Tooltip("Cooldown time to prevent the same sound from playing too frequently")]
    private float soundCooldown = 0.15f; // Cooldown time between playing the same sound

    [SerializeField]
    [Tooltip("Initial size of the audio source pool")]
    private int initialPoolSize = 10; // Initial number of audio sources to create

    [SerializeField]
    [Tooltip("Maximum size of the audio source pool")]
    private int maxPoolSize = 20; // Maximum number of audio sources in the pool

    private List<AudioSource> audioSourcesPool = new List<AudioSource>(); // List to store pooled AudioSource objects
    private GameObject _audioSourceContainer; // Container for the audio source objects
    private Dictionary<AudioClip, float> lastPlayTimes = new Dictionary<AudioClip, float>(); // Dictionary to track last play times of each sound

    private void Awake()
    {
        // Ensure there's only one instance of SoundManager
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Duplicate SoundManager found on script '{GetType().Name}' on object '{gameObject.name}', destroying this instance");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject); // Prevent this object from being destroyed when loading new scenes

        InitializePool(); // Initialize the audio source pool
    }

    // Initialize the audio source pool with initialPoolSize
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource(); // Create and add new audio sources to the pool
        }
    }

    // Get or create the container for audio source objects
    private GameObject GetAudioSourceContainer()
    {
        if (_audioSourceContainer == null)
        {
            // Create a new container if it doesn't exist
            _audioSourceContainer = new GameObject("AudioSourceContainer");
            _audioSourceContainer.transform.SetParent(_instance.transform);
            Debug.LogWarning($"No audioSourceContainer has been found on script '{GetType().Name}' on object '{gameObject.name}', creating a new one");
        }
        return _audioSourceContainer;
    }

    /// <summary>
    /// Plays a sound at a specific location with optional random pitch and looping.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="position">The position at which to play the sound.</param>
    /// <param name="randomizePitch">Whether to randomize the pitch of the sound.</param>
    /// <param name="loop">Whether to loop the sound.</param>
    public void PlaySoundAtLocation(AudioClip clip, Vector3 position, bool randomizePitch = false, bool loop = false)
    {
        if (clip == null)
        {
            Debug.LogWarning($"No audio clip was provided on script '{GetType().Name}' on object '{gameObject.name}'");
            return;
        }

        // Check if the sound is on cooldown
        if (lastPlayTimes.ContainsKey(clip) && Time.time - lastPlayTimes[clip] < soundCooldown)
        {
            return; // Sound is still on cooldown
        }

        // Get an available audio source from the pool
        AudioSource audioSource = GetAvailableAudioSource();
        if (audioSource == null)
        {
            Debug.LogWarning($"No available AudioSource in the pool on script '{GetType().Name}' on object '{gameObject.name}', increasing pool size");
            CreateNewAudioSource();
            audioSource = GetAvailableAudioSource(); // Try again after increasing pool size
            if (audioSource == null)
            {
                Debug.LogWarning($"Unable to play sound. AudioSource pool is full and limit has been reached on script '{GetType().Name}' on object '{gameObject.name}'");
                return;
            }
        }

        // Configure and play the audio source
        audioSource.gameObject.SetActive(true);
        audioSource.transform.position = position;
        audioSource.clip = clip;
        audioSource.Play();

        if (randomizePitch) audioSource.pitch = GetRandomNumber(0.9f, 1.1f); // Randomize pitch if specified
        audioSource.loop = loop; // Set looping if specified

        // Return to pool after playing if not looping
        if (!loop)
            StartCoroutine(ReturnToPool(audioSource, clip.length));

        // Update the last play time
        lastPlayTimes[clip] = Time.time;
    }

    // Load a sound clip from resources
    private AudioClip LoadSound(string soundName)
    {
        return Resources.Load<AudioClip>("Sounds/" + soundName);
    }

    // Get an available audio source from the pool
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcesPool)
        {
            if (!source.gameObject.activeSelf)
                return source; // Return the first inactive audio source
        }
        return null; // No available audio sources
    }

    // Create a new audio source and add it to the pool
    private void CreateNewAudioSource()
    {
        if (audioSourcesPool.Count >= maxPoolSize)
            return; // Do not exceed the maximum pool size

        GameObject newAudioSource = new GameObject("AudioSource");
        AudioSource audioSource = newAudioSource.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1; // Set the audio source to 3D
        audioSourcesPool.Add(audioSource);
        newAudioSource.transform.SetParent(GetAudioSourceContainer().transform);
        newAudioSource.gameObject.SetActive(false);
    }

    // Coroutine to return an audio source to the pool after a delay
    private IEnumerator ReturnToPool(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.pitch = 1;
        audioSource.gameObject.SetActive(false);
    }

    // Get a random number within a specified range
    private float GetRandomNumber(float min, float max)
    {
        return Random.Range(min, max);
    }
}
