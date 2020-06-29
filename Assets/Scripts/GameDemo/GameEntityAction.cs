using PathFind;
using System.Collections;
using System.Collections.Generic;
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
        switch (strategy)
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


    #region  原型  移动到可攻击敌人/关照友军的点后，使用之前选择好的技能进行挂机循环操作至 敌人死亡/友军被自己奶死（如果可以的话）
    protected IEnumerator playerAction2Entity()
    {
        while (entity.GetTargetEntity() != null)
        {
            yield return move2target();
        }
    }

    protected IEnumerator move2target()
    {
        if (entity.IsTargetEntityInAttackSight())
        {
            if (PReleaseSelectedSkill())
            {
                DoReleaseSelectedSkill();
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
                        {
                            entity.GetEntityVisual().PlayAnim(EntityAnimEnum.Idle);
                            yield break;
                        }
                    }
                    startIndex = startIndex + 1;
                }
            }
        }
    }
    protected void DoReleaseSelectedSkill()
    {
        entity.DoAttack();
        HDebug.Log("entity " + entity.gameObject.name + ":" + "target " + entity.GetTargetEntity().gameObject.name);
    }
    protected bool PReleaseSelectedSkill()
    {
        return entity.PAttack();
    }
    #endregion
}

//protected IEnumerator move2target()
//{
//    if (entity.IsTargetEntityInAttackSight())
//    {
//        if (PAttack())
//        {
//            DoAttack();
//        }
//        yield return new WaitForEndOfFrame();
//    }
//    else
//    {
//        MapController map = GameCore.GetRegistServices<MapController>();
//        IList<ICell> path = map.GetPathFinder().FindPathOnMap(
//            map.GetMap().GetCell(entity.CurrentPoint),
//            map.GetMap().GetCell(entity.GetTargetEntity().CurrentPoint),
//            map.GetMap());
//        if (path != null && path.Count >= 2)
//        {
//            path.RemoveAt(path.Count - 1);
//            ICell dest = path[path.Count - 1];
//            int startIndex = 0;
//            int destIndex = path.Count - 1;
//            while (true && startIndex < path.Count - 1)
//            {
//                if (startIndex + 1 <= path.Count - 1)
//                {
//                    yield return entity.movefromApoint2Bpoint(path[startIndex], path[startIndex + 1]);
//                    if (entity.IsTargetEntityInAttackSight())
//                    {
//                        entity.GetEntityVisual().PlayAnim(EntityAnimEnum.Idle);
//                        yield break;
//                    }
//                }
//                startIndex = startIndex + 1;
//            }
//        }
//    }
//}