using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleButtonColor : MonoBehaviour
{
    public Color colorOn = Color.red;
    public Color colorOff = Color.green;

    private Image buttonImage;
    private bool isOn = false;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.color = colorOff;

        GetComponent<Button>().onClick.AddListener(ToggleColor);
    }

    void ToggleColor()
    {
        isOn = !isOn;
        buttonImage.color = isOn ? colorOn : colorOff;
    }
}