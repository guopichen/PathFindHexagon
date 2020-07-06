using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "PathFind/SkillAssets")]

public class Skill :ScriptableObject
{
    public const int PingA = 1;
    [Header("主键")]
    public int skillID;

    [Header("播放动画")]

    public EntityAnimEnum playAniWhenRelease;//决定用什么动作
    //public float preCd;//前摇
    //public float lateCd;//后摇
    [Header("冷却")]
    public float cd;

    [Header("持续时间")]
    public float effectTime;

    [Header("与伤害公式xiang")]
    public ValueChangeType skillEffectType;//技能是加血还是减护甲
    [Header("作用于自己还是敌人")]
    public SkillTarget target = SkillTarget.Target;//该技能应该作用于谁
    [Header("技能嵌套")]
    public List<Skill> subSkills;//子技能系统，比如一个技能是即伤害别人又治疗自己还对具体点位有释放技能


#if UNITY_EDITOR
    [TextArea]
    public string desc;
#endif

}

public enum ValueChangeType : int
{
    Unknown = 0,
    TiliDown,
    TiliUp,
    HPDown,
    HPUp,
    HuaJiaDown,
    HuaJiaUp,
    AtkDown,
    AtkUp,
    AutoSkillChange,
}

public enum SkillTarget
{
    Target, //对目标
    Self,   //对自己
    Point,  //对具体点位
}