using UnityEngine;
using UnityEngine.AI;

public class DoorController : MonoBehaviour
{
    public bool isOpen = false;
    public Vector3 openOffset = new Vector3(0, 3, 0);

    private Vector3 closedPos;
    private NavMeshObstacle obstacle;

    void Start()
    {
        closedPos = transform.position;
        obstacle = GetComponent<NavMeshObstacle>();
    }

    public void SetOpen(bool open)
    {
        isOpen = open;

        transform.position = open ? closedPos + openOffset : closedPos;

        if (obstacle != null)
            obstacle.carving = !open; // carve when closed
    }

    public void Toggle()
    {
        SetOpen(!isOpen);
    }
}
