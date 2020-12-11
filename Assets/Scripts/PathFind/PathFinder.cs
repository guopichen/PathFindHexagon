using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathFind
{
    public class PathFinder : IPathFinder
    {
        public IList<ICell> FindPathOnMap(ICell cellStart, ICell cellEnd, IMap map)
        {
            var findPath = new List<ICell>();
            if (cellStart == null || cellEnd == null)
            {
                Debug.Log("没有找到路！");
                return findPath;
            }

            var mapSize = map.GetMapSize();
            var mapDic = map.GetCells();
            Debug.Log($"=>FindPathOnMap mapSize:{mapSize}");

            //var opened = new HashSet<ICell>();
            //var closed = new HashSet<ICell>();
            var opened = new List<ICell>();
            var closed = new List<ICell>();

            opened.Add(cellStart);

            var moveList = new List<Vector2Int>(8);
            var isFindEnd = false;

            var mapRect = new RectInt(0, 0, mapSize.x, mapSize.y);

            while (opened.Count > 0)
            {
                //var minOpened = opened.OrderBy(x => x.Summ).First();
                opened.Sort((x,y) => {
                    if(x.Summ < y.Summ)
                        return -1;
                    return 1;
                });
                var minOpened = opened[0];
                closed.Add(minOpened);
                opened.Remove(minOpened);

                moveList.Add(Neighbors.oddr_neighbor(minOpened.Point, Neighbors.HexCellDirection.right + 0));
                moveList.Add(Neighbors.oddr_neighbor(minOpened.Point, Neighbors.HexCellDirection.right + 1));
                moveList.Add(Neighbors.oddr_neighbor(minOpened.Point, Neighbors.HexCellDirection.right + 2));
                moveList.Add(Neighbors.oddr_neighbor(minOpened.Point, Neighbors.HexCellDirection.right + 3));
                moveList.Add(Neighbors.oddr_neighbor(minOpened.Point, Neighbors.HexCellDirection.right + 4));
                moveList.Add(Neighbors.oddr_neighbor(minOpened.Point, Neighbors.HexCellDirection.right + 5));

                #region xxx
                //if (minOpened.Point.y % 2 != 0)
                //{
                //    var n1 = new Vector2Int(minOpened.Point.x, minOpened.Point.y - 1);
                //    var n2 = new Vector2Int(minOpened.Point.x + 1, minOpened.Point.y - 1);
                //    var n3 = new Vector2Int(minOpened.Point.x - 1, minOpened.Point.y);
                //    var n4 = new Vector2Int(minOpened.Point.x + 1, minOpened.Point.y);
                //    var n5 = new Vector2Int(minOpened.Point.x, minOpened.Point.y + 1);
                //    var n6 = new Vector2Int(minOpened.Point.x + 1, minOpened.Point.y + 1);
                //    moveList.Add(n1);
                //    moveList.Add(n2);
                //    moveList.Add(n3);
                //    moveList.Add(n4);
                //    moveList.Add(n5);
                //    moveList.Add(n6);
                //}
                //else
                //{
                //    var n1 = new Vector2Int(minOpened.Point.x - 1, minOpened.Point.y - 1);
                //    var n2 = new Vector2Int(minOpened.Point.x, minOpened.Point.y - 1);
                //    var n3 = new Vector2Int(minOpened.Point.x - 1, minOpened.Point.y);
                //    var n4 = new Vector2Int(minOpened.Point.x + 1, minOpened.Point.y);
                //    var n5 = new Vector2Int(minOpened.Point.x - 1, minOpened.Point.y + 1);
                //    var n6 = new Vector2Int(minOpened.Point.x, minOpened.Point.y + 1);
                //    moveList.Add(n1);
                //    moveList.Add(n2);
                //    moveList.Add(n3);
                //    moveList.Add(n4);
                //    moveList.Add(n5);
                //    moveList.Add(n6);
                //}
                #endregion

                for (int i = 0; i < moveList.Count; i++)
                {
                    var movePosition = moveList[i];
                    if (mapRect.Contains(movePosition))
                    {
                        //var element = mapDic[movePosition];
                        var element = map.GetCell(movePosition);
                        if (element != null && closed.Contains(element) == false && element.IsWall == false)
                        {
                            var isOpened = opened.Contains(element);
                            var addDistance = 10;
                            var distance = minOpened.Distance + addDistance;

                            if (isOpened)
                            {
                                if (element.Distance > minOpened.Distance + addDistance)
                                {
                                    element.SetDistance(distance);
                                    element.SetParent(minOpened);
                                }
                            }
                            else
                            {
                                opened.Add(element);
                                element.SetDistance(distance);
                                element.SetParent(minOpened);
                            }

                            //var HeurX = element.Point.x > cellEnd.Point.x ? element.Point.x - cellEnd.Point.x : cellEnd.Point.x - element.Point.x;
                            //var HeurY = element.Point.y > cellEnd.Point.y ? element.Point.y - cellEnd.Point.y : cellEnd.Point.y - element.Point.y;
                            //element.SetHeuristic((int)Math.Sqrt(HeurX * HeurX + HeurY * HeurY) * addDistance);//简单的勾股定理估算
                            element.SetHeuristic(Distance.Cube_distance(Coords.Point_to_Cube(element.Point), Coords.Point_to_Cube(cellEnd.Point)) * addDistance);//基于6边形tile的距离特性估算

                            if (element == cellEnd)
                            {
                                isFindEnd = true;
                            }
                            else
                            {
                                if (!opened.Contains(element))
                                    opened.Add(element);
                            }
                        }
                    }
                }
                moveList.Clear();
                if (isFindEnd)
                {
                    break;
                }
            }

            if (isFindEnd)
            {
                var current = cellEnd;
                findPath.Add(current);
                while (current != cellStart)
                {
                    current = current.Parent;
                    findPath.Add(current);
                }

                findPath.Reverse();
            }

            //Reset
            foreach (var cellPair in mapDic)
            {
                var cell = cellPair.Value;
                cell.SetParent(null);
                cell.SetDistance(0);
                cell.SetWeight(0);
                cell.SetHeuristic(0);
            }

            return findPath;
        }
    }
}


