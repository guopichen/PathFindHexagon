using PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

//用于实践UniRx的思想
//将entity的行为基于观察者模式进行拆分
//本质上很多游戏的核心都可以解释成基于数据的观察行为
public partial class GameEntity
{
    BoolReactiveProperty RX_PathChanged = new BoolReactiveProperty(false);
    BoolReactiveProperty RX_alive = new BoolReactiveProperty(false);
    Vector2ReactiveProperty RX_moveFrom = null;// new Vector2ReactiveProperty(Vector2.zero);
    Vector2ReactiveProperty RX_moveTo = null;// new Vector2ReactiveProperty(Vector2.zero);
    //new Vector2ReactiveProperty(Vector2.zero);
    FloatReactiveProperty RX_movePer = new FloatReactiveProperty(0);


    ReactiveCollection<Vector2> a;

    IDisposable transformDisposable;//负责改变transform
    IDisposable animatorDisposable;//负责改变animator



    bool allowMove = false;


    const float C_TimeToReachOnePoint = 0.1f;

    IDisposable timeLineDispose;

    void initUniRxPrograming()
    {
        RX_moveFrom = new Vector2ReactiveProperty(currentCell);
        RX_moveTo = new Vector2ReactiveProperty(currentCell);
        RX_moveTo.Subscribe((to) =>
        {
            //Debug.Log("change allow ");
            if (RX_PathChanged.Value)
                allowMove = false;
            else
                allowMove = true;
        });
        RX_movePer.Subscribe(per =>
        {
            Vector3 fromVisualPos = Coords.PointToVisualPosition(Vector2Int.CeilToInt(RX_moveFrom.Value));
            Vector3 toVisualPos = Coords.PointToVisualPosition(Vector2Int.CeilToInt(RX_moveTo.Value));
            Vector3 v = Vector3.Lerp(fromVisualPos, toVisualPos, RX_movePer.Value);
            m_Transform.position = v;
            if (per >= 0.5f)
            {
                enterCellPoint(Vector2Int.CeilToInt(RX_moveTo.Value));
            }
            else
            {
                enterCellPoint(Vector2Int.CeilToInt(RX_moveFrom.Value));
            }
        });
        

        //如何过滤过快的点击切换路径？
        RX_PathChanged.Throttle(TimeSpan.FromMilliseconds(100)).Where(changed => changed).Subscribe(_ =>
        {
            //allowMove = false;
            RX_PathChanged.Value = false;
            timeLineDispose?.Dispose();
            //获得要行进的路线   1-2-3-4-5
            IList<ICell> path = mapController.CalculatePath();

            timeLineDispose = Observable.EveryUpdate().Where(cnt => RX_movePer.Value >= 1).Subscribe(h =>
            {
                if (path.Count > 1)
                {
                    RX_moveFrom.Value = path[0].Point;
                    RX_moveTo.Value = path[1].Point;
                    RX_movePer.Value = 0;
                    path.RemoveAt(0);
                }
                else
                {
                    timeLineDispose.Dispose();
                }
            });
            if (path.Count > 0 && false)
            {
                //转变为 (1-2)-(2-3)-(3-4)-(4-5)队列
                var rawPath = path.ToObservable<ICell>();
                var skipheader = rawPath.Skip(1);
                var from_to_pathset = rawPath.Zip(skipheader, (raw, skip) =>
                {
                    return new { from = raw.Point, to = skip.Point };
                });

                //要求路线按每隔 XXs 发出1个,其中第一段希望立即发出  这个时间没有基于gamecore状态
                var timeLine = Observable.Interval(TimeSpan.FromSeconds(C_TimeToReachOnePoint)).Take(path.Count - 1).StartWith(1);
                timeLineDispose = timeLine.Zip(from_to_pathset, (time, from_to) =>
                {
                    return new { time = DateTime.Now, from_to = from_to };
                }).Subscribe(time_fromto =>
                {
                    var from_to = time_fromto.from_to;
                    //Debug.Log(DateTime.Now + from_to.from.ToString() + " " + from_to.to + time_fromto.time);
                    RX_moveFrom.Value = from_to.from;
                    RX_moveTo.Value = from_to.to;
                    RX_movePer.Value = 0;
                },
                () =>
                {
                    allowMove = false;
                });
            }
        });

        transformDisposable = Observable.EveryUpdate().Subscribe(_ =>
        {
            if (allowMove)
            {
                float f = RX_movePer.Value;
                if (f >= 1)
                    return;
                f += Time.deltaTime / (C_TimeToReachOnePoint);
                if (f >= 1)
                    f = 1;
                RX_movePer.Value = f;
            }
        });

        RX_alive.Value = BeAlive();
        var onDeath = RX_alive.Where(alive => !alive);
        onDeath.Subscribe(_ =>
        {
            transformDisposable.Dispose();
        }, () => { });


    }




}
