using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    private Vector2 targetPosition;
    private bool moving = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (moving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if ((Vector2)transform.position == targetPosition)
                moving = false;
        }
    }

    public void MoveTo(Vector2 position)
    {
        targetPosition = position;
        moving = true;
    }
}