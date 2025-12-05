using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    public MusicFader fader;

    void Start()
    {
        StartCoroutine(fader.FadeIn(1.0f, 0.15f));
    }
}