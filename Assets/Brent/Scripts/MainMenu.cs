using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Fade Timing")]
    public float fadeOutDuration = 1.0f;
    public float postFadeDelay = 0.1f;

    public MusicFader menuFader;

    public void StartGame()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        SFXManager.Instance.PlayStartMenuClick();

        // fade out menu music
        if (menuFader != null)
            yield return StartCoroutine(menuFader.FadeOut(fadeOutDuration));

        // small delay after fade
        yield return new WaitForSeconds(postFadeDelay);

        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index + 1);
    }
}