using UnityEngine;

public class PuzzleInteractable : Interactable
{
    public NumberPuzzleSolvable puzzle;
    public Item rewardItem; // optional

    public override void Interact(GameObject player)
    {
        if (puzzle == null || player == null) return;

        HeroNavAgent2D hero = player.GetComponent<HeroNavAgent2D>();
        if (hero != null) hero.agent.isStopped = true;

        puzzle.OpenPuzzle(success: () =>
        {
            if (rewardItem != null)
            {
                Inventory inv = player.GetComponent<Inventory>();
                if (inv != null)
                    inv.AddItem(rewardItem, 1);
            }

            if (hero != null)
                hero.agent.isStopped = false;
        });
    }
}