using PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntityMgr : GameServiceBase
{
    public static GameEntityMgr Instance { get; private set ; }


    public GameEntityMgr() : base()
    {
        Instance = this;
    }


    private List<GameEntity> allEntities = new List<GameEntity>();

    public void RegEntity(GameEntity entity)
    {
        if (!Instance.allEntities.Contains(entity))
        {
            Instance.allEntities.Add(entity);
        }
    }

    private void initImplement(MapController map)
    {
        map.OnPathFind += Instance.FirePathFindEvent;
        //map.OnStartCellSelect += instance.DrawRingCell;
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
        if(allEntities.Count > 0)
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


    private void FirePathFindEvent(IList<ICell> path)
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
    }

    public override void OnStartGame()
    {
        foreach (GameEntity entity in allEntities)
        {
            entity.gameObject.SetActive(true);
        }
    }

    public override void OnInitGame()
    {
        initImplement(GameCore.GetRegistServices<MapController>());
    }


}
