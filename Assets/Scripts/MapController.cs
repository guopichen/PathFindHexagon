using Newtonsoft.Json;
using PathFind;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PathFind
{
    public class MapController : MonoBehaviour
    {
        public Action<ICell> OnStartCellSelect = delegate { };
        public Action<ICell> OnEndCellSelect = delegate { };
        public Action<IList<ICell>> OnPathFind = delegate { };

        [SerializeField] private CellSelector m_cellSelector = null;
        [SerializeField] private CellAssets m_prefabs = null;
        [SerializeField] private int m_mapSizeX = 3;
        [SerializeField] private int m_mapSizeY = 3;

        [Header("地图的唯一ID")]
        public string MapID;

        public int chunkWidth;
        public int chunkHeight;

        private IMap _map;
        private Dictionary<Vector2Int, CellView> _cellsView;
        private IPathFinder _pathFinder;
        private ICell _cellStart;
        private ICell _cellEnd;

        public GameObject viewer;

        Map CreateMap()
        {
            Map map = null;
            ClientTerrainExportSet mapDataSet = LoadCustomData();
            if (mapDataSet==null)
            {
                map = new Map(m_mapSizeX, m_mapSizeY);
            }
            else
            {
                List<Vector2Int> terrainPosDatas = new List<Vector2Int>(mapDataSet.terrain.Count);
                foreach (var terrainDatain in mapDataSet.terrain)
                {
                    if (terrainDatain.x > m_mapSizeX)
                        m_mapSizeX = terrainDatain.x;
                    if (terrainDatain.y > m_mapSizeY)
                        m_mapSizeY = terrainDatain.y;
                    terrainPosDatas.Add(new Vector2Int(terrainDatain.x, terrainDatain.y));
                }
                map = new Map(terrainPosDatas, m_mapSizeX, m_mapSizeY);
            }
            map.DivedIntoChuns(chunkWidth, chunkHeight);
            return map;
        }

        private void Start()
        {
            GameCore.RegistOtherServices<MapController>(this);
            m_cellSelector.OnStartPoint += OnSetPointStart;
            m_cellSelector.OnEndPoint += OnSetPointEnd;

            _pathFinder = new PathFinder();
            _cellsView = new Dictionary<Vector2Int, CellView>();

            _map = CreateMap();
            var mapSize = GetMapSize();

            //chunkGenerat(mapSize);
            normalGenerate(mapSize);
        }

        private ClientTerrainExportSet LoadCustomData()
        {
            if (!File.Exists(GetCustomDataPath()))
            {
                Debug.LogWarning("没有该地图文件：" + MapID);
                return null;
            }
            string str = File.ReadAllText(GetCustomDataPath());
            //
            Debug.Log("地图文件数据：" + str);
            ClientTerrainExportSet mapDataSet = JsonConvert.DeserializeObject<ClientTerrainExportSet>(str);
            Debug.Log("地图数据：" + JsonConvert.SerializeObject(mapDataSet));
            return mapDataSet;

        }

        public string GetCustomDataPath()
        {
            return dirName() + getFileName();
        }

        string dirName()
        {
            return Application.dataPath + "\\..\\ExportTileMap\\DataGen\\";
        }
        string getFileName()
        {
            return "Map_" + MapID + ".txt";
        }

        private void chunkGenerat(Vector2Int mapSize)
        {
            var chunks = _map.GetChunks();
            foreach (KeyValuePair<Vector2Int, MapChunk> chunk in chunks)
            {
                var cells = chunk.Value._cells;
                chunk.Value.GenerateBounds();
                chunk.Value.go.transform.SetParent(transform);
                chunk.Value.go.transform.localPosition = Vector3.zero;

                foreach (var cellPair in cells)
                {
                    var point = cellPair.Key;
                    var cell = cellPair.Value;

                    var prefabItem = m_prefabs.GetRandomPrefab(!cell.IsWall);
                    var position = HexCoords.GetHexVisualCoords(point, mapSize);
                    var go = Instantiate(prefabItem.Prefab, position, Quaternion.identity);
                    go.transform.SetParent(chunk.Value.go.transform);
                    go.name += cell.Col + ":" + cell.Row;
                    var cellView = go.GetComponent<CellView>();
                    if (cellView == null)
                        cellView = go.AddComponent<CellView>();
                    cellView.SetPoint(point, position);
                    _cellsView[point] = cellView;
                }

            }
        }

        private void normalGenerate(Vector2Int mapSize)
        {
            var cells = _map.GetCells();
            foreach (var cellPair in cells)
            {
                var point = cellPair.Key;
                var cell = cellPair.Value;

                var prefabItem = m_prefabs.GetRandomPrefab(!cell.IsWall);
                //var position = HexCoords.GetHexVisualCoords(point, mapSize);
                Vector3 position = Coords.PointToVisualPosition(point);
                var go = Instantiate(prefabItem.Prefab, position, Quaternion.identity);
                go.transform.SetParent(transform);
                go.name += cell.Col + ":" + cell.Row;
                var cellView = go.GetComponent<CellView>();
                if (cellView == null)
                    cellView = go.AddComponent<CellView>();
                cellView.SetPoint(point, position);
                _cellsView[point] = cellView;
            }
        }

        public Vector2Int GetMapSize() => new Vector2Int(m_mapSizeX, m_mapSizeY);

        void OnSetPointStart(Vector2Int point)
        {
            _cellStart = _map.GetCell(point);
            OnStartCellSelect?.Invoke(_cellStart);
#if TEST
            Calculate();
#endif
        }

        void OnSetPointEnd(Vector2Int point)
        {
            _cellEnd = _map.GetCell(point);
            if(_cellEnd != null)
            {
                Debug.Log("OnSetPointEnd: " + point);
                OnEndCellSelect?.Invoke(_cellEnd);
            }

            //Calculate();
        }

        void Calculate()
        {
            var path = _pathFinder.FindPathOnMap(_cellStart, _cellEnd, _map);
            OnPathFind?.Invoke(path);
        }

        public IList<ICell> CalculatePath()
        {
            var path = _pathFinder.FindPathOnMap(_cellStart, _cellEnd, _map);
            if(_cellEnd != null)
            {
                Debug.Log($"==>CalculatePath start:{_cellStart.Point},end:{_cellEnd.Point},cnt:{path.Count}");
            }
            return path;
        }


        private void OnDestroy()
        {
            m_cellSelector.OnStartPoint -= OnSetPointStart;
            m_cellSelector.OnEndPoint -= OnSetPointEnd;
        }

        public CellView GetCellView(Vector2Int key)
        {
            if (_cellsView.TryGetValue(key, out CellView view))
            {
                return view;
            }
            return null;
        }


        public void SetStartPoint(Vector2Int start)
        {
            m_cellSelector.SetStartPointManually(start);
        }

        public IMap GetMap()
        {
            return _map;
        }

        public ICell GetRandomCell()
        {
            Vector2Int size = _map.GetMapSize();
            Vector2Int point = new Vector2Int(UnityEngine.Random.Range(0, size.x), UnityEngine.Random.Range(0, size.y));
            ICell cell = _map.GetCell(point);
            if ( null == cell)
            {
                // 可能有空的cell，递归随机
                return GetRandomCell();
            }
            return cell;
        }

        public IPathFinder GetPathFinder()
        {
            return _pathFinder;
        }

        public void QuestNewPath()
        {
            Calculate();
        }

        private void OnDrawGizmos()
        {
            if (_map != null)
            {

                foreach (KeyValuePair<Vector2Int, MapChunk> chunk in _map.GetChunks())
                {
                    Bounds bounds = chunk.Value.bounds;
                    if (bounds != null)
                        Gizmos.DrawCube(bounds.center, bounds.size);
                }
            }
        }
    }
}