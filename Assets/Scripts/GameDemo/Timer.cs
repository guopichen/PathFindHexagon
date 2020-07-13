#if UNITYEDITOR
#define HTools
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using Debug = UnityEngine.Debug;
public static class GameTimer
{
    #region 用于通常情况下的
    public static void AwaitSeconds(float seconds, Action action)
    {
        //using (Subject<float> subject = new Subject<float>())
        //{
        //    subject.Subscribe((_inputData) =>
        //    {
        //        TimeSpan _seconds = new TimeSpan((long)_inputData * TimeSpan.TicksPerSecond);
        //        Observable.Timer(_seconds).Subscribe(__ =>
        //        {
        //            action();
        //        });
        //    });
        //    subject.OnNext(seconds);
        //    subject.OnCompleted();
        //}

        Observable.Create<float>((observer) =>
        {
            observer.OnNext(seconds);
            observer.OnCompleted();
            return Disposable.Empty;
        }).Subscribe(_inputseconds =>
        {
            TimeSpan _seconds = TimeSpan.FromSeconds(_inputseconds);
            Observable.Timer(_seconds).Subscribe(__ =>
            {
                action();
            });
        });
    }

    public static void AwaitLoopSeconds(float seconds, Func<bool> action)
    {
        #region 写法1
        //Observable.Create<float>((observer) =>
        //{
        //    observer.OnNext(seconds);
        //    observer.OnCompleted();
        //    return Disposable.Empty;
        //}).Subscribe(_inputseconds =>
        //{
        //    TimeSpan _seconds = TimeSpan.FromSeconds(_inputseconds);
        //    Observable.Timer(_seconds).Subscribe(__ =>
        //    {
        //        if (action())
        //        {
        //            AwaitLoopSeconds(seconds, action);
        //        }
        //    });
        //});
        #endregion
        #region 写法2 低gc
        Observable.Create<float>(observer =>
        {
            bool loop = false;
            float start = 0;
            Action innerAction = () =>
            {
                start = 0;
                loop = action();
                if (!loop)
                {
                    observer.OnCompleted();
                }
            };
            IDisposable disposable = Observable.EveryEndOfFrame().Subscribe(frameCnt =>
            {
                start += Time.deltaTime;
                if (start >= seconds)
                {
                    innerAction();
                }
            });
            innerAction += () =>
            {
                if (!loop)
                    disposable.Dispose();
            };
            return
            Disposable.Empty;
        }).Subscribe();
        #endregion
    }

    #endregion

    #region 基于GameCore的情况,时间流逝需要考虑到GameCore的状态，同时一定要在GameCore工作之后使用
    public static void DelayJobBaseOnCore(float seconds, Action action)
    {
        AwaitSecondsBaseOnCore(seconds, action);
    }


    public static void AwaitSecondsBaseOnCore(float seconds, Action action)
    {
        //最简单的使用unitx，用subject
        //using (Subject<float> subject = new Subject<float>())
        //{
        //    subject.Subscribe((_totalSeconds) =>
        //    {
        //        Action innerAction = () => { action(); };
        //        IDisposable disposable = Observable.EveryEndOfFrame().Subscribe(frameCnt =>
        //            {
        //                if (GameCore.Instance.coreStatus == GameStatus.Run)
        //                {
        //                    _totalSeconds -= Time.deltaTime;
        //                }
        //                if (_totalSeconds <= 0)
        //                {
        //                    innerAction();
        //                }
        //            });
        //        innerAction += () => { disposable.Dispose(); };
        //    });
        //    subject.OnNext(seconds);
        //    subject.OnCompleted();
        //}

        Observable.Create<float>(observer =>
        {
            float start = 0;
            Action innerAction = () =>
            {
                action();
                observer.OnCompleted();
            };
            IDisposable disposable = Observable.EveryEndOfFrame().Subscribe(frameCnt =>
            {
                if (GameCore.Instance.coreStatus == GameStatus.Run)
                {
                    start += Time.deltaTime;
                }
                if (start >= seconds)
                {
                    innerAction();
                }
            });
            innerAction += () =>
            {
                disposable.Dispose();
            };
            return Disposable.Empty;
        }).Subscribe();

        return;
    }

    public static void LoopJobBaseOnCore(float seconds, Func<bool> action)
    {
        AwaitLoopSecondsBaseOnCore(seconds, action);
    }

    public static void AwaitLoopSecondsBaseOnCore(float seconds, Func<bool> action)
    {
        Observable.Create<float>(observer =>
        {
            bool loop = false;
            float start = 0;
            Action innerAction = () =>
            {
                start = 0;
                loop = action();
                if (!loop)
                {
                    observer.OnCompleted();
                }
            };
            IDisposable disposable = Observable.EveryEndOfFrame().Subscribe(frameCnt =>
            {
                if (GameCore.Instance.coreStatus == GameStatus.Run)
                {
                    start += Time.deltaTime;
                }
                if (start >= seconds)
                {
                    innerAction();
                }
            });
            innerAction += () =>
            {
                if (!loop)
                    disposable.Dispose();
            };
            return Disposable.Empty;
        }).Subscribe();
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
        await task;
        //do nothing
        if (task.Exception != null)
        {
            HDebug.Log(task.Exception);
        }

    }
}

