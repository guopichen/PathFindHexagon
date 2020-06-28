using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface GameRemote
{
    int UpdateInterval { get; }
    void OnInitGame();
    void OnStartGame();
    void OnUpdateGame();
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
    private Dictionary<GameRemote, int> updateTimeSet = new Dictionary<GameRemote, int>();


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
    }

    public IEnumerator gameTick()
    {
        yield return null;
        while(coreStatus == GameStatus.Run)
        {
            yield return null;

        }
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
        string prefabname = "GameEntity";
        GameObject prefab = Resources.Load<GameObject>(prefabname);
        GameObject clone = GameObject.Instantiate(prefab);
        GameEntity entity = clone.GetComponent<GameEntity>();
        entity.SetControllType(EntityControllType.Player);
        GetRegistServices<GameEntityMgr>().RegEntity(clone.GetComponent<GameEntity>());

    }

    private Dictionary<string, object> servicesUnknownSet = new Dictionary<string, object>();
    public static void RegistOtherServices<T>(T service) where T : class
    {
        Type t =(service.GetType());
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
    }

    private void OnDestroy()
    {
        coreStatus = GameStatus.End;
        foreach (GameRemote remote in gameRemoteSet)
        {
            remote.OnEndGame();
        }
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
