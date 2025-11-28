using UnityEngine;
using UnityEngine.UI;

public class OpenPanelButton : MonoBehaviour
{
    [Header("Panel to Open")]
    public GameObject panelToOpen;

    [Header("Optional: Panel to Close")]
    public GameObject panelToClose;

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);

        if (panelToOpen != null)
            panelToOpen.SetActive(true);
    }
}
