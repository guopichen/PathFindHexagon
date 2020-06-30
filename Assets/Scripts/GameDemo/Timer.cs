#if UNITYEDITOR
#define HTools
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public static class GameTimer
{

    #region 用于通常情况下的
    public static async Task AwaitSeconds(float seconds, Action action)
    {
        await new WaitForSeconds(seconds);
        action();
    }

    public static async Task AwaitLoopSeconds(float seconds, Action action)
    {
        await new WaitForSeconds(seconds);
        while (GameCore.GetGameStatus() != GameStatus.Run)
            await new WaitForEndOfFrame();
        action();
        AwaitLoopSeconds(seconds, action).ForgetAwait();
    }

    #endregion

    #region 基于GameCore的情况,时间流逝需要考虑到GameCore的状态，同时一定要在GameCore工作之后使用
    public static async Task AwaitSecondsBaseOnCore(float seconds,Action action)
    {
        while (seconds > 0)
        {
            await GameCore.Instance.CoreTick();
            seconds -= Time.deltaTime;
        }
        action();
    }

    public static async Task AwaitLoopSecondsBaseOnCore(float seconds, Action action)
    {
        while (seconds > 0)
        {
            await GameCore.Instance.CoreTick();
            seconds -= Time.deltaTime;
        }
        action();
        AwaitLoopSecondsBaseOnCore(seconds, action).ForgetAwait();
    }

    #endregion
}


public static class HDebug
{

    [Conditional("HTools")]
    public static void Log(object msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    [Conditional("HTools")]
    public static void Error(object msg)
    {
        UnityEngine.Debug.LogError(msg);
    }
}


public static class TaskExpand
{
    public static void ForgetAwait(this Task task)
    {
        task.ContinueWith(handleException);
    }

    private static async void handleException(Task task)
    {
        //do nothing
        if(task.Exception != null)
        {
            HDebug.Log(task.Exception);
        }
    }
}

