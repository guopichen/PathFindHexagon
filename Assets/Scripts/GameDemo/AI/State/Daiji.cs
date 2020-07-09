using PathFind;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Daiji : GSNPCStateRemote
{
    //不移动 不追击 被动防守
    GameEntity entity;

    bool inAttackSight = false;
    public void EvalRule()
    {
        if(!inAttackSight)
        {
            entity.AimAtTargetEntity(null);
        }
    }

    public void ExecuteAction()
    {
    }

    public void GainFocus(GameEntity npc)
    {
        this.entity = npc;
    }

    public void LoseFocus()
    {
    }

    //public async Task ExecuteActionAsync()
    //{
    //    GameEntityControllRemote remote = entity.GetControllRemote();
    //    if (inAttackSight && remote.PReleaseSkill(remote.SelectedSkillID))
    //    {
    //        entity.DoReleaseSkill(remote.SelectedSkillID);
    //    }
    //    await new WaitForEndOfFrame();
    //}
    public IEnumerator ExecuteActionAsync()
    {
        GameEntityControllRemote remote = entity.GetControllRemote();
        if (inAttackSight && remote.PReleaseSkill(remote.SelectedSkillID))
        {
            entity.DoReleaseSkill(remote.SelectedSkillID);
        }
        yield return new WaitForEndOfFrame();
    }

    public void UpdateSensor()
    {
        inAttackSight = entity.IsTargetEntityInAttackSight() && entity.GetTargetEntity().BeAlive();
    }

    public void SendCmd(int fromID, Command msg, string arg)
    {
        if (msg == Command.CaughtDamage)
        {
            //如果在不移动的情况下可以攻击就还击
            GameEntity fromEntity = GameCore.GetRegistServices<GameEntityMgr>().GetGameEntity(fromID);
            if (fromEntity != null && fromEntity != entity.GetTargetEntity()
              && entity.IsEntityInAttackSight(fromEntity))
            {
                bool donotmove = false;
                entity.AimAtTargetEntity(fromEntity, donotmove);
            }
        }
    }
}
