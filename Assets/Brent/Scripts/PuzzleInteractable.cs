using UnityEngine;
using System;

public class PuzzleInteractable : Interactable
{
    [Header("Puzzle Settings")]
    public SliderPuzzle puzzle;
    public Item rewardItem;

    public override void Interact(GameObject player)
    {
        if (puzzle == null || player == null) return;

        // stop hero movement while puzzle is active
        HeroNavAgent2D hero = player.GetComponent<HeroNavAgent2D>();
        if (hero != null)
            hero.agent.isStopped = true;

        // start the puzzle
        puzzle.StartPuzzle(
            success: () =>
            {
                // give reward
                Inventory inv = player.GetComponent<Inventory>();
                if (inv != null)
                    inv.AddItem(rewardItem, 1);

                // re-enable movement
                if (hero != null)
                    hero.agent.isStopped = false;

                Debug.Log("Puzzle success! Item awarded.");
            },
            fail: () =>
            {
                // re-enable movement
                if (hero != null)
                    hero.agent.isStopped = false;

                Debug.Log("Puzzle failed!");
            }
        );
    }
}