using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
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
    }

    public void SetClue(int number)
    {
        if (numberText != null)
            numberText.text = number.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRevealedClue) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (currentState == TileState.Revealed)
            {
                currentState = TileState.Hidden;
                img.color = Color.white;

                // Left-click removed green - increase remaining nodes if it was previously revealed
                puzzle.IncreaseRemainingNodes();
            }
            else
            {
                currentState = TileState.Revealed;
                img.color = Color.green;

                // Left-click added - decrease remaining nodes
                puzzle.DecreaseRemainingNodes();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // right-click - toggle flagged
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

        puzzle.CheckForWin();
    }

    public bool GetPlayerState() => currentState == TileState.Revealed;
}