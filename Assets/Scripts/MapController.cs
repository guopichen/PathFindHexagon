using PathFind;
using System;
using System.Collections.Generic;
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

        public int chunkWidth;
        public int chunkHeight;

        private IMap _map;
        private Dictionary<Vector2Int, CellView> _cellsView;
        private IPathFinder _pathFinder;
        private ICell _cellStart;
        private ICell _cellEnd;

        public GameObject viewer;

        private void Start()
        {
            GameCore.RegistOtherServices<MapController>(this);
            m_cellSelector.OnStartPoint += OnSetPointStart;
            m_cellSelector.OnEndPoint += OnSetPointEnd;

            _pathFinder = new PathFinder();
            _cellsView = new Dictionary<Vector2Int, CellView>();
            Map map =
            new Map(m_mapSizeX, m_mapSizeY);
            _map = map;
            map.DivedIntoChuns(chunkWidth, chunkHeight);
            var mapSize = GetMapSize();

            //chunkGenerat(mapSize);

            normalGenerate(mapSize);
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
            OnEndCellSelect?.Invoke(_cellEnd);
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
            return _map.GetCell(point);
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