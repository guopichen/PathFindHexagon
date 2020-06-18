using PathFind;
using System;
using UnityEngine;

namespace PathFind
{
    public class CellSelector : MonoBehaviour
    {
        [SerializeField] private Camera m_camera;

        public Action<Vector2Int> OnStartPoint = delegate { };
        public Action<Vector2Int> OnEndPoint = delegate { };

        enum MouseButton
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        private void Update()
        {
#if Test
            var mouseButton = Input.GetMouseButtonDown(0) ? MouseButton.Left : Input.GetMouseButtonDown(1) ? MouseButton.Right : MouseButton.None;
            if (mouseButton != MouseButton.None)
            {
                var ray = m_camera.ScreenPointToRay(Input.mousePosition);
                var cell = Raycast(ray);
                if (cell != null)
                {
                    var point = cell.GetPoint();
                    if (mouseButton == MouseButton.Left) OnStartPoint?.Invoke(point);
                    if (mouseButton == MouseButton.Right) OnEndPoint?.Invoke(point);
                }
            }
#else
            var mouseButton = Input.GetMouseButtonDown(0) ? MouseButton.Left : MouseButton.None;
            if (mouseButton != MouseButton.None)
            {
                var ray = m_camera.ScreenPointToRay(Input.mousePosition);
                var cell = Raycast(ray);
                if (cell != null)
                {
                    var point = cell.GetPoint();
                    OnEndPoint?.Invoke(point);
                }
            }
#endif


        }

        private CellView Raycast(Ray ray)
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red);

            var result = default(CellView);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                result = hit.transform.GetComponent<CellView>();
            }
            return result;
        }


        public void SetStartPointManually(Vector2Int point)
        {
            OnStartPoint?.Invoke(point);
        }

    }
}