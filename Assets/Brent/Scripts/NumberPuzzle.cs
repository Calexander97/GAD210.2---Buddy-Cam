using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems

public class NumberPuzzle : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public Tile tilePrefab;
    public Transform gridParent;

    private Tile[,] tiles;

    void Start()
    {
        GenerateGrid();
        AssignSolutionPattern();
        AssignClues();
    }

    void GenerateGrid()
    {
        tiles = new Tile[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile t = Instantiate(tilePrefab, gridParent);
                t.Init(x, y, this); // pass the NumberPuzzleSolvable reference
                tiles[x, y] = t;

                // Left-click
                var btn = t.GetComponent<Button>();
                btn.onClick.AddListener(() => t.OnClick());

                // Right-click for flag
                EventTrigger trigger = t.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entry.callback.AddListener((data) =>
                {
                    PointerEventData ped = (PointerEventData)data;
                    if (ped.button == PointerEventData.InputButton.Right)
                        t.OnClick(flag: true);
                });
                trigger.triggers.Add(entry);
            }
        }
    }

    void AssignSolutionPattern()
    {
        // random solution
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tiles[x, y].isSolution = Random.value > 0.65f;
            }
        }
    }

    void AssignClues()
    {
        // randomly assign tiles to be clue tiles
        int clueCount = (width * height) / 4;

        for (int i = 0; i < clueCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            Tile t = tiles[x, y];

            int clueNumber = CountAdjacentSolutionTiles(x, y);
            t.SetClue(clueNumber);
        }
    }

    int CountAdjacentSolutionTiles(int cx, int cy)
    {
        int count = 0;

        for (int y = cy - 1; y <= cy + 1; y++)
        {
            for (int x = cx - 1; x <= cx + 1; x++)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                    continue;
                if (x == cx && y == cy)
                    continue;

                if (tiles[x, y].isSolution)
                    count++;
            }
        }
        return count;
    }

    public void CheckForWin()
    {
        foreach (Tile t in tiles)
        {
            if (t.isRevealedClue)
                continue;

            if (t.isSolution != t.GetPlayerState())
                return;
        }

        Debug.Log("Puzzle solved!");
    }
}