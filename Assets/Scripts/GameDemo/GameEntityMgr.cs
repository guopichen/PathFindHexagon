using PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntityMgr
{
    public static GameEntityMgr instance = new GameEntityMgr();
    private GameEntityMgr()
    {

    }

    public static void Init(MapController map)
    {
        if (map != null)
            instance.initImplement(map);
    }

    private void initImplement(MapController map)
    {
        this.map = map;
        map.OnPathFind += instance.FirePathFindEvent;
        //map.OnStartCellSelect += instance.DrawRingCell;
    }
    public MapController map;
    private void DrawRingCell(ICell centerCell)
    {
        List<Vector2Int> vector2Ints = new List<Vector2Int>();
        if (HexCoords.GetHexCellByRadius(centerCell.Point, 3, ref vector2Ints))
        {
            foreach (Vector2Int v in vector2Ints)
            {
                CellView cell = map.GetCellView(v);
                if(cell != null)
                {
                    cell.SetCellViewStatus( CellViewStatus.EyeSight );
                }
            }
        }

    }

    private GameEntity selectedEntity = null;

    public GameEntity SelectedEntity
    {
        get => selectedEntity;
        set
        {
            selectedEntity?.LoseFocus();
            selectedEntity = value;
            selectedEntity?.GainFocus();
        }
    }

    private void FirePathFindEvent(IList<ICell> path)
    {
        SelectedEntity?.MoveAlongPath(path);
    }

    public static void SetSelectedEntity(GameEntity gameEntity)
    {
        instance.SelectedEntity = gameEntity;
    }
}
