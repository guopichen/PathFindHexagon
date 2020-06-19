using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PathFind
{
    public static class HexCoords
    {
        public static Vector3 GetHexVisualCoords(Vector2Int point)
        {
            return GetHexVisualCoords(point, GameCore.GetRegistServices<MapController>().GetMapSize());
        }


        public const float CellHeight = 0.88f;

        public static Vector3 GetHexVisualCoords(Vector2Int point, Vector2Int mapSize)
        {

#if !DEFAULT
            var shift = point.y % 2 == 0 ? 0 : 0.5f;
            var x = point.x + shift - ((float)mapSize.x / 2) + 0.25f;
            var y = point.y * CellHeight - (mapSize.y * CellHeight / 2f);
            return new Vector3(x, 0, y);
#else

            float x = 0;
            float y = 0;
            float z = 0;

            x = point.x + point.y * 0.5f;
            z = point.y * 0.866f;


            return new Vector3(x, y, z);
            
#endif
        }


        public static bool GetHexCellByRadius(Vector2Int point, int R, ref List<Vector2Int> container)
        {


            if (container == null || R < 0)
                return false;
            //container = point.GetCellRingSides(R);


            return true;
            switch (R)
            {
                case 0:
                    container.Add(point);
                    return true;
                case 1:
                    container.Add(point);
                    container.AddRange(point.GetCellNeighbor());
                    //container = container.Distinct<Vector2Int>().ToList<Vector2Int>();
                    return true;
                default:
                    List<Vector2Int> neighbour = point.GetCellNeighbor();
                    foreach (Vector2Int nearP in neighbour)
                    {
                        GetHexCellByRadius(nearP, R - 1, ref container);
                    }
                    return true;
            }
        }

        public static List<Vector2Int> aa(Vector2Int point, int R, IMap map)
        {
            List<Vector2Int> b = point.GetCellRingSides(R);

            IDictionary<Vector2Int, ICell> set = map.GetCells();
            int minX, maxX, minY, maxY;
            b.ForEach((x) => { x = x - point; });

            for (int i = 0; i < b.Count; i++)
                Debug.Log(b[i]);

            Vector2Int v1 = b[1];
            Vector2Int v2 = b[2];
            Vector2Int v3 = b[3];
            Vector2Int v4 = b[4];
            Vector2Int v5 = b[5];
            Vector2Int v6 = b[0];
            maxX = Mathf.Max(v1.x, v2.x, v3.x, v4.x, v5.x, v6.x);
            minX = Mathf.Min(v1.x, v2.x, v3.x, v4.x, v5.x, v6.x);

            minY = Mathf.Min(v1.y, v2.y, v3.y, v4.y, v5.y, v6.y);
            maxY = Mathf.Max(v1.y, v2.y, v3.y, v4.y, v5.y, v6.y);

            int minAbsX = Mathf.Min(Mathf.Abs(minX), Mathf.Abs(maxX));
            int minAbsY = Mathf.Min(Mathf.Abs(minY), Mathf.Abs(maxY));


            Func<int, float> v1_v2 = (x) =>
            {
                float k = (v2.y - v1.y) / (1f * (v2.x - v1.x));
                return k * (x - v1.x) - v1.y + v1.x * k;
            };

            Func<int, float> v2_v3 = (x) =>
            {
                float k = (v3.y - v2.y) / (1f * (v3.x - v2.x));
                return k * (x - v2.x) + v2.y;
            };

            Func<int, float> v4_v5 = (x) =>
            {
                float k = (v5.y - v4.y) / (1f * (v5.x - v4.x));
                return k * (x - v4.x) + v4.y;
            };

            Func<int, float> v5_v6 = (x) =>
            {
                float k = (v6.y - v5.y) / (1f * (v6.x - v5.x));
                return k * (x - v5.x) + v5.y;
            };

            b.Clear();
            b.AddRange(set.Keys.Select<Vector2Int, Vector2Int>((x) =>
            {
                Vector2Int delta = x - point;

                if (Mathf.Abs(delta.x) <= minAbsX)
                {
                    if (Mathf.Abs(delta.y) < minAbsY)
                        return x;
                }
                else
                {
                    if (delta.x > 0)
                    {
                        if (delta.y <= v1_v2(delta.x) && delta.y >= v2_v3(delta.x))
                            return x;
                    }
                    else
                    {
                        if (delta.y <= v5_v6(delta.x) && delta.y >= v4_v5(delta.x))
                            return x;
                    }
                }
                return point;
            }));
            return b;
        }
    }
}