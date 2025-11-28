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
        btn.onClick.AddListener(() => OnClick(false));
    }

    public void SetClue(int number)
    {
        if (numberText != null)
            numberText.text = number.ToString();
    }

    public void OnClick(bool flag = false)
    {
        if (isRevealedClue) return;

        if (flag)
        {
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
        }
        else
        {
            currentState = TileState.Revealed;
            img.color = isSolution ? Color.green : Color.gray;
        }

        puzzle.CheckForWin();
    }

    public bool GetPlayerState() => currentState == TileState.Revealed;
}