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
    public void QuestSkillCalculate(int skillID, GameEntityControllRemote opSender, GameEntityControllRemote opTarget)
    {
        Skill skill = GetSkillByID(skillID);
        if (skill == null)
        {
            HDebug.Error("null skill " + skill);
            return;
        }
        calculateResult result = sss(skill, opSender, opTarget);
        if ((result.type & resultEnum.enum1) == resultEnum.enum1)
        {
            Debug.Log(result.finalDamage * -4);
            opTarget.CaughtDamage(result.finalDamage, DamageTypeEnum.ColdWeapon);
        }
        if ((result.type & resultEnum.enum2) == resultEnum.enum2)
        {
            Debug.Log(result.finalDamage * 100);
        }
    }
    private calculateResult sss(Skill skill, GameEntityControllRemote opSender, GameEntityControllRemote opTarget)
    {
        GameEntityRuntimeData senderRuntime = opSender.UpdateRuntimeData(null);
        GameEntityRuntimeData targetRuntime = opTarget.UpdateRuntimeData(null);
        return new calculateResult() { type = resultEnum.enum1 | resultEnum.enum2, finalDamage = 20 };
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
        public int finalDamage;
    }


    private class damageResult
    {
        public DamageTypeEnum type;
        public int finalDamage;
    }



    Dictionary<int, Skill> skillSet = new Dictionary<int, Skill>();
    private Skill GetSkillByID(int skillID)
    {
        if (skillSet.TryGetValue(skillID, out Skill skill))
            return skill;

        Skill loadSkill = Resources.Load<Skill>("S" + skillID);
        skillSet.Add(skillID, loadSkill);
        return loadSkill;
    }
}
