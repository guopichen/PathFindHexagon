using UnityEngine;
using System.Collections;

public class GSNPCBehaveRule{
    //GSEnemyCommonMove a;
    //GSEnemyMoveOffSet b;
    //GSEnemyAttack c;
    //GSHostageFightBack d;
    //GSHostageCommonMove e;
    //GSTankAttack  f;
    //stateA : 敌人向终点移动
    //stateB : 敌人随机偏移
    //stateC : 敌人攻击人质
    //stateD : 人质还击敌人
    //stateE : 人质向终点移动
    //stateF : 坦克攻击人质

    //FULL NAME is_enemy_stateA_can_translate_to_enemy_stateC

    /*
public static bool Is_StateA_To_StateC(GSNPCRemote remote,ControlGSNPC target)
{
    return remote != null
        && (remote.GetNpcType() == GSNPCTypeEnum.Boom || remote.GetNpcType() == GSNPCTypeEnum.Spy)
        && remote.IsCanAttackHostage() 
        && target != null;
}
public static bool Is_StateA_To_StateF(GSNPCRemote remote,ControlGSNPC target)
{
    return remote != null
        && (remote.GetNpcType() == GSNPCTypeEnum.Tank)
        && remote.IsCanAttackHostage()
        && target != null;
}

public static bool Is_StateA_To_StateB(GSNPCBase _npc,float myDeltaTime,float maxDelta)
{
    return (_npc != null)
        && maxDelta > 0
        && (myDeltaTime >= maxDelta)
        && (_npc.GetNpcType() == GSNPCTypeEnum.Boom || _npc.GetNpcType() == GSNPCTypeEnum.Spy)
        && (_npc.GetHudUIStyle() == GSNPCHudStyle.White || _npc.GetHudUIStyle() == GSNPCHudStyle.None)
        ;
}

public static bool Is_StateB_To_StateA(float remainDistance,float minDistance)
{
    return remainDistance < minDistance;
}

public static bool Is_StateB_To_StateC(GSNPCRemote remote, ControlGSNPC target)
{
    return Is_StateA_To_StateC(remote,target);
}

public static bool Is_StateC_To_StateA(GSNPCBase hunter,ControlGSNPC target,float hpper)
{
    //超过攻击方的range
    //被攻击者不是人质
    //被攻击者当前血量为0

    bool isOutRange = true;
    if(target != null)
    {
        isOutRange = Vector3.Distance(hunter.RootTrans.position,target.transform.position) > GSNPCBase.GetNpcUnit(hunter.GetNpcType()).attackRange;
    }

    bool isHostage = false;
    if(target != null)
    {
        isHostage = target.npcType == GSNPCTypeEnum.Hostage || target.npcType == GSNPCTypeEnum.Hostage2;
    }

    //攻击与被攻击的引用不匹配
    //bool isMatch = false;
    //isMatch = hunter.ControlNpc == ((GSNPCBase)(target.GetNPCRemote())).AttackTarget;
    return 
        //!isMatch ||
        isOutRange || !isHostage || hpper == 0;
}

public static bool Is_StateE_To_StateD(ControlGSNPC hunter)
{
    return hunter != null;
}
public static bool Is_StateD_To_StateE(float hpper)
{
    return hpper == 0;
}

public static bool Is_StateF_To_StateA(GSNPCBase hunter,ControlGSNPC target,float hpper)
{
    return Is_StateC_To_StateA(hunter,target,hpper);
}
*/
}
