using Unity.AI.Navigation;
using UnityEngine;

namespace NavigationUtility.Components
{
    public class BuildNavMesh : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

}
