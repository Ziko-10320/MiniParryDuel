using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Tutorial")]
    public GameObject tutorialPanel;
    public float tutorialDuration = 4f;
    public SceneFader fader;
    [Header("Spawn Points")]
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;
    [Header("Health Bars")]
    public HealthBarUI p1HealthBar;
    public HealthBarUI p2HealthBar;
    public Sprite[] characterPortraits;
    [Header("Camera")]
    public CameraController cameraController;
    [Header("Backgrounds")]
    public GameObject[] skies;
    [Header("Countdown UI")]
    public TextMeshProUGUI countdownText;
    [Header("Environment")] // <-- ADD THIS
    public AudioClip environmentLoop;
    [Header("Result UI")]
    public TextMeshProUGUI resultText;

    [Header("End Panel")]
    public GameObject endPanel;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;

    [Header("Character Names (same order as CharacterSelect)")]
    public string[] characterNames = { "Knight", "Ninja", "Caveman", "Soldier", "Viking" };

    private GameObject player1Instance;
    private GameObject player2Instance;

    private bool gameStarted = false;
    private bool gameOver = false;
    private bool isPaused = false;

    private bool p1Finishable = false;
    private bool p2Finishable = false;

    void Awake()
    {
        Instance = this;
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && environmentLoop != null)
        {
            audioSource.clip = environmentLoop;
            audioSource.Play();
        }
    }

    void Start()
    {
        SelectRandomSky();
        if (endPanel != null) endPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (resultText != null) resultText.gameObject.SetActive(false);

        restartButton?.onClick.AddListener(RestartGame);
        mainMenuButton?.onClick.AddListener(GoToMainMenu);
        resumeButton?.onClick.AddListener(ResumeGame);
        pauseRestartButton?.onClick.AddListener(RestartGame);
        pauseMainMenuButton?.onClick.AddListener(GoToMainMenu);
        pauseButton?.onClick.AddListener(PauseGame);

        StartCoroutine(SpawnAndCountdown());
    }

    void Update()
    {
        if (gameOver) return;
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    IEnumerator SpawnAndCountdown()
    {
        if (tutorialPanel != null && !PlayerPrefs.HasKey("TutorialShown"))
        {
            PlayerPrefs.SetInt("TutorialShown", 1);
            tutorialPanel.SetActive(true);
            yield return new WaitForSeconds(tutorialDuration);
            tutorialPanel.SetActive(false);
        }
        else if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        int p1Index = GameData.Instance != null ? GameData.Instance.Player1CharacterIndex : 0;
        int p2Index = GameData.Instance != null ? GameData.Instance.Player2CharacterIndex : 1;
       
        GameObject p1Prefab = Resources.Load<GameObject>(characterNames[p1Index]);
        GameObject p2Prefab = Resources.Load<GameObject>(characterNames[p2Index]);

        if (p1Prefab == null) Debug.LogError($"Prefab '{characterNames[p1Index]}' not found in Resources!");
        if (p2Prefab == null) Debug.LogError($"Prefab '{characterNames[p2Index]}' not found in Resources!");

        player1Instance = Instantiate(p1Prefab, player1SpawnPoint.position, player1SpawnPoint.rotation);
        player2Instance = Instantiate(p2Prefab, player2SpawnPoint.position, player2SpawnPoint.rotation);

        Vector3 p2Scale = player2Instance.transform.localScale;
        player2Instance.transform.localScale = new Vector3(-Mathf.Abs(p2Scale.x), p2Scale.y, p2Scale.z);
        if (p1HealthBar != null)
            p1HealthBar.SetCharacter(player1Instance,
                characterPortraits[p1Index], characterNames[p1Index]);
        if (p2HealthBar != null)
            p2HealthBar.SetCharacter(player2Instance,
                characterPortraits[p2Index], characterNames[p2Index]);
        if (cameraController != null)
            cameraController.SetTargets(player1Instance.transform, player2Instance.transform);

        if (InputManager.Instance != null)
            InputManager.Instance.SetPlayers(player1Instance, player2Instance);

        SetPlayersInputEnabled(false);

        if (countdownText != null) countdownText.gameObject.SetActive(true);
        countdownText.text = "3"; yield return new WaitForSeconds(1f);
        countdownText.text = "2"; yield return new WaitForSeconds(1f);
        countdownText.text = "1"; yield return new WaitForSeconds(1f);
        countdownText.text = "FIGHT!"; yield return new WaitForSeconds(0.6f);
        countdownText.gameObject.SetActive(false);

        SetPlayersInputEnabled(true);
        gameStarted = true;
    }
    void SelectRandomSky()
    {
        // First, make sure the list isn't empty
        if (skies == null || skies.Length == 0)
        {
            Debug.LogWarning("Skies list is empty! Cannot select a random sky.");
            return;
        }

        // 1. Disable all skies first to ensure a clean state
        foreach (GameObject sky in skies)
        {
            sky.SetActive(false);
        }

        // 2. Pick a random index from the list
        int randomIndex = Random.Range(0, skies.Length);

        // 3. Enable the randomly chosen sky
        skies[randomIndex].SetActive(true);
    }
    public void OnPlayerFinishable(GameObject player)
    {
        if (gameOver) return;
        if (player == player1Instance) p1Finishable = true;
        if (player == player2Instance) p2Finishable = true;

        if (p1Finishable && p2Finishable)
            StartCoroutine(ShowResultThenPanel("Tie"));
    }

    public void OnPlayerDisabled(GameObject player)
    {
        if (gameOver) return;

        bool p1Gone = player1Instance == null || !player1Instance.activeInHierarchy;
        bool p2Gone = player2Instance == null || !player2Instance.activeInHierarchy;

        if (p1Gone && p2Gone)
        {
            StartCoroutine(ShowResultThenPanel("Tie"));
            return;
        }

        if (player == player1Instance)
            StartCoroutine(ShowResultThenPanel("P2 Wins!"));
        else if (player == player2Instance)
            StartCoroutine(ShowResultThenPanel("P1 Wins!"));
    }

    IEnumerator ShowResultThenPanel(string result)
    {
        if (gameOver) yield break;
        gameOver = true;
        SetPlayersInputEnabled(false);

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = result;
        }

        yield return new WaitForSeconds(1.5f);
        if (endPanel != null) endPanel.SetActive(true);
    }

    public void PauseGame()
    {
        if (gameOver) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void SetPlayersInputEnabled(bool enabled)
    {
        if (InputManager.Instance != null)
            InputManager.Instance.inputEnabled = enabled;
        EnableMovement(player1Instance, enabled);
        EnableMovement(player2Instance, enabled);
    }

    void EnableMovement(GameObject player, bool enabled)
    {
        if (player == null) return;
        var knight = player.GetComponent<KnightMovement>();
        var viking = player.GetComponent<VikingMovement>();
        var ninja = player.GetComponent<NinjaMovement>();
        var soldier = player.GetComponent<SoldierMovement>();
        var caveman = player.GetComponent<CaveManMovement>();

        if (knight != null) knight.enabled = enabled;
        if (viking != null) viking.enabled = enabled;
        if (ninja != null) ninja.enabled = enabled;
        if (soldier != null) soldier.enabled = enabled;
        if (caveman != null) caveman.enabled = enabled;
    }
    IEnumerator LoadWithFade(string sceneName)
    {
        yield return StartCoroutine(fader.FadeOut());
        SceneManager.LoadScene(sceneName);
    }
    void RestartGame()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadWithFade("SampleScene"));
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadWithFade("MainMenu"));
    }
}