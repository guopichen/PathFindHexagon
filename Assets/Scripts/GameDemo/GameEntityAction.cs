using PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        if(entity.GetControllType() == EntityControllType.Player)
        {
            await playerAction2Entity();
        }
        else if(entity.GetControllType() == EntityControllType.AI)
        {
            await aiAction2Entity();
        }
    }

    private IEnumerator aiAction2Entity()
    {
        while(entity.targetEntity != null)
        {
            yield return aiLogic();
        }
    }

    IEnumerator aiLogic()
    {
        if (entity.IsTargetInPursueSight())
        {
            yield return move2target();
        }
        else
        {
            //out of pursue,stop action to targetentity
            entity.AimAtTargetEntity(null);
        }
    }

    IEnumerator playerAction2Entity()
    {
        while (entity.targetEntity != null)
        {
            yield return move2target();
        }
    }

  
    private void DoAttack()
    {
        entity.DoAttack();
        HDebug.Log("entity " + entity.gameObject.name + ":" + "target " + entity.targetEntity.gameObject.name);
    }

    IEnumerator move2target()
    {

        //while (entity.targetEntity != null)
        {
            if (entity.IsTargetEntityInAttackSight())
            {
                if (PAttack())
                {
                    DoAttack();
                }
                yield return new WaitForEndOfFrame();
            }
            else
            {
                MapController map = GameCore.GetRegistServices<MapController>();
                IList<ICell> path = map.GetPathFinder().FindPathOnMap(
                    map.GetMap().GetCell(entity.CurrentPoint),
                    map.GetMap().GetCell(entity.targetEntity.CurrentPoint),
                    map.GetMap());
                if (path != null && path.Count >= 2)
                {
                    path.RemoveAt(path.Count - 1);
                    ICell dest = path[path.Count - 1];
                    int startIndex = 0;
                    int destIndex = path.Count - 1;
                    while (true && startIndex < path.Count - 1)
                    {
                        if (startIndex + 1 <= path.Count - 1)
                        {
                            yield return entity.movefromApoint2Bpoint(path[startIndex], path[startIndex + 1]);
                            if (entity.IsTargetEntityInAttackSight())
                                yield break;
                        }
                        startIndex = startIndex + 1;
                    }
                }
            }
        }
    }

    private bool PAttack()
    {
        return entity.PAttack();
    }

}
