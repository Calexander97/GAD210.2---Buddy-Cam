using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NumberPuzzleSolvable : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 4;
    public int height = 4;
    [Range(1, 100)] public int solutionPercent = 30;

    [Header("Puzzle Settings")]
    [Range(1, 8)] public int minCluesPerSolution = 2;
    [Range(0, 16)] public int extraClues = 4;

    [Header("References")]
    public Tile tilePrefab;
    public Transform gridParent;

    [Header("UI")]
    public GameObject winPanel;
    public TMP_Text winText;

    private Tile[,] tiles;

    void Start()
    {
        GenerateGrid();
        GenerateSolution();
        PlaceCluesWithSettings();
    }

    void GenerateGrid()
    {
        tiles = new Tile[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile t = Instantiate(tilePrefab, gridParent);
                t.Init(x, y);
                tiles[x, y] = t;

                // Left-click = reveal
                var btn = t.GetComponent<Button>();
                btn.onClick.AddListener(() => t.OnClick(false));

                // Right-click = flag
                EventTrigger trigger = t.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entry.callback.AddListener((data) =>
                {
                    PointerEventData ped = (PointerEventData)data;
                    if (ped.button == PointerEventData.InputButton.Right)
                        t.OnClick(true);
                });
                trigger.triggers.Add(entry);
            }
        }
    }

    void GenerateSolution()
    {
        int totalTiles = width * height;
        int solutionCount = Mathf.CeilToInt(totalTiles * solutionPercent / 100f);

        HashSet<Vector2Int> chosen = new HashSet<Vector2Int>();
        while (chosen.Count < solutionCount)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector2Int pos = new Vector2Int(x, y);
            if (!chosen.Contains(pos))
            {
                chosen.Add(pos);
                tiles[x, y].isSolution = true;
            }
        }
    }

    void PlaceCluesWithSettings()
    {
        Dictionary<Vector2Int, int> coverage = new Dictionary<Vector2Int, int>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (tiles[x, y].isSolution)
                    coverage[new Vector2Int(x, y)] = 0;

        int attempts = 0;
        int maxAttempts = 200;

        while (coverage.Count > 0 && attempts < maxAttempts)
        {
            attempts++;
            List<Vector2Int> uncovered = new List<Vector2Int>();
            foreach (var kvp in coverage)
                if (kvp.Value < minCluesPerSolution) uncovered.Add(kvp.Key);
            if (uncovered.Count == 0) break;

            Vector2Int solTile = uncovered[Random.Range(0, uncovered.Count)];
            List<Vector2Int> neighbors = GetNeighbors(solTile.x, solTile.y);
            neighbors.Shuffle();

            foreach (var n in neighbors)
            {
                Tile t = tiles[n.x, n.y];
                if (t.isSolution || t.isRevealedClue) continue;

                // Assign as clue
                t.isRevealedClue = true;
                t.SetClue(CountAdjacentSolutionTiles(n.x, n.y));

                foreach (var nb in GetNeighbors(n.x, n.y))
                    if (coverage.ContainsKey(nb))
                        coverage[nb]++;

                break;
            }

            List<Vector2Int> done = new List<Vector2Int>();
            foreach (var kvp in coverage)
                if (kvp.Value >= minCluesPerSolution)
                    done.Add(kvp.Key);
            foreach (var d in done)
                coverage.Remove(d);
        }

        // Extra random clues
        for (int i = 0; i < extraClues; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Tile t = tiles[x, y];
            if (!t.isSolution && !t.isRevealedClue)
            {
                t.isRevealedClue = true;
                t.SetClue(CountAdjacentSolutionTiles(x, y));
            }
        }
    }

    int CountAdjacentSolutionTiles(int cx, int cy)
    {
        int count = 0;
        foreach (var n in GetNeighbors(cx, cy))
            if (tiles[n.x, n.y].isSolution)
                count++;
        return count;
    }

    List<Vector2Int> GetNeighbors(int cx, int cy)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        for (int y = cy - 1; y <= cy + 1; y++)
            for (int x = cx - 1; x <= cx + 1; x++)
            {
                if (x < 0 || y < 0 || x >= width || y >= height) continue;
                if (x == cx && y == cy) continue;
                neighbors.Add(new Vector2Int(x, y));
            }
        return neighbors;
    }

    public void CheckForWin()
    {
        foreach (Tile t in tiles)
        {
            if (t.isRevealedClue) continue;
            if (t.isSolution != t.GetPlayerState()) return;
        }
        Debug.Log("Puzzle solved!");
        OnPuzzleSolved();
    }

    private void OnPuzzleSolved()
    {
        // Show the win panel
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winText != null)
                winText.text = "Puzzle Solved!";
        }

        // Hide the puzzle grid
        if (gridParent != null)
            gridParent.gameObject.SetActive(false);
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}