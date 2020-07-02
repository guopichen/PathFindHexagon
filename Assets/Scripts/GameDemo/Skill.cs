using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Skill", menuName = "PathFind/SkillAssets")]

public class Skill :ScriptableObject
{
    public const int PingA = 1;
    public int skillID;
    public EntityAnimEnum playAniWhenRelease;//决定用什么动作

    public float cd;

    public ValueChangeType skillEffectType;
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