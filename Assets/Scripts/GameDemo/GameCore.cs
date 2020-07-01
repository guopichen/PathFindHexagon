using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface GameRemote: IGameInit, IGameStart,IGameUpdate, IGameEnd
{
}

public interface IGameInit
{
    void OnInitGame();
}

public interface IGameStart
{
    void OnStartGame();

}


public interface IGameUpdate
{
    int UpdateInterval { get; }//间隔帧数
    void OnUpdateGame();

}
public interface IGameEnd
{
    void OnEndGame();

}

public enum GameStatus
{
    None,
    Idle,
    Pause,
    Run,
    End,
}

public class GameCore : MonoBehaviour
{
    public GameStatus coreStatus = GameStatus.None;

    private List<GameRemote> gameRemoteSet = new List<GameRemote>();
    private Dictionary<IGameUpdate, int> updateTimeSet = new Dictionary<IGameUpdate, int>();


    private List<IGameInit> callOnCoreFirstActive = new List<IGameInit>();
    private List<IGameStart> callOnGameStart = new List<IGameStart>();
    private List<IGameUpdate> callOnCoreUpdate = new List<IGameUpdate>();
    private List<IGameEnd> callOnGameEnd = new List<IGameEnd>();

    private void initCore()
    {
        gameRemoteSet.Add(new GameEntityMgr());
        gameRemoteSet.Add(new BattleService());


        coreStatus = GameStatus.Idle;
    }
    IEnumerator Start()
    {
        foreach (GameRemote remote in gameRemoteSet)
        {
            remote.OnInitGame();
        }
        foreach(IGameInit remote in callOnCoreFirstActive)
        {
            remote.OnInitGame();
        }
        yield return new WaitForSeconds(3);
        StartGame();
    }

    public void StartGame()
    {
        coreStatus = GameStatus.Run;
        foreach (GameRemote remote in gameRemoteSet)
        {
            remote.OnStartGame();
        }
        foreach(IGameStart remote in callOnGameStart)
        {
            remote.OnStartGame();
        }
    }

    public IEnumerator CoreTick()
    {
        do
        {
            yield return null;
        } while (coreStatus != GameStatus.Run);
    }

    internal static void RunOrPauseCore()
    {
        if (!Instance.gameObject.activeInHierarchy)
        {
            Instance.gameObject.SetActive(true);
        }

        if (Instance.coreStatus == GameStatus.Run)
        {
            Instance.coreStatus = GameStatus.Pause;
        }
        else if (Instance.coreStatus == GameStatus.Pause)
            Instance.coreStatus = GameStatus.Run;
    }

    internal static void SpawnNPC()
    {
        string prefabname = "GameEntity";
        GameObject prefab = Resources.Load<GameObject>(prefabname);
        GameObject clone = GameObject.Instantiate(prefab);
        clone.name = "npc";
        GameEntity entity = clone.GetComponent<GameEntity>();
        entity.SetControllType(EntityControllType.AI);
        GetRegistServices<GameEntityMgr>().RegEntity(entity);
    }

    internal static void SpawnPlayer()
    {
        GameEntityMgr entityMgr = GetRegistServices<GameEntityMgr>();
        int cnt = entityMgr.GetAllPlayers().Count;
        if (cnt >= ProjectConsts.MAXPLAYER_CONTROLL_ENTITY_CNT)
        {
            return;
        }


        string prefabname = "GameEntity";
        GameObject prefab = Resources.Load<GameObject>(prefabname);
        GameObject clone = GameObject.Instantiate(prefab);
        GameEntity entity = clone.GetComponent<GameEntity>();
        entity.SetControllType(EntityControllType.Player);
        entityMgr.RegEntity(clone.GetComponent<GameEntity>());

    }

    private Dictionary<string, object> servicesUnknownSet = new Dictionary<string, object>();
    public static void RegistOtherServices<T>(T service) where T : class
    {
        Type t = (service.GetType());
        string key = t.ToString();
        if (Instance.servicesUnknownSet.ContainsKey(key))
            return;

        Instance.servicesUnknownSet.Add(key, service);
    }

    public static T GetRegistServices<T>() where T : class
    {
        Type t = typeof(T);
        string key = t.ToString();
        if (Instance.servicesUnknownSet.TryGetValue(key, out object o))
        {
            return o as T;
        }
        return null;
    }




    void Update()
    {
        if (coreStatus != GameStatus.Run)
            return;

        foreach (GameRemote remote in gameRemoteSet)
        {
            if (!updateTimeSet.ContainsKey(remote))
            {
                updateTimeSet.Add(remote, 0);
            }
            if (--updateTimeSet[remote] <= 0)
            {
                remote.OnUpdateGame();
                updateTimeSet[remote] = remote.UpdateInterval;
            }
        }

        foreach(IGameUpdate remote in callOnCoreUpdate)
        {
            if (!updateTimeSet.ContainsKey(remote))
            {
                updateTimeSet.Add(remote, 0);
            }
            if (--updateTimeSet[remote] <= 0)
            {
                remote.OnUpdateGame();
                updateTimeSet[remote] = remote.UpdateInterval;
            }
        }


    }

    private void OnDestroy()
    {
        coreStatus = GameStatus.End;
        foreach (GameRemote remote in gameRemoteSet)
        {
            remote.OnEndGame();
        }
        foreach (IGameEnd remote in gameRemoteSet)
            remote.OnEndGame();
    }

    public void RegGameRemote(GameRemote remote)
    {
        if (gameRemoteSet.Contains(remote) == false)
        {
            gameRemoteSet.Add(remote);
        }
    }


    static GameCore gameCore = null;

    public static GameCore Instance
    {
        get
        {
            if (gameCore == null)
            {
                GameObject go = new GameObject("GameCore");
                go.SetActive(false);
                gameCore = go.AddComponent<GameCore>();
                gameCore.initCore();
            }
            return gameCore;
        }
    }

    public static GameObject RequestServiceGo(GameServiceBase serviceBase)
    {
        GameCore.RegistOtherServices<GameServiceBase>(serviceBase);
        Instance.RegGameRemote(serviceBase);
        GameObject go = new GameObject(serviceBase.GetType().ToString());
        return go;
    }

    public static GameStatus GetGameStatus()
    {
        if (Instance == null)
            return GameStatus.None;
        return Instance.coreStatus;
    }

    public void AddIGame(IGameInit init)
    {
        callOnCoreFirstActive.Add(init);
    }
    public void AddIGame(IGameStart start)
    {
        callOnGameStart.Add(start);
    }

    public void AddIGame(IGameUpdate update)
    {
        callOnCoreUpdate.Add(update);
    }
    public void AddIGame(IGameEnd end)
    {
        callOnGameEnd.Add(end);
    }

}

public class GameServiceBase : GameRemote
{
    GameObject serviceGo;
    public GameServiceBase()
    {
        serviceGo = GameCore.RequestServiceGo(this);
    }

    public virtual int UpdateInterval => 1;

    public void OnEndGame()
    {
    }

    public virtual void OnInitGame()
    {
    }

    public virtual void OnStartGame()
    {
    }

    public virtual void OnUpdateGame()
    {
    }
}
