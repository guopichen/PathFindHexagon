using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Threading.Tasks;

public class UniRxTutorial
{


    void Example1()
    {
        Subject<string> sub = new Subject<string>();
        sub.Subscribe((inputstr) =>
        {

        });
        sub.OnNext("1st");
        sub.OnNext("2nd");
        sub.OnNext("3rd");

        sub.OnCompleted();

        ReplaySubject<int> replaySubject = new ReplaySubject<int>();

        replaySubject.Subscribe((_int) => { });


        replaySubject.OnNext(1);
        replaySubject.OnNext(2);
        replaySubject.OnCompleted();
    }


    void Example2()
    {
        Observable.Create<string>(observer =>
        {
            observer.OnNext("1st");
            observer.OnNext("2nd");
            observer.OnNext("3rd");
            return Disposable.Empty;
        }).Subscribe(_inputstr =>
        {
            //deal with onnext input

        });
    }

    void Example3()
    {
        Observable.Timer(TimeSpan.FromMilliseconds(1)).Subscribe((time) =>
        {
            //first onnext when time elapsed
        }, () =>
        {
            //when onnext finish,oncomplete
        });
    }

    void Example4()
    {
        Observable.Start(() =>
        {
            //Job A
        }).Subscribe(publishbyJobA =>
        {
            //when Job A finish , it publish a value and send value here
        }, () =>
        {
            //when deal with publishbyJobA finish, do this oncomplete
        });

        Observable.Start(() =>
        {
            return "value";
        }).Subscribe(value =>
        {
            //deal with value
        }, () =>
        {
            //on deal with value complete
        });
    }

    void Example5()
    {
        var observable = Task.Run(() => { }).ToObservable();
        observable.Subscribe((unit) => { }, () => { });

    }

    public void Example6()
    {
        var observable = ienumerator().ToObservable();
        observable.Subscribe((unit) =>
        {
            Debug.Log("ienumerator run finish");
        }, () => { Debug.Log("completesssss"); });
    }

    IEnumerator ienumerator()
    {
        while (true)
        {
            yield return "sssss";
        }
    }

    void Example7(IList<GameEntityRemote> sss)
    {
        Observable.Create<GameEntityRemote>(observer =>
        {
            foreach (var remote in sss)
            {
                observer.OnNext(remote);
            }
            return Disposable.Empty;
        }).Subscribe((inputRemote) => { }, () => { });
    }


    void Example8()
    {
        Observable.Range(1, 10)
            .Where(i => i % 2 == 0)
            .Subscribe(
            (OuShu) =>
            {
            },
            () =>
            {
            });
    }

    void Example9()
    {
        int[] set = new int[] { 1, 2, 1, 2 };

        Subject<int> sub = new Subject<int>();
        IObservable<int> distinct = sub.Distinct<int>();
        IObservable<int> distinctUntilChange = sub.DistinctUntilChanged<int>();


        distinctUntilChange.Subscribe(a =>
        {
            Debug.Log("distinctUntilChange " + a);

        });
        distinct.Subscribe((a) =>
        {
            Debug.Log("distinct " + a);
        });
        sub.Subscribe((a) =>
        {
            Debug.Log("raw " + a);
        });

        foreach (int i in set)
        {
            sub.OnNext(i);//this will publish to distinct/distinctUntilChange/sub
        }

        List<GameEntity> set2 = new List<GameEntity>();
        Subject<GameEntity> sub2 = new Subject<GameEntity>();
        IObservable<GameEntity> distinct2 = sub2.Distinct(x => x.GetTargetEntity());//基于当前目标敌人作为主键筛选


    }


    void Example10()
    {
        int[] set = new int[] { 1, 2, 1, 2 };

        Subject<int> sub = new Subject<int>();
        var noInt = sub.IgnoreElements<int>();
        noInt.Subscribe(a =>
        {

        }, () =>
        {
            //FINISH
        });
        foreach (int i in set)
        {
            sub.OnNext(i);
        }
    }

    void Example11()
    {
        int[] set = new int[] { 1, 2, 1, 2 };
        set.ToObservable<int>().Take(3).Subscribe(a =>
        {
            //T T T F
            //1 2 1 2
        });

        set.ToObservable<int>().Skip(3).Subscribe(a =>
        {
            //F F F T
            //1 2 1 2
        });


        Observable.Interval(TimeSpan.FromSeconds(1)).Take(3).Subscribe(callcnt =>
        {
            // 0 1 2
            //本来是无限的通知，但却变成了 3秒前每隔1秒就执行一次
        },
        () =>
        {
            //执行到3s后，完毕  oncomplete
        });

    }
    void Example12()
    {
        int[] set = new int[] { 1, 2, 1, 2, 9, 1, 2, 1, 2, 9 };
        set.ToObservable<int>().TakeWhile(i => { return i < 9; }).Subscribe(a =>
       {
           //T  T  T  T  F  x x x x
           //1, 2, 1, 2, 9 ,1,2,1,2,9
       });

        set.ToObservable<int>().SkipWhile(i => { return i < 9; }).Subscribe(a =>
        {

            //X  X  X  X  F  T T T T T
            //1, 2, 1, 2, 9 ,1,2,1,2,9
        });
    }

    void Example13()
    {
        int[] set = new int[] { 1, 2, 1, 2, 9, 1, 2, 1, 2, 9 };
        Subject<int> sub = new Subject<int>();
        var defaultempty = sub.DefaultIfEmpty();
        var default42 = sub.DefaultIfEmpty(42);
        defaultempty.Subscribe(a =>
        {
            //same as set
        });
        default42.Subscribe(a =>
        {
            //EXTRA 42 INPUT
        });

        foreach (int i in set)
        {
            sub.OnNext(i);
        }

        sub.OnCompleted();
    }

