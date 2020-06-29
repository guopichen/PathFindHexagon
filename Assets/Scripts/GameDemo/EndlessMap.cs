using PathFind;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessMap : MonoBehaviour
{
    public Vector2Int maxViewDst = new Vector2Int(9, 13);
    public Vector2Int betterView = Vector2Int.zero;//用于补充视野填充
    public Rect viewRect;//用于修正误差
    Vector2Int chunksVisibleInViewDst;
    Vector2Int chunkSize;
    public Transform viewer;
    MapController controller;
    void OnEnable()
    {
        controller = this.GetComponent<MapController>();
        if (controller)
        {
            float factorX = 1;
            float factorY = 1f;
            chunkSize.x = Mathf.RoundToInt(controller.chunkWidth * factorX);
            chunkSize.y = Mathf.RoundToInt(controller.chunkHeight * factorY);


            chunksVisibleInViewDst.x = Mathf.RoundToInt(maxViewDst.x / (chunkSize.x));
            chunksVisibleInViewDst.y = Mathf.RoundToInt(maxViewDst.y / (chunkSize.y));
        }
    }
    Vector2 viewerPosition_inChunkSpace;
    void Update()
    {
        if (controller == null || GameEntityMgr.Instance == null)
            return;
        GameEntity entity = GameEntityMgr.Instance.GetRandomAlivePlayerEntity();
        Vector3 position_worldSpace = viewer.position;

        //坐标转换至chunkspace中
        position_worldSpace -= HexCoords.GetHexVisualCoords(controller.GetMap().GetChunks()[Vector2Int.zero].GetCenter());
        viewerPosition_inChunkSpace.x = position_worldSpace.x;
        viewerPosition_inChunkSpace.y = position_worldSpace.z;
        viewerPosition_inChunkSpace.x += viewRect.x;
        viewerPosition_inChunkSpace.y += viewRect.y;

        UpdateVisibleChunks();
    }

    Vector2Int currentChunkCoord;
    GameObject go2;
    List<MapChunk> terrainChunksVisibleLastUpdate = new List<MapChunk>();
    void UpdateVisibleChunks()
    {
      
        if (go2 == null)
            go2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        currentChunkCoord.x = Mathf.RoundToInt(viewerPosition_inChunkSpace.x / chunkSize.x);
        currentChunkCoord.y = Mathf.RoundToInt(viewerPosition_inChunkSpace.y / (chunkSize.y));
        go2.transform.position = viewerPosition_inChunkSpace;
        HDebug.Log(currentChunkCoord);

        for (int yOffset = -chunksVisibleInViewDst.y; yOffset <= chunksVisibleInViewDst.y; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst.x; xOffset <= chunksVisibleInViewDst.x; xOffset++)
            {
                Vector2Int viewedChunkCoord = currentChunkCoord + new Vector2Int(xOffset, yOffset);
                HDebug.Log("sub "+viewedChunkCoord);
                IDictionary<Vector2Int, MapChunk> chunks = controller.GetMap().GetChunks();
                if (chunks.ContainsKey(viewedChunkCoord))
                {
                    chunks[viewedChunkCoord].UpdateMapChunk(viewerPosition_inChunkSpace, maxViewDst + betterView);
                    if (chunks[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(chunks[viewedChunkCoord]);
                    }
                }
                else
                {
                    //terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }
}
