using PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface GameEntityMgrRemote
{
    List<GameEntity> GetAllPlayers();
    void ChangeAllPlayerEntityStrategy(GSNPCStrategyEnum strategy);
    void ChangePlayerEntityStrategy(int entityID, GSNPCStrategyEnum strategy);
}

public class GameEntityMgr : GameServiceBase, GameEntityMgrRemote
{
    public static GameEntityMgr Instance { get; private set; }


    public GameEntityMgr() : base()
    {
        Instance = this;
    }


    private List<GameEntity> allEntities = new List<GameEntity>();
    private List<GameEntity> playerEntities = new List<GameEntity>();

    private Dictionary<int, GameEntity> id2allEntities = new Dictionary<int, GameEntity>();

    static int entityHistoryCnt = 0;
    public void RegEntity(GameEntity entity)
    {
        entityHistoryCnt++;
        int key = entity.GetInstanceID();
        entity.entityID = key;

        entity.ModelID = entityHistoryCnt;


        if (!id2allEntities.ContainsKey(key))
        {
            id2allEntities.Add(key, entity);
            Instance.allEntities.Add(entity);
            if (GameCore.GetGameStatus() == GameStatus.Run)
            {
                OnStartGame(entity);
            }
            if (entity.GetControllType() == EntityControllType.Player)
            {
                playerEntities.Add(entity);
            }

        }
    }
    public GameEntity GetGameEntity(int id)
    {
        if (id2allEntities.TryGetValue(id, out GameEntity e))
        {
            if (e.gameObject == null)
                return null;
            return e;
        }
        return null;

    }

    //获取最近敌对实例
    public GameEntity GetNearestOpSideEntity(GameEntity centerEntity)
    {
        int minDistance = 999;
        int minID = 0;
        foreach (int id in allEntities.Select((x) =>
         {
             if (x.GetControllType() != centerEntity.GetControllType() && x.BeAlive())
                 return x.entityID;
             return 0;
         }))
        //foreach(GameEntity entity in playerEntities)
        {
            if (id == 0)
                continue;
            GameEntity entity = id2allEntities[id];
            Vector2Int targetPoint = entity.CurrentPoint;
            int distance = (targetPoint.x - centerEntity.CurrentPoint.x) * (targetPoint.x - centerEntity.CurrentPoint.x)
                + (targetPoint.y - centerEntity.CurrentPoint.y) * (targetPoint.y - centerEntity.CurrentPoint.y);
            if (distance < minDistance)
            {
                minDistance = distance;
                minID = entity.entityID;
            }
        }
        if (id2allEntities.TryGetValue(minID, out GameEntity result))
            return result;
        return null;
    }

    //public GameEntity GetNearEntityFast(Vector2Int point)
    //{
    //    return null;
    //}


    private void initImplement(MapController map)
    {
        map.OnPathFind += Instance.OnPathFind;
        map.OnEndCellSelect += Instance.OnEndCellSelect;
        //map.OnStartCellSelect += instance.DrawRingCell;
    }

    private void OnEndCellSelect(ICell obj)
    {
        if (SelectedEntity != null)
        {
            SelectedEntity.AimAtTargetEntity(null);
            SelectedEntity.PredictPathWillChange();
        }
    }

    private void DrawRingCell(ICell centerCell)
    {
        List<Vector2Int> vector2Ints = new List<Vector2Int>();
        if (HexCoords.GetHexCellByRadius(centerCell.Point, 3, ref vector2Ints))
        {
            foreach (Vector2Int v in vector2Ints)
            {
                CellView cell = GameCore.GetRegistServices<MapController>().GetCellView(v);
                if (cell != null)
                {
                    cell.SetCellViewStatus(CellViewStatus.EyeSight);
                }
            }
        }

    }

    public GameEntity GetRandomActiveEntity()
    {
        if (allEntities.Count > 0)
        {
            return allEntities[0];
        }
        return null;
    }

    private GameEntity selectedEntity = null;

    public GameEntity SelectedEntity
    {
        get => selectedEntity;
        set
        {
            selectedEntity?.LoseFocus();
            selectedEntity = value;
            if (selectedEntity != null)
                GameCore.GetRegistServices<MapController>().SetStartPoint(selectedEntity.CurrentPoint);
            selectedEntity?.GainFocus();
        }
    }

    public static bool EnableRandomMove = false;

    private void OnPathFind(IList<ICell> path)
    {
        SelectedEntity?.MoveAlongPath(path);
    }

    public static void SetSelectedEntity(GameEntity gameEntity)
    {
        Instance.SelectedEntity = gameEntity;
    }

    public static GameEntity GetSelectedEntity()
    {
        return Instance.SelectedEntity;
    }


    public override void OnUpdateGame()
    {
        if (selectedEntity != null)
            GameCore.GetRegistServices<MapController>().SetStartPoint(selectedEntity.CurrentPoint);

        foreach (GameEntity entity in allEntities)
        {
            entity.UpdateEntityRuntime(Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            EnableRandomMove = !EnableRandomMove;
        }
    }

    public override void OnStartGame()
    {
        foreach (GameEntity entity in allEntities)
        {
            OnStartGame(entity);
        }
    }

    private void OnStartGame(GameEntity gameEntity)
    {
        gameEntity.gameObject.SetActive(true);
    }

    public override void OnInitGame()
    {
        initImplement(GameCore.GetRegistServices<MapController>());
    }

    public List<GameEntity> GetAllPlayers()
    {
        return playerEntities;
    }

    public void ChangeAllPlayerEntityStrategy(GSNPCStrategyEnum strategy)
    {
        foreach (GameEntity entity in playerEntities)
        {
            entity.ChangeStrategy(strategy);
        }
    }

    public void ChangePlayerEntityStrategy(int entityID, GSNPCStrategyEnum strategy)
    {
        if(id2allEntities.TryGetValue(entityID,out GameEntity entity))
        {
            if (entity.GetControllType() == EntityControllType.Player)
                entity.ChangeStrategy(strategy);
        }
    }
}
