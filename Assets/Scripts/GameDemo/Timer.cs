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
        //await GameCore.Instance.gameTick();
        while (GameCore.GetGameStatus() != GameStatus.Run)
            await new WaitForEndOfFrame();
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

    public static async Task AwaitSeconds(float seconds,Task t,Action<Task> t2)
    {
        await new WaitForSeconds(seconds);
        while (GameCore.GetGameStatus() != GameStatus.Run)
            await new WaitForEndOfFrame();
        t.ContinueWith(t2).ForgetAwait();
    }
    #endregion

    #region 基于GameCore的情况


    #endregion
}


public static class HDebug
{

    [Conditional("HTools")]
    public static void Log(object msg)
    {
        UnityEngine.Debug.Log(msg);
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

