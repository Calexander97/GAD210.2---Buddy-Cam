using NavMeshPlus.Components;
using UnityEngine;
[RequireComponent(typeof(NavMeshSurface))]
public class BuildNavMeshOnStart : MonoBehaviour
{
    void Start() => GetComponent<NavMeshSurface>().BuildNavMesh();
}
