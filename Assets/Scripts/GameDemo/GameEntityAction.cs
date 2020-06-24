using PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum Command
{
    None,
    CaughtDamage,
}
public interface GameEntityActionRemote
{
    void Action2Entity(GameEntity targetEntity);
    void Action2Point(int skillID, Vector2Int targetPoint);
    IEnumerator AutoUpdate();

}

public interface GameEntityMsg
{
    void SendCmd(int fromID, Command msg, string arg);
}


public enum EntityActionEnum
{
    None,
    Warrior,
    Magical,
    Tank,
}

public enum GSNPCStrategyEnum
{
    Empty,
    Daiji,
    Jingjie,
    AutoFight,
}


//为职业而抽象 ， 各职业的行为由状态机组成
public class GameEntityAction : GameEntityActionRemote, GameEntityMsg
{
    protected GameEntity entity;
    protected static GSNPCStateRemote state_empty = new GSNPCStateBase();

    private GSNPCStateRemote mutexState1 = state_empty;

    private GSNPCStateRemote DaijiRemote = new Daiji();
    private GSNPCStateRemote JingjieRemote = new Jingjie();
    private GSNPCStateRemote AutoFightRemote = new AutoFight();

    public GameEntityAction(GameEntity entity)
    {
        this.entity = entity;
        if (entity.GetControllType() == EntityControllType.AI)
        {
            ChangeBehaveState(AutoFightRemote);
        }
    }

    public void ChangeStrategy(GSNPCStrategyEnum strategy)
    {
        switch(strategy)
        {
            case GSNPCStrategyEnum.AutoFight:
                ChangeBehaveState(AutoFightRemote);

                break;
            case GSNPCStrategyEnum.Daiji:
                ChangeBehaveState(DaijiRemote);
                break;
            case GSNPCStrategyEnum.Jingjie:
                ChangeBehaveState(JingjieRemote);
                break;
            case GSNPCStrategyEnum.Empty:
            default:
                mutexState1 = state_empty;
                break;
        }
    }

    public bool ChangeBehaveState(GSNPCStateRemote targetState)
    {
        if (mutexState1 == targetState)
            return false;
        if (mutexState1 != null)
        {
            mutexState1.LoseFocus();
        }
        mutexState1 = targetState;
        if (targetState != null)
        {
            targetState.GainFocus(this.entity);
        }
        return true;
    }

    public virtual void Action2Entity(GameEntity gameEntity)
    {

    }

    public virtual void Action2Point(int skillID, Vector2Int point)
    {

    }

    public void SendCmd(int fromID, Command msg, string arg)
    {
        mutexState1.SendCmd(fromID, msg, arg);
    }


    public virtual IEnumerator AutoUpdate()
    {
        yield return null;
        while (entity.BeAlive())
        {
            mutexState1?.UpdateSensor();
            mutexState1?.EvalRule();
            if (mutexState1 != null)
            {
                mutexState1.ExecuteAction();
                yield return mutexState1?.ExecuteActionAsync().AsIEnumerator();
            }
            else
            {
                yield return null;
            }
        }
    }
}

//近战（类似战士）
public class WarriorEntityAction : GameEntityAction
{
    public WarriorEntityAction(GameEntity entity) : base(entity)
    {

    }
    public override async void Action2Entity(GameEntity targetEntity)
    {
        if (entity.GetControllType() == EntityControllType.Player)
        {
            await playerAction2Entity();
        }
        else if (entity.GetControllType() == EntityControllType.AI)
        {
            //await aiAction2Entity();
        }
    }

    #region AI 测试原型

    private IEnumerator aiAction2Entity()
    {
        while (entity.GetTargetEntity() != null)
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
        while (entity.GetTargetEntity() != null)
        {
            yield return move2target();
        }
    }


    private void DoAttack()
    {
        entity.DoAttack();
        HDebug.Log("entity " + entity.gameObject.name + ":" + "target " + entity.GetTargetEntity().gameObject.name);
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
                    map.GetMap().GetCell(entity.GetTargetEntity().CurrentPoint),
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
    #endregion
}
