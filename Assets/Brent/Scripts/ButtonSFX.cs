using UnityEngine;
using UnityEngine.UI;

public class ButtonSFX : MonoBehaviour
{
    public AudioClip clip;

    void Awake()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    void PlaySound()
    {
        if (clip != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(clip);
        }
    }
}