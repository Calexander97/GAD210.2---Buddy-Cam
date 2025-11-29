using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalController : MonoBehaviour
{
    public GameObject terminalPanel;
    public Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTerminal);

        if (terminalPanel != null)
            terminalPanel.SetActive(false);
    }

    public void CloseTerminal()
    {
        if (terminalPanel != null)
            terminalPanel.SetActive(false);

        Time.timeScale = 1f; // Resume game
    }
}