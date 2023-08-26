using UnityEngine;

    public class PatrolPath : MonoBehaviour
    {
        [Tooltip("The parent object of the enemies that will be assigned to this path on Start.")]
        public GameObject assignedEnemiesParent;

        internal Transform[] pathNodes;//The Nodes making up the path.

        void Start()
        {
            var assignedEnemies =
                assignedEnemiesParent.GetComponentsInChildren<NpcController>();

            foreach (var enemy in assignedEnemies)
                enemy.PatrolPath = this;

            pathNodes = GetComponentsInChildren<Transform>();
        }

        public float GetDistanceToNode(Vector3 origin, int destinationNodeIndex)
        {
            bool noNodeIndex = destinationNodeIndex < 0;
            bool nodeIndexGreaterThanLength = destinationNodeIndex >= pathNodes.Length;
            bool nodeIsNull = pathNodes[destinationNodeIndex] == null;

            if (noNodeIndex || nodeIndexGreaterThanLength || nodeIsNull)
                return -1f;

            return (pathNodes[destinationNodeIndex].position - origin).magnitude;
        }

        public Vector3 GetPositionOfPathNode(int nodeIndex)
        {
            bool noNodeIndex = nodeIndex < 0;
            bool nodeIndexGreaterThanLength = nodeIndex >= pathNodes.Length;
            bool nodeIsNull = pathNodes[nodeIndex] == null;

            if (noNodeIndex || nodeIndexGreaterThanLength || nodeIsNull)
            {
                Debug.LogWarning("Something may be wrong with the patrol path nodes.");
                return Vector3.zero;
            }

            return pathNodes[nodeIndex].position;
        }

        void OnDrawGizmosSelected()
        {
            pathNodes = GetComponentsInChildren<Transform>();

            Gizmos.color = Color.cyan;
            for (int i = 0; i < pathNodes.Length; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex >= pathNodes.Length)
                {
                    nextIndex -= pathNodes.Length;
                }

                Gizmos.DrawLine(pathNodes[i].position, pathNodes[nextIndex].position);
                Gizmos.DrawSphere(pathNodes[i].position, 0.1f);
            }
        }
    }
