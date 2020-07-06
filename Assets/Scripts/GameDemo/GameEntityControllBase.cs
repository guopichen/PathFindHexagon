using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface DamageRemote
{
    void CaughtDamage(int damage, DamageTypeEnum type);
}

public interface IDataForCalculateEntityRuntimeData
{
    GameEntityConfig GetStaticConfig();
    List<ItemsCanAffectRuntimeData> GetAffectSet();
}

public interface ItemsCanAffectRuntimeData
{

}


public interface RuntimeDataRemote
{
    //void InitRuntimeData(GameEntity entity);
    GameEntityRuntimeData GetOrUpdateRuntimeData(IDataForCalculateEntityRuntimeData forruntime);
}

public interface GameEntityControllRemote : DamageRemote, RuntimeDataRemote
{
    //void SendCmd(ControllMsg cmd, string arg);
    bool PTiliMove(int cost);
    void ChangeHP(int delta);
    void ChangeTili(int delta);
    void ChangeHujia(int delta);
    void CalledEverySeconds();
    void CalledEveryFrame();
    bool PReleaseSkill(int skillID);
    void DoReleaseSkill(Skill skill);
    float GetHPPer();
    float GetMagicPer();
    bool BeAlive();
    int SelectedSkillID { get; }
    void ChangeSelectedSkill(int skillid);
}

//public delegate void OnRuntimeValueChanged(int newValue, float delta);
public delegate void OnRuntimeValueChanged(ValueChangeType type = ValueChangeType.Unknown);



//设计意图：用来表示战斗相关的数据
public class GameEntityControllBase : GameEntityControllRemote
{

    public static GameEntityControllBase emptyEntityControll = new GameEntityControllBase();

    private GameEntityRuntimeData runtimeData = new GameEntityRuntimeData();
    private int id;

    public int SelectedSkillID => runtimeData.selectSkillID;

    public void SetEntityID(int entityID)
    {
        id = entityID;
    }



    public bool BeAlive()
    {
        return Mathf.CeilToInt(runtimeData.hp) > 0;
    }

    public void CaughtDamage(int damage, DamageTypeEnum type)
    {
        int resultDamage = 0;
        switch (type)
        {
            case DamageTypeEnum.ColdWeapon:
                resultDamage = damage - Mathf.CeilToInt(runtimeData.hujia);
                break;
        }
        ChangeHP(-resultDamage);
    }


    public void ChangeHP(int delta)
    {
        if (delta != 0)
        {
            runtimeData.hp += delta;
            if (delta > 0)
                GameEntityMgr.Instance.valueChangedCenter(id, runtimeData.hp, delta, ValueChangeType.HPUp);
            else
                GameEntityMgr.Instance.valueChangedCenter(id, runtimeData.hp, delta, ValueChangeType.HPDown);
        }
    }

    public float GetHPPer()
    {
        return Mathf.Clamp01(runtimeData.hp / (1.0f * runtimeData.maxHP));
    }

    public float GetMagicPer()
    {
        return Mathf.Clamp01(runtimeData.mag / (1.0f * runtimeData.maxMag));

    }


    public bool PTiliMove(int cost = 1)
    {
        return runtimeData.tili - cost >= 0;
    }

    public void ChangeTili(int delta = -1)
    {
        runtimeData.tili += delta;
        if(delta > 0)
        {
            GameEntityMgr.Instance.valueChangedCenter(id, runtimeData.tili, delta, ValueChangeType.TiliUp);
        }
        else
            GameEntityMgr.Instance.valueChangedCenter(id, runtimeData.tili, delta, ValueChangeType.TiliDown);

    }

    public void CalledEverySeconds()
    {
        //runtimeData.tili += runtimeData.deltaTili / runtimeData.deltaTileTime;
        runtimeData.tili += 1;
        runtimeData.tili = Mathf.Clamp(runtimeData.tili, 0, runtimeData.maxTili);


        foreach(int equipSkill in runtimeData.activeSkillSockets)
        {
            if(runtimeData.cdSet.ContainsKey(equipSkill))
            {
                runtimeData.cdSet[equipSkill] += 1;
                runtimeData.cdSet[equipSkill] = Mathf.Clamp(runtimeData.cdSet[equipSkill], -30, 0);
            }
        }
    }

