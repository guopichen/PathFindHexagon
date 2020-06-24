using PathFind;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

//自动战斗   找到一个目标就去攻击并重复
public class AutoFight : GSNPCStateRemote
{
    GameEntity entity;
    AttackTargetFullMap targetSensor = new AttackTargetFullMap();
    public void EvalRule()
    {
        if(targetSensor.IsDirty)
        {
            entity.AimAtTargetEntity(targetSensor.Value);
        }
    }

    public void ExecuteAction()
    {
    }

    public void GainFocus(GameEntity npc)
    {
        entity = npc;
    }

    public void LoseFocus()
    {

    }

    public async Task ExecuteActionAsync()
    {
        await aiLogic();
    }

    public void UpdateSensor()
    {
        if(entity.GetControllType() == EntityControllType.Player)
        targetSensor.TakeSample(entity);
    }

    public void SendCmd(int fromID, Command msg, string arg)
    {
        //全局探索模式下，仇恨还击没有意义，就是一个干字
        //if (msg == Command.CaughtDamage)
        //{
        //    GameEntity fromEntity = GameCore.GetRegistServices<GameEntityMgr>().GetGameEntity(fromID);
        //    if (fromEntity != null && fromEntity != entity.GetTargetEntity())
        //    {
        //        entity.AimAtTargetEntity(fromEntity, false);
        //    }
        //}
    }



    IEnumerator aiLogic()
    {
        if (entity.GetTargetEntity() != null && entity.GetTargetEntity().BeAlive())
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
}
