using System.Collections.Generic;
using UnityEngine;

namespace PathFind
{
    public class ClientTerrainExportSet
    {
        public string mapID;
        public int xRange;
        public int yRange;

        public List<TerrainData> terrain = new List<TerrainData>();
        public List<RebornData> reborn = new List<RebornData>() { };
        public List<PortalData> portal = new List<PortalData>();
        public class TerrainData
        {
            public int x;
            public int y;
            public int tileSafe;//安全点
            public int tileLookType;//用于再次还原场景
            public int tileObstacle;//用于标记是否障碍地块
            public string tileFuncType;//用于指明进入地块触发事件,且需要额外参数
        }

        public class PortalData
        {
            public int x;
            public int y;
            public string dstMapID;
            public int dstMapX;
            public int dstMapY;
        }

        public class RebornData
        {
            public int x;
            public int y;
        }



        public PortalData GetPortalDataOrNew(Vector3Int exportIndex)
        {
            PortalData data = portal.Find(x =>
            {
                return x.x == exportIndex.x && x.y == exportIndex.y;
            });
            if (data == null)
            {
                data = new PortalData()
                {
                    x = exportIndex.x,
                    y = exportIndex.y
                };
                portal.Add(data);
            }

            return data;
        }

        public TerrainData GetTerrainData(Vector3Int exportIndex)
        {
            return terrain.Find(x =>
            {
                return x.x == exportIndex.x && x.y == exportIndex.y;
            });
        }
    }
}

