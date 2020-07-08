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
    GSNPCStrategy AIStrategy { get; }

}

public interface GameEntityMsg
{
    void SendCmd(int fromID, Command msg, string arg);
}


public enum EntityZhiye
{
    None,
    Warrior,
    Magical,
    Mushi,
}

public enum GSNPCStrategy
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


    private GSNPCStrategy currentStrategy = GSNPCStrategy.Empty;
    public GSNPCStrategy AIStrategy => currentStrategy;

    public GameEntityAction(GameEntity entity)
    {
        this.entity = entity;
        if (entity.GetControllType() != EntityType.Player)
        {
            ChangeBehaveState(AutoFightRemote);
        }
    }

    public void ChangeStrategy(GSNPCStrategy strategy)
    {
        currentStrategy = strategy;
        switch (strategy)
        {
            case GSNPCStrategy.AutoFight:
                ChangeBehaveState(AutoFightRemote);
                break;
            case GSNPCStrategy.Daiji:
                ChangeBehaveState(DaijiRemote);
                break;
            case GSNPCStrategy.Jingjie:
                ChangeBehaveState(JingjieRemote);
                break;
            case GSNPCStrategy.Empty:
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

    public virtual async void Action2Entity(GameEntity gameEntity)
    {
        if (entity.GetControllType() == EntityType.Player)
        {
            await entity.playerAction2Entity();
        }
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
        entity.GetEntityVisual().Status = EntityAnimStatus.Death;
    }


    #region  原型  移动到可攻击敌人/关照友军的点后，使用之前选择好的技能进行挂机循环操作至 敌人死亡/友军被自己奶死（如果可以的话）
    protected IEnumerator playerAction2Entity()
    {
        while (entity.BeAlive() && entity.GetTargetEntity() != null && entity.GetTargetEntity().BeAlive())
        {
            yield return move2target();
        }
        if (entity.BeAlive())
            entity.GetEntityVisual().Status = EntityAnimStatus.Idle;
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
                        if (entity.IsTargetEntityInAttackSight() || entity.GetControllRemote().PTiliMove(1) == false)
                        {
                            entity.GetEntityVisual().PlayAnim(EntityAnimEnum.Idle);
                            yield break;
                        }
                        yield return entity.movefromApoint2Bpoint(path[startIndex], path[startIndex + 1]);
                    }
                    startIndex = startIndex + 1;
                }
            }
        }
    }
    protected void DoReleaseSelectedSkill()
    {
        entity.DoReleaseSkill(entity.GetControllRemote().SelectedSkillID);
        HDebug.Log("entity " + entity.gameObject.name + ":" + "target " + entity.GetTargetEntity().gameObject.name);
    }
    protected bool PReleaseSelectedSkill()
    {
        return entity.PReleaseSkill(entity.GetControllRemote().SelectedSkillID);
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