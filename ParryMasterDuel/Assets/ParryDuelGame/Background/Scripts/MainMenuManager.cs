using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public SceneFader fader;

    public void OnPlayPressed()
    {
        // Make sure GameData exists before we go anywhere
        if (GameData.Instance == null)
        {
            GameObject gd = new GameObject("GameData");
            gd.AddComponent<GameData>();
        }
        StartCoroutine(LoadWithFade("CharacterSelect"));
    }

    IEnumerator LoadWithFade(string sceneName)
    {
        yield return StartCoroutine(fader.FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
