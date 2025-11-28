using UnityEngine;

public class PuzzleInteractable : Interactable
{
    [Header("Puzzle Settings")]
    public NumberPuzzleSolvable puzzle;
    public Item rewardItem; // optional

    private HeroNavAgent2D hero;

    private void Awake()
    {
        if (puzzle != null)
        {
            // Register callbacks once
            puzzle.OnPuzzleOpened += () => { gameObject.SetActive(false); };
            puzzle.OnPuzzleClosed += () =>
            {
                gameObject.SetActive(true);
                if (hero != null)
                    hero.agent.isStopped = false;
            };
        }
    }

    public override void Interact(GameObject player)
    {
        if (puzzle == null || player == null) return;

        hero = player.GetComponent<HeroNavAgent2D>();
        if (hero != null)
            hero.agent.isStopped = true;

        puzzle.OpenPuzzle(
            success: () =>
            {
                if (rewardItem != null)
                {
                    Inventory inv = player.GetComponent<Inventory>();
                    if (inv != null)
                        inv.AddItem(rewardItem, 1);
                }

                if (hero != null)
                    hero.agent.isStopped = false;
            },
            fail: () =>
            {
                if (hero != null)
                    hero.agent.isStopped = false;
            }
        );
    }
}