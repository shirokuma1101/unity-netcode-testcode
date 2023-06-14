using Unity.AI.Navigation;
using UnityEngine;

public class BuildNavMesh : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
