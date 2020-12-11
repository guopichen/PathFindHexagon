
using System.Collections.Generic;
using UnityEngine;

namespace PathFind
{
    public class Map : IMap
    {
        public static bool randowWall = false;


        private Dictionary<Vector2Int, ICell> _cells;
        private Vector2Int _mapSize;

        public Map(List<Vector2Int> terrainPosDatas, int sizeX, int sizeY)
        {
            _cells = new Dictionary<Vector2Int, ICell>();
            _mapSize = new Vector2Int(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    var point = new Vector2Int(x, y);
                    var cell = new Cell(point);
                    _cells[point] = cell;
                }
            }
            //    foreach (var terrainPosData in terrainPosDatas)
            //{
            //    var point = new Vector2Int(terrainPosData.x, terrainPosData.y);
            //    var cell = new Cell(point);
            //    //if (randowWall)
            //    //{
            //    //    if (Random.Range(0, 100) > 70) cell.SetIsWall(true);
            //    //}
            //    _cells[point] = cell;
            //}

        }

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
            ICell cell;
            _cells.TryGetValue(point, out cell);
            return cell;
        }


        int chunkWidth;
        int chunkHeight;
        public void DivedIntoChuns(int w, int h)
        {
            //根据w h 将整体的区域按照逻辑关系划分成小块
            chunkWidth = w;
            chunkHeight = h;
            MapChunk.Height = h;
            MapChunk.Width = w;
            foreach (var kvp in _cells)
            {
                Vector2Int point = kvp.Key;
                MapChunk chunk = GetMapChunk(point);
                chunk?.AddCell(point, kvp.Value);
            }

        }

        MapChunk GetMapChunk(Vector2Int s)
        {
            int x = s.x / this.chunkWidth;
            int y = s.y / this.chunkHeight;
            Vector2Int chunkPos = new Vector2Int(x, y);
            MapChunk chunk = null;
            if (!chunkPos2chunk.ContainsKey(chunkPos))
            {
                chunk = new MapChunk(chunkPos);
                chunkPos2chunk.Add(chunkPos, chunk);
            }
            chunk = chunkPos2chunk[chunkPos];


            return chunk;
        }

        public IDictionary<Vector2Int, MapChunk> GetChunks()
        {
            return chunkPos2chunk;
        }

        Dictionary<Vector2Int, MapChunk> chunkPos2chunk = new Dictionary<Vector2Int, MapChunk>();

    }

    public class MapChunk
    {
        public GameObject go;
        public Dictionary<Vector2Int, ICell> _cells;
        Vector2 position;
        public static int Width;
        public static int Height;

        GameObject betterUnderStanding;
        public MapChunk( Vector2Int chunkpos)
        {
            position = chunkpos;
            _cells = new Dictionary<Vector2Int, ICell>();
            go = new GameObject("chunk :" + chunkpos);
            go.SetActive(false);

        }

        public void AddCell(Vector2Int point, ICell cell)
        {
            _cells.Add(point, cell);
        }

        bool visible = false;

        internal bool IsVisible()
        {
            return visible;
        }

        internal void SetVisible(bool v)
        {
            visible = v;
            go.SetActive(v);
            if(v)
            {
                betterUnderStanding.GetComponent<Renderer>().material.SetColor("_Color",Color.green);
            }
            else
            {
                betterUnderStanding.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
        }

        public void UpdateMapChunk(Vector2 viewerPosition, Vector2Int maxViewDst)
        {
            //float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            float viewerDstFromNearestEdge = (bounds.SqrDistance(viewerPosition));

            bool visible = viewerDstFromNearestEdge <= maxViewDst.magnitude;
            SetVisible(visible);
        }


        public Bounds bounds;
        Vector2Int center;
        public void GenerateBounds()
        {
            int cnt = _cells.Count;
            if (cnt == 0)
                return;
            
            foreach(Vector2Int v in _cells.Keys)
            {
                center += v;
            }
            center.x /= cnt;
            center.y /= cnt;
            bounds = new Bounds(new Vector2(center.x,center.y), new Vector2(Width,Height));
#if UNITY_EDITOR
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = position.ToString();
            BoxCollider box = go.AddComponent<BoxCollider>();
            go.transform.position = bounds.center;
            betterUnderStanding = go;
            go.transform.SetParent(GameObject.Find("Directional Light").transform);
            //box.center = bounds.center;
            box.size = bounds.size;
#endif
            
        }

        public Vector2Int GetCenter()
        {
            return center;
        }
    }

}