using PathFind;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace PathFind
{
    public class CellSelector : MonoBehaviour
    {
        [SerializeField] private Camera m_camera = null;

        public Action<Vector2Int> OnStartPoint = delegate { };
        public Action<Vector2Int> OnEndPoint = delegate { };


        public static Dictionary<Vector2Int, bool> allowClickSet = new Dictionary<Vector2Int, bool>();

        enum MouseButton
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        private void Update()
        {
#if Test
            test();
#else

            var mouseButton = Input.GetMouseButtonDown(0) ? MouseButton.Left : MouseButton.None;
            if (mouseButton != MouseButton.None)
            {
                //====
                var ray = m_camera.ScreenPointToRay(Input.mousePosition);
                Vector2Int point = RaycastGetPoint(ray);
                bool allowClick = false;
                allowClickSet.TryGetValue(point, out allowClick);
                Debug.Log( $"CellSelector click point：{point},allowClick:{allowClick}");
                if (allowClick == false)
                    return;
                OnEndPoint?.Invoke(point);
                //====
                //var ray = m_camera.ScreenPointToRay(Input.mousePosition);
                //var cell = Raycast(ray);
                //if (cell != null)
                //{
                //    bool allowClick = false;
                //    allowClickSet.TryGetValue(cell.GetPoint(), out allowClick);
                //    if (allowClick == false)
                //        return;

                //    var point = cell.GetPoint();
                //    OnEndPoint?.Invoke(point);
                //}

            }
#endif
        }

        Vector2Int RaycastGetPoint(Ray ray)
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red);

            var result = default(Vector2Int);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                result = Coords.Visualposition2Point(hit.point);
            }
            return result;
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

        private void test()
        {
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
        }
    }
}