    void Example14()
    {
        int[] set = new int[] { 1, 2, 1, 2, 9, 1, 2, 1, 2, 9 };
        Subject<int> sub = new Subject<int>();
        var sequence = sub.ToSequentialReadOnlyReactiveProperty<int>();
        sequence.Subscribe(cnt =>
        {

        });

        foreach (int i in set)
        {
            sub.OnNext(i);
        }

        sub.OnCompleted();
    }

    void Example15()
    {
        IObservable<long> noend = Observable.Interval(TimeSpan.FromSeconds(3));
        Debug.Log(noend.First());//这里会blocking 3 s
        noend.FirstOrDefault();//block

        noend.Last();//block till noend complete
        noend.LastOrDefault();//block till noend complete

        noend.Single();//block until noend publish one value and complete, if over one value published,error happens

    }



    //重点
    void Example16()
    {
        //将 IObservable<T1> 转变为 IObservable<T2>
        int[] set = new int[] { 1, 2, 1, 2, 9, 1, 2, 1, 2, 9 };
        IObservable<int> t1 = set.ToObservable<int>();
        IObservable<InnerInt> t2 = t1.Select(x => { return new InnerInt(x); });
        t2.Subscribe(innerInt =>
        {
            innerInt.DoSomething();
        });
    }
    class InnerInt
    {
        public int i;
        public InnerInt(int i)
        {
            this.i = i;
        }
        public void DoSomething()
        {

        }
    }


    public void Example17()
    {
        object[] objset = new object[] { 1, 1D, 1f, 1L, "1", new InnerInt(1) };

        var intobservable = objset.ToObservable<object>().OfType<object, int>();
        var doubleobservable = objset.ToObservable<object>().OfType<object, double>();
        var floatobservable = objset.ToObservable<object>().OfType<object, float>();
        var longobservable = objset.ToObservable<object>().OfType<object, long>();
        var stringobservable = objset.ToObservable<object>().OfType<object, string>();
        var innerobservable = objset.ToObservable<object>().OfType<object, InnerInt>();

        intobservable.Subscribe(_ =>
        {
            Debug.Log("int" + _);
        });
        doubleobservable.Subscribe(_ => { Debug.Log("double " + _); });
        floatobservable.Subscribe(_ => { Debug.Log("float " + _); });
        longobservable.Subscribe(_ => { Debug.Log("long " + _); });
        stringobservable.Subscribe(_ => { Debug.Log("string " + _); });
        innerobservable.Subscribe(_ => { Debug.Log("class " + _); });

        objset.ToObservable<object>().Subscribe();
    }

    public void Example18()
    {
        Observable.Interval(TimeSpan.FromSeconds(1)).Take(3).Timestamp().Dump("timestamp");
        Observable.Interval(TimeSpan.FromSeconds(1)).Take(3).TimeInterval().Dump("timeinterval");
        Observable.Interval(TimeSpan.FromSeconds(1)).Take(3).Materialize().Dump("materialize");
    }

    public void Example19()
    {
        Observable.Range(1, 30).SelectMany((i) => { if (i < 27 && i > 0) return Observable.Return<string>("A"); return Observable.Empty<string>(); }).Dump("select many");
    }

    void Example20()
    {
        var query = from i in Observable.Range(1, 5)
                    where i % 2 == 0
                    select i;
        Observable.Range(1, 5).Where(i => i % 2 == 0).Select(i => i);
    }


    public void Example21()
    {
        var range = Observable.Range(0, 3);
        int index = -1;
        var errorResult = range.Select(i => { index++; return i; });
        errorResult.Subscribe(i => { Debug.Log("input is " + i + " index is " + index); }, () => { /*index = -1;*/ });
        errorResult.Subscribe(i => { Debug.Log("input2 is " + i + " index2 is " + index); });


        var result = range.Select((idx, element) => new { Index = idx, e = element });
        result.Subscribe(i => { Debug.Log("ok input is " + i.e + " index is " + i.Index); });
        result.Subscribe(i => { Debug.Log("ok input2 is " + i.e + " index2 is " + i.Index); });
    }

    void Example22()
    {
        Subject<InnerInt> sub = new Subject<InnerInt>();

        sub.Subscribe(innerInt => innerInt.i = 999);
        sub.Subscribe(innerInt => Debug.Log(innerInt.i));


        sub.OnNext(new InnerInt(1));
        sub.OnNext(new InnerInt(10));//传递的是引用
        sub.OnCompleted();
    }

    public  void Example23()
    {
        
        var result = Observable.Range(1, 3).Do(i => { Debug.Log("call before onnext"); }, e => { Debug.Log("call before error"); }, () => { Debug.Log("call before complete"); });
        result.Subscribe(i => { Debug.Log("on next " + i); }, () => { Debug.Log("complete"); });

        //建议do中完成数据的改动
        Subject<InnerInt> sub = new Subject<InnerInt>();

        var better = sub.Do(innerInt => innerInt.i = 999);
        better.Subscribe(innerInt => Debug.Log(innerInt.i));
        sub.OnNext(new InnerInt(1));
        sub.OnNext(new InnerInt(10));//传递的是引用
        sub.OnCompleted();
    }

    public void Example24()
    {
        Observable.Range(1, 3).Merge(Observable.Range(4, 4)).Dump("merge");//i do not know

    }
}

public static class expand
{
    public static void Dump<T>(this IObservable<T> source, string name)
    {
        source.Subscribe(i =>
        {
            Debug.Log(string.Format("{0}-->{1}", name, i));
        }, ex =>
        {
            Debug.LogError("error");
        }, () =>
        {
            Debug.Log(string.Format("{0}-->finish", name));
        });
    }
}

