using UnityEngine;
using UnityEngine.UI;

public class Hack : Interactable
{
    [Header("Hack References")]
    public GameObject door;
    public KeypadPuzzle keypadPuzzle;

    public override void Interact(GameObject player)
    {
        if (!PlayerInRange || keypadPuzzle == null || player == null)
            return;

        keypadPuzzle.OpenKeypad(this, player);
    }

    public void UnlockDoor()
    {
        if (door != null)
        {
            Collider2D col = door.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    public void OnKeypadClosed(GameObject player)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.canMove = true;
    }
}