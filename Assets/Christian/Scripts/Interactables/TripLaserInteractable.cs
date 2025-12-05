using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TripLaserInteractable : Interactable
{
    [Header("References")]
    [Tooltip("Parent TripLaser on the root object.")]
    public TripLaser tripLaser;
    [Tooltip("Optional: the puzzle you want to open on interact.")]
    public NumberPuzzleSolvable puzzle;
    [Tooltip("Optional reward to grant on successful hack.")]
    public Item rewardItem;

    [Header("Setup")]
    [Tooltip("If InteractionManager.player is not set, we fall back to this tag.")]
    public string playerTag = "Player";

    InteractionManager im;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    void Awake()
    {
        if (!tripLaser) tripLaser = GetComponentInParent<TripLaser>();
        im = FindFirstObjectByType<InteractionManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!im) return;

        bool isPlayer =
            (im.player && other.gameObject == im.player) ||
            (!im.player && other.CompareTag(playerTag));

        if (isPlayer) im.SetCurrentInteractable(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!im) return;

        bool isPlayer =
            (im.player && other.gameObject == im.player) ||
            (!im.player && other.CompareTag(playerTag));

        if (isPlayer) im.ClearCurrentInteractable(this);
    }

    public override void Interact(GameObject player)
    {
        if (!tripLaser) return;

        // If already disarmed, optionally just show the terminal (if puzzle has one)
        if (!tripLaser.armed)
        {
            if (puzzle && puzzle.terminalPanel) puzzle.terminalPanel.SetActive(true);
            return;
        }

        if (!puzzle) return; // no hacking available

        // Pause the hero while the puzzle is open
        var hero = player ? player.GetComponent<HeroNavAgent2D>() : null;
        if (hero) hero.agent.isStopped = true;

        if (puzzle.puzzleCompleted)
        {
            if (puzzle.terminalPanel) puzzle.terminalPanel.SetActive(true);
            if (hero) hero.agent.isStopped = false;
            tripLaser.DisarmPermanently();
            return;
        }

        puzzle.OpenPuzzle(success: () =>
        {
            if (rewardItem && player)
            {
                var inv = player.GetComponent<Inventory>();
                if (inv) inv.AddItem(rewardItem, 1);
            }

            if (hero) hero.agent.isStopped = false;
            tripLaser.DisarmPermanently();
        });
    }
}
