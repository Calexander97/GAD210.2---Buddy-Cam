using UnityEngine;

public class PuzzleInteractable : Interactable
{
    [Header("Puzzle Settings")]
    public NumberPuzzleSolvable puzzle;
    public Item rewardItem; // optional

    public override void Interact(GameObject player)
    {
        if (puzzle == null || player == null) return;

        HeroNavAgent2D hero = player.GetComponent<HeroNavAgent2D>();
        if (hero != null)
            hero.agent.isStopped = true;

        // Puzzle events
        puzzle.OnPuzzleOpened += () => { gameObject.SetActive(false); };
        puzzle.OnPuzzleClosed += () =>
        {
            gameObject.SetActive(true);

            // Re-enable movement only after puzzle is fully hidden
            if (hero != null)
                hero.agent.isStopped = false;
        };

        puzzle.OpenPuzzle(
            success: () =>
            {
                if (rewardItem != null)
                {
                    Inventory inv = player.GetComponent<Inventory>();
                    if (inv != null)
                        inv.AddItem(rewardItem, 1);
                }

                // hero movement will be re-enabled in OnPuzzleClosed
            },
            fail: () =>
            {
                // hero movement will be re-enabled in OnPuzzleClosed
            }
        );
    }
}