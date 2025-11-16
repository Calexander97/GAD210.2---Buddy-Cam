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
    private PlayerMovement playerMovement;
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

        playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.canMove = false;
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
            if (activeHack != null)
                activeHack.UnlockDoor();

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

        if (activeHack != null && playerMovement != null)
            activeHack.OnKeypadClosed(playerMovement.gameObject);

        if (playerMovement != null)
            playerMovement.canMove = true;

        activeHack = null;
        playerMovement = null;
        currentInput = "";
        UpdateDisplay();
    }

    public void OnCancelPressed()
    {
        CloseKeypad();
    }
}