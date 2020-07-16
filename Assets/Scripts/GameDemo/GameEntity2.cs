using PathFind;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
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
    Vector2ReactiveProperty RX_LastMoveFrom = null;// new Vector2ReactiveProperty(Vector2.zero);
    Vector2ReactiveProperty RX_LastMoveTo = null;// new Vector2ReactiveProperty(Vector2.zero);
    FloatReactiveProperty RX_moveFromA2BPer = new FloatReactiveProperty(0);
    Vector2ReactiveProperty RX_LastClickCell = new Vector2ReactiveProperty();

    Vector2ReactiveProperty RX_currentPoint = null;// new Vector2ReactiveProperty();

    ReactiveProperty<GameEntity> RX_targetEntity = new ReactiveProperty<GameEntity>();

    FloatReactiveProperty RX_moveSpeed = new FloatReactiveProperty(0);

    IDisposable Disposable_moveFromA2BPer;//负责改变transform


    IDisposable Disposable_movementTimeLine;
    IObservable<float> RX_reachFragment;

    const float C_TimeToReachOnePoint = .6f;
    #region 
    void initUniRxPrograming()
    {

        RX_LastMoveFrom = new Vector2ReactiveProperty(CurrentPoint);
        RX_LastMoveTo = new Vector2ReactiveProperty(CurrentPoint);
        RX_currentPoint = new Vector2ReactiveProperty(CurrentPoint);

        RX_LastClickCell.Subscribe(_ =>
        {
            RX_targetEntity.Value = null;
        });

        //如何过滤过快的点击切换路径？
        //假若RX_LastClickCell的输入频率是 0.3s 内来了 10个数据（玩家0.3s内点击了10个可以移动的地方）
        //则以最后一个数据为准作为通知
        RX_LastClickCell.Throttle(TimeSpan.FromSeconds(0.2f)).Subscribe(_ =>
        {
            RX_PathChanged.Value = true;
        });

        RX_currentPoint.Subscribe(point =>
        {
            mapController.SetStartPoint(Vector2Int.CeilToInt(point));
        });

        RX_LastMoveTo.Subscribe((to) =>
        {
            m_Transform.LookAt(HexCoords.GetHexVisualCoords(Vector2Int.CeilToInt(to)));
            RX_moveFromA2BPer.Value = 0;
            RX_moveSpeed.Value = 1;
        });
        RX_LastMoveTo.Buffer(RX_LastMoveTo.Where(per => per == RX_currentPoint.Value).Throttle(TimeSpan.FromSeconds(C_TimeToReachOnePoint))).Subscribe(_ =>
        {
            RX_moveSpeed.Value = 0;
        });


        #region 控制移动
        RX_moveFromA2BPer.Skip(1).Subscribe(per =>
        {
            entityVisual.Run2();
            Vector3 fromVisualPos = Coords.PointToVisualPosition(Vector2Int.CeilToInt(RX_LastMoveFrom.Value));
            Vector3 toVisualPos = Coords.PointToVisualPosition(Vector2Int.CeilToInt(RX_LastMoveTo.Value));
            Vector3 v = Vector3.Lerp(fromVisualPos, toVisualPos, RX_moveFromA2BPer.Value);
            m_Transform.position = v;

            //float near_start_or_near_dst = -0.5f + per;
            if (per >= 0.5f)
            {
                RX_currentPoint.Value = RX_LastMoveTo.Value;
            }
            else
            {
                RX_currentPoint.Value = RX_LastMoveFrom.Value;
            }
        });

        var idle = RX_moveSpeed.Where(speed => speed == 0);
        var confused = idle.Where(_ => { return false; });
        idle.Subscribe(_ =>
        {
            entityVisual.Idle2();
        });


        #endregion


        //0 0.5 0.9 1 1 1 0 0.6 1 -> 0 0.5 0.9 1 0 0.6 1
        RX_reachFragment = RX_moveFromA2BPer.DistinctUntilChanged();
        RX_reachFragment.Subscribe(_ =>
        {
            //Debug.Log(_);
        });
        RX_reachFragment.Where(per => per == 1).Subscribe(_ =>
        {
            RXEnterCellPoint(Vector2Int.CeilToInt(RX_currentPoint.Value));
        }
        );

        RX_moveFromA2BPer.Buffer(RX_moveFromA2BPer.Where(per => per == 1).Throttle(TimeSpan.FromSeconds(0.3f)))
            .Where(buffer => GameCore.GetGameStatus() == GameStatus.Run && buffer.Count >= 2).Subscribe(_ =>
        {
            if (GameEntityMgr.GetSelectedEntity() == this)
                ShowEyeSight();
        });


        RX_PathChanged.Where(changed => changed).Subscribe(_ =>
        {
            //allowMove = false;
            RX_PathChanged.Value = false;
            Disposable_movementTimeLine?.Dispose();

            IList<ICell> path = mapController.CalculatePath();

            //Disposable_movementTimeLine = Observable.FromCoroutine(MoveM).Subscribe(unit =>
            Disposable_movementTimeLine = Observable.FromCoroutine((tok) => MoveMS(path)).SelectMany(aftermove).Subscribe();

            //RX_moveAlongPath(path, tellmeHowToMove(UseForClickCellMove));
            //Disposable_movementTimeLine = Observable.EveryUpdate().CombineLatest(RX_reachFragment, (frame, per) =>
            //{
            //    return per;
            //}).Where(per => per >= 1).Where(per =>
            //{
            //    if (!controllRemote.PTiliMove(1))
            //    {
            //        Disposable_movementTimeLine.Dispose();
            //        return false;
            //    }
            //    return true;
            //}).StartWith(1).Subscribe(h =>
            //{
            //    Debug.Log("reach and show next fragment");
            //    if (path.Count > 1)
            //    {
            //        RX_LastMoveFrom.Value = path[0].Point;
            //        RX_LastMoveTo.Value = path[1].Point;
            //        path.RemoveAt(0);
            //    }
            //    else
            //    {
            //        Disposable_movementTimeLine.Dispose();
            //    }
            //});
            //if (path.Count > 0)
            //{
            //    //转变为 (1-2)-(2-3)-(3-4)-(4-5)队列
            //    var rawPath = path.ToObservable<ICell>();
            //    var skipheader = rawPath.Skip(1);
            //    var from_to_pathset = rawPath.Zip(skipheader, (raw, skip) =>
            //    {
            //        return new FromAndTo(raw.Point, skip.Point);
            //    });


            //    //要求路线按每隔 XXs 发出1个,其中第一段希望立即发出  这个时间没有基于gamecore状态
            //    var timeLine = Observable.EveryUpdate().CombineLatest(RX_reachFragment, (frame, per) =>
            //    {
            //        return per;
            //    }).Where(per => per == 1);
            //    Disposable_movementTimeLine = timeLine.Subscribe(async (__) =>
            //   {
            //       var s = await from_to_pathset.ToYieldInstruction();
            //       Debug.Log(__ + s.from.ToString() + " " + s.to);
            //       RX_LastMoveFrom.Value = s.from;
            //       RX_LastMoveTo.Value = s.to;

            //   });

            //    Disposable_movementTimeLine = timeLine.StartWith(1).Zip(from_to_pathset, (time, from_to) =>
            //    {
            //        return new { from_to = from_to, per = time };
            //    }).Where(zip => zip.per == 1).Do(zip =>
            //    {
            //        Debug.Log("change next ");
            //        var from_to = zip.from_to;
            //        RX_LastMoveFrom.Value = from_to.from;
            //        RX_LastMoveTo.Value = from_to.to;
            //    },
            //    () =>
            //    {
            //    }).Subscribe();
            //}
        });



        RX_alive.Value = BeAlive();
        var onDeath = RX_alive.Where(alive => !alive);
        onDeath.Subscribe(_ =>
            {
                Disposable_moveFromA2BPer.Dispose();
            }, () => { });


        //检测当前选中的玩家和第一个被点击的非玩家对象
        var getFrameStream = Observable.EveryUpdate();
        var selectEntityStream = getFrameStream.Select(_ => GameEntityMgr.GetSelectedEntity());// 1 0 2 0 1  
        var onSelectEntityChangedStream = selectEntityStream.DistinctUntilChanged().Where(newEntity => newEntity != null);// 1 0 2 0 1 => 1  2  1
        var selectEntityChooseNPCStream = RX_targetEntity.DistinctUntilChanged();//1 0 2 

        onSelectEntityChangedStream.Subscribe(_ =>
                {
                    //Debug.Log(_.entityID);
                });
        selectEntityChooseNPCStream.Subscribe(_ =>
        {
        });

        //选中玩家后，第一次选中不同的npc
        var rx_selectPlayer_then_npc = onSelectEntityChangedStream.CombineLatest(selectEntityChooseNPCStream, (frameDuringSelected, choosenpc) =>
        {
            //int error = -1;
            //if (choosenpc == null)
            //    return error;
            //if (choosenpc.BeAlive())
            //{
            //    return choosenpc.entityID;
            //}
            //return error;
            return choosenpc;
        })/*.DistinctUntilChanged()*/.Where(combineResults => /*combineResults != -1*/combineResults != null);

        rx_selectPlayer_then_npc.Subscribe(npc =>
                {
                    Debug.Log(npc.entityID);
                    //move to entity then attack
                    IList<ICell> path2reachEntity = mapController.GetPathFinder().FindPathOnMap(mapController.GetMap().GetCell(CurrentPoint), mapController.GetMap().GetCell(npc.CurrentPoint), mapController.GetMap());
                    ForgetMovement();
                    Disposable_movementTimeLine = Observable.FromCoroutine((tok) => MoveMS(path2reachEntity)).Subscribe();

                    //RX_moveAlongPath(path2reachEntity, tellmeHowToMove(UseForBattleMove));

                    //Observable.FromCoroutine(playerAction2Entity).Subscribe();
                });

        var rx_selectPlayer_then_mapcell = getFrameStream.CombineLatest(RX_LastClickCell.DistinctUntilChanged(), (frameDuringSelected, selectPoint) =>
        {
            return selectPoint;
        }).DistinctUntilChanged();

        rx_selectPlayer_then_mapcell.Subscribe(Cell =>
                {
                    //Debug.Log(Cell);
                });

    }

    IEnumerator aftermove()
    {
        entityVisual.Idle2();
        yield return null;
    }


    private IEnumerator MoveMS(IList<ICell> path)
    {
        while (path != null && path.Count > 1)
        {
            if (controllRemote.PTiliMove(1))
            {
                ICell start = path[0];
                ICell next = path[1];
                RX_LastMoveFrom.Value = start.Point;
                RX_LastMoveTo.Value = next.Point;
                while (RX_moveFromA2BPer.Value < 1)
                {
                    float f = RX_moveFromA2BPer.Value;
                    if (GameCore.GetGameStatus() == GameStatus.Run)
                        f += RX_moveSpeed.Value * Time.deltaTime / (C_TimeToReachOnePoint);
                    if (f >= 1)
                        f = 1;
                    RX_moveFromA2BPer.Value = f;
                    yield return null;
                }
                path.RemoveAt(0);
            }
            else
            {
                //yield return null;
                path = null;
            }
        }
    }

    class FromAndTo
    {
        public Vector2Int from;
        public Vector2Int to;
        public FromAndTo(Vector2Int from, Vector2Int to)
        {
            this.from = from;
            this.to = to;
        }
    }

    private void moveWhenClickCell()
    {
        Disposable_moveFromA2BPer = Observable.EveryUpdate().Where(_ =>
        {
            bool result = controllRemote.PTiliMove(1);
            if (result == false)
                Disposable_movementTimeLine?.Dispose();//当体力不足则取消buffer 路径
            return result;
        }).Subscribe(_ =>
           {
               float f = RX_moveFromA2BPer.Value;
               if (f >= 1)
                   return;
               f += Time.deltaTime / (C_TimeToReachOnePoint);
               if (f >= 1)
                   f = 1;
               RX_moveFromA2BPer.Value = f;
           });

    }

    private Action tellmeHowToMove(Func<long, bool> frameMovement)
    {
        if (frameMovement == null)
            return null;
        return () =>
        {
            Disposable_moveFromA2BPer?.Dispose();
            Disposable_moveFromA2BPer = Observable.EveryUpdate().Where(frameMovement).Subscribe(_ =>
            {
                float f = RX_moveFromA2BPer.Value;
                if (f >= 1)
                    return;
                f += Time.deltaTime / (C_TimeToReachOnePoint);
                if (f >= 1)
                    f = 1;
                RX_moveFromA2BPer.Value = f;
            });
        };
    }



    private bool UseForClickCellMove(long frameCnt)
    {
        bool result = controllRemote.PTiliMove(1);
        if (result == false)
            ForgetMovement();//当体力不足则取消buffer 路径
        return result;
    }

    private bool UseForBattleMove(long frameCnt)
    {
        bool tili = controllRemote.PTiliMove(1);
        bool targetInAttackSight = IsTargetEntityInAttackSight();

        if (!tili || targetInAttackSight)
        {
            ForgetMovement();
            RX_targetEntity.Value = null;
            return false;
        }
        return tili && !targetInAttackSight;
    }

    void ForgetMovement()
    {
        Disposable_movementTimeLine?.Dispose();
    }


    private void RX_moveAlongPath(IList<ICell> path, Action howtoMoveBetweenCell)
    {
        //当移动到下一个点之后，更新下一段；若完成移动，则销毁观察者

        //RX_reachFragment.Th(2).Where(per=> per == 1);
        Disposable_movementTimeLine = Observable.EveryUpdate().CombineLatest(RX_reachFragment, (frame, per) =>
        {
            return per;
        }).Where(per => per >= 1).Where(per =>
         {
             if (!controllRemote.PTiliMove(1))
             {
                 Disposable_movementTimeLine.Dispose();
                 return false;
             }
             return true;
         }).Subscribe(h =>
         {
             Debug.Log("reach and show next fragment");
             if (path.Count > 1)
             {
                 RX_LastMoveFrom.Value = path[0].Point;
                 RX_LastMoveTo.Value = path[1].Point;
                 path.RemoveAt(0);
             }
             else
             {
                 Disposable_movementTimeLine.Dispose();
             }
         });
        //howtoMoveBetweenCell();

    }

    private void RXEnterCellPoint(Vector2Int point)
    {
        if (currentCell.x == point.x && currentCell.y == point.y)
            return;
        currentCell = point;
        calculateRangeOnEnterPoint();
        fireEntityEvent(entityEvent.enterNewCell);
    }

    #endregion

}

public partial class GameEntityVisual
{
    IDisposable Disposable_idle;//负责改变animator
    ReactiveProperty<EntityAnimStatus> RX_status = new ReactiveProperty<EntityAnimStatus>(EntityAnimStatus.None);

    void initUniRx()
    {
        RX_status.Where(st => st == EntityAnimStatus.Idle).Subscribe(st =>
        {
            PlayAnim(EntityAnimEnum.Idle);
        });

        RX_status.Where(st => st == EntityAnimStatus.Run).Subscribe(st =>
        {
            PlayAnim(EntityAnimEnum.Run);
        });

        RX_status.Where(st => st == EntityAnimStatus.Battle).Subscribe(st =>
        {

        });

        //Disposable_idle = Observable.EveryUpdate().Where(_ => status == EntityAnimStatus.Idle).Subscribe(_ => {

        //});
    }

    public void Run2()
    {
        RX_status.Value = EntityAnimStatus.Run;
    }

    public void Idle2()
    {
        RX_status.Value = EntityAnimStatus.Idle;
    }
}