public static class ExtendFunc
{
    public static List<Vector2Int> GetCellNeighbor(this Vector2Int minOpened)
    {
        List<Vector2Int> result = new List<Vector2Int>(8);
        if (minOpened.y % 2 != 0)
        {
            var n1 = new Vector2Int(minOpened.x, minOpened.y - 1);
            var n5 = new Vector2Int(minOpened.x, minOpened.y + 1);
            var n6 = new Vector2Int(minOpened.x + 1, minOpened.y + 1);
            var n2 = new Vector2Int(minOpened.x + 1, minOpened.y - 1);
            var n4 = new Vector2Int(minOpened.x + 1, minOpened.y);
            var n3 = new Vector2Int(minOpened.x - 1, minOpened.y);
            result.Add(n1);
            result.Add(n2);
            result.Add(n3);
            result.Add(n4);
            result.Add(n5);
            result.Add(n6);
        }
        else
        {
            var n2 = new Vector2Int(minOpened.x, minOpened.y - 1);
            var n6 = new Vector2Int(minOpened.x, minOpened.y + 1);
            var n4 = new Vector2Int(minOpened.x + 1, minOpened.y);

            var n1 = new Vector2Int(minOpened.x - 1, minOpened.y - 1);
            var n3 = new Vector2Int(minOpened.x - 1, minOpened.y);
            var n5 = new Vector2Int(minOpened.x - 1, minOpened.y + 1);
            result.Add(n1);
            result.Add(n2);
            result.Add(n3);
            result.Add(n4);
            result.Add(n5);
            result.Add(n6);
        }
        return result;
    }

    public static List<Vector2Int> GetCellRingSides(this Vector2Int point, int R)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        if (point.y % 2 != 0)
        {
            var left_down = new Vector2Int(point.x - R / 2, point.y - R);
            var right_up = new Vector2Int(point.x + (R + 1) / 2, point.y + R);
            var left_up = new Vector2Int(point.x - R, point.y);
            var right_down = new Vector2Int(point.x + R, point.y);
            var up = new Vector2Int(point.x - R / 2, point.y + R);
            var down = new Vector2Int(point.x + (R + 1) / 2, point.y - R);

            result.Add(up);
            result.Add(right_up);
            result.Add(right_down);
            result.Add(down);
            result.Add(left_down);
            result.Add(left_up);


        }
        else
        {
            var left_down = new Vector2Int(point.x - (R + 1) / 2, point.y - R);
            var right_up = new Vector2Int(point.x + (R) / 2, point.y + R);
            var left_up = new Vector2Int(point.x - R, point.y);
            var right_down = new Vector2Int(point.x + R, point.y);
            var up = new Vector2Int(point.x - (R + 1) / 2, point.y + R);
            var down = new Vector2Int(point.x + R / 2, point.y - R);

            result.Add(up);
            result.Add(right_up);
            result.Add(right_down);
            result.Add(down);
            result.Add(left_down);
            result.Add(left_up);



        }


        return result;
    }
}