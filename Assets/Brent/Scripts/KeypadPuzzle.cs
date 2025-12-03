using UnityEngine;
using TMPro;

public class KeypadPuzzle : MonoBehaviour
{
    [Header("UI References")]
    public GameObject keypadPanel;
    public TMP_Text displayText;

    [Header("Code Settings")]
    public string correctCode = "386";

    private string currentInput = "";
    private Hack activeHack;
    private HeroNavAgent2D heroController;
    private bool isOpen = false;

    private void Start()
    {
        if (keypadPanel != null)
            keypadPanel.SetActive(false);
    }

    public void OpenKeypad(Hack hack, GameObject player)
    {
        if (keypadPanel == null || hack == null || player == null) return;

        activeHack = hack;
        currentInput = "";
        UpdateDisplay();
        keypadPanel.SetActive(true);
        isOpen = true;

        UIBlocker.Instance.uiOpen = true;

        // stop hero movement
        heroController = player.GetComponent<HeroNavAgent2D>();
        if (heroController != null)
        {
            heroController.agent.isStopped = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PressNumber(string num)
    {
        if (!isOpen || currentInput.Length >= 4) return;

        currentInput += num;
        UpdateDisplay();
    }

    public void ClearInput()
    {
        if (!isOpen) return;

        currentInput = "";
        UpdateDisplay();
    }

    public void Submit()
    {
        if (!isOpen) return;

        if (currentInput == correctCode)
        {
            Debug.Log("Access Granted!");
            activeHack?.UnlockDoor();
            CloseKeypad();
        }
        else
        {
            Debug.Log("Access Denied!");
            currentInput = "";
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
            displayText.text = currentInput;
    }

    public void CloseKeypad()
    {
        if (!isOpen) return;

        keypadPanel.SetActive(false);
        isOpen = false;

        // re-enable hero movement
        if (heroController != null)
        {
            heroController.agent.isStopped = false;
        }

        if (activeHack != null)
            activeHack.OnKeypadClosed(heroController.gameObject);

        activeHack = null;
        heroController = null;
        currentInput = "";
        UpdateDisplay();

        UIBlocker.Instance.uiOpen = false;
    }

    public void OnCancelPressed()
    {
        CloseKeypad();
    }
}