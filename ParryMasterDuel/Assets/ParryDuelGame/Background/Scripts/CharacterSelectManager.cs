using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character Names (must match prefab names in Resources)")]
    public string[] characterNames = { "Knight", "Ninja", "Caveman", "Soldier", "Viking" };
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip browseSound;
    public AudioClip confirmSound;
    [Header("Character Sprites")]
    public Sprite[] characterSprites;
    public SceneFader fader;
    [Header("Character Stats (order matches characterNames)")]
    public int[] healthStats = { 3, 2, 4, 3, 6 };
    public int[] damageStats = { 4, 5, 3, 6, 3 };
    public int[] speedStats = { 3, 6, 2, 3, 3 };

    [Header("Confirm Colors")]
    public Color confirmedNameColor = Color.green;
    public Color unconfirmedNameColor = Color.white;
    public Color confirmedOutlineColor = Color.green;
    public Color unconfirmedOutlineColor = Color.white;

    [Header("Player 1 UI")]
    public Image p1CharacterImage;
    public Outline p1ImageOutline;
    public TextMeshProUGUI p1CharacterName;
    public TextMeshProUGUI p1StatusText;

    [Header("Player 1 Stat Segments")]
    public Image[] p1HealthSegments = new Image[6];
    public Image[] p1DamageSegments = new Image[6];
    public Image[] p1SpeedSegments = new Image[6];

    [Header("Player 2 UI")]
    public Image p2CharacterImage;
    public Outline p2ImageOutline;
    public TextMeshProUGUI p2CharacterName;
    public TextMeshProUGUI p2StatusText;

    [Header("Player 2 Stat Segments")]
    public Image[] p2HealthSegments = new Image[6];
    public Image[] p2DamageSegments = new Image[6];
    public Image[] p2SpeedSegments = new Image[6];

    [Header("Center UI")]
    public TextMeshProUGUI centerText;

    // ?? State ?????????????????????????????????????????????????
    private int p1Index = 0;
    private int p2Index = 1;

    private bool p1Confirmed = false;
    private bool p2Confirmed = false;
    private bool loadingScene = false;

    private const float LOAD_DELAY = 1.5f;

    void Start()
    {
        if (GameData.Instance == null)
        {
            GameObject gd = new GameObject("GameData");
            gd.AddComponent<GameData>();
        }

        if (p1StatusText != null) p1StatusText.text = "";
        if (p2StatusText != null) p2StatusText.text = "";
        if (centerText != null) centerText.text = "A/D  |  E to confirm                  Arrows  |  M to confirm";

        RefreshUI();
    }

    void Update()
    {
        if (loadingScene) return;
        HandlePlayer1Input();
        HandlePlayer2Input();
    }

    void HandlePlayer1Input()
    {
        if (!p1Confirmed)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                p1Index = (p1Index - 1 + characterNames.Length) % characterNames.Length;
                RefreshUI();
                if (browseSound != null) audioSource.PlayOneShot(browseSound);
            }
            if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                p1Index = (p1Index + 1) % characterNames.Length;
                RefreshUI();
                if (browseSound != null) audioSource.PlayOneShot(browseSound);
            }
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                p1Confirmed = true;
                if (p1StatusText != null) p1StatusText.text = "READY!";
                SetP1ConfirmVisuals(true);
                if (confirmSound != null) audioSource.PlayOneShot(confirmSound);
                CheckBothConfirmed();
                return;
            }
        }
        else
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                p1Confirmed = false;
                if (p1StatusText != null) p1StatusText.text = "";
                SetP1ConfirmVisuals(false);
            }
        }
    }
    IEnumerator LoadWithFade(string sceneName)
    {
        yield return StartCoroutine(fader.FadeOut());
        SceneManager.LoadScene(sceneName);
    }
    void HandlePlayer2Input()
    {
        if (!p2Confirmed)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                p2Index = (p2Index - 1 + characterNames.Length) % characterNames.Length;
                RefreshUI();
                if (browseSound != null) audioSource.PlayOneShot(browseSound);
            }
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                p2Index = (p2Index + 1) % characterNames.Length;
                RefreshUI();
                if (browseSound != null) audioSource.PlayOneShot(browseSound);
            }
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                p2Confirmed = true;
                if (p2StatusText != null) p2StatusText.text = "READY!";
                SetP2ConfirmVisuals(true);
                if (confirmSound != null) audioSource.PlayOneShot(confirmSound);
                CheckBothConfirmed();
                return;
            }
        }
        else
        {
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                p2Confirmed = false;
                if (p2StatusText != null) p2StatusText.text = "";
                SetP2ConfirmVisuals(false);
            }
        }
    }

    void SetP1ConfirmVisuals(bool confirmed)
    {
        if (p1CharacterName != null)
            p1CharacterName.color = confirmed ? confirmedNameColor : unconfirmedNameColor;
        if (p1ImageOutline != null)
            p1ImageOutline.effectColor = confirmed ? confirmedOutlineColor : unconfirmedOutlineColor;
    }

    void SetP2ConfirmVisuals(bool confirmed)
    {
        if (p2CharacterName != null)
            p2CharacterName.color = confirmed ? confirmedNameColor : unconfirmedNameColor;
        if (p2ImageOutline != null)
            p2ImageOutline.effectColor = confirmed ? confirmedOutlineColor : unconfirmedOutlineColor;
    }

    void UpdateSegments(Image[] segments, int activeCount)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] != null)
                segments[i].gameObject.SetActive(i < activeCount);
        }
    }

    void RefreshUI()
    {
        // P1
        if (p1CharacterImage != null && characterSprites != null && p1Index < characterSprites.Length)
            p1CharacterImage.sprite = characterSprites[p1Index];
        if (p1CharacterName != null)
            p1CharacterName.text = characterNames[p1Index];

        UpdateSegments(p1HealthSegments, healthStats[p1Index]);
        UpdateSegments(p1DamageSegments, damageStats[p1Index]);
        UpdateSegments(p1SpeedSegments, speedStats[p1Index]);

        // P2
        if (p2CharacterImage != null && characterSprites != null && p2Index < characterSprites.Length)
            p2CharacterImage.sprite = characterSprites[p2Index];
        if (p2CharacterName != null)
            p2CharacterName.text = characterNames[p2Index];

        UpdateSegments(p2HealthSegments, healthStats[p2Index]);
        UpdateSegments(p2DamageSegments, damageStats[p2Index]);
        UpdateSegments(p2SpeedSegments, speedStats[p2Index]);
    }

    void CheckBothConfirmed()
    {
        if (p1Confirmed && p2Confirmed)
            StartCoroutine(LoadGameplay());
    }

    IEnumerator LoadGameplay()
    {
        loadingScene = true;
        if (centerText != null) centerText.text = "Get ready!";
        yield return new WaitForSeconds(LOAD_DELAY);
        GameData.Instance.Player1CharacterIndex = p1Index;
        GameData.Instance.Player2CharacterIndex = p2Index;
        StartCoroutine(LoadWithFade("SampleScene"));
    }
}