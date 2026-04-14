using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public int Player1CharacterIndex = 0;
    public int Player2CharacterIndex = 1;
    public int gamesPlayedSinceAd = 0;    
    public int gamesBeforeAd = 2;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}