using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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


        public static bool GetHexCellByRadius(Vector2Int point,int R, ref List<Vector2Int> container)
        {
            if (container == null || R < 0)
                return false;
            switch(R)
            {
                case 0:
                    container.Add(point);
                    return true;
                case 1:
                    container.Add(point);
                    container.AddRange(point.GetCellNeighbor());
                    container = container.Distinct<Vector2Int>().ToList<Vector2Int>();
                    return true;
                default:
                    List<Vector2Int> neighbour = point.GetCellNeighbor();
                    foreach(Vector2Int nearP in neighbour)
                    {
                        GetHexCellByRadius(nearP, R - 1, ref container);
                    }
                    return true;
            }
        }
    }
}