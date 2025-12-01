using UnityEngine;
using TMPro;

/// Simple top-of-screen banner for objective messages.
/// Add a Canvas → TMP Text (UGUI). Anchor at top. Drag into 'label'.
public class ObjectiveBanner : MonoBehaviour
{
    public TMP_Text label;
    public float showTime = 2.5f;

    float timer;

    void Awake()
    {
        if (label) label.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!label) return;

        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) label.gameObject.SetActive(false);
        }
    }

    public void Show(string msg)
    {
        if (!label) return;
        label.text = msg;
        label.gameObject.SetActive(true);
        timer = showTime;
    }
}
