using UnityEngine;

namespace PathFind
{
    public enum CellViewStatus : int
    {
        None = 1,
        EyeSight = 2,
        AttackSight = 4,
    }

    public class CellView : MonoBehaviour
    {
        private Vector2Int _point;
        private Vector3 viewPosition;


        Renderer renderer;
        Material[] materials;

        static Material eyeSight;

        static Material[] replace = new Material[1];

        private void Start()
        {
            renderer = this.gameObject.GetComponent<Renderer>();
            materials = renderer.materials;
            if (eyeSight == null)
            {
                eyeSight = Resources.Load<Material>("EyeSight");
                replace[0] = eyeSight;
            }
        }

        public void SetPoint(Vector2Int point, Vector3 viewPos)
        {
            _point = point;
            viewPosition = viewPos;
        }

        public Vector2Int GetPoint() => _point;



#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (viewStatus == CellViewStatus.EyeSight)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(viewPosition, 0.35f);
            }
        }
#endif
        private CellViewStatus viewStatus;
        public void SetCellViewStatus(CellViewStatus status)
        {
            viewStatus = status;
            if ((status & CellViewStatus.EyeSight) == CellViewStatus.EyeSight)
            {
                renderer.materials = replace;
            }

            if (status == CellViewStatus.None)
            {
                renderer.materials = materials;
            }
        }
    }
}
