
using System.Collections.Generic;
using UnityEngine;

namespace PathFind
{
    public class Map : IMap
    {
        public static bool randowWall = false;


        private Dictionary<Vector2Int, ICell> _cells;
        private Vector2Int _mapSize;
        public Map(int sizeX, int sizeY)
        {
            _cells = new Dictionary<Vector2Int, ICell>();

            _mapSize = new Vector2Int(sizeX, sizeY);
#if !DEFAULT
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    var point = new Vector2Int(x, y);
                    var cell = new Cell(point);
                    if (randowWall)
                    {
                        if (Random.Range(0, 100) > 70) cell.SetIsWall(true);
                    }
                    _cells[point] = cell;
                }
            }
#else
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (y % 2 == 0)
                    {
                        var point = new Vector2Int(2 * x, y);
                        var cell = new Cell(point);
                        _cells[point] = cell;
                    }
                    else
                    {
                        var point = new Vector2Int(2 * x + 1, y);
                        var cell = new Cell(point);
                        _cells[point] = cell;
                    }
                }
            }
#endif



        }

        public Vector2Int GetMapSize() => _mapSize;

        public IDictionary<Vector2Int, ICell> GetCells()
        {
            return _cells;
        }

        public void SetCell(ICell cell)
        {

        }

        public ICell GetCell(Vector2Int point)
        {
            return _cells[point];
        }

    }
}