    public void CalledEveryFrame()
    {
        foreach(KeyValuePair<int,float> kvp in runtimeData.skillContinueSet)
        {
            int key = kvp.Key;
            float effectTime = kvp.Value;
            if (effectTime == 0)
                continue;

            effectTime -= Time.deltaTime;
            if(effectTime <= 0)
            {
                BattleService battle = GameCore.GetRegistServices<BattleService>();
                battle.GetSkillByID(key);
            }
            effectTime = Mathf.Min(0, effectTime);
        }
    }

    public bool PReleaseSkill(int skillID)
    {
        return runtimeData.cdSet[skillID] >= 0;
    }

    public void DoReleaseSkill(Skill skill)
    {
        runtimeData.cdSet[skill.skillID] -= skill.cd;
        if (runtimeData.skillContinueSet.ContainsKey(skill.skillID))
        {
            runtimeData.skillContinueSet[skill.skillID] = skill.effectTime;
        }
        else
            runtimeData.skillContinueSet.Add(skill.skillID, skill.effectTime);
    }

    public GameEntityRuntimeData GetOrUpdateRuntimeData(IDataForCalculateEntityRuntimeData forruntime)
    {
        if (forruntime == null)
            return runtimeData;

        GameEntityConfig config = forruntime.GetStaticConfig();
        runtimeData.hp = config.hp_config;
        runtimeData.maxHP = config.hp_config;

        runtimeData.atk = config.atk_config;
        runtimeData.mag = config.mag_config;
        runtimeData.maxMag = config.mag_config;

        runtimeData.tili = config.tili_config;
        runtimeData.maxTili = config.tili_config;

        //2s  +1 tili
        runtimeData.deltaTili = 1;
        runtimeData.deltaTiliTime = 2;

        runtimeData.maxSingleMove = config.maxSingleMove_config;
        runtimeData.speed = config.speed_config;
        runtimeData.eyeSight = config.eyeSight_config;
        runtimeData.attackSight = config.attackSight_config;
        runtimeData.pursueSight = config.pursueSight_config;
        runtimeData.hujia = config.hujia_config;

        runtimeData.ChangeActiveSkillSockets(config.skillSocketSet1);
        return runtimeData;
    }

    public void ChangeSelectedSkill(int skillid)
    {
        int lastselected = runtimeData.selectSkillID;
        runtimeData.selectSkillID = skillid;
        GameEntityMgr.Instance.valueChangedCenter(id, skillid, lastselected, ValueChangeType.AutoSkillChange);
    }

    public void ChangeHujia(int delta)
    {
        runtimeData.hujia += delta;
    }
}

public enum EntityType
{
    None,
    Player,
    AI,
    PlayerSummon,
}
[System.Serializable]
public class GameEntityRuntimeData
{
    public int hp;
    public int maxHP;

    public float atk;


    public int mag;
    public int maxMag;

    public int tili;
    public int maxTili;

    public float hujia;

    //tili_recovery_config
    public float deltaTili;
    public float deltaTiliTime;

    public int maxSingleMove;
    public int speed;
    public int eyeSight;
    public int attackSight;
    public int pursueSight;

    public int selectSkillID = Skill.PingA;

    public bool visible = true;


    public List<int> activeSkillSockets;
    public Dictionary<int, float> cdSet = new Dictionary<int, float>();
    public Dictionary<int, float> skillContinueSet = new Dictionary<int, float>();


    public void ChangeActiveSkillSockets(List<int> newSockets)
    {
        foreach(int skill in newSockets)
        {
            if (!cdSet.ContainsKey(skill))
                cdSet.Add(skill, 0);
        }
        activeSkillSockets = newSockets;
    }
}

