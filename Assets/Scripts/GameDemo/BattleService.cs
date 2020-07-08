using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface BattleServiceRemote
{
    //把传统的平砍 也当作释放技能  
    //把使用道具也当作释放技能
    void QuestSkillCalculate(int skillID, GameEntityControllRemote opSender, GameEntityControllRemote opTarget);
}



public class BattleService : GameServiceBase, BattleServiceRemote
{
    public void QuestSkillCalculate(int skillID, GameEntity sender)
    {
        Skill skill = GetSkillByID(skillID);
        if (skill == null)
        {
            HDebug.Error("null skill " + skill);
            return;
        }

        //因为策划的因素，在demo演示阶段正式立项前均采用最快法则完成技能处理
        switch (skillID)
        {
            case 1:
                pingA(sender);
                break;
            case 2:
                addHujia(skill,sender);
                break;
            case 3:
                becomeInvisible(skill,sender);
                break;
            case 4:
                doLeidianshu(sender);
                break;
            case 5:
                doZhaohuan(skill,sender);
                break;
            case 6:
                doShanxian(skill, sender);
                break;
        }

    }

    private void doShanxian(Skill skill, GameEntity sender)
    {

    }

    private void doZhaohuan(Skill skill, GameEntity sender)
    {
        List<Vector2Int> neighbors = sender.CurrentPoint.GetCellNeighbor();
        bool zhaohuan = false;
        foreach(Vector2Int v in neighbors)
        {
            if (zhaohuan)
                break;

            EntityType entityControllType = sender.GetControllType();
            if(entityControllType == EntityType.Player)
            {
                GameCore.SpawnBaobao(EntityZhiye.Magical);
            }
            else if(entityControllType == EntityType.AI)
            {
                GameCore.SpawnNPC(EntityZhiye.Magical);
            }
            zhaohuan = true;
        }

        GameTimer.DelayJobBaseOnCore(skill.effectTime, () => {
            //处理召唤对象
        });
    }

    private void doLeidianshu(GameEntity sender)
    {
        GameEntity entity = sender.GetTargetEntity();
        GameEntityRuntimeData targetData = entity.GetControllRemote().GetOrUpdateRuntimeData(null);
        GameEntityRuntimeData senderData = sender.GetControllRemote().GetOrUpdateRuntimeData(null);

        int finalDamage = Mathf.CeilToInt(senderData.atk);

        entity.GetControllRemote().ChangeHP(finalDamage);
    }

    private void becomeInvisible(Skill skill, GameEntity sender)
    {
        GameEntityRuntimeData data = sender.GetControllRemote().GetOrUpdateRuntimeData(null);
        data.visible = false;
        GameTimer.DelayJobBaseOnCore(skill.effectTime, () => {
            data.visible = true;
        });
    }

    private void addHujia(Skill skill, GameEntity sender)
    {
        GameEntityRuntimeData data = sender.GetControllRemote().GetOrUpdateRuntimeData(null);
        sender.GetControllRemote().ChangeHujia(Mathf.CeilToInt(data.hujia * 0.1f));//这里应该基于伤害公式来计算，这里是错误的临时写法

        GameTimer.DelayJobBaseOnCore(skill.effectTime, () => {
            //恢复护甲
        });
    }

    private void pingA(GameEntity sender)
    {
        GameEntity entity = sender.GetTargetEntity();
        GameEntityRuntimeData targetData = entity.GetControllRemote().GetOrUpdateRuntimeData(null);
        GameEntityRuntimeData senderData = sender.GetControllRemote().GetOrUpdateRuntimeData(null);

        int finalDamage = Mathf.CeilToInt(senderData.atk - targetData.hujia);
        entity.GetControllRemote().CaughtDamage(finalDamage, DamageTypeEnum.ColdWeapon);
    }




    public void QuestSkillCalculate(int skillID, GameEntityControllRemote opSender, GameEntityControllRemote opTarget)
    {
        Skill skill = GetSkillByID(skillID);
        if (skill == null)
        {
            HDebug.Error("null skill " + skill);
            return;
        }
        opSender.DoReleaseSkill(skill);
        calculateResult result = calculateSkill(skill, opSender, opTarget);
        if ((result.type & resultEnum.enum1) == resultEnum.enum1)
        {
            opTarget.CaughtDamage(result.damagePart.finalDamage, DamageTypeEnum.ColdWeapon);
        }
        if ((result.type & resultEnum.enum2) == resultEnum.enum2)
        {
        }
    }
    private calculateResult calculateSkill(Skill skill, GameEntityControllRemote opSender, GameEntityControllRemote opTarget)
    {
        GameEntityRuntimeData senderRuntime = opSender.GetOrUpdateRuntimeData(null);
        GameEntityRuntimeData targetRuntime = opTarget.GetOrUpdateRuntimeData(null);
        return new calculateResult() { type = resultEnum.enum1 | resultEnum.enum2, damagePart = new damageResult() { finalDamage = Mathf.CeilToInt(skill.cd * 2) * 0 } };
    }


    private enum resultEnum : short
    {
        enum1 = 1,
        enum2 = 2,
        enum3 = 4,
    }


    private class calculateResult
    {
        public resultEnum type;
        public damageResult damagePart;
    }


    private class damageResult
    {
        //public DamageTypeEnum type;
        public int finalDamage;
    }



    Dictionary<int, Skill> skillSet = new Dictionary<int, Skill>();
    public Skill GetSkillByID(int skillID)
    {
        if (skillSet.TryGetValue(skillID, out Skill skill))
            return skill;

        Skill loadSkill = Resources.Load<Skill>("S" + skillID);
        skillSet.Add(skillID, loadSkill);
        return loadSkill;
    }
}
