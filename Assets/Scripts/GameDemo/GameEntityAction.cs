using PathFind;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface GameEntityActionRemote
{
    void Action2Entity(GameEntity targetEntity);
    void Action2Point(int skillID, Vector2Int targetPoint);
}


public enum EntityActionEnum
{
    None,
    Warrior,
    Magical,
    Tank,

}
public class GameEntityAction : GameEntityActionRemote
{
    protected GameEntity entity;
    public GameEntityAction(GameEntity entity)
    {
        this.entity = entity;
    }

    public virtual void Action2Entity(GameEntity gameEntity)
    {
    }

    public virtual void Action2Point(int skillID, Vector2Int point)
    {
    }
}


public class WarriorEntityAction : GameEntityAction
{
    public WarriorEntityAction(GameEntity entity) : base(entity)
    {

    }
    public override async void Action2Entity(GameEntity targetEntity)
    {
        await move2target(targetEntity);
    }

    IEnumerator move2target(GameEntity target)
    {
        yield return null;
        while(true)
        {
            MapController map = GameCore.GetRegistServices<MapController>();
            IList<ICell> path = map.GetPathFinder().FindPathOnMap(map.GetMap().GetCell(entity.CurrentPoint), map.GetMap().GetCell(target.CurrentPoint),map.GetMap());
            path.RemoveAt(path.Count - 1);
            ICell dest = path[path.Count - 1];
            entity.MoveAlongPath(path);

            while(!entity.IsEntityAtPoint(dest.Point))
            {
                yield return new WaitForSeconds(0.3f);
            }

        }
    }

    private bool PAttack()
    {
        return false;
    }

}
