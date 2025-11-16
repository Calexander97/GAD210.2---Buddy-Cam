using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using NavMeshPlus.Components;

public class Hack : Interactable
{
    [Header("Hack References")]
    public GameObject door;
    public KeypadPuzzle keypadPuzzle;

    private NavMeshObstacle doorObstacle;

    private void Awake()
    {
        if (door != null)
            doorObstacle = door.GetComponent<NavMeshObstacle>();
    }

    public override void Interact(GameObject player)
    {
        if (!PlayerInRange || keypadPuzzle == null || player == null) return;

        keypadPuzzle.OpenKeypad(this, player);
    }

    public void UnlockDoor()
    {
        if (door == null) return;

        // disable visuals and colliders
        var col2D = door.GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = false;
        var col3D = door.GetComponent<Collider>();
        if (col3D != null) col3D.enabled = false;

        var sr = door.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        var mr = door.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        // disable obstacle (optional)
        var obstacle = door.GetComponent<NavMeshObstacle>();
        if (obstacle != null)
            obstacle.enabled = false;

        // Rebuild NavMesh at runtime so the agent can walk through
        NavMeshSurface surface = FindAnyObjectByType<NavMeshSurface>();
        if (surface != null)
            surface.BuildNavMesh();

        door.SetActive(false);
    }

    public void OnKeypadClosed(GameObject player)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.canMove = true;
    }
}