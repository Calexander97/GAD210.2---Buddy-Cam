using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x, y;
    public bool isSolution = false;
    public bool isRevealedClue = false;

    public enum TileState { Hidden, Revealed, Flagged }
    public TileState currentState = TileState.Hidden;

    private Image img;
    private TMP_Text numberText;
    private NumberPuzzleSolvable puzzle;

    public void Init(int x, int y, NumberPuzzleSolvable puzzle)
    {
        this.x = x;
        this.y = y;
        this.puzzle = puzzle;

        img = GetComponent<Image>();
        numberText = GetComponentInChildren<TMP_Text>();
        currentState = TileState.Hidden;
        img.color = Color.white;

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnLeftClick); // left click = normal button click
    }

    public void SetClue(int number)
    {
        if (numberText != null)
            numberText.text = number.ToString();
    }

    void Update()
    {
        if (isRevealedClue) return; // clues cannot be toggled

        // detect right click
        if (Input.GetMouseButtonDown(1))
        {
            // check if mouse is over this tile
            if (RectTransformUtility.RectangleContainsScreenPoint(
                img.rectTransform,
                Input.mousePosition,
                null))
            {
                OnRightClick();
            }
        }
    }

    void OnLeftClick()
    {
        if (isRevealedClue) return;

        if (currentState == TileState.Revealed)
        {
            currentState = TileState.Hidden;
            img.color = Color.white;
        }
        else
        {
            currentState = TileState.Revealed;
            img.color = isSolution ? Color.green : Color.gray;
        }

        puzzle.CheckForWin();
    }

    void OnRightClick()
    {
        if (isRevealedClue) return;

        if (currentState == TileState.Flagged)
        {
            currentState = TileState.Hidden;
            img.color = Color.white;
        }
        else
        {
            currentState = TileState.Flagged;
            img.color = Color.red;
        }

        puzzle.CheckForWin();
    }

    public bool GetPlayerState() => currentState == TileState.Revealed;
}