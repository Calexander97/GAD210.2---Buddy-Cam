using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberPuzzleSolvable : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 4;
    public int height = 4;

    [Header("Puzzle Settings")]
    public Tile tilePrefab;
    public Transform gridParent;

    [Header("References")]
    public GameObject winPanel;
    public Button redoButton;
    public TMP_Text remainingText;
    private HeroNavAgent2D heroController;

    [Header("Help Panel")]
    public GameObject helpPanel;
    public Button helpCloseButton;

    [Header("Terminal")]
    public GameObject terminalPanel;

    [Header("Puzzle Generation")]
    [Range(1, 100)] public int solutionPercent = 30;
    [Range(1, 8)] public int minCluesPerSolution = 2;
    [Range(0, 16)] public int extraClues = 4;

    private Tile[,] tiles;
    private Action successCallback;
    private int remainingNodes;
    public bool puzzleCompleted { get; private set; } = false;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (redoButton != null)
            redoButton.onClick.AddListener(ResetPuzzle);

        if (helpCloseButton != null)
            helpCloseButton.onClick.AddListener(CloseHelp);
    }

    public void OpenPuzzle(Action success = null)
    {
        // BLOCK ALL CLICKS / MOVEMENT WHILE PUZZLE IS OPEN
        UIBlocker.Instance.uiOpen = true;

        successCallback = success;

        gameObject.SetActive(true);
        winPanel.SetActive(false);

        // Clear any old tiles
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        puzzleCompleted = false;

        // Stop player movement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            heroController = player.GetComponent<HeroNavAgent2D>();
        if (heroController != null)
            heroController.agent.isStopped = true;

        // Generate a new puzzle
        GenerateGrid();
        GenerateSolution();
        PlaceCluesWithSettings();

        remainingNodes = 0;
        foreach (Tile t in tiles)
            if (t.isSolution)
                remainingNodes++;

        UpdateRemainingUI();
    }

    private void UpdateRemainingUI()
    {
        if (remainingText != null)
            remainingText.text = "Nodes Remaining: " + remainingNodes;
    }

    public void DecreaseRemainingNodes()
    {
        remainingNodes--;
        UpdateRemainingUI();
    }

    public void IncreaseRemainingNodes()
    {
        remainingNodes++;
        UpdateRemainingUI();
    }

    public void CheckForWin()
    {
        foreach (Tile t in tiles)
        {
            if (t.isRevealedClue) continue;
            if (t.isSolution != t.GetPlayerState()) return;
        }

        Win();
    }

    private void Win()
    {
        puzzleCompleted = true;
        winPanel.SetActive(true);
        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        SFXManager.Instance.PlaySFX(SFXManager.Instance.puzzleComplete);
        yield return new WaitForSeconds(2f);

        // hide puzzle visuals
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);

        gameObject.SetActive(false);

        // RE-ENABLE PLAYER MOVEMENT
        if (heroController != null)
            heroController.agent.isStopped = false;

        // ppen terminal panel
        if (terminalPanel != null)
        {
            UIBlocker.Instance.uiOpen = true;
            terminalPanel.SetActive(true);

        }

        successCallback?.Invoke();
    }

    private void ResetPuzzle()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        winPanel.SetActive(false);

        GenerateGrid();
        GenerateSolution();
        PlaceCluesWithSettings();

        CountInitialSolutions();
        UpdateRemainingUI();
    }

    private void CountInitialSolutions()
    {
        remainingNodes = 0;
        foreach (Tile t in tiles)
            if (t.isSolution)
                remainingNodes++;

        UpdateRemainingUI();
    }

    void GenerateGrid()
    {
        tiles = new Tile[width, height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Tile t = Instantiate(tilePrefab, gridParent);
                t.Init(x, y, this);
                tiles[x, y] = t;
            }
    }

    void GenerateSolution()
    {
        int totalTiles = width * height;
        int solutionCount = Mathf.CeilToInt(totalTiles * solutionPercent / 100f);
        HashSet<Vector2Int> chosen = new HashSet<Vector2Int>();

        while (chosen.Count < solutionCount)
        {
            int x = UnityEngine.Random.Range(0, width);
            int y = UnityEngine.Random.Range(0, height);
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

        int attempts = 0, maxAttempts = 200;

        while (coverage.Count > 0 && attempts < maxAttempts)
        {
            attempts++;
            List<Vector2Int> uncovered = new List<Vector2Int>();
            foreach (var kvp in coverage)
                if (kvp.Value < minCluesPerSolution) uncovered.Add(kvp.Key);
            if (uncovered.Count == 0) break;

            Vector2Int solTile = uncovered[UnityEngine.Random.Range(0, uncovered.Count)];
            List<Vector2Int> neighbors = GetNeighbours(solTile.x, solTile.y);
            neighbors.Shuffle();

            foreach (var n in neighbors)
            {
                Tile t = tiles[n.x, n.y];
                if (t.isSolution || t.isRevealedClue) continue;

                t.isRevealedClue = true;

                foreach (var nb in GetNeighbours(n.x, n.y))
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

        for (int i = 0; i < extraClues; i++)
        {
            int x = UnityEngine.Random.Range(0, width);
            int y = UnityEngine.Random.Range(0, height);
            Tile t = tiles[x, y];
            if (!t.isSolution && !t.isRevealedClue)
                t.isRevealedClue = true;
        }

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Tile t = tiles[x, y];
                if (t.isRevealedClue)
                    t.SetClue(CountAdjacentSolutionTiles(x, y));
            }
    }

    int CountAdjacentSolutionTiles(int cx, int cy)
    {
        int count = 0;
        foreach (var n in GetNeighbours(cx, cy))
            if (tiles[n.x, n.y].isSolution)
                count++;
        return count;
    }

    List<Vector2Int> GetNeighbours(int cx, int cy)
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

    public void ClosePuzzle()
    {
        // hide UI
        gameObject.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        // re-enable movement
        if (heroController != null)
            heroController.agent.isStopped = false;

        // re-enable game clicks
        UIBlocker.Instance.uiOpen = false;

        Debug.Log("Puzzle closed.");
    }

    public void OpenHelp()
    {
        Debug.Log("HELP BUTTON CLICKED");

        if (helpPanel == null)
        {
            Debug.Log("helpPanel is NULL");
            return;
        }

        helpPanel.SetActive(true);
    }

    public void CloseHelp()
    {
        if (helpPanel == null) return;

        helpPanel.SetActive(false);

        if (heroController != null)
            heroController.agent.isStopped = false;
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}