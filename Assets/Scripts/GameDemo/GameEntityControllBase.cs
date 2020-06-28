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
    GameEntityRuntimeData UpdateRuntimeData(IDataForCalculateEntityRuntimeData forruntime);

}

public interface GameEntityControllRemote : DamageRemote, RuntimeDataRemote
{
    //void SendCmd(ControllMsg cmd, string arg);
    bool PTiliMove(int cost = 1);
    void ChangeHP(int delta);
    void ChangeTili(int delta = -1);
    void CalledEverySeconds();
    void CalledEveryFrame();
    bool PAttack(int skillID);
    void DoAttack(int skillID);
}

public delegate void OnRuntimeValueChanged(float newValue, float delta);


public interface ControllEvent
{
    OnRuntimeValueChanged hpChanged { get; }
    OnRuntimeValueChanged tiliChanged { get; }

}

//设计意图：用来表示战斗相关的数据
public class GameEntityControllBase : GameEntityControllRemote
{
    public static GameEntityControllBase emptyEntityControll = new GameEntityControllBase();

    private GameEntityRuntimeData runtimeData;
    private event OnRuntimeValueChanged hpEvent;
    private event OnRuntimeValueChanged tiliEvent;


    protected GameEntityControllBase()
    {
        runtimeData = new GameEntityRuntimeData();
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
            hpEvent?.Invoke(runtimeData.hp, delta);
        }
    }


    public bool PTiliMove(int cost = 1)
    {
        return runtimeData.tili - cost >= 0;
    }

    public void ChangeTili(int delta = -1)
    {
        runtimeData.tili += delta;
        tiliEvent?.Invoke(runtimeData.tili,delta);
    }

    public void CalledEverySeconds()
    {
        //runtimeData.tili += runtimeData.deltaTili / runtimeData.deltaTileTime;
        runtimeData.tili += 5;
        runtimeData.cd1 += 1;
        runtimeData.cd1 = Mathf.Clamp(runtimeData.cd1, -30, 0);

        runtimeData.cd2 += 1;
        runtimeData.cd2 = Mathf.Clamp(runtimeData.cd2, -30, 0);

        runtimeData.cd3 += 1;
        runtimeData.cd3 = Mathf.Clamp(runtimeData.cd3, -30, 0);
    }

    public void CalledEveryFrame()
    {
    }

    public bool PAttack(int i)
    {
        bool cd = false;
        if (i == 1)
            cd = runtimeData.cd1 >= 0;
        else if (i == 2)
            cd = runtimeData.cd2 >= 0;
        else if (i == 3)
            cd = runtimeData.cd3 >= 0;

        return cd;
    }

    public void DoAttack(int i)
    {
        if (i == 1)
            runtimeData.cd1 -= 3;
        else if (i == 2)
            runtimeData.cd2 -= 5;
        else if (i == 3)
            runtimeData.cd3 -= 10;
    }

    public GameEntityRuntimeData UpdateRuntimeData(IDataForCalculateEntityRuntimeData forruntime)
    {
        if (forruntime == null)
            return runtimeData;

        GameEntityConfig config = forruntime.GetStaticConfig();
        runtimeData.hp = config.hp_config;
        runtimeData.atk = config.atk_config;
        runtimeData.mag = config.mag_config;
        runtimeData.tili = config.tili_config;

        //2s  +1 tili
        runtimeData.deltaTili = 1;
        runtimeData.deltaTiliTime = 2;

        runtimeData.maxSingleMove = config.maxSingleMove_config;
        runtimeData.speed = config.speed_config;
        runtimeData.eyeSight = config.eyeSight_config;
        runtimeData.attackSight = config.attackSight_config;
        runtimeData.pursueSight = config.pursueSight_config;
        runtimeData.hujia = config.hujia_config;

        runtimeData.skillSockets = config.skillSocketSet1;
        return runtimeData;
    }

   
}

public enum EntityControllType
{
    None,
    Player,
    AI,
}
[System.Serializable]
public class GameEntityRuntimeData
{
    public float hp;

    public float atk;
    public float mag;
    public float tili;
    public float hujia;

    //tili_recovery_config
    public float deltaTili;
    public float deltaTiliTime;

    public int maxSingleMove;
    public int speed;
    public int eyeSight;
    public int attackSight;
    public int pursueSight;


    public List<int> skillSockets;
    public float cd1;
    public float cd2;
    public float cd3;


}