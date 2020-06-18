using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public static class GameTimer
{
    public static async Task AwaitSeconds(float seconds, Action action)
    {
        await new WaitForSeconds(seconds);
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
}


public static class HDebug
{
    [Conditional("Debug")]
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

    private static void handleException(Task arg1)
    {
        //do nothing
    }
}

