using UnityEngine;


namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        const float waypointGizmosRadius = 0.3f;
        private void OnDrawGizmos()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                int j = GetNextIndex(i);
                Gizmos.DrawSphere(GetPosition(i), waypointGizmosRadius);
                Gizmos.DrawLine(GetPosition(i), GetPosition(j));

            }
        }

        public int GetNextIndex(int i)
        {
            if (i + 1 >= transform.childCount)
            {
                return 0;
            }
            return i + 1;
        }

        public Vector3 GetPosition(int i)
        {
            return transform.GetChild(i).position;
        }
    }
}

