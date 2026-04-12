using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioClip menuMusic; // The music clip you want to play
    public float fadeDuration = 1.5f; // How long it takes to fade in/out

    private AudioSource audioSource;
    private string[] scenesToKeepMusic = { "MainMenu", "CharacterSelect" };

    void Awake()
    {
        // --- This is the Singleton pattern ---
        // If an Instance already exists, destroy this new one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Otherwise, this is the one and only Instance.
        Instance = this;

        // --- Make it persistent ---
        // Don't destroy this GameObject when a new scene loads.
        DontDestroyOnLoad(gameObject);

        // --- Setup the AudioSource ---
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // Make sure the music loops
        audioSource.playOnAwake = false; // We will control playback manually
        audioSource.volume = 0f; // Start with volume at 0 to fade in
    }

    void Start()
    {
        // Assign the music clip and start the fade-in process
        if (menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.Play();
            StartCoroutine(FadeIn());
        }
    }

    // This function is called every time a new scene is loaded
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // The logic to decide if we should stop the music
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldKeepMusic = false;
        foreach (string sceneName in scenesToKeepMusic)
        {
            if (scene.name == sceneName)
            {
                shouldKeepMusic = true;
                break;
            }
        }

        // If the new scene is NOT in our list, fade out and destroy
        if (!shouldKeepMusic)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    // Coroutine to smoothly fade the music IN
    public IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = 1f;
    }

    // Coroutine to smoothly fade the music OUT and then self-destruct
    public IEnumerator FadeOutAndDestroy()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }
        audioSource.Stop();
        Destroy(gameObject); // The MusicManager removes itself
    }
}
