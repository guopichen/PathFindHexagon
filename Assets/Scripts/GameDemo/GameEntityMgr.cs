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

    private Action onAddNewPlayerEntityDuringRun = delegate { };
    private Action onAddNewNPCEntityDuringRun = delegate { };

    public void OnAddNewPlayerDuringRun(Action action)
    {
        onAddNewPlayerEntityDuringRun += action;
    }
    public void OnAddNewNPCDuringRun(Action action)
    {
        onAddNewNPCEntityDuringRun += action;
    }


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
            EntityControllType type = entity.GetControllType();
            if (type == EntityControllType.Player)
            {
                playerEntities.Add(entity);
            }

            if (GameCore.GetGameStatus() == GameStatus.Run)
            {
                ActiveEntity(entity);
                if (type == EntityControllType.AI)
                {
                    onAddNewNPCEntityDuringRun();
                }
                else if (type == EntityControllType.Player)
                    onAddNewPlayerEntityDuringRun();
            }
            
            entityID2ValuechangedDelegates.Add(key, new OnRuntimeValueChanged((changeType) => { }));

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
            if (!entity.BeAlive())
                continue;
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

    public void tiliChangedCenter(int id, int tili, int delta)
    {
    }

    internal void hpChangedCenter(int id, int hp, int delta)
    {

    }

   

    //只要数据变动，若要更细致到hp的话，在valuechangedcenter处理
    private Dictionary<int, OnRuntimeValueChanged> entityID2ValuechangedDelegates = new Dictionary<int, OnRuntimeValueChanged>();

    public OnRuntimeValueChanged onSelectedEntityValueChange = delegate { };
    public void AddEntityRuntimeValueChangedListenerByEntityID(int entityid, OnRuntimeValueChanged yours)
    {
        if(entityID2ValuechangedDelegates.ContainsKey(entityid))
        {
            OnRuntimeValueChanged onEntityRuntimeChanged = entityID2ValuechangedDelegates[entityid];
            onEntityRuntimeChanged += yours;
            entityID2ValuechangedDelegates[entityid] = onEntityRuntimeChanged;
        }
        //else
        //{
        //    entityID2ValuechangedDelegates.Add(entityid, yours);
        //}
    }

    public void AddEntityRuntimeValueChangedListenerByIndex(int indexOfTeam, OnRuntimeValueChanged yours)
    {
        if(allEntities.Count > indexOfTeam && indexOfTeam >= 0)
        {
            GameEntity entity = allEntities[indexOfTeam];
            AddEntityRuntimeValueChangedListenerByEntityID(entity.entityID, yours);
        }
    }

    public void RemoveRuntimeValueChangedListener(int entityid, OnRuntimeValueChanged yours)
    {
        if (entityID2ValuechangedDelegates.ContainsKey(entityid))
        {
            OnRuntimeValueChanged onEntityRuntimeChanged = entityID2ValuechangedDelegates[entityid];
            onEntityRuntimeChanged -= yours;
            entityID2ValuechangedDelegates[entityid] = onEntityRuntimeChanged;
        }
    }


    public void ClearEntityRuntimeListener(int entityid)
    {
        OnRuntimeValueChanged onEntityRuntimeChanged = entityID2ValuechangedDelegates[entityid];
        if (onEntityRuntimeChanged == null)
            return;
        //onEntityRuntimeChanged += yours;
        onEntityRuntimeChanged = delegate { };
    }


    //public const int Tili = 1;
    //public const int HP = 2;
    

    public void valueChangedCenter(int id, int valueAfaterChange, int delta, ValueChangeType valueType)
    {
        entityID2ValuechangedDelegates[id]?.Invoke(valueType);
        if(selectedEntity != null && id == selectedEntity.entityID)
        {
            onSelectedEntityValueChange(valueType);
        }
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

    public GameEntity GetRandomAlivePlayerEntity()
    {
        foreach (GameEntity e in playerEntities)
        {
            if (e.BeAlive())
                return e;
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
            onEntitySelected();
            selectedEntity?.GainFocus();
        }
    }


    private void OnPathFind(IList<ICell> path)
    {
        SelectedEntity?.MoveAlongPath(path);
    }

    public Action onEntitySelected = delegate () { };

    public static void SetSelectedEntity(GameEntity gameEntity)
    {
        if (Instance == null)
            return;
        Instance.SelectedEntity = gameEntity;

    }

    public static GameEntity GetSelectedEntity()
    {
        return Instance?.SelectedEntity;
    }

    public static bool IsEntitySelected(int entityID)
    {
        return Instance?.SelectedEntity != null && Instance.SelectedEntity.entityID == entityID; 
    }


    public override void OnUpdateGame()
    {
        if (selectedEntity != null)
            GameCore.GetRegistServices<MapController>().SetStartPoint(selectedEntity.CurrentPoint);

        foreach (GameEntity entity in allEntities)
        {
            entity.UpdateEntityRuntime(Time.deltaTime);
        }

        if (SelectedEntity != null && SelectedEntity.BeAlive() == false)
            SetSelectedEntity(null);
    }

    public override void OnStartGame()
    {
        foreach (GameEntity entity in allEntities)
        {
            ActiveEntity(entity);
        }
    }

    private void ActiveEntity(GameEntity gameEntity)
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

    public List<GameEntity> GetAllEntities()
    {
        return allEntities;
    }

    public void ChangeAllPlayerEntityStrategy(GSNPCStrategyEnum strategy)
    {
        foreach (GameEntity entity in playerEntities)
        {
            entity.ChangeAutoPlayStrategy(strategy);
        }
    }

    public void ChangePlayerEntityStrategy(int entityID, GSNPCStrategyEnum strategy)
    {
        if (id2allEntities.TryGetValue(entityID, out GameEntity entity))
        {
            if (entity.GetControllType() == EntityControllType.Player)
                entity.ChangeAutoPlayStrategy(strategy);
        }
    }
}


//public class EntityEvent
//{
//    public OnEntityDataChanged handler;
//}